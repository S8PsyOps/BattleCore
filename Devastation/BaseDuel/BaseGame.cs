using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseDuel
{
    class BaseGame
    {
        public BaseGame()
        {
            this.m_Round = new BaseRound();
            this.m_RoundsPlayed = new List<BaseRound>();
            this.m_Status = BaseGameStatus.GameIdle;
            this.m_AllowSafeWin = true;
            this.m_AllowAfterStartJoin = true;
            this.m_AlphaScore = 0;
            this.m_BravoScore = 0;
            this.m_MinimumWin = 5;
            this.m_WinBy = 2;
        }

        public BaseGame(ShortChat msg, MyGame psyGame, ChatTypes BotSpamSetting)
        {
            this.msg = msg;
            this.psyGame = psyGame;
            this.m_Round = new BaseRound();
            this.m_RoundsPlayed = new List<BaseRound>();
            this.m_Status = BaseGameStatus.GameIdle;
            this.m_AllowSafeWin = true;
            this.m_AllowAfterStartJoin = true;
            this.m_AlphaScore = 0;
            this.m_BravoScore = 0;
            this.m_MinimumWin = 5;
            this.m_WinBy = 2;
            this.m_BotSpamSetting = BotSpamSetting;
        }
        
        ShortChat msg;
        MyGame psyGame;
        private BaseRound m_Round;
        private List<BaseRound> m_RoundsPlayed;
        private BaseGameStatus m_Status;
        private ushort m_AlphaFreq, m_BravoFreq, m_AlphaStartFreq, m_BravoStartFreq;
        private bool m_AllowSafeWin;
        private bool m_AllowAfterStartJoin;
        private int m_AlphaScore;
        private int m_BravoScore;
        private int m_MinimumWin;
        private int m_WinBy;
        private ChatTypes m_BotSpamSetting;
        private Base m_Base;

        public Base Base
        { get { return m_Base; } }

        /// <summary>
        /// Current match being played
        /// </summary>
        public BaseRound Round
        {
            get { return m_Round; }
            set { m_Round = value; }
        }

        /// <summary>
        /// List of all played matches in the current game.
        /// </summary>
        public List<BaseRound> AllRounds
        {
            get { return m_RoundsPlayed; }
            set { m_RoundsPlayed = value; }
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
        /// Alpha Team's Start Freq
        /// </summary>
        public ushort AlphaStartFreq
        {
            get { return m_AlphaStartFreq; }
            set { m_AlphaStartFreq = value; }
        }
        /// <summary>
        /// Bravo Team's Start Freq
        /// </summary>
        public ushort BravoStartFreq
        {
            get { return m_BravoStartFreq; }
            set { m_BravoStartFreq = value; }
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

        /// <summary>
        /// Minimum amount of points for a team before they can win
        /// </summary>
        public int MinimumWin
        {
            get { return m_MinimumWin; }
            set { m_MinimumWin = value; }
        }

        /// <summary>
        /// The amount of points you have to win by.
        /// </summary>
        public int WinBy
        {
            get { return m_WinBy; }
            set { m_WinBy = value; }
        }

        public void loadBase(Base mybase)
        {
            this.m_Base = mybase;
            this.m_Round.BaseNumber = (short)mybase.Number;
        }

        public void removePlayer(SSPlayer p)
        {
            if (this.getStatus() != BaseGameStatus.GameIdle)
            {
                bool InAlpha;
                BasePlayer b = this.getPlayer(p,out InAlpha);
                if (b != null)
                {
                    b.setActive(false);
                }

                if (this.m_Round.getTeam(p).getActiveCount() <= 0)
                {
                    psyGame.Send(msg.arena("Team is empty."));
                }
            }
        }

        public BaseGameStatus getStatus()
        {
            return this.m_Status;
        }

        public void setStatus(BaseGameStatus status)
        {
            this.m_Status = status;
        }

        public BasePlayer getPlayer(SSPlayer p, out bool InAlpha)
        {
            return this.m_Round.getPlayer(p, out InAlpha);
        }

        public void resetPlayers()
        {
            this.m_Round.flushTeams();
        }

        public void saveRound()
        {
            this.m_RoundsPlayed.Add(this.m_Round.getSavedRound());
        }

        public void restartRound(ChatEvent e)
        {
            if (this.getStatus() == BaseGameStatus.GameOn)
            {
                //send players to center
                psyGame.Send(msg.team_pm(this.m_AlphaFreq, "?prize warp"));
                psyGame.Send(msg.team_pm(this.m_BravoFreq, "?prize warp"));
            }

            this.setStatus(BaseGameStatus.GameIntermission);
            this.resetPlayers();
        }

        public bool teamIsOut(out bool Alpha, out bool Bravo)
        {
            Alpha = this.m_Round.AlphaTeam.teamIsOut();
            Bravo = this.m_Round.BravoTeam.teamIsOut();
            if (Alpha || Bravo) return true;
            return false;
        }

        public void startGame()
        {
            if (this.getStatus() == BaseGameStatus.GameIntermission)
            {
                this.resetPlayers();
            }
            // send players to start and reset ships
            psyGame.Send(msg.team_pm(m_AlphaFreq, "?|warpto " + m_Base.AlphaStartX + " " + m_Base.AlphaStartY + "|shipreset"));
            psyGame.Send(msg.team_pm(m_BravoFreq, "?|warpto " + m_Base.BravoStartX + " " + m_Base.BravoStartY + "|shipreset"));
            
            sendBotSpam("- Current Base        : " + m_Base.Number);
            
            if (this.m_RoundsPlayed.Count == 0)
            {
                sendBotSpam("- Team vs Team Rules  : " + (this.m_AllowSafeWin ? "BaseClear and Safes" : "BaseClear"));
                sendBotSpam("- Auto Scoring        : Not implemented yet.");
            }

            // Record start time
            this.m_Round.StartTime = DateTime.Now;
            // set status
            this.setStatus(BaseGameStatus.GameOn);
            psyGame.SafeSend(msg.debugChan("GameStatus change to GameOn."));
        }

        public BaseGame getSavedGame()
        {
            BaseGame game = new BaseGame();
            game.AllowAfterStartJoin = this.m_AllowAfterStartJoin;
            game.AllowSafeWin = this.m_AllowSafeWin;
            game.AllRounds = this.m_RoundsPlayed;
            game.AlphaFreq = this.m_AlphaFreq;
            game.AlphaScore = this.m_AlphaScore;
            game.BravoFreq = this.m_BravoFreq;
            game.BravoScore = this.m_BravoScore;
            game.MinimumWin = this.m_MinimumWin;
            game.Round = this.m_Round;
            game.setStatus(this.getStatus());
            game.WinBy = this.m_WinBy;
            return game;
        }

        // send printouts according to setting
        private void sendBotSpam(string Message)
        {
            ChatEvent spam = new ChatEvent();
            spam.Message = Message;

            if (m_BotSpamSetting != ChatTypes.Team)
            {
                spam.ChatType = m_BotSpamSetting;
                psyGame.Send(spam);
                return;
            }

            psyGame.Send(msg.team_pm(this.m_AlphaStartFreq, Message));
            psyGame.Send(msg.team_pm(this.m_BravoStartFreq, Message));
            psyGame.Send(msg.team_pm(this.m_AlphaFreq, Message));
            psyGame.Send(msg.team_pm(m_BravoFreq, Message));
        }
    }
}
