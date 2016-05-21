//-----------------------------------------------------------------------
//
// NAME:        ReliablePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Reliable Packet implementation.
//
// NOTES:       None.
//
// $History: ReliablePacket.cs $
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
   /// PlayerUpdatePacket Object.  This packet is received when the 
   /// player ship and frequency information is updated is set.
   /// </summary>
   internal class ReliablePacket : IPacket
   {
      private UInt32   m_nTransactionId; // 
      private Byte[]   m_pMessage;       // Message to transmit
      private TimeSpan m_nTimestamp;     // Message Timestamp

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Transaction identifier property
      /// </summary>
      public UInt32 TransactionId
      {
         set { m_nTransactionId = value; }
         get { return m_nTransactionId; }
      }

      /// <summary>
      /// Message Property
      /// </summary>
      public Byte[] Message
      {
         set { m_pMessage = value; }
         get { return m_pMessage; }
      }

      /// <summary>
      /// TimeStamp Property
      /// </summary>
      public TimeSpan TimeStamp
      {
         set { m_nTimestamp = value; }
         get { return m_nTimestamp; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Set the event data from the packet
            m_nTransactionId = BitConverter.ToUInt32 (value, 2);

            // Extract the encapsulated message
            m_pMessage = new Byte[value.Length - 6];
            Array.Copy (value, 6, m_pMessage, 0, value.Length - 6);
         }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (m_pMessage.Length + 6);

            // Write the packet data to the memory stream
            packet.WriteByte (0x00);
            packet.WriteByte (0x03);
            packet.Write (BitConverter.GetBytes (m_nTransactionId), 0, 4);
            packet.Write (m_pMessage, 0, m_pMessage.Length);

            // Convert the packet to a byte array
            return packet.ToArray ();
         }
      }
   }
}
