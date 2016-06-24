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

        public List<BasePlayer> Players
        { get { return this.m_Players; } }

        public List<BasePlayer> Inactives
        { get { return this.m_Inactives; } }

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

            List<BasePlayer> players = new List<BasePlayer>();
            List<BasePlayer> inactives = new List<BasePlayer>();
            
            foreach(BasePlayer b in this.m_Players) players.Add(b.GetCopy());
            foreach (BasePlayer b in this.m_Inactives) inactives.Add(b.GetCopy());

            copy.m_Players = players;
            copy.m_Inactives = inactives;

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

            this.m_Players.Add(b.GetCopy());
            this.m_Inactives.Remove(b);
        }

        public void removePlayer(string PlayerName)
        {
            BasePlayer b = m_Players.Find(item => item.PlayerName == PlayerName);

            if (b == null) return;
            // Might be copying a pointer here - may have to do copy over
            m_Inactives.Add(b.GetCopy());
            m_Players.Remove(b);
        }
    }
}
