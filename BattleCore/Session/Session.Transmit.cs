//-----------------------------------------------------------------------
//
// NAME:        Session.Transmit.cs
//
// PROJECT:     Battle Core Session Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Session implementation.
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
using System.Collections;
using System.Threading;
using BattleCore.Protocol;

// Namespace declaration
namespace BattleCore.Session
{
   /// <summary>
   /// Session Object.  This object handles a session with the server
   /// </summary>
   partial class Session
   {
      /// <summary>
      /// Transmit a message packet to the server
      /// </summary>
      /// <param name="packet">packet object</param>
      public void TransmitPacket (IPacket packet)
      {
         // If the packet is reliable
         if (packet.Reliable)
         {
            // Transmit the reliable packet
            m_sessionSocket.TransmitReliablePacket (packet.Packet);
         }
         else
         {
            // Transmit the packet
            m_sessionSocket.TransmitPacket (packet.Packet);
         }
      }

      /// <summary>
      /// Compose and send an encryption request packet to the server
      /// </summary>  
      private void SendEncryptionRequest ()
      {
         // Create the encryption request packet
         EncryptionRequestPacket packet = new EncryptionRequestPacket ();

         // Add the game packet handlers
         AddGamePacketHandler (0x00, new SessionPacketHandler (SpecialPacketReceiveHandler));

         // Add the special packet handlers
         AddSpecialPacketHandler (0x02, new SessionPacketHandler (HandleEncryptionResponse));
         AddSpecialPacketHandler (0x05, new SessionPacketHandler (HandleSynchronizeRequest));
         AddSpecialPacketHandler (0x07, new SessionPacketHandler (HandleDisconnect));

         // Log the connection message
         m_sessionLogger (String.Format ("Connecting to {0}:{1}",
                                         m_sessionSettings.ServerAddress,
                                         m_sessionSettings.ServerPort));

         // Set the client encryption key value
         packet.EncryptionKey = m_sessionSocket.ClientEncryptKey;

         // Send the packet to the server
         TransmitPacket (packet);
      }

      /// <summary>
      /// Compose and send a login request packet to the server
      /// </summary>
      /// <param name="bNewUser">new user flag</param>
      private void SendLoginRequest (Boolean bNewUser)
      {
         // Create the password packet
         PasswordPacket packet = new PasswordPacket ();

         // Set the client password values
         packet.NewUser = bNewUser;
         packet.UserName = m_sessionSettings.UserName;
         packet.Password = m_sessionSettings.Password;
         packet.Continuum = m_sessionSettings.ForceContinuum;

         // Log the connection request
         m_sessionLogger (String.Format ("Sending Password for {0}",
                                          m_sessionSettings.UserName));

         // Send the packet to the server
         TransmitPacket (packet);
      }

      /// <summary>
      /// Compose and send an arena enter request to the server
      /// </summary>
      private void SendArenaEnterRequest ()
      {
         // Create a new arena enter packet
         ArenaEnterPacket packet = new ArenaEnterPacket ();

         // Set the initial arena name
         packet.ArenaName = m_sessionSettings.InitialArena;

         // Log the arena entered event to the session logger
         m_sessionLogger (String.Format ("Enterning Arena: {0}",
                                          m_sessionSettings.InitialArena));

         // Send the packet to the server
         TransmitPacket (packet);
      }

      /// <summary>
      /// Compose and send an arena spectate request to the server
      /// </summary>
      private void SendArenaSpectateRequest ()
      {
         // Send the packet to the server
         TransmitPacket (new SpectatePlayerPacket ());
      }

      /// <summary>
      /// Compose and send a registration packet to the server
      /// </summary>
      private void SendPlayerRegistration ()
      {
         // Create a new disconnect message packet
         RegistrationPacket packet = new RegistrationPacket ();

         // Set the registration values
         packet.RealName = m_sessionSettings.RealName;
         packet.EmailAddress = m_sessionSettings.EmailAddress;
         packet.City = m_sessionSettings.City;
         packet.State = m_sessionSettings.State;
         packet.RegisteredName = m_sessionSettings.RegisteredName;
         packet.Organization = m_sessionSettings.Organization;

         // Log the message to the session message log
         m_sessionLogger ("Sending new player registration");

         // Send the packet to the server
         TransmitPacket (packet);
      }

      /// <summary>
      /// Compose and send a disconnect request to the server
      /// </summary>
      private void SendDisconnectRequest ()
      {
         // Log the message to the session message log
         m_sessionLogger ("Disconnecting from server");

         // Send the packets to the server
         TransmitPacket (new ArenaLeavePacket ());
         TransmitPacket (new DisconnectPacket ());
         TransmitPacket (new DisconnectPacket ());
      }

      /// <summary>
      /// Compose and send a synchronize packet to the server
      /// </summary>
      private void SendSynchronizeRequest ()
      {
         // Create the sync request packet
         SyncRequestPacket packet = new SyncRequestPacket ();

         // Set the sync packet data values
         packet.TimeStamp = (UInt32)(Environment.TickCount / 10);
         packet.PacketsSent = m_sessionSocket.PacketsSent;
         packet.PacketsReceived = m_sessionSocket.PacketsReceived;

         // Set the server sync timeout to request again
         m_serverSyncTime = TimeSpan.FromMilliseconds (Environment.TickCount)
                          + TimeSpan.FromSeconds (m_sessionSettings.ServerSyncTime);

         // Send the sync packet
         TransmitPacket (packet);
      }
   }
}