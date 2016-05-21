//-----------------------------------------------------------------------
//
// NAME:        EncryptionResponsePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Encryption Response Packet implementation.
//
// NOTES:       None.
//
// $History: EncryptionResponsePacket.cs $
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
   /// EncryptionResponsePacket object.
   /// </summary>
   internal class EncryptionResponsePacket : IPacket
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
      UInt32 EncryptionKey { get { return m_nEncryptionKey; } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set 
         {
            // Extract the encryption key
            m_nEncryptionKey = BitConverter.ToUInt32 (value, 2);
         }

         get
         {
            // return the packet data buffer
            return new Byte[1];
         }
      }

   }
}
