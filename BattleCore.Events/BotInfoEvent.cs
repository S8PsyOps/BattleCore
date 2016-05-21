//-----------------------------------------------------------------------
//
// NAME:        BotInfoEvent.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Bot Info Event implementation.
//
// NOTES:       None.
//
// $History: BotInfoEvent.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using BattleCore;

namespace BattleCore.Events
{
   /// <summary>
   /// BotInfoEvent object.  This event is used to retrieve  
   /// bot information from the core.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle player info.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnBotInfo (object sender, BotInfoEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <para>
   /// To request bot information from the core, create 
   /// the BotInfoEvent object and send it to the core.</para>
   /// <code lang="C#" escaped="true">
   /// BotInfoEvent e = new BotInfoEvent ();
   /// SendGameEvent (e);
   /// </code>
   /// </remarks>
   public class BotInfoEvent : EventArgs
   {
      // Bot player information
      PlayerInfo m_botInfo = new PlayerInfo ();

      /// <summary>
      /// Player Informantion Property containing all known information 
      /// about the bot.
      /// </summary>
      public PlayerInfo BotInfo
      {
         set { m_botInfo = value; }
         get { return m_botInfo; }
      }
   }
}
