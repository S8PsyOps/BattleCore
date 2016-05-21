//-----------------------------------------------------------------------
//
// NAME:        SyncResponsePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Sync Response Packet implementation.
//
// NOTES:       None.
//
// $History: SyncResponsePacket.cs $
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
   /// SyncResponsePacket Object.  This packet is received when the 
   /// server requests a time sync.
   /// </summary>
   internal class SyncResponsePacket : IPacket
   {
      private UInt32 m_nClientTime; // Time extracted from the sync request 
      private UInt32 m_nServerTime;
      private UInt16 m_nPingTime;

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Client Time Property
      /// </summary>
      public UInt32 ClientTime
      {
         set { m_nClientTime = value; }
         get { return m_nClientTime; }
      }

      /// <summary>
      /// Server Time Property
      /// </summary>
      public UInt32 ServerTime
      {
         set { m_nServerTime = value; }
         get { return m_nServerTime; }
      }

      /// <summary>
      /// Ping Time Property
      /// </summary>
      public UInt16 PingTime
      {
         set { m_nPingTime = value; }
         get { return m_nPingTime; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Extract the data from the sync message 
            m_nClientTime = BitConverter.ToUInt32 (value, 2);
            m_nServerTime = BitConverter.ToUInt32 (value, 6);
            m_nPingTime   = (UInt16)((UInt32)(Environment.TickCount / 10) - m_nClientTime);
         }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (10);

            // Get the client timestamp value
            m_nClientTime = (UInt32)(Environment.TickCount / 10);

            // Write the packet data to the memory stream
            packet.WriteByte (0x00);
            packet.WriteByte (0x06);
            packet.Write (BitConverter.GetBytes (m_nServerTime), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nClientTime), 0, 4);

            // Convert the packet to a byte array
            return packet.ToArray ();
         }
      }
   }
}
