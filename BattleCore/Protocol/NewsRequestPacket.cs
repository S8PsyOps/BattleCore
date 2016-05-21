//-----------------------------------------------------------------------
//
// NAME:        NewsRequestPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: News Request Packet implementation.
//
// NOTES:       None.
//
// $History: NewsRequestPacket.cs $
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
   /// NewsRequestPacket object.  This object is used to request the
   /// news file from the server.
   /// </summary>
   internal class NewsRequestPacket : IPacket
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
            // Create the news request packet
            Byte[] packet = new Byte[1];

            // Set the news request packet data
            packet[0] = 0x0D;

            // Return the packet data
            return packet;
         }
      }
   }
}
