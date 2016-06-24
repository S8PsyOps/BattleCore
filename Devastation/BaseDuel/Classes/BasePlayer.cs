using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    public class BasePlayer
    {
        public BasePlayer(string PlayerName)
        {
            this.m_PlayerName = PlayerName;
            this.m_InLobby = true;
            this.m_Stats = new BasePlayerStats();
        }

        private string m_PlayerName;
        private bool m_InLobby;
        private BasePlayerStats m_Stats;
        
        public string PlayerName
        { get { return m_PlayerName; } }

        // Set or check if player is in lobby
        public bool inLobby()
        { return this.m_InLobby; }
        public void inLobby(bool InLobby)
        { this.m_InLobby = InLobby; }

        public BasePlayerStats Stats
        {
            get { return this.m_Stats; }
            set { this.m_Stats = value; }
        }

        public BasePlayer GetCopy()
        {
            BasePlayer copy = new BasePlayer(this.m_PlayerName);
            copy.m_Stats = this.m_Stats.GetCopy();
            return copy;
        }
    }
}
