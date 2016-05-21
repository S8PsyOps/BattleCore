/***********************************************************************
*
* NAME:        DestroyTurretEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Player Death Event implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE      AUTHOR       CHANGES
* --------  ---------    -------------------------------------------------
* 12-24-12  lightbender  Initial Creation
*
************************************************************************/

// Namespace usage
using System;

// Namespace 
namespace BattleCore.Events
{
    /// <summary>
    /// DestroyTurretEvent object.  This event is triggered when a turret
    /// driver detaches from his turret gunners.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage: Implement this in a method to handle turret driver detach events.</para>
    /// <code lang="C#" escaped="true">
    /// public void OnDestroyTurretEvent (object sender, DestroyTurretEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// <code lang="Java" escaped="true">
    /// public void OnDestroyTurretEvent (object sender, DestroyTurretEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// </remarks>
    public class DestroyTurretEvent : EventArgs
    {
        private String m_strTurretHost;       // Turret Host name
        private UInt16 m_nTurretHostId;       // Turret Host identifier

        ///<summary>Turret Host name Property</summary>
        public String TurretHostName
        {
            set { m_strTurretHost = value; }
            get { return m_strTurretHost; }
        }

        ///<summary>Killed Player Identifier Property</summary>
        public UInt16 TurretHostId
        {
            set { m_nTurretHostId = value; }
            get { return m_nTurretHostId; }
        }
    }
}
