using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel
{
    class BaseMatch
    {
        public BaseMatch()
        {
            m_AlphaTeam = new BaseTeam();
            m_BravoTeam = new BaseTeam();
        }

        private BaseTeam m_AlphaTeam, m_BravoTeam;
        private bool m_AlphaWin;
        private WinType m_WinType;
        private DateTime m_MatchStartTimeStamp;
        private DateTime m_EndTime;

        /// <summary>
        /// Who won match.
        /// Alpha:True Bravo:False
        /// </summary>
        public bool AlphaWin
        {
            get { return m_AlphaWin; }
            set { m_AlphaWin = value; }
        }

        /// <summary>
        /// How Match was won
        /// </summary>
        public WinType WinType
        {
            get { return m_WinType; }
            set { m_WinType = value; }
        }

        /// <summary>
        /// Gets a player using name, and sends back team
        /// </summary>
        /// <param name="PlayerName">Player's name</param>
        /// <param name="alpha">Player is on alpha team: result</param>
        /// <returns>BasePlayer container</returns>
        public BasePlayer GetPlayer( string PlayerName, out bool alpha)
        {
            BasePlayer b = m_AlphaTeam.TeamList.Find(item => item.PlayerName == PlayerName);

            if ( b != null)
            {
                alpha = true;
                return b;
            }

            alpha = false;
            return m_BravoTeam.TeamList.Find(item => item.PlayerName == PlayerName);
        }

        /// <summary>
        /// Grab bot teams from match
        /// </summary>
        /// <param name="AlphaTeam">Alpha Team Duh</param>
        /// <param name="BravoTeam">Bravo Team Duh</param>
        public void GetTeams(out BaseTeam AlphaTeam, out BaseTeam BravoTeam)
        {
            AlphaTeam = m_AlphaTeam;
            BravoTeam = m_BravoTeam;
        }

        /// <summary>
        /// Lets you know if one of the 2 teams is out.
        /// </summary>
        /// <param name="alpha">Alpha is out: result</param>
        /// <param name="bravo">Bravo is out: result</param>
        /// <returns></returns>
        public bool TeamIsOut( out bool alpha, out bool bravo)
        {
            alpha = m_AlphaTeam.AllOut;
            bravo = m_BravoTeam.AllOut;

            return (alpha != bravo || alpha == true);
        }

        /// <summary>
        /// Copy over Teams from Game to Match.
        /// BaseGame will hold master list.
        /// </summary>
        /// <param name="Alpha"></param>
        /// <param name="Bravo"></param>
        public void CreateTeams(BaseTeam Alpha, BaseTeam Bravo)
        {
            for (int i = 0; i < Alpha.TeamList.Count; i++)
                m_AlphaTeam.TeamList.Add(new BasePlayer(Alpha.TeamList[i].PlayerName));
            for (int i = 0; i < Bravo.TeamList.Count; i++)
                m_BravoTeam.TeamList.Add(new BasePlayer(Bravo.TeamList[i].PlayerName));
        }
    }
}
