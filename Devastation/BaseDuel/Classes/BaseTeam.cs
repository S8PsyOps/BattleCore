using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    class BaseTeam
    {
        public BaseTeam()
        {
            this.resetTeam();
        }

        private List<BasePlayer> m_Players;
        private List<BasePlayer> m_Inactives;
        private string m_TeamName;

        public BasePlayer getPlayer(string PlayerName)
        {
            return m_Players.Find(item => item.PlayerName == PlayerName);
        }

        public bool teamAllOut()
        {
            return !(m_Players.FindAll(player => !player.inLobby()).Count > 0);
        }

        public void resetTeam()
        {
            this.m_Players = new List<BasePlayer>();
            this.m_Inactives = new List<BasePlayer>();
        }

        public int teamCount()
        { return m_Players.Count; }

        public void playerJoin(string PlayerName)
        { this.m_Players.Add(new BasePlayer(PlayerName)); }

        public string teamName()
        { return this.m_TeamName; }
        public void teamName(string teamName)
        { this.m_TeamName = teamName; }

        public void removePlayer(string PlayerName)
        {
            BasePlayer b = m_Players.Find(item => item.PlayerName == PlayerName);

            // Might be copying a pointer here - may have to do copy over
            m_Inactives.Add(b);
            m_Players.Remove(b);
        }
    }
}
