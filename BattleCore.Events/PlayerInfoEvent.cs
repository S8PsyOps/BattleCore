//-----------------------------------------------------------------------
//
// NAME:        PlayerInfoEvent.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Info Event implementation.
//
// NOTES:       None.
//
// $History: PlayerInfoEvent.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using BattleCore;

namespace BattleCore.Events
{
   /// <summary>
   /// PlayerInfoEvent object.  This event is used to retrieve  
   /// player information from the core.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle player info.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnPlayerInfo (object sender, PlayerInfoEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <para>
   /// To request all player information from the core, create 
   /// the PlayerInfoEvent object and send it to the core.</para>
   /// <code lang="C#" escaped="true">
   /// PlayerInfoEvent e = new PlayerInfoEvent ();
   /// SendGameEvent (e);
   /// </code>
   /// <para>
   /// To request information about specific players, add the
   /// player names to the Players field before sending the event</para>
   /// <code lang="C#" escaped="true">
   /// PlayerInfoEvent e = new PlayerInfoEvent ();
   /// e.Players.Add ("udp");
   /// e.Players.Add ("BattlePub.Bot");
   /// SendGameEvent (e);
   /// </code>
   /// </remarks>
   public class PlayerInfoEvent : EventArgs
   {
      // Player name list
      List<string> m_strPlayers = new List<string>();

      // List containing player information
      List<PlayerInfo> m_playerInfo = new List<PlayerInfo>();

      /// <summary>
      /// Player names property used to request information about
      /// specific players.  
      /// </summary>
      public List<string> Players 
      {
         set { m_strPlayers = value; }
         get { return m_strPlayers; }
      }

      /// <summary>
      /// Player List Property containing all known information 
      /// about requested players.
      /// </summary>
      public List<PlayerInfo> PlayerList
      {
         set { m_playerInfo = value; }
         get { return m_playerInfo; }
      }
   }
}
