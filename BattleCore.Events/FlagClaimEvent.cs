/***********************************************************************
*
* NAME:        FlagClaimEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Flag Claim Event implementation.
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

// Namespace declaration
namespace BattleCore.Events
{
   /// <summary>
   /// FlagClaimEvent object.  This event is triggered when the flag is 
   /// claimed by a player.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle flag claim events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnFlagClaimEvent (object sender, FlagClaimEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnFlagClaimEvent (Object sender, FlagClaimEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class FlagClaimEvent : EventArgs
   {
      private UInt16    m_nFlagId;       // Flag identifier
      private String    m_strPlayerName; // Player Name
      private UInt16    m_nPlayerId;     // Player identifier
      private ModLevels m_modLevel;      // Moderator Level

      ///<summary>Flag Identifier Property</summary>
      public UInt16 FlagId
      {
         set { m_nFlagId = value; }
         get { return m_nFlagId; }
      }

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
   }
}
