//-----------------------------------------------------------------------
//
// NAME:        DisconnectPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Disconnect Packet implementation.
//
// NOTES:       None.
//
// $History: DisconnectPacket.cs $
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
   /// DisconnectPacket object.  This object is used to disconnect
   /// from the server.
   /// </summary>
   internal class DisconnectPacket : IPacket
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
            // Create the disconnect packet
            Byte[] packet = new Byte[2];

            // Set the disconnect packet data
            packet[0] = 0x00;
            packet[1] = 0x07;

            // Return the packet data
            return packet;
         }
      }
   }
}
