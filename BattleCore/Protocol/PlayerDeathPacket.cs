//-----------------------------------------------------------------------
//
// NAME:        PlayerDeathPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Death Packet implementation.
//
// NOTES:       None.
//
// $History: PlayerDeathPacket.cs $
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
   /// PlayerDeathPacket object.  This object is used to receive player
   /// death packets from the server.
   /// </summary>
   internal class PlayerDeathPacket : IPacket
   {
      /// <summary>Player Death Packet Event Data</summary>
      private PlayerDeathEvent m_event = new PlayerDeathEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Player Death Event Property
      /// </summary>
      public PlayerDeathEvent Event
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
            m_event.DeathGreen = value[1];
            m_event.KillerId = BitConverter.ToUInt16 (value, 2);
            m_event.KilledId = BitConverter.ToUInt16 (value, 4);
            m_event.Bounty = BitConverter.ToUInt16 (value, 6);
            m_event.FlagsCarried = BitConverter.ToUInt16 (value, 8);
         }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (5);

            // Write the packet data to the memory stream
            packet.WriteByte (0x05);
            packet.Write (BitConverter.GetBytes (m_event.KillerId), 0, 2);
            packet.Write (BitConverter.GetBytes (m_event.Bounty), 0, 2);

            // return the packet data
            return packet.GetBuffer (); ;
         }
      }
   }
}
