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
* 09-11-07  udp        Initial Creation
*
************************************************************************/

// Namespace usage
using System;
using BattleCore;

// Namespace declaration
namespace BattleCore.Events
{
    /// <summary>
   /// ListModEvent object.  This event is triggered when a listmod command is handled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage: Implement this in a method to handle flag claim events.</para>
    /// <code lang="C#" escaped="true">
    /// public void OnListModEvent (object sender, ListModEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// <code lang="Java" escaped="true">
    /// public void OnListModEvent (Object sender, ListModEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// </remarks>
    public class ListModEvent : EventArgs
    {
        private PlayerInfo m_moderatorInfo = new PlayerInfo ();

        ///<summary>Moderator Information Property</summary>
       public PlayerInfo Moderator
        {
           set { m_moderatorInfo = value; }
           get { return m_moderatorInfo; }
        }
    }
}
