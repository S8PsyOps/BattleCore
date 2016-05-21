//-----------------------------------------------------------------------
//
// NAME:        SpectatePlayerEvent.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Spectate Player Event implementation.
//
// NOTES:       None.
//
// $History: SpectatePlayerEvent.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using BattleCore;

// namespace declaration
namespace BattleCore.Events
{
   /// <summary>
   /// SpectatePlayerEvent object.  This event is used to spectate  
   /// a player in the arena.
   /// </summary>
   /// <remarks>
   /// <para>Usage: Create and send this event to spectate a player.</para>
   /// <code lang="C#" escaped="true">
   /// public void SpectatePlayer (UInt16 PlayerId) 
   /// { 
   ///    // Create the spectate player event
   ///    SpectatePlayerEvent e = new SpectatePlayerEvent ();
   ///    e.PlayerId = PlayerId;
   /// 
   ///    // Send the event to the game server
   ///    SendGameEvent (e);
   /// }
   /// </code>
   /// </remarks>
   public class SpectatePlayerEvent : EventArgs
   {
      // Private member data
      private UInt16 m_nPlayerId = 0xFFFF;
      private string m_strPlayerName = "";

      /// <summary>
      /// Player Identifier Property
      /// </summary>
      public UInt16 PlayerId
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }

      /// <summary>
      /// Player Identifier Property
      /// </summary>
      public string PlayerName
      {
         set { m_strPlayerName = value; }
         get { return m_strPlayerName; }
      }
   }
}
