/***********************************************************************
*
* NAME:        LVZToggleEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: LVZ Toggle Event implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE        AUTHOR       CHANGES
* --------    ---------    -------------------------------------------------
* 1015-11-06  lightbender  Initial Creation
*
************************************************************************/

// Namespace usage
using System;

// Namespace 
namespace BattleCore.Events
{
    /// <summary>
    /// LVZToggleEvent object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage: Implement this in a method to LVZ Toggle events.</para>
    /// <code lang="C#" escaped="true">
    /// public void OnLVZToggleEvent (object sender, LVZToggleEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// <code lang="Java" escaped="true">
    /// public void OnLVZToggleEvent (object sender, LVZToggleEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// </remarks>
    public class LVZToggleEvent : EventArgs
    {
        private String m_strTargetPlayer;       // LVZ Toggle Target Player Name
        private UInt16 m_nTargetPlayerId;       // LVZ Toggle Target Player Identifier
        private System.Collections.Generic.Dictionary<UInt16, Boolean> m_LVZObjects;   // LVZ Objects Storage

        ///<summary>LVZ Toggle Target Player Name Property</summary>
        public String TargetPlayerName
        {
            set { m_strTargetPlayer = value; }
            get { return m_strTargetPlayer; }
        }

        ///<summary>LVZ Toggle Target Player Identifier Property</summary>
        public UInt16 TargetPlayerId
        {
            set { m_nTargetPlayerId = value; }
            get { return m_nTargetPlayerId; }
        }

        ///<summary>LVZ Objects Storage Property</summary>
        public System.Collections.Generic.Dictionary<UInt16, Boolean> LVZObjects
        {
            set { m_LVZObjects = new System.Collections.Generic.Dictionary<UInt16, Boolean>(value); }
            get { return m_LVZObjects; }
        }
    }
}
