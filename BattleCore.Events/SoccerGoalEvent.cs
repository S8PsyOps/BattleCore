/***********************************************************************
*
* NAME:        SoccerGoalEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Soccer Goal Event implementation.
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
   /// SoccerGoalEvent object.  This event is triggered when the a goal is 
   /// scored in a soccer arena.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle soccer goal events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnSoccerGoalEvent (object sender, SoccerGoalEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnSoccerGoalEvent (Object sender, SoccerGoalEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class SoccerGoalEvent : EventArgs
   {
      private UInt16 m_nFrequency;  // Frequency number
      private UInt32 m_nPoints;     // points Awarded

      ///<summary>Team Frequency Property</summary>
      public UInt16 Frequency
      {
         set { m_nFrequency = value; }
         get { return m_nFrequency; }
      }

      ///<summary>Points Awarded Property</summary>
      public UInt32 Points
      {
         set { m_nPoints = value; }
         get { return m_nPoints; }
      }
   }
}
