/***********************************************************************
*
* NAME:        CreateTurretEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Create Turret Event implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE      AUTHOR       CHANGES
* --------  ---------    -------------------------------------------------
* 12-22-12  lightbender  Initial Creation
*
************************************************************************/

// Namespace usage
using System;

// Namespace 
namespace BattleCore.Events
{
    /// <summary>
    /// CreateTurretEvent object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage: Implement this in a method to handle player death events.</para>
    /// <code lang="C#" escaped="true">
    /// public void OnPlayerDeathEvent (object sender, PlayerDeathEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// <code lang="Java" escaped="true">
    /// public void OnPlayerDeathEvent (Object sender, PlayerDeathEvent e) 
    /// { 
    ///    ... 
    /// }
    /// </code>
    /// </remarks>
    public class CreateTurretEvent : EventArgs
    {
        private String m_strTurretHost;       // Turret Host name
        private UInt16 m_nTurretHostId;       // Turret Host identifier

        ///<summary>Turret Host Name Property</summary>
        public String TurretHostName
        {
            set { m_strTurretHost = value; }
            get { return m_strTurretHost; }
        }

        ///<summary>Turret Host Identifier Property</summary>
        public UInt16 TurretHostId
        {
            set { m_nTurretHostId = value; }
            get { return m_nTurretHostId; }
        }
    }
}
