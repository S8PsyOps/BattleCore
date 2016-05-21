using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace DevaBot.BaseDuel
{
    class BaseGame
    {
        public BaseGame()
        {
            this.msg = new ShortChat();
            this.m_CurrentMatch = new BaseMatch();
            this.m_MatchesPlayed = new List<BaseMatch>();
            this.m_Status = BaseGameStatus.GameIdle;
            this.m_AllowSafeWin = true;
            this.m_AllowAfterStartJoin = true;
            this.m_AlphaScore = 0;
            this.m_BravoScore = 0;
        }
        
        ShortChat msg;
        private BaseMatch m_CurrentMatch;
        private List<BaseMatch> m_MatchesPlayed;
        private BaseGameStatus m_Status;
        private ushort m_AlphaFreq, m_BravoFreq;
        private bool m_AllowSafeWin;
        private bool m_AllowAfterStartJoin;
        private int m_AlphaScore;
        private int m_BravoScore;

        /// <summary>
        /// Current match being played
        /// </summary>
        public BaseMatch CurrentMatch
        {
            get { return m_CurrentMatch; }
            set { m_CurrentMatch = value; }
        }

        /// <summary>
        /// List of all played matches in the current game.
        /// </summary>
        public List<BaseMatch> Matches
        {
            get { return m_MatchesPlayed; }
            set { m_MatchesPlayed = value; }
        }

        /// <summary>
        /// Current status of Game
        /// </summary>
        public BaseGameStatus Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        /// <summary>
        /// Alpha Team's Freq
        /// </summary>
        public ushort AlphaFreq
        {
            get { return m_AlphaFreq; }
            set { m_AlphaFreq = value; }
        }
        /// <summary>
        /// Bravo Team's Freq
        /// </summary>
        public ushort BravoFreq
        {
            get { return m_BravoFreq; }
            set { m_BravoFreq = value; }
        }

        /// <summary>
        /// Allow players to join after game has already started
        /// </summary>
        public bool AllowAfterStartJoin
        {
            get { return m_AllowAfterStartJoin; }
            set { m_AllowAfterStartJoin = value; }
        }

        /// <summary>
        /// If you want to allow Teams to win by going into opposing team's safe.
        /// </summary>
        public bool AllowSafeWin
        {
            get { return m_AllowSafeWin; }
            set { m_AllowSafeWin = true; }
        }

        /// <summary>
        /// Alpha Teams current score in current game.
        /// </summary>
        public int AlphaScore
        {
            get { return m_AlphaScore; }
            set { m_AlphaScore = value; }
        }

        /// <summary>
        /// Bravo Teams current score in current game.
        /// </summary>
        public int BravoScore
        {
            get { return m_BravoScore; }
            set { m_BravoScore = value; }
        }

        ///// <summary>
        ///// Move players from wait list to assigned teams
        ///// </summary>
        ///// <param name="Alpha">Alpha Wait List</param>
        ///// <param name="Bravo">Bravo Wait List</param>
        //public void PrepareGameStart(List<string> Alpha, List<string> Bravo)
        //{
        //    for (int i = 0; i < Alpha.Count; i++)
        //        addPlayerToTeam(Alpha[i], true);
        //    for (int i = 0; i < Bravo.Count; i++)
        //        addPlayerToTeam(Bravo[i], false);
        //}

        public void NewPlayerJoin(string PlayerName)
        {
            int a, b;
            GetTeamCounts(out a, out b);
            if (a <= b)
                addPlayerToTeam(PlayerName, true);
            else
                addPlayerToTeam(PlayerName, false);
        }

        private void addPlayerToTeam( string PlayerName, bool InAlpha)
        {
            BasePlayer b = new BasePlayer();
            b.PlayerName = PlayerName;
            if (InAlpha) m_CurrentMatch.AlphaTeam.TeamMembers.Add(b);
            else m_CurrentMatch.BravoTeam.TeamMembers.Add(b);
        }

        /// <summary>
        /// Get the total count of active players
        /// </summary>
        /// <param name="AlphaCount">How many players are active on Alpha Team</param>
        /// <param name="BravoCount">How many players are active on Bravo Team</param>
        public void GetTeamCounts(out int AlphaCount, out int BravoCount)
        {
            int a = 0;
            int b = 0;
            for (int i = 0; i < m_CurrentMatch.AlphaTeam.TeamMembers.Count; i++)
                if (m_CurrentMatch.AlphaTeam.TeamMembers[i].Active) a++;
            
            for (int i = 0; i < m_CurrentMatch.BravoTeam.TeamMembers.Count; i++)
                if (m_CurrentMatch.BravoTeam.TeamMembers[i].Active) b++;

            AlphaCount = a;
            BravoCount = b;
        }

        /// <summary>
        /// Check to see if teams are out
        /// </summary>
        /// <param name="Alpha">If Alpha Team is out</param>
        /// <param name="Bravo">If Bravo Team is out</param>
        public void CheckAllOut(out bool Alpha, out bool Bravo)
        {
            bool a = true;
            bool b = true;
            for (int i = 0; i < m_CurrentMatch.AlphaTeam.TeamMembers.Count; i++)
                if (m_CurrentMatch.AlphaTeam.TeamMembers[i].Active && !m_CurrentMatch.AlphaTeam.TeamMembers[i].InLobby ) a = false;
            
            for (int i = 0; i < m_CurrentMatch.BravoTeam.TeamMembers.Count; i++)
                if (m_CurrentMatch.BravoTeam.TeamMembers[i].Active && !m_CurrentMatch.BravoTeam.TeamMembers[i].InLobby) b = false;

            Alpha = a;
            Bravo = b;
        }

        public void StartMatch()
        {
            m_CurrentMatch.StartTime = DateTime.Now;
        }

        public void MatchEnded(WinType winType, bool AlphaWon, short BaseNumber, out string TotalTime )
        {
            m_CurrentMatch.AlphaWon = AlphaWon;
            m_CurrentMatch.WinType = winType;
            m_CurrentMatch.BaseNumber = BaseNumber;

            if (winType != WinType.NoCount)
            {
                if (AlphaWon) m_AlphaScore++;
                else m_BravoScore++;
            }
            
            m_CurrentMatch.MatchEnded();
            saveMatch();

            for (int i = m_CurrentMatch.AlphaTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_CurrentMatch.AlphaTeam.TeamMembers[i].Active)
                    m_CurrentMatch.AlphaTeam.TeamMembers.Remove(m_CurrentMatch.AlphaTeam.TeamMembers[i]);
                else m_CurrentMatch.AlphaTeam.TeamMembers[i].ResetPlayer();
            }
            for (int i = m_CurrentMatch.BravoTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_CurrentMatch.BravoTeam.TeamMembers[i].Active)
                    m_CurrentMatch.BravoTeam.TeamMembers.Remove(m_CurrentMatch.BravoTeam.TeamMembers[i]);
                else m_CurrentMatch.BravoTeam.TeamMembers[i].ResetPlayer();
            }

            // make sure to send back time
            TotalTime = m_CurrentMatch.TotalTimeFormatted;
        }

        private void saveMatch()
        {
            BaseMatch newMatch = new BaseMatch();
            newMatch.AlphaTeam = m_CurrentMatch.AlphaTeam;
            newMatch.AlphaWon = m_CurrentMatch.AlphaWon;
            newMatch.BaseNumber = m_CurrentMatch.BaseNumber;
            newMatch.BravoTeam = m_CurrentMatch.BravoTeam;
            newMatch.TotalTime = m_CurrentMatch.TotalTime;
            newMatch.WinType = m_CurrentMatch.WinType;
            m_MatchesPlayed.Add(newMatch);
        }

        public void GetGameInfo(string PlayerName, Queue<EventArgs> q)
        {
            q.Enqueue(msg.pm(PlayerName, "Game Info -----------------------------------"));
            q.Enqueue(msg.pm(PlayerName, "GameStatus      :".PadRight(20) + m_Status.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "----------------"));
            q.Enqueue(msg.pm(PlayerName, "Alpha Team Name :".PadRight(20) + m_CurrentMatch.AlphaTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Alpha Freq      :".PadRight(20) + m_AlphaFreq.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Team Count      :".PadRight(20) + m_CurrentMatch.AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Alpha Score     :".PadRight(20) + m_AlphaScore.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Players --"));
            for ( int i = 0; i < m_CurrentMatch.AlphaTeam.TeamMembers.Count; i++)
                q.Enqueue(msg.pm(PlayerName, m_CurrentMatch.AlphaTeam.TeamMembers[i].PlayerName.PadRight(20) + ("Active :" + m_CurrentMatch.AlphaTeam.TeamMembers[i].Active).PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "----------------"));
            q.Enqueue(msg.pm(PlayerName, "Bravo Team Name :".PadRight(20) + m_CurrentMatch.BravoTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Bravo Freq      :".PadRight(20) + m_BravoFreq.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Team Count      :".PadRight(20) + m_CurrentMatch.BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Bravo Score     :".PadRight(20) + m_BravoScore.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Players --"));
            for (int i = 0; i < m_CurrentMatch.BravoTeam.TeamMembers.Count; i++)
                q.Enqueue(msg.pm(PlayerName, m_CurrentMatch.BravoTeam.TeamMembers[i].PlayerName.PadRight(20) + ("Active :" + m_CurrentMatch.BravoTeam.TeamMembers[i].Active).PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "----------------"));
            q.Enqueue(msg.pm(PlayerName, "Allow Safe Win  :".PadRight(20) + m_AllowSafeWin.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Join After Start:".PadRight(20) + m_AllowSafeWin.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Matches stored  :".PadRight(20) + m_MatchesPlayed.Count.ToString().PadLeft(25)));
        }
        public void GetMatchInfo(string PlayerName, Queue<EventArgs> q)
        {
            q.Enqueue(msg.pm(PlayerName, "Match Info ----------------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Matches Stored    :".PadRight(20) + m_MatchesPlayed.Count.ToString().PadLeft(25)));
            for (int i = 0; i < m_MatchesPlayed.Count; i++)
            {
                q.Enqueue(msg.pm(PlayerName, "----------------"));
                if (m_MatchesPlayed[i].WinType != WinType.NoCount)
                    q.Enqueue(msg.pm(PlayerName, "Winner          :".PadRight(20) + (m_MatchesPlayed[i].AlphaWon?"Alpha Team":"Bravo Team").ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Win Type        :".PadRight(20) + m_MatchesPlayed[i].WinType.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Match Time      :".PadRight(20) + m_MatchesPlayed[i].TotalTimeFormatted.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Match Number    :".PadRight(20) + (i + 1).ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Base Number     :".PadRight(20) + m_MatchesPlayed[i].BaseNumber.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Alpha Team Name :".PadRight(20) + m_MatchesPlayed[i].AlphaTeam.TeamName.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Alpha Team Count:".PadRight(20) + m_MatchesPlayed[i].AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Bravo Team Name :".PadRight(20) + m_MatchesPlayed[i].BravoTeam.TeamName.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Bravo Team Count:".PadRight(20) + m_MatchesPlayed[i].BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            }
        }
    }
}
