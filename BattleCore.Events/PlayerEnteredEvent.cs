/***********************************************************************
*
* NAME:        PlayerEnteredEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Player Entered Event implementation.
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
   /// PlayerEnteredEvent object.  This event is triggered when a player 
   /// enters the arena. 
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle player enter events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnPlayerEnteredEvent (object sender, PlayerEnteredEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnPlayerEnteredEvent (Object sender, PlayerEnteredEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class PlayerEnteredEvent : EventArgs
   {
      private String m_strPlayerName;    // Player name
      private String m_strSquadName;     // Squad name
      private ShipTypes m_shipType;         // Ship type    
      private UInt16 m_nPlayerId;        // Player identifier
      private Boolean m_bAcceptsAudio;    // Accepts Audio messages
      private UInt32 m_nFlagPoints;      // Flag Points
      private UInt32 m_nKillPoints;      // Kill Points
      private UInt16 m_nFrequency;       // Player Frequency
      private UInt16 m_nWins;            // Number of wins
      private UInt16 m_nLosses;          // Number of losses
      private UInt16 m_nTurretPlayerId;  // Id of the turret player
      private UInt16 m_nFlagsCarried;    // Number of flags carried
      private Boolean m_bHasKOTH;         // King of the hill state

      ///<summary>Player name Property</summary>
      public String PlayerName
      {
         set 
         { 
            m_strPlayerName = value;

            int nIndex = m_strPlayerName.IndexOf ("\0");
            if (nIndex > 0)
            {
               m_strPlayerName = m_strPlayerName.Remove (nIndex);
            }
         }
         get { return m_strPlayerName; }
      }

      ///<summary>Player name Property</summary>
      public String SquadName
      {
         set 
         {  
            m_strSquadName = value; 

            int nIndex = m_strSquadName.IndexOf ("\0");
            if (nIndex > 0)
            {
               m_strSquadName = m_strSquadName.Remove (nIndex);
            }
         }
         get { return m_strSquadName; }
      }

      ///<summary>Player Ship Type Property</summary>
      public ShipTypes ShipType
      {
         set { m_shipType = value; }
         get { return m_shipType; }
      }

      ///<summary>Player Identifier Property</summary>
      public UInt16 PlayerId
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }

      ///<summary>Player Accepts Audio Property</summary>
      public Boolean AcceptsAudio
      {
         set { m_bAcceptsAudio = value; }
         get { return m_bAcceptsAudio; }
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

      ///<summary>Player Frequency Property</summary>
      public UInt16 Frequency
      {
         set { m_nFrequency = value; }
         get { return m_nFrequency; }
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

      ///<summary>Turret Player Identifier Property</summary>
      public UInt16 TurretPlayerId
      {
         set { m_nTurretPlayerId = value; }
         get { return m_nTurretPlayerId; }
      }

      ///<summary>Player Flags Carried Property</summary>
      public UInt16 FlagsCarried
      {
         set { m_nFlagsCarried = value; }
         get { return m_nFlagsCarried; }
      }

      ///<summary>Player has King of the Hill Property</summary>
      public Boolean HasKOTH
      {
         set { m_bHasKOTH = value; }
         get { return m_bHasKOTH; }
      }
   }
}
