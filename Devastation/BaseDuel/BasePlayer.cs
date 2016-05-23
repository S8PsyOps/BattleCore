using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel
{
    class BasePlayer
    {
        public BasePlayer()
        {
            m_WarpStamp = DateTime.Now;
            m_InLobby = true;
            m_Active = true;
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
            set { m_Active = value; }
        }

        /// <summary>
        /// Is player in the center start area?
        /// </summary>
        public bool InLobby
        {
            get { return m_InLobby; }
            set { m_InLobby = value; }
        }
    }
}
