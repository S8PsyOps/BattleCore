//-----------------------------------------------------------------------
//
// NAME:        PlayerPositionPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Position Packet implementation.
//
// NOTES:       None.
//
// $History: FlagClaimPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BattleCore.Events;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// FlagClaimPacket object.  This object is used to receive player
   /// flag claim packets from the server.
   /// </summary>
   internal class PlayerPositionPacket : IPacket
   {
      /// <summary>Player Position Packet Event Data</summary>
      private PlayerPositionEvent m_event = new PlayerPositionEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Player Position Event Property
      /// </summary>
      public PlayerPositionEvent Event
      {
         set { m_event = value; }
         get { return m_event; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Set the event data from the packet
            m_event.ShipRotation = value[1];
            m_event.MapPositionX = BitConverter.ToUInt16 (value, 4);
            m_event.Ping = value[6];
            m_event.Bounty = value[7];
            m_event.PlayerId = (UInt16)value[8];
            m_event.ShipState.Value = value[9];
            m_event.VelocityY = BitConverter.ToUInt16 (value, 10);
            m_event.MapPositionY = BitConverter.ToUInt16 (value, 12);
            m_event.VelocityX = BitConverter.ToUInt16 (value, 14);

            if (value.Length >= 18)
            {
               m_event.Energy = BitConverter.ToUInt16 (value, 16);
            }

            if (value.Length == 26)
            {
               m_event.ServerToClientLag = BitConverter.ToUInt16 (value, 18);
               m_event.Energy = BitConverter.ToUInt16 (value, 20);
               m_event.Items.Value = BitConverter.ToUInt32 (value, 22);
            }
         }

         // Return an empty array
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (32);

            // Write the packet data to the memory stream
            packet.WriteByte (0x03);
            packet.WriteByte (m_event.ShipRotation);
            packet.Write (BitConverter.GetBytes ((UInt32)(Environment.TickCount/10)), 0, 4);
            packet.Write (BitConverter.GetBytes (m_event.VelocityX), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.MapPositionY), 0, 2);
            packet.WriteByte (0x00);
            packet.WriteByte (m_event.ShipState.Value);
            packet.Write (BitConverter.GetBytes (m_event.MapPositionX), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.VelocityY), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.Bounty), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.Energy), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.Weapon.Value), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.Energy), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.ServerToClientLag), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.Timer), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.Items.Value), 0, 4);

            // Get the packet data buffer
            Byte[] positionPacket = packet.GetBuffer ();

            // Calculate the simple checksum
 	         Byte crc = 0;
	         for (UInt32 i = 0; i < 22; ++i)
	         {
		         crc ^= positionPacket[i];
	         }
            positionPacket[10] = crc;

            return positionPacket;
         }
      }
   }
}
