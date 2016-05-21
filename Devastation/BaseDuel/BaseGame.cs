using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseDuel
{
    class BaseGame
    {
        public BaseGame()
        {
            this.msg = new ShortChat();
        }

        private BaseTeam m_AlphaTeam, m_BravoTeam;
        private ushort m_AlphaFreq, m_BravoFreq;
        private DateTime m_StartTime;
        private BaseMatch m_CurrentMatch;
        private List<BaseMatch> m_Matches;
        private ShortChat msg;
        private bool m_Alternate;

        public int MatchCount
        { get { return m_Matches.Count; } }

        public void CreateFreqs(ushort AlphaFreq, ushort BravoFreq)
        {
            this.m_AlphaFreq = AlphaFreq;
            this.m_BravoFreq = BravoFreq;
        }

        public void CreateTeams(List<string> m_AlphaWaitList, List<string> m_BravoWaitList)
        {
            m_AlphaTeam = new BaseTeam();
            while (m_AlphaWaitList.Count > 0)
            {
                m_AlphaTeam.PlayerJoin(m_AlphaWaitList[0]);
                m_AlphaWaitList.RemoveAt(0);
            }
            m_BravoTeam = new BaseTeam();
            while (m_BravoWaitList.Count > 0)
            {
                m_BravoTeam.PlayerJoin(m_BravoWaitList[0]);
                m_BravoWaitList.RemoveAt(0);
            }

            m_Matches = new List<BaseMatch>();
            ResetMatch();
        }

        private void ResetMatch()
        {
            m_CurrentMatch = new BaseMatch();
            m_CurrentMatch.CreateTeams(m_AlphaTeam, m_BravoTeam);
        }

        public void MatchOver(bool AlphaWon, WinType winType)
        {
            m_CurrentMatch.WinType = winType;
            m_Matches.Add(m_CurrentMatch);
            ResetMatch();
        }

        public BasePlayer GetPlayer(string PlayerName, out bool alpha)
        {
            return m_CurrentMatch.GetPlayer(PlayerName, out alpha);
        }

        public void WarpPlayersToStart(Base LoadedBase, Queue<EventArgs> Q)
        {
            m_Alternate = !m_Alternate;

            Q.Enqueue(msg.teamPM(
                (m_Alternate ? m_AlphaTeam.TeamList[0].PlayerName : m_BravoTeam.TeamList[0].PlayerName),
                "?|warpto " + (m_Alternate ? LoadedBase.AlphaStartX : LoadedBase.BravoStartX) + " " + 
                (m_Alternate ? LoadedBase.AlphaStartY:LoadedBase.BravoStartY) + "|shipreset"
                ));
            Q.Enqueue(msg.teamPM(
               (!m_Alternate ? m_AlphaTeam.TeamList[0].PlayerName : m_BravoTeam.TeamList[0].PlayerName),
               "?|warpto " + (!m_Alternate ? LoadedBase.AlphaStartX : LoadedBase.BravoStartX) + " " +
               (!m_Alternate ? LoadedBase.AlphaStartY : LoadedBase.BravoStartY) + "|shipreset"
               ));
        }

        public void RemovePlayer(string PlayerName)
        {
            bool alpha;
            BasePlayer b = m_CurrentMatch.GetPlayer(PlayerName, out alpha);
            b.Active = false;

            // Check match for uneven teams here --
        }

        public void GetTeams(out BaseTeam AlphaTeam, out BaseTeam BravoTeam)
        {
            m_CurrentMatch.GetTeams( out AlphaTeam, out BravoTeam);
        }

        public bool TeamIsOut( out bool alpha, out bool bravo)
        {
            return m_CurrentMatch.TeamIsOut(out alpha, out bravo);
        }
    }
}
