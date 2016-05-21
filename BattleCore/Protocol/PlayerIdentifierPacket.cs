//-----------------------------------------------------------------------
//
// NAME:        PlayerIdentifierPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Identifier Packet implementation.
//
// NOTES:       None.
//
// $History: PlayerIdentifierPacket.cs $
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
   /// PlayerIdentifierPacket Object.  This packet is received when the 
   /// bot identifier is set.
   /// </summary>
   internal class PlayerIdentifierPacket : IPacket
   {
      /// <summary>Player Identifier</summary>
      private UInt16 m_nIdentifier;

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Player Identifier Property
      /// </summary>
      public UInt16 Identifier
      {
         get { return m_nIdentifier; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Set the event data from the packet
            m_nIdentifier = BitConverter.ToUInt16 (value, 1);
         }

         // Return an empty array
         get { return new Byte[1]; }
      }
   }
}
