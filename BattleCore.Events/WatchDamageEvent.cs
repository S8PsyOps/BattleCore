/***********************************************************************
*
* NAME:        WatchDamageEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Watch Damage Event implementation.
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
   /// WatchDamageEvent object.  This event is triggered when a player
   /// damage event is received.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle watch damage events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnWatchDamageEvent (object sender, WatchDamageEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnWatchDamageEvent (Object sender, WatchDamageEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class WatchDamageEvent : EventArgs
   {
      private String      m_strPlayerName;    // Player Name
      private String      m_strAttackerName;  // Attacker Name
      private UInt16      m_nPlayerId;        // Player identifier
      private UInt16      m_nAttackerId;      // Attacker identifier
      private ModLevels   m_playerModLevel;   // Moderator Level
      private ModLevels   m_attackerModLevel; // Moderator Level
      private WeaponTypes m_weaponType;       // Weapon Type
      private UInt16      m_nEnergy;          // Energy
      private UInt16      m_nDamage;          // Damage
      private UInt32      m_nTimeStamp;       // Timestamp

      ///<summary>Player name Property</summary>
      public String PlayerName
      {
         set { m_strPlayerName = value; }
         get { return m_strPlayerName; }
      }

      ///<summary>Attacker name Property</summary>
      public String AttackerName
      {
         set { m_strAttackerName = value; }
         get { return m_strAttackerName; }
      }

      ///<summary>Player Identifier Property</summary>
      public UInt16 PlayerId
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }

      ///<summary>Attacker Identifier Property</summary>
      public UInt16 AttackerId
      {
         set { m_nAttackerId = value; }
         get { return m_nAttackerId; }
      }

      ///<summary>Player Moderator Level Property</summary>
      public ModLevels PlayerModLevel
      {
         set { m_playerModLevel = value; }
         get { return m_playerModLevel; }
      }

      ///<summary>Attacker Moderator Level Property</summary>
      public ModLevels AttackerModLevel
      {
         set { m_attackerModLevel = value; }
         get { return m_attackerModLevel; }
      }

      ///<summary>Energy Value Property</summary>
      public UInt16 Energy
      {
         set { m_nEnergy = value; }
         get { return m_nEnergy; }
      }

      ///<summary>Damage Value Property</summary>
      public UInt16 Damage
      {
         set { m_nDamage = value; }
         get { return m_nDamage; }
      }

      ///<summary>Weapon Type Property</summary>
      public WeaponTypes Weapon
      {
         set { m_weaponType = value; }
         get { return m_weaponType; }
      }

      ///<summary>Timestamp Value Property</summary>
      public UInt32 TimeStamp
      {
         set { m_nTimeStamp = value; }
         get { return m_nTimeStamp; }
      }
   }
}
