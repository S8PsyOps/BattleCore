using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    public class BaseTeam
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

        public void flushTeam()
        {
            // I am considering making another class Stats.cs
            // and then reseting it on a flush
            // ill do this for now
            for (int i = 0; i < m_Players.Count; i++)
            {
                string name = m_Players[i].PlayerName;
                m_Players[i] = new BasePlayer(name);
            }
            // completely delete any inactives
            m_Inactives = new List<BasePlayer>();
        }

        public BaseTeam getCopy()
        {
            BaseTeam copy = new BaseTeam();
            copy.m_Players = (this.m_Players.ToArray()).ToList();
            copy.m_Inactives = (this.m_Inactives.ToArray()).ToList();
            return copy;
        }

        public void resetTeam()
        {
            this.m_Players = new List<BasePlayer>();
            this.m_Inactives = new List<BasePlayer>();
        }

        public int teamCount()
        { return m_Players.Count; }

        public void playerJoin(string PlayerName)
        {
            BasePlayer b = this.m_Inactives.Find(item => item.PlayerName == PlayerName);

            if (b == null)
            {
                this.m_Players.Add(new BasePlayer(PlayerName));
                return;
            }

            this.m_Players.Add(b);
            this.m_Inactives.Remove(b);
        }

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
