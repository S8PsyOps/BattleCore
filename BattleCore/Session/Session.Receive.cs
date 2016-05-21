//-----------------------------------------------------------------------
//
// NAME:        Session.Receive.cs
//
// PROJECT:     Battle Core Session Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Session recieve implementation.
//
// NOTES:       None.
//
// $History: Session.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using BattleCore.Protocol;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

// Namespace declaration
namespace BattleCore.Session
{
   /// <summary>
   /// Session Object.  This object handles a session with the server
   /// </summary>
   partial class Session
   {
      /// <summary>
      /// Packet Receive handler.  This method routes the packets to
      /// the correct handler.
      /// </summary>
      /// <param name="packet">packet received from the server</param>
      internal void PacketReceiveHandler (Byte[] packet)
      {
         if (packet.Length > 0)
         {
            // Handle the normal packet
            int nIndex = m_gamePacketHandlers.IndexOfKey (packet[0]);

            if (nIndex != -1)
            {
               // Get the existing reference to the packet handler
               ((PacketHandler)(m_gamePacketHandlers.GetByIndex (nIndex))).Handler (packet);
            }
         }
      }

      /// <summary>
      /// Special Packet Receive handler.  This method routes the packets to
      /// the correct handler.
      /// </summary>
      /// <param name="packet">packet received from the server</param>
      private void SpecialPacketReceiveHandler (Byte[] packet)
      {
         if (packet.Length > 0)
         {
            // Handle the normal packet
            int nIndex = m_specialPacketHandlers.IndexOfKey (packet[1]);

            if (nIndex != -1)
            {
               // Get the existing reference to the packet handler
               ((PacketHandler)(m_specialPacketHandlers.GetByIndex (nIndex))).Handler (packet);
            }
         }
      }

      /// <summary>
      /// Handler called when we successfully logged in
      /// </summary>
      private void HandleLoginSuccessful ()
      {
         // Add the game packet handlers
         AddGamePacketHandler (0x02, new SessionPacketHandler (HandleInGameFlag));
         AddGamePacketHandler (0x05, new SessionPacketHandler (HandlePlayerWeapon));
         AddGamePacketHandler (0x18, new SessionPacketHandler (HandleSecurityRequest));
         AddGamePacketHandler (0x29, new SessionPacketHandler (HandleLevelInfo));

         // Remove handlers that are no longer used
         m_gamePacketHandlers.Remove ((Byte)0x0A);
         m_gamePacketHandlers.Remove ((Byte)0x31);
      }

      /// <summary>
      /// Handle disconnect notification
      /// </summary>
      private void HandleDisconnectNotify ()
      {
         // Reset the session statistics
         m_sessionStatistics.Reset ();

         // Check if the socket is active
         if (m_sessionSocket.Active)
         {
            // Close the session socket
            m_sessionSocket.Close ();
         }

         // Clear all handlers
         m_specialPacketHandlers.Clear ();
         m_gamePacketHandlers.Clear ();
      }

      /// <summary>
      /// Handle the server encryption response
      /// </summary>
      /// <param name="pBuffer"></param>
      private void HandleEncryptionResponse (Byte[] pBuffer)
      {
         // Remove the stuff we won't be needing
         m_specialPacketHandlers.Remove ((Byte)0x02);
         m_specialPacketHandlers.Remove ((Byte)0x05);

         // Sync the client and server clocks
         SendSynchronizeRequest ();

         // Add the special packet handlers
         AddSpecialPacketHandler (0x06, new SessionPacketHandler (HandleSynchronizeResponse));

         // Add the game packet handlers
         AddGamePacketHandler (0x0A, new SessionPacketHandler (HandlePasswordResponse));
         AddGamePacketHandler (0x10, new SessionPacketHandler (HandleFileTransfer));
         AddGamePacketHandler (0x31, new SessionPacketHandler (HandleLoginNext));

         // Send the login request packet
         SendLoginRequest (false);
      }

      /// <summary>
      /// Handle the server time syncronization request
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleSynchronizeRequest (Byte[] pBuffer)
      {
         // Parse the sync request packet
         SyncRequestPacket syncRequest = new SyncRequestPacket ();
         syncRequest.Packet = pBuffer;

         // Create the sync response packet
         SyncResponsePacket syncResponse = new SyncResponsePacket ();
         syncResponse.ServerTime = syncRequest.TimeStamp;

         // Send the packet to the server
         TransmitPacket (syncResponse);
      }

      /// <summary>
      /// Handle the server time syncronization response
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleSynchronizeResponse (Byte[] pBuffer)
      {
         SyncResponsePacket packet = new SyncResponsePacket ();

         // Set the response packet
         packet.Packet = pBuffer;

         // Set the current ping time
         m_sessionStatistics.CurrentPing = packet.PingTime;
      }

      /// <summary>
      /// Handle the In Game flag
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleInGameFlag (Byte[] pBuffer)
      {
         // if the staff password is set, send it now
         if (m_sessionSettings.StaffPassword.Length != 0)
         {
            // Send the staff password packet to the server
            ChatPacket chatPacket = new ChatPacket ();
            chatPacket.Event.Message = "*" + m_sessionSettings.StaffPassword;
            TransmitPacket (chatPacket);
         }
      }

      /// <summary>
      /// Handle the disconnect request
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleDisconnect (Byte[] pBuffer)
      {
         m_sessionLogger ("Disconnected by server");
      }

      /// <summary>
      /// Handle the password response
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandlePasswordResponse (Byte[] pBuffer)
      {
         // Create the password response packet
         PasswordResponsePacket packet = new PasswordResponsePacket ();
         packet.Packet = pBuffer;

         // Log the response to the Console
         m_sessionLogger (packet.ResponseString);

         // Check if player registration is requested
         if (packet.RequestRegistration)
         {
            // Send the player registration form
            SendPlayerRegistration ();
         }

         switch (packet.Error)
         {
         case LoginResponseError.BillerDown:
         case LoginResponseError.Success:
         case LoginResponseError.SpectateOnly:
            // Notify the handlers that login was successful
            m_loginSuccessNotify ();

            // Send the arena enter request
            SendArenaEnterRequest ();

            // Spectate the arena
            SendArenaSpectateRequest ();
            break;

         case LoginResponseError.UnknownUser:
            // Wait one second, then try to reconnect
            Thread.Sleep (1000);
            SendLoginRequest (true);
            break;

         case LoginResponseError.NoNewConnections:
         case LoginResponseError.FullArena:
            // Wait one second, then try to reconnect
            Thread.Sleep (1000);
            SendLoginRequest (false);
            break;

         default:
            Close ();
            break;
         }
      }

      /// <summary>
      /// Handle the level information packet
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleLevelInfo (Byte[] pBuffer)
      {
         LevelInfoPacket packet = new LevelInfoPacket ();

         // Set the player weapon packet
         packet.Packet = pBuffer;

         // Load the file and initialize the security checksum
         string assemblyLoc = Assembly.GetExecutingAssembly ().Location;
         string currentDirectory = assemblyLoc.Substring (0, assemblyLoc.LastIndexOf (Path.DirectorySeparatorChar) + 1);

         try
         {
            string levelDir = Path.Combine(currentDirectory, "maps" + Path.DirectorySeparatorChar);
            
            // Open the level file into a stream
            FileStream levelFile = new FileStream (levelDir + packet.FileName, FileMode.Open);

            Byte[] fileData = new Byte[levelFile.Length];
            levelFile.Read (fileData, 0, (int)levelFile.Length);

            UInt32 diff = 0;

            // Skip the tileset (if present)
            if ((fileData[0] == 'B') &&
                (fileData[1] == 'M'))
            {
               diff = BitConverter.ToUInt32 (fileData, 2);	
            }

            Byte[] mapData = new Byte[0x100000];

            Byte[] psyMapData = new Byte[levelFile.Length - diff];
            for (Int32 i = (Int32)diff; i < fileData.Length; i += 1)
                psyMapData[i - diff] = fileData[i];

            // Fill map-tiles
            for (Int32 i = (Int32)diff; i < fileData.Length; i += 4)
            {
               UInt32 tileData = BitConverter.ToUInt32 (fileData, i);

	            UInt16 x = (UInt16)(tileData & 0xFFF);
	            UInt16 y = (UInt16)((tileData >> 12) & 0xFFF);
	            Byte type = (Byte)(tileData >> 24);

               if (x < 0 || x >= 0x400) continue;
               if (y < 0 || y >= 0x400) continue;

               mapData[(y << 10) | x] = type;
            }

            // Set the level file data
            m_securityChecksum.LevelData = mapData;
            m_securityChecksum.PsyLevelData = psyMapData;
            m_securityChecksum.MapFile = packet.FileName;
         }
         catch (Exception e)
         {
            // Write the exception message to the console
            Console.WriteLine (e.Message);
         }
      }

      /// <summary>
      /// Handle the player weapon fired packet
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandlePlayerWeapon (Byte[] pBuffer)
      {
         PlayerWeaponPacket packet = new PlayerWeaponPacket ();

         // Set the player weapon packet
         packet.Packet = pBuffer;

         // Check if this packet contains a weapon
         if (packet.Event.Weapon.Type != WeaponTypes.NoWeapon)
         {
            // Increment the weapon count
            m_sessionStatistics.IncrementWeaponCount ();
         }
      }

      /// <summary>
      /// Handle the security checksum packet update
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleSecurityRequest (Byte[] pBuffer)
      {
         SecurityRequestPacket securityRequest = new SecurityRequestPacket ();

         // Parse the security request packet
         securityRequest.Packet = pBuffer;

         // Only send the security checksum if the map is loaded
         if ((securityRequest.ChecksumKey != 0) && (m_securityChecksum.LevelData != null))
         {
            // Calculate the security checksums
            UInt32 executableChecksum = m_securityChecksum.generateEXEChecksum (securityRequest.ChecksumKey);
            UInt32 levelChecksum = m_securityChecksum.generateLevelChecksum (securityRequest.ChecksumKey);
            UInt32 parameterChecksum = m_securityChecksum.generateParameterChecksum (securityRequest.ChecksumKey);

            // Create the security response packet
            SecurityResponsePacket securityResponse = new SecurityResponsePacket ();

            securityResponse.CurrentPing = m_sessionStatistics.CurrentPing;
            securityResponse.AveragePing = m_sessionStatistics.AveragePing;
            securityResponse.HighPing = m_sessionStatistics.HighPing;
            securityResponse.LowPing = m_sessionStatistics.LowPing;
            securityResponse.ReliableCount = m_sessionStatistics.ReliableReceiveCount;
            securityResponse.WeaponCount = m_sessionStatistics.WeaponCount;
            securityResponse.S2CSlowCurrent = m_sessionStatistics.S2CSlowCurrent;
            securityResponse.S2CSlowTotal = m_sessionStatistics.S2CSlowTotal;
            securityResponse.S2CFastCurrent = m_sessionStatistics.S2CFastCurrent;
            securityResponse.S2CFastTotal = m_sessionStatistics.S2CFastTotal;
            securityResponse.SlowFrame = 0x00;
            securityResponse.ExecutableChecksum = executableChecksum;
            securityResponse.LevelChecksum = levelChecksum;
            securityResponse.ParameterChecksum = parameterChecksum;

            // Transmit the security response packet
            TransmitPacket (securityResponse);
         }
      }

      /// <summary>
      /// Handle the file transfer packet
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleFileTransfer (Byte[] pBuffer)
      {

      }

      /// <summary>
      /// andle the Login next packet
      /// </summary>
      /// <param name="pBuffer">Packet Data buffer</param>
      private void HandleLoginNext (Byte[] pBuffer)
      {

      }
   }
}