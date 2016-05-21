//-----------------------------------------------------------------------
//
// NAME:        PlayerWeaponPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Weapon Packet implementation.
//
// NOTES:       None.
//
// $History: PlayerWeaponPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using BattleCore.Events;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// PlayerWeaponPacket object.  This object is used to receive player
   /// weapon packets from the server.
   /// </summary>
   internal class PlayerWeaponPacket : IPacket
   {
      /// <summary>Weapon Packet Event Data</summary>
      PlayerPositionEvent m_event = new PlayerPositionEvent ();

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
            m_event.VelocityY = BitConverter.ToUInt16 (value, 6);
            m_event.PlayerId = BitConverter.ToUInt16 (value, 8);
            m_event.VelocityX = BitConverter.ToUInt16 (value, 10);
            m_event.ShipState.Value = value[13];
            m_event.Ping = (UInt16)value[14];
            m_event.MapPositionY = BitConverter.ToUInt16 (value, 15);
            m_event.Bounty = BitConverter.ToUInt16 (value, 17);
            m_event.Weapon.Value = BitConverter.ToUInt16 (value, 19);

            // Check if energy was sent
            if (value.Length >= 23)
            {
               m_event.Energy = BitConverter.ToUInt16 (value, 21);
            }

            // Check if item information is sent
            if (value.Length == 31)
            {
               // Set the energy information
               m_event.ServerToClientLag = BitConverter.ToUInt16 (value, 23);
               m_event.Items.Value = BitConverter.ToUInt32 (value, 27);
            }
         }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (32);

            // Write the packet data to the memory stream
            packet.WriteByte (0x03);
            packet.WriteByte (m_event.ShipRotation);
            packet.Write (BitConverter.GetBytes ((UInt32)(Environment.TickCount / 10)), 0, 4);
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
