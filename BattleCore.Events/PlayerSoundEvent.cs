/***********************************************************************
*
* NAME:        PlayerLeftEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Player Left Event implementation.
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
   /// PlayerSoundEvent object.  This event is triggered when a player
   /// sends a wav message to the bot.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle player sound events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnPlayerSoundEvent (object sender, PlayerSoundEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnPlayerSoundEvent (Object sender, PlayerSoundEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class PlayerSoundEvent : EventArgs
   {
      private String m_strPlayerName; // Player Name
      private UInt16 m_nPlayerId;     // Player identifier
      private ModLevels m_modLevel;   // Moderator Level
      private ShipTypes m_shipType;   // Player ship type
      private Byte[] m_soundFile;     // Sound File

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

      ///<summary>Player Sound Property</summary>
      public Byte[] SoundFile
      {
         set { m_soundFile = value; }
         get { return m_soundFile; }
      }
   }
}
