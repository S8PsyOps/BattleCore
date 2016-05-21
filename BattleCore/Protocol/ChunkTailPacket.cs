//-----------------------------------------------------------------------
//
// NAME:        ChunkTailPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Chunk Tail Packet implementation.
//
// NOTES:       None.
//
// $History: ChunkTailPacket.cs $
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
   /// ChunkTailPacket object.  This object is used to send the last 
   /// chunk of a message.
   /// </summary>
   internal class ChunkTailPacket : IPacket
   {
      /// <summary>
      /// Memory stream containing the packet data
      /// </summary>
      private MemoryStream m_Message = new MemoryStream ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// Message Property
      /// </summary>
      public Byte[] Message
      {
         set { m_Message.Write (value, 0, value.Length); }
         get { return m_Message.GetBuffer (); }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Extract the packet data
            m_Message.Write (value, 2, value.Length - 2);
         }

         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream ();

            // Write the packet data
            packet.WriteByte (0x00);
            packet.WriteByte (0x09);
            packet.Write (m_Message.GetBuffer (), 0, (int)m_Message.Length);

            // Return the packet data
            return packet.GetBuffer ();
         }
      }
   }
}
