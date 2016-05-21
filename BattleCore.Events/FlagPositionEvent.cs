/***********************************************************************
*
* NAME:        FlagPositionEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Flag Position Event implementation.
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
   /// FlagPositionEvent object.  This event is triggered when the flag is 
   /// respawned at a different position.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle flag position events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnFlagPositionEvent (object sender, FlagPositionEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnFlagPositionEvent (Object sender, FlagPositionEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class FlagPositionEvent : EventArgs
   {
      private UInt16 m_nFlagId;        // Flag identifier
      private UInt16 m_nPositionX;     // X map position
      private UInt16 m_nPositionY;     // Y map position
      private UInt16 m_nFrequency;     // Frequency number

      ///<summary>Flag Identifier Property</summary>
      public UInt16 FlagId
      {
         set { m_nFlagId = value; }
         get { return m_nFlagId; }
      }

      ///<summary>Map Postion X Property</summary>
      public UInt16 MapPositionX
      {
         set { m_nPositionX = value; }
         get { return m_nPositionX; }
      }

      ///<summary>Map Postion Y Property</summary>
      public UInt16 MapPositionY
      {
         set { m_nPositionY = value; }
         get { return m_nPositionY; }
      }

      ///<summary>Player Frequency Property</summary>
      public UInt16 Frequency
      {
         set { m_nFrequency = value; }
         get { return m_nFrequency; }
      }
   }
}
