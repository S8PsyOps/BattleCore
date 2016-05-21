/***********************************************************************
*
* NAME:        PrizeCollectedEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Player Prize Event implementation.
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
using BattleCore;

// Namespace 
namespace BattleCore.Events
{
   /// <summary>
   /// PlayerPrizeEvent object.  This event is triggered when a player
   /// collects a prize.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle prize collected events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnPrizeCollectedEvent (object sender, PrizeCollectedEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnPrizeCollectedEvent (Object sender, PrizeCollectedEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class PrizeCollectedEvent : EventArgs
   {
      private String m_strPlayerName;  // Player Name
      private UInt32 m_nTimeStamp;     // Time Stamp
      private UInt16 m_nPositionX;     // X map position
      private UInt16 m_nPositionY;     // Y map position
      private UInt16 m_nPlayerId;      // Player identifier
      private PrizeTypes m_prizeType;      // Prize number
      private ModLevels m_modLevel;       // Moderator Level

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

      ///<summary>Player Timestamp Property</summary>
      public UInt32 TimeStamp
      {
         set { m_nTimeStamp = value; }
         get { return m_nTimeStamp; }
      }

      ///<summary>Map Postion X Property</summary>
      public UInt16 MapPositionX
      {
         set { m_nPositionX = value; }
         get { return m_nPositionX; }
      }

      ///<summary>Map Postion Y Property</summary>
      public UInt16 MapPositionY
      {
         set { m_nPositionY = value; }
         get { return m_nPositionY; }
      }

      ///<summary>Prize Type Property</summary>
      public PrizeTypes PrizeType
      {
         set { m_prizeType = value; }
         get { return m_prizeType; }
      }
   }
}
