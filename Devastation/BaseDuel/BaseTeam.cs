using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseDuel
{
    class BaseTeam
    {
        //public BaseTeam()
        public BaseTeam()
        {
            this.m_TeamPlayers = new List<BasePlayer>();
            this.msg = new ShortChat();
        }

        private ShortChat msg;
        private List<BasePlayer> m_TeamPlayers;
        private int m_Score;

        // Find out if everyone is out
        public bool AllOut
        {
            get
            {
                for (int i = 0; i < m_TeamPlayers.Count; i++)
                    if (!m_TeamPlayers[i].InLobby) return false;
                return true;
            }
        }

        public int TeamActiveCount
        {   
            get 
            {
                int count = 0;

                for (int i = 0; i < m_TeamPlayers.Count; i++)
                    if (m_TeamPlayers[i].Active) count++;

                return count; 
            } 
        }

        public int Score
        {
            get { return m_Score; }
            set { m_Score = value; }
        }

        public List<BasePlayer> TeamList
        {
            get { return m_TeamPlayers; }
            set { m_TeamPlayers = value; }
        }

        public EventArgs PlayerJoin(string PlayerName)
        {
            BasePlayer b = m_TeamPlayers.Find(item => item.PlayerName == PlayerName);

            if (b != null)
            {
                if (!b.Active) b.Active = true;
                return msg.arena("Player[ " + b.PlayerName + " ] status was changed from inactive, to active.");
            }

            b = new BasePlayer(PlayerName);
            m_TeamPlayers.Add(b);
            return msg.arena("Player[ "+PlayerName+" ] was added to TeamPlayers.");
        }
    }
}
