//-----------------------------------------------------------------------
//
// NAME:        LevelRequestPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Level Request Packet implementation.
//
// NOTES:       None.
//
// $History: LevelRequestPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// LevelRequestPacket object.  This object is used to request the
   /// level file from the server.
   /// </summary>
   internal class LevelRequestPacket : IPacket
   {
      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }
         get
         {
            // Create the disconnect packet
            Byte[] packet = new Byte[1];

            // Set the level request packet data
            packet[0] = 0x0C;

            // Return the packet data
            return packet;
         }
      }
   }
}
