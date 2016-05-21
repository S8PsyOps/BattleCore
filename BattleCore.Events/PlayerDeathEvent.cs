/***********************************************************************
*
* NAME:        PlayerDeathEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Player Death Event implementation.
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
    /// PlayerDeathEvent object.  This event is triggered when a player
    /// is killed.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle player death events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnPlayerDeathEvent (object sender, PlayerDeathEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnPlayerDeathEvent (Object sender, PlayerDeathEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class PlayerDeathEvent : EventArgs
    {
        private String    m_strKillerName;   // Killer Player name
        private String    m_strKilledName;   // Killed Player name
        private UInt16    m_nKillerId;       // Killer Player identifier
        private UInt16    m_nKilledId;       // Killed Player identifier
        private UInt16    m_nBounty;         // Bounty
        private UInt16    m_nFlagsCarried;   // Number of flags carried
        private Byte      m_nDeathGreen;     // Generated death green
        private ModLevels m_killedModLevel;  // Killed Moderator Level
        private ModLevels m_killerModLevel;  // Killer Moderator Level

        ///<summary>Killer Player name Property</summary>
        public String KillerName
        {
            set { m_strKillerName = value; }
            get { return m_strKillerName; }
        }

        ///<summary>Killed Player name Property</summary>
        public String KilledName
        {
            set { m_strKilledName = value; }
            get { return m_strKilledName; }
        }

        ///<summary>Killer Player Identifier Property</summary>
        public UInt16 KillerId
        {
            set { m_nKillerId = value; }
            get { return m_nKillerId; }
        }

        ///<summary>Killed Player Identifier Property</summary>
        public UInt16 KilledId
        {
            set { m_nKilledId = value; }
            get { return m_nKilledId; }
        }

         ///<summary>Player Bounty Property</summary>
         public UInt16 Bounty 
         {
            set { m_nBounty = value; }
            get { return m_nBounty; }
         }

         ///<summary>Player Flags Carried Property</summary>
         public UInt16 FlagsCarried 
         {
            set { m_nFlagsCarried = value; }
            get { return m_nFlagsCarried; }
         }

         ///<summary>Player Flags Carried Property</summary>
         public Byte DeathGreen 
         {
            set { m_nDeathGreen = value; }
            get { return m_nDeathGreen; }
         }

        ///<summary>Killer Moderator Level Property</summary>
        public ModLevels KillerModLevel
        {
            set { m_killerModLevel = value; }
            get { return m_killerModLevel; }
        }

        ///<summary>Killed Player Moderator Level Property</summary>
        public ModLevels KilledModLevel
        {
            set { m_killedModLevel = value; }
            get { return m_killedModLevel; }
        }
    }
}
