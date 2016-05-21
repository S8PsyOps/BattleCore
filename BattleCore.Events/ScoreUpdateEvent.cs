/***********************************************************************
*
* NAME:        ScoreUpdateEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Score Update Event implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE      AUTHOR     CHANGES
* --------  ---------  -------------------------------------------------
* 12-29-06  udp        Initial Creation
*
************************************************************************/

// Namespace usage
using System;

// Namespace 
namespace BattleCore.Events
{
   /// <summary>
   /// ScoreUpdateEvent object.  This event is triggered when a player's
   /// score is updated.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle score update events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnScoreUpdateEvent (object sender, ScoreUpdateEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnScoreUpdateEvent (Object sender, ScoreUpdateEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class ScoreUpdateEvent : EventArgs
   {
      private String m_strPlayerName; // Player Name
      private UInt16 m_nPlayerId;     // Player identifier
      private ModLevels m_modLevel;      // Moderator Level
      private UInt32 m_nFlagPoints;   // Flag Points
      private UInt32 m_nKillPoints;   // Kill Points
      private UInt16 m_nWins;         // Number of wins
      private UInt16 m_nLosses;       // Number of losses

      ///<summary>Player name Property</summary>
      public String PlayerName
      {
         set { m_strPlayerName = value; }
         get { return m_strPlayerName; }
      }

      ///<summary>Player Identifier Property</summary>
      public UInt16 PlayerId
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }

      ///<summary>Player Moderator Level Property</summary>
      public ModLevels ModLevel
      {
         set { m_modLevel = value; }
         get { return m_modLevel; }
      }

      ///<summary>Player Kill Points Property</summary>
      public UInt32 KillPoints
      {
         set { m_nKillPoints = value; }
         get { return m_nKillPoints; }
      }

      ///<summary>Player Flag Points Property</summary>
      public UInt32 FlagPoints
      {
         set { m_nFlagPoints = value; }
         get { return m_nFlagPoints; }
      }

      ///<summary>Player Wins Property</summary>
      public UInt16 Wins
      {
         set { m_nWins = value; }
         get { return m_nWins; }
      }

      ///<summary>Player Losses Property</summary>
      public UInt16 Losses
      {
         set { m_nLosses = value; }
         get { return m_nLosses; }
      }
   }
}
