//-----------------------------------------------------------------------
//
// NAME:        SpectatePlayerPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Spectate Player Packet implementation.
//
// NOTES:       None.
//
// $History: SpectatePlayerPacket.cs $
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
   /// SpectatePlayerPacket Object.  This packet is sent to 
   /// spectate a player in the arena.
   /// </summary>
   internal class SpectatePlayerPacket : IPacket
   {
      /// <summary>Spectate Player Event Data</summary>
      private SpectatePlayerEvent m_event = new SpectatePlayerEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Spectate Player Event Property
      /// </summary>
      public SpectatePlayerEvent Event
      {
         set { m_event = value; }
         get { return m_event; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (3);

            // Write the packet data to the memory stream
            packet.WriteByte (0x08);
            packet.Write (BitConverter.GetBytes (m_event.PlayerId), 0, 2);

            // return the packet
            return packet.GetBuffer ();
         }
      }
   }
}
