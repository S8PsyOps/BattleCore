/***********************************************************************
*
* NAME:        PrizeGrantedEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Prize Granted Event implementation.
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
   /// PrizeGrantedEvent object.  This event is triggered when a prize
   /// is collected by the bot.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle prize granted events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnPrizeGrantedEvent (object sender, PrizeGrantedEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnPrizeGrantedEvent (Object sender, PrizeGrantedEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class PrizeGrantedEvent : EventArgs
   {
      private UInt16 m_nPrizeCount;      // Number of Prizes
      private PrizeTypes m_prizeType;    // Prize number

      ///<summary>Prize Count Property</summary>
      public UInt16 Count
      {
         set { m_nPrizeCount = value; }
         get { return m_nPrizeCount; }
      }

      ///<summary>Prize Type Property</summary>
      public PrizeTypes PrizeType
      {
         set { m_prizeType = value; }
         get { return m_prizeType; }
      }
   }
}
