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
            this.m_PlayerName = PlayerName;
            this.m_Active = true;
        }

        private string m_PlayerName;
        private bool m_InLobby;
        private bool m_Active;
        
        public string PlayerName
        { 
            get { return m_PlayerName; } 
        }

        public bool Active
        {
            get { return m_Active; }
            set { m_Active = value; }
        }

        public bool InLobby
        {
            get { return m_InLobby; }
            set { m_InLobby = value; }
        }
    }
}
