//-----------------------------------------------------------------------
//
// NAME:        ScoreResetPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Score Reset Packet implementation.
//
// NOTES:       None.
//
// $History: ScoreResetPacket.cs $
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
   /// ScoreResetPacket object.  This object is received when a player
   /// score is reset.
   /// </summary>
   internal class ScoreResetPacket : IPacket
   {
      /// <summary>Score Update Event Data</summary>
      private ScoreUpdateEvent m_event = new ScoreUpdateEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Score Update Event Property
      /// </summary>
      public ScoreUpdateEvent Event
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
            m_event.FlagPoints = 0;
            m_event.KillPoints = 0;
            m_event.Wins = 0;
            m_event.Losses = 0;
         }

         // Return an empty array
         get { return new Byte[1]; }
      }
   }
}
