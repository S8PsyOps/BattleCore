/***********************************************************************
*
* NAME:        ShipChangeEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Ship Change Event implementation.
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
   /// ShipChangeEvent object.  This event is triggered when a player
   /// changes teams.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle ship change events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnShipChangeEvent (object sender, ShipChangeEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnShipChangeEvent (Object sender, ShipChangeEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class ShipChangeEvent : EventArgs
   {
      private String    m_strPlayerName; // Player Name
      private UInt16    m_nPlayerId;     // Player identifier
      private ModLevels m_modLevel;      // Moderator Level
      private ShipTypes m_shipType;      // Ship Type
      private ShipTypes m_lastShipType;  // Previous Ship Type

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

      ///<summary>Player Ship Type Property</summary>
      public ShipTypes ShipType
      {
         set { m_shipType = value; }
         get { return m_shipType; }
      }

      ///<summary>Previous Ship Type Property</summary>
      public ShipTypes PreviousShipType
      {
         set { m_lastShipType = value; }
         get { return m_lastShipType; }
      }
   }
}
