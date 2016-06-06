using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Needed for timers
using System.Timers;
using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseDuel.Classes
{
    public class BaseGame
    {
        public BaseGame(ShortChat msg, MyGame psyGame, SSPlayerManager Players, BaseManager BaseManager, bool Multi, int GameNum)
        {
            this.msg = msg;
            this.psyGame = psyGame;
            this.m_Players = Players;
            this.m_MultiOn = Multi;
            this.m_BaseManager = BaseManager;
            this.m_Settings = new BDGameSettings();
            this.m_DefaultSettingType = BDSettingType.Normal;
            this.m_GameNum = GameNum;
            this.game_reset();
        }

        private ShortChat msg;
        private MyGame psyGame;
        private SSPlayerManager m_Players;
        private BaseManager m_BaseManager;
        private List<Misc.ArchivedGames> m_Archive;
        private bool m_MultiOn;
        private Base m_Lobby;
        private Base m_CurrentBase;
        private BasePoint m_CurrentPoint;
        private List<BasePoint> m_AllPoints;
        private Misc.BaseGameStatus m_Status;
        private int m_GameNum;
        
        private bool m_AllowSafeWin;
        private ushort m_AlphaFreq, m_BravoFreq;
        private bool m_Locked;
        //private int m_MinimumPoint, m_WinBy;
        private int m_AlphaPoints = 0, m_BravoPoints = 0;

        private Timer m_Timer;
        private double m_StartGameDelay;
        private double m_StartGameShow;
        private double m_StartGameDelay_Elapsed;
        private double m_TeamClearDelay;
        private double m_InactivityReset;

        private BDGameSettings m_Settings;
        private BDSettingType m_DefaultSettingType;
        
        private ChatTypes m_BotSpamSetting;

        public void setArchive(List<Misc.ArchivedGames> ArchivedGames)
        { this.m_Archive = ArchivedGames; }

        public void gameNum(int num)
        { this.m_GameNum = num; }
        public int gameNum()
        { return this.m_GameNum; }

        public bool lockedStatus()
        { return this.m_Locked; }
        public void lockedStatus(bool locked)
        { this.m_Locked = locked; }

        public void setAllowSafeWin(bool Allow)
        { this.m_AllowSafeWin = Allow; }

        public Misc.BaseGameStatus gameStatus()
        { return m_Status; }

        public ushort AlphaFreq
        { get { return m_AlphaFreq; } }
        public ushort BravoFreq
        { get { return m_BravoFreq; } }
        // Set both team freqs
        public void setFreqs(ushort AlphaFreq, ushort BravoFreq)
        {
            this.m_AlphaFreq = AlphaFreq;
            this.m_BravoFreq = BravoFreq;
            this.m_Lobby = this.m_BaseManager.Lobby;
            this.loadNextBase();
        }

        // get Current Base - need access to base for a reset in BaseDuel
        public Base loadedBase()
        { return this.m_CurrentBase; }

        //----------------------------------------------------------------------//
        //                       Commands                                       //
        //----------------------------------------------------------------------//
        public void command_Start(SSPlayer p)
        {
            if (this.m_Timer.Enabled || this.m_Status != Misc.BaseGameStatus.NotStarted)
            {
                psyGame.Send(msg.pm(p.PlayerName, "The game has already been started."));
                return;
            }

            // Get freq counts
            int aCount = this.m_Players.PlayerList.FindAll(item => item.Frequency == this.m_AlphaFreq && item.Ship != ShipTypes.Spectator).Count;
            int bCount = this.m_Players.PlayerList.FindAll(item => item.Frequency == this.m_BravoFreq && item.Ship != ShipTypes.Spectator).Count;
            
            if (aCount > 0 && bCount > 0)
            {
                this.freqDump(this.m_AlphaFreq, this.m_CurrentPoint.AlphaTeam(),true);
                this.freqDump(this.m_BravoFreq, this.m_CurrentPoint.BravoTeam(), false);

                psyGame.Send(msg.arena("[ BaseDuel ] A game will begin in " + this.m_StartGameDelay + " seconds. [ " + this.m_CurrentPoint.AlphaCount() + " vs " + this.m_CurrentPoint.BravoCount() + " ]" + (this.m_MultiOn ? "      - Game: [ " + this.m_GameNum + " ] -" : ".")));
                psyGame.Send(msg.team_pm(this.m_AlphaFreq, "Welcome to Team: " + this.m_CurrentPoint.AlphaTeam().teamName() + ". Good Luck!"));
                psyGame.Send(msg.team_pm(this.m_BravoFreq, "Welcome to Team: " + this.m_CurrentPoint.BravoTeam().teamName() + ". Good Luck!"));

                this.timer_Setup(Misc.TimerType.GameStart);
                return;
            }

            // Need one player on opfor
            psyGame.Send(msg.pm(p.PlayerName, "You need to have at least 1 player on each team to start. Please find an opponent and have them join Freq= " + (aCount == 0 ? this.m_AlphaFreq : this.m_BravoFreq) + "."));
        }

        public void command_Hold(SSPlayer p)
        {
            if (this.m_Status == Misc.BaseGameStatus.OnHold)
            {
                sendBotSpam("- Restarting point in " + m_StartGameDelay + " seconds! -");
                this.m_CurrentPoint.resetPoint();
                timer_Setup(Misc.TimerType.GameStart); 
                return;
            }

            if (this.m_Status != Misc.BaseGameStatus.InProgress || m_Timer.Enabled)
            {
                psyGame.Send(msg.pm(p.PlayerName, "This command can only be used when the game is in progress and no [Team Out] events have been triggered."));
                return;
            }

            this.m_Status = Misc.BaseGameStatus.OnHold;

            psyGame.Send(msg.team_pm(this.m_AlphaFreq, "Your game has currently been put on hold. We will resume the game shortly - " + p.PlayerName));
            psyGame.Send(msg.team_pm(this.m_BravoFreq, "Your game has currently been put on hold. We will resume the game shortly - " + p.PlayerName));

            psyGame.Send(msg.team_pm(this.m_AlphaFreq, "?|prize warp|prize fullcharge"));
            psyGame.Send(msg.team_pm(this.m_BravoFreq, "?|prize warp|prize fullcharge"));
        }

        public void command_ShuffleTeams( SSPlayer p, List<string> BlockedList)
        {
            if (this.gameStatus() == Misc.BaseGameStatus.NotStarted)
            {
                List<SSPlayer> all = m_Players.PlayerList.FindAll(
                    item => ((item.Frequency == this.m_AlphaFreq && item.Ship != ShipTypes.Spectator) 
                        || (item.Frequency == this.m_BravoFreq && item.Ship != ShipTypes.Spectator)) 
                        && !BlockedList.Contains(item.PlayerName));

                // Shuffle the list we just created
                Random ran = new Random();
                for (int h = 0; h < 10; h++)
                {
                    for (int i = 0; i < all.Count; i++)
                    {
                        SSPlayer temp = all[i];
                        int r = ran.Next(i, all.Count);
                        all[i] = all[r];
                        all[r] = temp;
                    }
                }

                bool alternate = false;

                // Only setfreq to players that need to change
                while (all.Count > 0)
                {
                    alternate = !alternate;

                    if (all[0].Frequency != (alternate ? this.m_AlphaFreq : this.m_BravoFreq))
                        psyGame.Send(msg.pm(all[0].PlayerName, "?|setfreq " + (alternate ? this.m_AlphaFreq : this.m_BravoFreq) + "|shipreset"));
                    all.RemoveAt(0);
                }
            }
            else
            {
                psyGame.Send(msg.pm(p.PlayerName, "This command can only be used before a game. If you wish to reshuffle you must end current game and reset."));
                return;
            }
        }

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        private void game_Save()
        {
            List<BasePoint> save = new List<BasePoint>();

            while (this.m_AllPoints.Count > 0)
            {
                save.Add(this.m_AllPoints[0]);
                this.m_AllPoints.RemoveAt(0);
            }

            Misc.ArchivedGames savedGame = new Misc.ArchivedGames(save);

            this.m_Archive.Add(savedGame);
        }
        private void game_reset()
        {
            this.m_CurrentPoint = new BasePoint();
            this.m_AllPoints = new List<BasePoint>();
            this.m_Status = Misc.BaseGameStatus.NotStarted;

            this.m_Timer = new Timer();
            this.m_StartGameDelay = 10;
            this.m_StartGameShow = 5;
            this.m_TeamClearDelay = 5;
            this.m_InactivityReset = 30;
            this.m_BotSpamSetting = ChatTypes.Team;
            this.m_AlphaPoints = 0;
            this.m_BravoPoints = 0;

            this.m_Settings.LoadSettings(this.m_DefaultSettingType);
        }
        private void game_Start()
        {
            this.m_Status = Misc.BaseGameStatus.InProgress;
            psyGame.Send(msg.team_pm(this.m_AlphaFreq, "?warpto " + this.m_CurrentBase.AlphaStartX + " " + this.m_CurrentBase.AlphaStartY + "|shipreset"));
            psyGame.Send(msg.team_pm(this.m_BravoFreq, "?warpto " + this.m_CurrentBase.BravoStartX + " " + this.m_CurrentBase.BravoStartY + "|shipreset"));

            sendBotSpam("- Current Base        : " + this.m_CurrentBase.Number.ToString().PadLeft(2,'0'));
            if (this.m_AllPoints.Count == 0)
            {
                sendBotSpam("- Team vs Team Rules  : " + (this.m_AllowSafeWin ? "BaseClear and Safes" : "BaseClear"));
                sendBotSpam("- Auto Scoring        :  - On -");
            }

            this.m_CurrentPoint.startPoint();
        }

        private void point_AwardWinner(Misc.WinType winType, bool AlphaWon, string PlayerName)
        {
            this.m_Status = Misc.BaseGameStatus.BetweenPoints;

            if (winType != Misc.WinType.NoCount)
            {
                if (AlphaWon) { this.m_AlphaPoints++; }
                else { this.m_BravoPoints++; }

                if (winType == Misc.WinType.BaseClear)
                {
                    sendBotSpam(("- " + (!AlphaWon ? this.m_CurrentPoint.AlphaTeam().teamName() : this.m_CurrentPoint.BravoTeam().teamName()) + " is all dead! --  Team " + (AlphaWon ? this.m_CurrentPoint.AlphaTeam().teamName() : this.m_CurrentPoint.BravoTeam().teamName()) + " scores! -"));
                }
                else if (winType == Misc.WinType.SafeWin)
                {
                    this.m_CurrentPoint.setSafeWinner(PlayerName);
                    sendBotSpam("- [ " + PlayerName + " ] has breached the enemy's defense! (SafeWin) " + (AlphaWon ? "[ " + this.m_CurrentPoint.AlphaTeam().teamName() + " Team Win ]" : "[ " + this.m_CurrentPoint.BravoTeam().teamName() + " Team Win ]"));
                }
                // display score
                sendBotSpam("- " + this.m_CurrentPoint.AlphaTeam().teamName() + " [ " + this.m_AlphaPoints.ToString() + " ]     Score     [ " + this.m_BravoPoints.ToString() + " ] " + this.m_CurrentPoint.BravoTeam().teamName() + " -");
                sendBotSpam("- ----------------------------------------------- -");
            }
            else
            {
                sendBotSpam("- Both teams out -- No count -");
            }

            this.m_AllPoints.Add(this.m_CurrentPoint.getSavedPoint(AlphaWon, winType));
            this.m_CurrentPoint.resetPoint();

            if ((this.m_AlphaPoints >= this.m_Settings.MinimumPoints || this.m_BravoPoints >= this.m_Settings.MinimumPoints)
                && Math.Abs(this.m_AlphaPoints - this.m_BravoPoints) >= this.m_Settings.WinBy)
            {
                string AlphaName = this.m_CurrentPoint.AlphaTeam().teamName();
                string BravoName = this.m_CurrentPoint.BravoTeam().teamName();
                int AlphaScore = this.m_AlphaPoints;
                int BravoScore = this.m_BravoPoints;

                this.game_Save();
                this.game_reset();

                psyGame.Send(msg.team_pm(this.m_AlphaFreq, "?prize warp"));
                psyGame.Send(msg.team_pm(this.m_BravoFreq, "?prize warp"));

                psyGame.Send(msg.arena("Final Score:   " + AlphaName + "[ " + AlphaScore + " ]  -  [ " + BravoScore + " ]" + BravoName + "     final print out here."));
                return;
            }

            if (winType != Misc.WinType.NoCount)
            {
                this.loadNextBase();
                sendBotSpam("- Starting next base in " + m_StartGameDelay + " seconds! -");
            }
            else
            { sendBotSpam("- Restarting point in " + m_StartGameDelay + " seconds! -"); }

            timer_Setup(Misc.TimerType.GameStart); 
        }

        private void player_TeamCountUpdate()
        {
            // Check for team count 0 here ---***

            if ( this.m_Settings.Type != BDSettingType.OneVsOne &&
                this.m_CurrentPoint.AlphaCount() == 1 &&
                    this.m_CurrentPoint.BravoCount() == 1)
            {
                this.m_Settings.LoadSettings(BDSettingType.OneVsOne);
                psyGame.Send(msg.debugChan("[ BaseDuel ] [ Game " + this.m_GameNum.ToString().PadLeft(2, '0') + " ]  Setting Changed to [ "+this.m_Settings.Type+" ]"));
            }
            else if ( this.m_Settings.Type == BDSettingType.OneVsOne &&
                this.m_CurrentPoint.AlphaCount() > 1 ||
                    this.m_CurrentPoint.BravoCount() > 1)
            {
                this.m_Settings.LoadSettings(this.m_DefaultSettingType);
                psyGame.Send(msg.debugChan("[ BaseDuel ] [ Game " + this.m_GameNum.ToString().PadLeft(2, '0') + " ]  Setting Changed to [ "+this.m_DefaultSettingType+" ]"));
            }
        }

        public void player_Remove(SSPlayer p)
        {
            this.m_CurrentPoint.removePlayer(p.PlayerName);
            this.player_TeamCountUpdate();
        }

        public void player_Join(SSPlayer p)
        {
            bool InAlpha;
            BasePlayer b = this.m_CurrentPoint.getPlayer(p.PlayerName, out InAlpha);

            // player is in right freq and is already part of team
            if (b != null && (InAlpha ? this.m_AlphaFreq : this.m_BravoFreq) == p.Frequency) return;

            if (this.m_Status != Misc.BaseGameStatus.NotStarted && this.m_Locked)
            {
                psyGame.Send(msg.arena("PlayerJoin ignored."));
                return;
            }

            if (p.Frequency == m_AlphaFreq)
            { 
                this.m_CurrentPoint.AlphaTeam().playerJoin(p.PlayerName); 
            }
            else if (p.Frequency == m_BravoFreq)
            { 
                this.m_CurrentPoint.BravoTeam().playerJoin(p.PlayerName);
            }
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void Event_TurretEvent(SSPlayer a, SSPlayer h)
        {
            bool InAlpha;
            BasePlayer attacher = this.m_CurrentPoint.getPlayer(a.PlayerName, out InAlpha);
            BasePlayer host = this.m_CurrentPoint.getPlayer(h.PlayerName, out InAlpha);
            bool InLobby = this.player_InRegion(h.Position, this.m_Lobby.BaseDimension);
            host.inLobby(InLobby);

            if (attacher == null)
            {
                psyGame.Send(msg.pm(a.PlayerName, "?|setship 9|setship " + ((int)a.Ship + 1) + "|"));
                return;
            }
            this.player_TeamCountUpdate();
            attacher.inLobby(InLobby);

            if (this.m_Status == Misc.BaseGameStatus.InProgress)
            {
                if (InLobby) allOutCheck();
            }
        }

        // Player position gets sent if InSafe
        public void Event_PlayerPosition(SSPlayer p)
        {
            bool InLobby = this.player_InRegion(p.Position, this.m_Lobby.BaseDimension);
            bool InAlpha;
            BasePlayer b = this.m_CurrentPoint.getPlayer(p.PlayerName, out InAlpha);
            
            // add player here maybe if == null
            if (b == null)
            {
                if (player_InRegion(p.Position, this.m_CurrentBase.BaseDimension))
                {
                    psyGame.Send(msg.pm(p.PlayerName, "?prize warp"));
                }
                return;
            }

            if (this.m_Status == Misc.BaseGameStatus.InProgress)
            {
                b.inLobby(InLobby);

                if (InLobby)
                { allOutCheck(); }
                else
                {
                    if ( this.m_Settings.SafeWin && this.player_InRegion(p.Position, (InAlpha ? this.m_CurrentBase.BravoSafe : this.m_CurrentBase.AlphaSafe)))
                    {
                        this.point_AwardWinner(Misc.WinType.SafeWin, InAlpha, p.PlayerName);
                    }
                }
            }
        }

        //----------------------------------------------------------------------//
        //                       Timer Stuff                                    //
        //----------------------------------------------------------------------//
        private void timer_Setup(Misc.TimerType t)
        {
            m_Timer = new Timer();

            // Set the timer for what we need it to do: Start or Clear Check
            if (t == Misc.TimerType.GameStart)
            {
                m_StartGameDelay_Elapsed = m_StartGameDelay;
                m_Timer.Elapsed += new ElapsedEventHandler(timer_StartGame);
                m_Timer.Interval = 1000;
            }
            else if (t == Misc.TimerType.BaseClear)
            {
                psyGame.Send(msg.debugChan("Starting Clear Timer"));
                m_Timer.Elapsed += new ElapsedEventHandler(timer_BaseClear);
                m_Timer.Interval = m_TeamClearDelay * 1000;
            }
            else if (t == Misc.TimerType.InactiveReset)
            {
                psyGame.Send(msg.debugChan("Starting Inactive Timer"));
                m_Timer.Elapsed += new ElapsedEventHandler(timer_InactiveTimer);
                m_Timer.Interval = m_InactivityReset * 1000;
            }

            m_Timer.Start();
        }
        private void timer_BaseClear(object source, ElapsedEventArgs e)
        {
            psyGame.Send(msg.debugChan("- All out timer expired. -"));
            m_Timer.Stop();

            bool aOut = this.m_CurrentPoint.AlphaTeam().teamAllOut();
            bool bOut = this.m_CurrentPoint.BravoTeam().teamAllOut();

            if (aOut || bOut)
            {
                this.point_AwardWinner(aOut == bOut?Misc.WinType.NoCount:Misc.WinType.BaseClear, !aOut, "");
            }
        }

        private void timer_StartGame(object source, ElapsedEventArgs e)
        {
            m_StartGameDelay_Elapsed--;

            if (m_StartGameDelay_Elapsed <= m_StartGameShow)
            {
                if (m_StartGameDelay_Elapsed != 0)
                {
                    psyGame.Send(msg.team_pm(this.m_AlphaFreq, "?|objon " + m_StartGameDelay_Elapsed));
                    psyGame.Send(msg.team_pm(this.m_BravoFreq, "?|objon " + m_StartGameDelay_Elapsed));
                    psyGame.Send(msg.arena( "", SoundCodes.MessageAlarm));
                }
                else
                {
                    psyGame.Send(msg.team_pm(this.m_AlphaFreq, "*objon " + m_StartGameDelay_Elapsed));
                    psyGame.Send(msg.team_pm(this.m_BravoFreq, "*objon " + m_StartGameDelay_Elapsed));
                    psyGame.Send(msg.arena("", SoundCodes.Goal));
                }
            }

            if (m_StartGameDelay_Elapsed > 0) return;

            m_Timer.Stop();

            // Make sure there is enough ppl in team to start
            if (this.m_CurrentPoint.AlphaCount() > 0 && this.m_CurrentPoint.BravoCount() > 0)
            {
                player_TeamCountUpdate();
                this.game_Start();
                return;
            }

            // reset Teams
            this.m_CurrentPoint.AlphaTeam().resetTeam();
            this.m_CurrentPoint.BravoTeam().resetTeam();
            sendBotSpam("- Not enough players. Start Game cancelled. -");   
        }
        private void timer_InactiveTimer(object source, ElapsedEventArgs e)
        {
            //int aCount = m_BaseGame.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).Count;
            //int bCount = m_BaseGame.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).Count;

            //if (aCount == 0 || bCount == 0)
            //{
            //    psyGame.Send(msg.arena("Game has been reset due to team inactivity.", SoundCodes.BassBeep));
            //    game_Reset();
            //}
            psyGame.Send(msg.arena("Timer stopped- Inactive.", SoundCodes.BassBeep));
            m_Timer.Stop();
        }

        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        public bool playerInGame(SSPlayer p, out Classes.BasePlayer basePlayer, out bool InAlpha)
        {
            basePlayer = m_CurrentPoint.getPlayer(p.PlayerName, out InAlpha);

            if (basePlayer == null)
            { return false; }

            return true;
        }
        private void allOutCheck()
        {
            if (!this.m_Timer.Enabled && (this.m_CurrentPoint.AlphaTeam().teamAllOut() || this.m_CurrentPoint.BravoTeam().teamAllOut()))
            {
                this.timer_Setup(Misc.TimerType.BaseClear);
            }
        }
        // Add everyone from the freq into corresponding team list
        private void freqDump(ushort Freq, BaseTeam team, bool IsAlpha)
        {
            // Grab names from freq
            List<SSPlayer> pList = this.m_Players.PlayerList.FindAll(item => item.Frequency == Freq);

            // Toss freqs into teams
            while (pList.Count > 0)
            {
                if (team.teamCount() == 0)
                    team.teamName(pList[0].SquadName == "~ no squad ~" ? (IsAlpha?"Alpha":"Bravo") : pList[0].SquadName);

                team.playerJoin(pList[0].PlayerName);
                pList.RemoveAt(0);
            }

            if (IsAlpha) return;

            if (this.m_CurrentPoint.AlphaTeam().teamName() == this.m_CurrentPoint.BravoTeam().teamName())
            {
                this.m_CurrentPoint.AlphaTeam().teamName("Alpha");
                this.m_CurrentPoint.BravoTeam().teamName("Bravo");
            }
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

            psyGame.Send(msg.team_pm(this.m_AlphaFreq, Message));
            psyGame.Send(msg.team_pm(this.m_BravoFreq, Message));
        }

        // Simple collision check
        private bool player_InRegion(PlayerPositionEvent p, ushort[] region)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        // must turn in old base before you can get a new one - basemanager rules no exceptions
        private void loadNextBase()
        {
            m_BaseManager.ReleaseBase(this.m_CurrentBase, "BaseDuel");
            this.m_CurrentBase = m_BaseManager.getNextBase("BaseDuel");
            this.m_CurrentPoint.setBaseNumber(this.m_CurrentBase.Number);
        }

        //----------------------------------------------------------------------//
        //                           PrintOuts                                  //
        //----------------------------------------------------------------------//
        public void getGameInfo(SSPlayer p)
        {
            int rightOffset = 35;
            int leftOffset = 20;
            psyGame.Send(msg.pm(p.PlayerName, "Status        :".PadRight(leftOffset) + (this.gameStatus().ToString().PadLeft(2, '0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Setting Type  :".PadRight(leftOffset) + (this.m_Settings.Type.ToString().PadLeft(2, '0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Alpha Freq    :".PadRight(leftOffset) + ("Freq: " + this.AlphaFreq.ToString().PadLeft(4,'0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Team Count    :".PadRight(leftOffset) + this.m_CurrentPoint.AlphaCount().ToString().PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Bravo Freq    :".PadRight(leftOffset) + ("Freq: " + this.BravoFreq.ToString().PadLeft(4, '0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Team Count    :".PadRight(leftOffset) + this.m_CurrentPoint.BravoCount().ToString().PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Minimum Point :".PadRight(leftOffset) + (this.m_Settings.MinimumPoints.ToString().PadLeft(2, '0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Win By        :".PadRight(leftOffset) + (this.m_Settings.WinBy.ToString().PadLeft(2, '0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Safe Win      :".PadRight(leftOffset) + ((this.m_AllowSafeWin?"- On -":"- Off -").PadLeft(2, '0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Locked        :".PadRight(leftOffset) + ((this.m_Locked ? "Locked" : "Unlocked").PadLeft(2, '0')).PadLeft(rightOffset)));
            this.getRoundInfo(p);
        }
        public void getRoundInfo(SSPlayer p)
        {
            for (int i = 0; i < this.m_AllPoints.Count; i++)
            {
                Queue<string> reply = this.m_AllPoints[i].getPointInfo();

                psyGame.Send(msg.pm(p.PlayerName, "=---------------------------------------------="));
                psyGame.Send(msg.pm(p.PlayerName, "Point/Round [ "+(i + 1).ToString()+" ]"));
                psyGame.Send(msg.pm(p.PlayerName, "=---------------------------------------------="));

                while (reply.Count > 0)
                    psyGame.Send(msg.pm(p.PlayerName, reply.Dequeue()));
            }
        }
    }
}
