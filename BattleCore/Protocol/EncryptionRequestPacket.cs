//-----------------------------------------------------------------------
//
// NAME:        EncryptionRequestPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Encryption Request Packet implementation.
//
// NOTES:       None.
//
// $History: EncryptionRequestPacket.cs $
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
   /// EncryptionRequestPacket object.
   /// </summary>
   internal class EncryptionRequestPacket : IPacket
   {
      /// <summary>
      /// Encryption Key Value
      /// </summary>
      private UInt32 m_nEncryptionKey;

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Encryption Key Property
      /// </summary>
      public UInt32 EncryptionKey { set { m_nEncryptionKey = value; } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }

         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (8);

            // Set the packet data
            packet.WriteByte (0x00);
            packet.WriteByte (0x01);
            packet.Write (BitConverter.GetBytes (m_nEncryptionKey), 0, 4);
            packet.WriteByte (0x01);
            packet.WriteByte (0x00);

            // return the packet data buffer
            return packet.GetBuffer ();
         }
      }
   }
}
