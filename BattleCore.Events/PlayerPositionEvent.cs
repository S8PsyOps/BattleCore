/***********************************************************************
*
* NAME:        PlayerPositionEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Player Position Event implementation.
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
   /// PlayerPositionEvent object.  This event is triggered when a player
   /// position is updated.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle player position events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnPlayerPositionEvent (object sender, PlayerLeftEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnPlayerPositionEvent (Object sender, PlayerLeftEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class PlayerPositionEvent : EventArgs
   {
      private String    m_strPlayerName;  // Player Name
      private UInt32    m_nTimeStamp;     // Time Stamp
      private UInt16    m_nPositionX;     // X map position
      private UInt16    m_nPositionY;     // Y map position
      private UInt16    m_nPlayerId;      // Player identifier
      private ModLevels m_modLevel;       // Moderator Level
      private Byte      m_nShipRotation;  // Player ship Rotation
      private UInt16    m_nBounty;        // Bounty
      private UInt16    m_nEnergy;        // Energy Level
      private UInt16    m_nPing;          // Player Ping Time
      private UInt16    m_nVelocityX;     // Player X Velocity
      private UInt16    m_nVelocityY;     // Player Y Velocity
      private UInt16    m_nS2CLag;        // Server to client lag
      private UInt16    m_nTimer;         // Timer Value
      private ItemInfo m_ItemInfo = new ItemInfo();
      private ShipStateInfo m_ShipStateInfo = new ShipStateInfo();
      private WeaponInfo m_WeaponInfo = new WeaponInfo();

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

      ///<summary>Player Ship Rotation Property</summary>
      public Byte ShipRotation
      {
         set { m_nShipRotation = value; }
         get { return m_nShipRotation; }
      }

      ///<summary>Player Bounty Property</summary>
      public UInt16 Bounty
      {
         set { m_nBounty = value; }
         get { return m_nBounty; }
      }

      ///<summary>Player Energy Level Property</summary>
      public UInt16 Energy
      {
         set { m_nEnergy = value; }
         get { return m_nEnergy; }
      }

      ///<summary>Player Ping Time Property</summary>
      public UInt16 Ping
      {
         set { m_nPing = value; }
         get { return m_nPing; }
      }

      ///<summary>Player X Velocity Property</summary>
      public UInt16 VelocityX
      {
         set { m_nVelocityX = value; }
         get { return m_nVelocityX; }
      }

      ///<summary>Player Y Velocity Property</summary>
      public UInt16 VelocityY
      {
         set { m_nVelocityY = value; }
         get { return m_nVelocityY; }
      }

      ///<summary>Player Server to Client Lag Property</summary>
      public UInt16 ServerToClientLag
      {
         set { m_nS2CLag = value; }
         get { return m_nS2CLag; }
      }

      ///<summary>Player Timer Property</summary>
      public UInt16 Timer
      {
         set { m_nTimer = value; }
         get { return m_nTimer; }
      }

      ///<summary>Player Ship State Information Property</summary>
      public ShipStateInfo ShipState
      {
         set { m_ShipStateInfo = value; }
         get { return m_ShipStateInfo; }
      }

      ///<summary>Player Item Information Property</summary>
      public ItemInfo Items
      {
         set { m_ItemInfo = value; }
         get { return m_ItemInfo; }
      }

      ///<summary>Player Weapon Informataion Property</summary>
      public WeaponInfo Weapon
      {
         set { m_WeaponInfo = value; }
         get { return m_WeaponInfo; }
      }
   }
}
