//-----------------------------------------------------------------------
//
// NAME:        PlayerLeftPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player left Packet implementation.
//
// NOTES:       None.
//
// $History: PlayerLeftPacket.cs $
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
   /// PlayerLeftPacket object.  This object is used to receive player
   /// leaving packets from the server.
   /// </summary>
   internal class PlayerLeftPacket : IPacket
   {
      /// <summary>Player Left Packet Event Data</summary>
      private PlayerLeftEvent m_event = new PlayerLeftEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Player Left Event Property
      /// </summary>
      public PlayerLeftEvent Event
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
            // Set the event data from the packet
            m_event.PlayerId = BitConverter.ToUInt16 (value, 1);
         }
         get
         {
            // return a new Byte
            return new Byte[1];
         }
      }
   }
}
