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
    }
}
