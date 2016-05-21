//-----------------------------------------------------------------------
//
// NAME:        SyncRequestPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Sync Request Packet implementation.
//
// NOTES:       None.
//
// $History: SyncRequestPacket.cs $
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
   /// SyncRequestPacket Object.  This packet is received when the 
   /// server requests a time sync.
   /// </summary>
   internal class SyncRequestPacket : IPacket
   {
      private UInt32 m_nTimeStamp;       // Current Tick count / 10
      private UInt32 m_nPacketsSent;     // Number of packets sent
      private UInt32 m_nPacketsReceived; // Number of packets received

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Time Stamp Property
      /// </summary>
      public UInt32 TimeStamp
      {
         set { m_nTimeStamp = value; }
         get { return m_nTimeStamp; }
      }

      /// <summary>
      /// Packets Sent Property
      /// </summary>
      public UInt32 PacketsSent
      {
         set { m_nPacketsSent = value; }
         get { return m_nPacketsSent; }
      }

      /// <summary>
      /// Packets Received Property
      /// </summary>
      public UInt32 PacketsReceived
      {
         set { m_nPacketsReceived = value; }
         get { return m_nPacketsReceived; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Set the data from the packet
            m_nTimeStamp = BitConverter.ToUInt32 (value, 2);

            if (value.Length > 6)
            {
               m_nPacketsSent = BitConverter.ToUInt32 (value, 6);
               m_nPacketsReceived = BitConverter.ToUInt32 (value, 10);
            }
         }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (14);

            // Write the packet data to the memory stream
            packet.WriteByte (0x00);
            packet.WriteByte (0x05);
            packet.Write (BitConverter.GetBytes (m_nTimeStamp), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nPacketsSent), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nPacketsReceived), 0, 4);

            // Convert the packet to a byte array
            return packet.ToArray ();
         }
      }
   }
}
