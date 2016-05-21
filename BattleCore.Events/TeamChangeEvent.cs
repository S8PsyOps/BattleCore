/***********************************************************************
*
* NAME:        TeamChangeEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Team Change Event implementation.
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
   /// TeamChangeEvent object.  This event is triggered when a player
   /// changes teams.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle team change events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnTeamChangeEvent (object sender, TeamChangeEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnTeamChangeEvent (Object sender, TeamChangeEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class TeamChangeEvent : EventArgs
   {
      private String    m_strPlayerName; // Player Name
      private UInt16    m_nPlayerId;     // Player identifier
      private ModLevels m_modLevel;      // Moderator Level
      private UInt16    m_nFrequency;    // Frequency identifier

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

      ///<summary>Player Frequency Property</summary>
      public UInt16 Frequency
      {
         set { m_nFrequency = value; }
         get { return m_nFrequency; }
      }
   }
}
