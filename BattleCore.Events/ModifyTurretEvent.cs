/***********************************************************************
*
* NAME:        ModifyTurretEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Modify Turret Event implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE      AUTHOR       CHANGES
* --------  ---------    -------------------------------------------------
* 12-26-12  lightbender  Initial Creation
*
************************************************************************/

// Namespace usage
using System;

// Namespace 
namespace BattleCore.Events
{
    /// <summary>
    /// ModifyTurretEvent object.  This event is triggered when a player
    /// attaches to or detaches from a turret.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage: Implement this in a method to handle turret modification events.</para>
    /// <code lang="C#" escaped="true">
    /// public void OnModifyTurretEvent (object sender, ModifyTurretEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// <code lang="Java" escaped="true">
    /// public void OnModifyTurretEvent (object sender, ModifyTurretEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// </remarks>
    public class ModifyTurretEvent : EventArgs
    {
        private String m_strTurretAttacher;   // Turret Attacher name
        private String m_strTurretHost;       // Turret Host name
        private UInt16 m_nTurretAttacherId;   // Turret Attacher identifier
        private UInt16 m_nTurretHostId;       // Turret Host identifier

        ///<summary>Turret Attacher name Property</summary>
        public String TurretAttacherName
        {
            set { m_strTurretAttacher = value; }
            get { return m_strTurretAttacher; }
        }

        ///<summary>Turret Host name Property</summary>
        public String TurretHostName
        {
            set { m_strTurretHost = value; }
            get { return m_strTurretHost; }
        }

        ///<summary>Killer Player Identifier Property</summary>
        public UInt16 TurretAttacherId
        {
            set { m_nTurretAttacherId = value; }
            get { return m_nTurretAttacherId; }
        }

        ///<summary>Killed Player Identifier Property</summary>
        public UInt16 TurretHostId
        {
            set { m_nTurretHostId = value; }
            get { return m_nTurretHostId; }
        }
    }
}
