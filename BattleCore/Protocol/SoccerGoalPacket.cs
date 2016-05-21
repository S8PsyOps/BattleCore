//-----------------------------------------------------------------------
//
// NAME:        SoccerGoalPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Soccer Goal Packet implementation.
//
// NOTES:       None.
//
// $History: SoccerGoalPacket.cs $
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
   /// SoccerGoalPacket object.  This object is received when a player
   /// scores a soccer goal.
   /// </summary>
   internal class SoccerGoalPacket : IPacket
   {
      /// <summary>Soccer Goal Packet Event Data</summary>
      private SoccerGoalEvent m_event = new SoccerGoalEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Flag Claim Event Property
      /// </summary>
      public SoccerGoalEvent Event
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
            m_event.Frequency = BitConverter.ToUInt16 (value, 1);
            m_event.Points = BitConverter.ToUInt16 (value, 3);
         }

         // Return an empty array
         get { return null; }
      }
   }
}
