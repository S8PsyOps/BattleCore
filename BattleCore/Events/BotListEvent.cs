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
   /// Bot List command event sent to the core to list
   /// all available and loaded bots.
   /// </summary>
   internal class BotListEvent : EventArgs
   {
      private UInt16 m_nPlayerId = 0xFFFF;
      /// <summary>
      /// Player Identifier Property
      /// </summary>
      public UInt16 PlayerId
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }
   }
}
