/***********************************************************************
*
* NAME:        FlagDropEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Flag Drop Event implementation.
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
   /// FlagDropEvent object.  This event is triggered when the flag is 
   /// dropped by a player.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle flag drop events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnFlagDropEvent (object sender, FlagDropEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnFlagDropEvent (Object sender, FlagDropEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class FlagDropEvent : EventArgs
   {
      private String m_strPlayerName; // Player Name
      private UInt16 m_nPlayerId;     // Player identifier
      private ModLevels m_modLevel;   // Moderator Level

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
