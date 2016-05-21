//-----------------------------------------------------------------------
//
// NAME:        PrizeCollectedPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Prize Collected Packet implementation.
//
// NOTES:       None.
//
// $History: PrizeCollectedPacket.cs $
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
   /// PrizeCollectedPacket Object.  This packet is received when a player
   /// collects a prize.
   /// </summary>
   internal class PrizeCollectedPacket : IPacket
   {
      /// <summary>Player Enter Packet Event Data</summary>
      private PrizeCollectedEvent m_event = new PrizeCollectedEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Player Entered Event Property
      /// </summary>
      public PrizeCollectedEvent Event
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
            m_event.TimeStamp = BitConverter.ToUInt32 (value, 1);
            m_event.MapPositionX = BitConverter.ToUInt16 (value, 5);
            m_event.MapPositionY = BitConverter.ToUInt16 (value, 7);
            m_event.PrizeType = (PrizeTypes)BitConverter.ToUInt16 (value, 9);
            m_event.PlayerId = BitConverter.ToUInt16 (value, 11);
         }
         get
         {
            // return a new Byte
            return new Byte[1];
         }
      }
   }
}
