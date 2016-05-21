//-----------------------------------------------------------------------
//
// NAME:        ArenaListEvent.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Arena List Event implementation.
//
// NOTES:       None.
//
/* $History: ArenaListEvent.cs $
* DATE        AUTHOR       CHANGES
* --------    ---------    -------------------------------------------------
* 2015-11-07  PsyOps        Initial Creation
*-----------------------------------------------------------------------*/

// Namespace usage
using System;
using System.Collections.Generic;
using BattleCore;

namespace BattleCore.Events
{
    /// <summary>
    /// ArenaListEvent object.  This event is used to retrieve  
    /// arena list information from the core.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage: Implement this in a method to handle arena list info.</para>
    /// <code lang="C#" escaped="true">
    /// public void OnArenaListInfo (object sender, ArenaListEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// </remarks>
    public class ArenaListEvent : EventArgs
    {
        private System.Collections.Generic.Dictionary<String, UInt16> m_ArenasList;

        ///<summary>List of arenas with population attached</summary>
        public System.Collections.Generic.Dictionary<String, UInt16> ArenasList
        {
            set { m_ArenasList = new System.Collections.Generic.Dictionary<String, UInt16>(value); }
            get { return m_ArenasList; }
        }
    }
}