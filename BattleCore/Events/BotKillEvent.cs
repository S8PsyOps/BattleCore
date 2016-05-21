//-----------------------------------------------------------------------
//
// NAME:        BotListEvent.cs
//
// PROJECT:     BattleCore Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Bot List Event implementation.
//
// NOTES:       None.
//
// $History: BotListEvent.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;

// namespace declaration
namespace BattleCore.Events
{
   /// <summary>
   /// Bot kill command event sent to the core to kill
   /// the bot.
   /// </summary>
   internal class BotKillEvent : EventArgs
   {
      private UInt16 m_nPlayerId = 0xFFFF;
      private String m_nMessage;
      /// <summary>
      /// Player Identifier Property
      /// </summary>
      public UInt16 PlayerId
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }
      public String Message
      {
          set { m_nMessage = value; }
          get { return m_nMessage; }
      }
   }
}
