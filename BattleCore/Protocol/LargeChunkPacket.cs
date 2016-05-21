//-----------------------------------------------------------------------
//
// NAME:        LargeChunkPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Large Chunk Packet implementation.
//
// NOTES:       None.
//
// $History: LargeChunkPacket.cs $
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
   /// LargeChunkPacket object.  This object is used to send a chunk of
   /// a message.
   /// </summary>
   internal class LargeChunkPacket : IPacket
   {
      /// <summary>
      /// Memory stream containing the packet data
      /// </summary>
      private MemoryStream m_Message = new MemoryStream ();
      private UInt32 m_TotalLength;

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Message Property
      /// </summary>
      public Byte[] Message
      {
         set { m_Message.Write (value, 0, value.Length); }
         get { return m_Message.ToArray (); }
      }

      /// <summary>
      /// Total Length Property
      /// </summary>
      public UInt32 TotalLength
      {
         set { m_TotalLength = value; }
         get { return m_TotalLength; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Extract the packet data
            m_TotalLength = BitConverter.ToUInt32 (value, 2);
            m_Message.Write (value, 6, value.Length - 6);
         }

         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream ();

            // Write the packet data
            packet.WriteByte (0x00);
            packet.WriteByte (0x0A);
            packet.Write (BitConverter.GetBytes (m_TotalLength), 0, 4);
            packet.Write (m_Message.GetBuffer (), 0, (int)m_Message.Length);

            // Return the packet data
            return packet.ToArray ();
         }
      }
   }
}
