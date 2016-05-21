/***********************************************************************
*
* NAME:        LoginEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Login Event implementation.
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
   /// LoginEvent object.  This event is triggered when the bot logs in.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle Login events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnLoginEvent (object sender, LoginEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// public void OnLoginEvent (Object sender, LoginEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// </remarks>
   public class LoginEvent : EventArgs
   {
      
   }
}
