//-----------------------------------------------------------------------
//
// NAME:        ArenaLeavePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Arena Leave Packet implementation.
//
// NOTES:       None.
//
// $History: ArenaLeavePacket.cs $
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
   /// ArenaLeavePacket object.  This object is used to leave an arena.
   /// </summary>
   internal class ArenaLeavePacket : IPacket
   {
      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }
         get
         {
            Byte[] packet = new Byte[1];

            // Create the packet memory stream
            packet[0] = 0x02;

            // Return the packet data
            return packet;
         }
      }
   }
}
