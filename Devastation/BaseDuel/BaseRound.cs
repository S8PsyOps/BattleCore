using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCorePsyOps;

namespace Devastation.BaseDuel
{
    class BaseRound
    {
        public BaseRound()
        {
            m_AlphaTeam = new BaseTeam();
            m_BravoTeam = new BaseTeam();
            m_StartTime = new DateTime();
            m_TotalTime = new TimeSpan();
        }

        private BaseTeam m_AlphaTeam;
        private BaseTeam m_BravoTeam;
        private DateTime m_StartTime;
        private TimeSpan m_TotalTime;
        private short m_BaseNumber;
        private WinType m_WinType;
        private bool m_AlphaWon;
        private string m_SafeWinner;

        private int m_AlphaDeaths = 0;
        private int m_BravoDeaths = 0;

        /// <summary>
        /// Alpha Team container
        /// </summary>
        public BaseTeam AlphaTeam
        {
            get { return m_AlphaTeam; }
            set { m_AlphaTeam = value; }
        }

        /// <summary>
        /// Bravo Team container
        /// </summary>
        public BaseTeam BravoTeam
        {
            get { return m_BravoTeam; }
            set { m_BravoTeam = value; }
        }

        public DateTime StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        /// <summary>
        /// Who won match. Alpha = true ; Bravo = false;
        /// </summary>
        public bool AlphaWon
        {
            get { return m_AlphaWon; }
            set { m_AlphaWon = value; }
        }

        /// <summary>
        /// How was match won.
        /// </summary>
        public WinType WinType
        {
            get { return m_WinType; }
            set { m_WinType = value; }
        }

        public short BaseNumber
        {
            set { m_BaseNumber = value; }
            get { return m_BaseNumber; }
        }

        public string SafeWinner
        {
            get { return m_SafeWinner; }
            set { m_SafeWinner = value; }
        }

        public TimeSpan TotalTime
        {
            get { return m_TotalTime; }
            set { m_TotalTime = value; }
        }

        public string TotalTimeFormatted
        {
            get { return
                m_TotalTime.Hours.ToString().PadLeft(2, '0') + "h:" 
                + m_TotalTime.Minutes.ToString().PadLeft(2, '0') + "m:"
                + m_TotalTime.Seconds.ToString().PadLeft(2, '0') + "s";
            }
        }

        public int AlphaDeaths
        {
            get { return m_AlphaDeaths; }
            set { m_AlphaDeaths = value; }
        }

        public int BravoDeaths
        {
            get { return m_BravoDeaths; }
            set { m_BravoDeaths = value; }
        }

        public BaseTeam getTeam(SSPlayer p)
        {
            BasePlayer b = this.m_AlphaTeam.getPlayer(p);
            if (b != null)
            {
                return this.m_AlphaTeam;
            }
            else
            {
                b = this.m_BravoTeam.getPlayer(p);
                if (b != null)
                {
                    return this.m_BravoTeam;
                }
            }
            return null;
        }

        public BasePlayer getPlayer(SSPlayer p)
        {
            BasePlayer b = this.m_AlphaTeam.getPlayer(p);

            if (b == null)
            {
                b = this.m_BravoTeam.getPlayer(p);
            }

            return b;
        }

        public void flushTeams()
        {
            // Update players - itirate backwards so you can delete inactives
            for (int i = this.m_AlphaTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!this.m_AlphaTeam.TeamMembers[i].Active)
                {
                    this.m_AlphaTeam.TeamMembers.Remove(this.m_AlphaTeam.TeamMembers[i]);
                }
                else
                {
                    string name = this.m_AlphaTeam.TeamMembers[i].PlayerName;
                    this.m_AlphaTeam.TeamMembers[i] = new BasePlayer(name);
                }
            }
            // Update players - itirate backwards so you can delete inactives
            for (int i = this.m_BravoTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!this.m_BravoTeam.TeamMembers[i].Active)
                {
                    this.m_BravoTeam.TeamMembers.Remove(this.m_BravoTeam.TeamMembers[i]);
                }
                else
                {
                    string name = this.m_BravoTeam.TeamMembers[i].PlayerName;
                    this.m_BravoTeam.TeamMembers[i] = new BasePlayer(name);
                }
            }
        }

        public BaseRound getSavedRound()
        {
            BaseRound r = new BaseRound();
            r.AlphaTeam = this.m_AlphaTeam;
            r.BravoTeam = this.m_BravoTeam;
            r.AlphaWon = this.m_AlphaWon;
            r.BaseNumber = this.m_BaseNumber;
            r.StartTime = this.m_StartTime;
            r.TotalTime = this.m_TotalTime;
            r.WinType = this.m_WinType;
            return r;
        }
    }
}
