//-----------------------------------------------------------------------
//
// NAME:        ChatPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Chat Packet implementation.
//
// NOTES:       None.
//
// $History: ChatPacket.cs $
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
   /// ChatPacket object.  This object is used to receive chat packets
   /// from the server, and send chat packets to the server.
   /// </summary>
   internal class ChatPacket : IPacket
   {
      /// <summary>Chat Packet Event Data</summary>
      private ChatEvent m_event = new ChatEvent();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// Chat Event Property
      /// </summary>
      public ChatEvent Event
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
            // Set the chat event data from the packet
            m_event.ChatType = (ChatTypes)value[1];
            m_event.SoundCode = (SoundCodes)value[2];
            m_event.PlayerId = BitConverter.ToUInt16 (value, 3);
            m_event.Message = (new ASCIIEncoding ()).GetString (value, 5, value.Length - 6);
         }
         get 
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (m_event.Message.Length + 6);

            // Write the packet data to the memory stream
            packet.WriteByte (0x06);
            packet.WriteByte ((Byte)m_event.ChatType);
            packet.WriteByte ((Byte)m_event.SoundCode);
            packet.Write (BitConverter.GetBytes (m_event.PlayerId), 0, 2);
            packet.Write ((new ASCIIEncoding ()).GetBytes (m_event.Message), 0, m_event.Message.Length);
            packet.WriteByte (0x00);

            // Convert the packet to a byte array
            return packet.ToArray ();
         } 
      }
   }
}
