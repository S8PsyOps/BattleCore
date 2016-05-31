using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel
{
    class BasePlayer
    {
        public BasePlayer(string PlayerName)
        {
            this.m_WarpStamp = DateTime.Now;
            this.m_InLobby = true;
            this.m_Active = true;
            this.m_PlayerName = PlayerName;
        }

        private string m_PlayerName;
        private bool m_Active;
        private DateTime m_WarpStamp;
        private bool m_InLobby;

        /// <summary>
        /// Player's Name
        /// </summary>
        public string PlayerName
        {
            get { return m_PlayerName; }
            set { m_PlayerName = value; }
        }

        /// <summary>
        /// If player is currently playing or did he leave.
        /// </summary>
        public bool Active
        {
            get { return m_Active; }
        }

        /// <summary>
        /// Is player in the center start area?
        /// </summary>
        public bool InLobby
        {
            get { return m_InLobby; }
            set { m_InLobby = value; }
        }

        /// <summary>
        /// Set player's active status
        /// </summary>
        /// <param name="ActiveStatus">Active or Not</param>
        public void setActive(bool ActiveStatus)
        {
            this.m_Active = ActiveStatus;
        }
    }
}
