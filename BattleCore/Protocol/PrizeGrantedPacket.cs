//-----------------------------------------------------------------------
//
// NAME:        PrizeGrantedPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Prize Collected Packet implementation.
//
// NOTES:       None.
//
// $History: PrizeGrantedPacket.cs $
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
   /// PrizeGrantedPacket Object.  This packet is received when the 
   /// bot collects a prize.
   /// </summary>
   internal class PrizeGrantedPacket : IPacket
   {
      /// <summary>Flag Claim Packet Event Data</summary>
      private PrizeGrantedEvent m_event = new PrizeGrantedEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Prize Granted Event Property
      /// </summary>
      public PrizeGrantedEvent Event
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
            m_event.Count = BitConverter.ToUInt16 (value, 1);
            m_event.PrizeType = (PrizeTypes)BitConverter.ToUInt16 (value, 3);
         }

         // Return an empty array
         get { return new Byte[1]; }
      }
   }
}
