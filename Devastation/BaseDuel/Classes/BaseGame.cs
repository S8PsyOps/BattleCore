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
            this.m_InactiveTimerActivated = true;
            this.m_InactiveTimeLimit = 5;
            this.m_BaseSize = BaseSize.Off;
            this.game_Reset();

            this.m_ATeamName = "Alpha";
            this.m_BTeamName = "Bravo";
            this.m_DefaultATeamName = "Alpha";
            this.m_DefaultBTeamName = "Bravo";
        }

        private ShortChat msg;
        private MyGame psyGame;
        private SSPlayerManager m_Players;
        private BaseManager m_BaseManager;
        private List<string> m_SpamMeList;
        private List<Misc.ArchivedGames> m_Archive;
        private bool m_MultiOn;
        private Base m_Lobby;
        private Base m_CurrentBase;
        private BaseSize m_BaseSize;
        private BasePoint m_CurrentPoint;
        private List<BasePoint> m_AllPoints;
        private Misc.BaseGameStatus m_Status;
        private int m_GameNum;
        
        private ushort m_AlphaFreq, m_BravoFreq;
        private string m_ATeamName, m_BTeamName;
        private string m_DefaultATeamName, m_DefaultBTeamName;
        private int m_AlphaPoints = 0, m_BravoPoints = 0;
        private bool m_Locked;

        private Timer m_Timer;
        
        private Timer m_InactiveTimer;
        private DateTime m_ActivityTimeStamp;
        private bool m_InactiveTimerActivated;
        private double m_InactiveTimeLimit;
        
        private double m_StartGameDelay;
        private double m_StartGameShow;
        private double m_StartGameDelay_Elapsed;
        private double m_TeamClearDelay;

        private BDGameSettings m_Settings;
        private BDSettingType m_DefaultSettingType;

        private string m_GameTag
        { get { return "[ Game "+this.m_GameNum.ToString().PadLeft(2)+" ] "; } }

        public void setSpamMeList(List<string> SpamMeList)
        { this.m_SpamMeList = SpamMeList; }

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

        public Misc.BaseGameStatus gameStatus()
        { return m_Status; }

        public ushort AlphaFreq
        { get { return m_AlphaFreq; } }
        public ushort BravoFreq
        { get { return m_BravoFreq; } }

        public int AlphaScore
        { get { return this.m_AlphaPoints; } }
        public int BravoScore
        { get { return this.m_BravoPoints; } }

        public string AlphaName
        { get { return this.m_ATeamName; } }
        public string BravoName
        { get { return this.m_BTeamName; } }
        
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
        public void command_GameStart(SSPlayer p)
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

                psyGame.Send(msg.arena("[ BaseDuel ] A game will begin in " + this.m_StartGameDelay + " seconds. [ " + this.m_CurrentPoint.AlphaCount() + " vs " + this.m_CurrentPoint.BravoCount() + " ]" + (this.m_MultiOn ? "      - "+this.m_GameTag+" -" : ".")));
                psyGame.Send(msg.team_pm(this.m_AlphaFreq, "Welcome to Team: " + this.m_ATeamName + ". Good Luck!"));
                psyGame.Send(msg.team_pm(this.m_BravoFreq, "Welcome to Team: " + this.m_BTeamName + ". Good Luck!"));

                this.timer_Setup(Misc.TimerType.GameStart);
                return;
            }

            // Need one player on opfor
            psyGame.Send(msg.pm(p.PlayerName, "You need to have at least 1 player on each team to start. Please find an opponent and have them join Freq= " + (aCount == 0 ? this.m_AlphaFreq : this.m_BravoFreq) + "."));
        }

        public void command_GameLock(SSPlayer p)
        {
            this.m_Locked = !this.m_Locked;
            string message2send = "Game has been [ "+(this.m_Locked?"Locked":"Unlocked")+" ] "+(this.m_Locked?"Players may not join game after GameStart.":"Players are free to join after GameStart.")+" - " + p.PlayerName;
            psyGame.Send(msg.pm(p.PlayerName, "You have " + (this.m_Locked ? "Locked" : "Unlocked") + " " + this.m_GameTag + " ." + (this.m_Locked ? "Players may not join game after GameStart." : "Players are free to join after GameStart.")));
            this.sendAllTeamMessage(message2send);
        }

        public void command_GameHold(SSPlayer p)
        {
            if (this.m_Status == Misc.BaseGameStatus.OnHold)
            {
                this.point_Reset();
                return;
            }

            if (this.m_Status != Misc.BaseGameStatus.InProgress || m_Timer.Enabled)
            {
                psyGame.Send(msg.pm(p.PlayerName, "This command can only be used when the game is in progress and no [Team Out] events have been triggered."));
                return;
            }

            this.m_Status = Misc.BaseGameStatus.OnHold;

            this.sendAllTeamMessage("Your game has currently been put on hold. We will resume the game shortly - " + p.PlayerName);
            this.players_WarptoCenter();
        }

        public void command_GameReset(SSPlayer p)
        {
            this.game_Reset();
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

        public void command_GetPlayerStats(SSPlayer p)
        {
            bool InAlpha;
            BasePlayer player = this.m_CurrentPoint.getPlayer(p.PlayerName, out InAlpha);

            psyGame.Send(msg.pm(p.PlayerName, "Kills[ " + player.Stats.Kills + " ]   Deaths[ " + player.Stats.Deaths + " ]"));
        }

        public void command_PointReset(SSPlayer p)
        {
            this.sendAllTeamMessage("- Point has been reset. - " + p.PlayerName);
            this.players_WarptoCenter();
            this.point_Reset();
        }

        public void command_SettingChange(SSPlayer p, ChatEvent e)
        {
            string fullcommand = e.Message.Split(' ')[1];
            string command = fullcommand.Split(':')[0];

            switch (command)
            {
                case "score":
                    command_ChangeScore(p,fullcommand);
                    return;
                case "size":
                    command_ChangeSize(p, fullcommand);
                    return;
            }

            psyGame.Send(msg.arena("Command Handled. Setting[ "+ e.Message.Split(' ')[1] + " ]    " + m_GameTag));
        }
        private void command_ChangeSize(SSPlayer p, string command)
        {
        }
        private void command_ChangeScore(SSPlayer p, string command)
        {
            if (this.m_Status != Misc.BaseGameStatus.OnHold)
            {
                psyGame.Send(msg.pm(p.PlayerName,"This command can only be used when the game is on Hold. To get a game into hold status, type  [ .bd hold ]  while the game is in progress."));
                return;
            }

            int aScore, bScore;
            string[] data = command.Split(':');

            if (data.Length == 3 && int.TryParse(data[1], out aScore) && int.TryParse(data[2], out bScore))
            {
                this.m_AlphaPoints = aScore;
                this.m_BravoPoints = bScore;
                string message = "Scores have been changed. New Scores:   " + this.m_ATeamName + " " + this.m_AlphaPoints.ToString() + " - " + this.m_BravoPoints.ToString() + " " + this.m_BTeamName + "   - ChangedBy: " + p.PlayerName;
                this.sendPlayerSpam(message);
                this.sendAllTeamMessage(message);
                return;
            }

            psyGame.Send(msg.pm(p.PlayerName, "The command is not in the proper format. Syntax for command is:  .bdset score:1:2    - where Alpha will have a score of 1 ,and Bravo a score of 2."));
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

        private void game_Reset()
        {
            this.m_ATeamName = m_DefaultATeamName;
            this.m_BTeamName = m_DefaultBTeamName;
            this.m_CurrentPoint = new BasePoint(this.m_ATeamName,this.m_BTeamName);
            this.m_AllPoints = new List<BasePoint>();
            this.m_Status = Misc.BaseGameStatus.NotStarted;

            this.m_Timer = new Timer();
            this.m_InactiveTimer = new Timer();
            this.m_StartGameDelay = 10;
            this.m_StartGameShow = 5;
            this.m_TeamClearDelay = 5;
            this.m_AlphaPoints = 0;
            this.m_BravoPoints = 0;

            this.m_Settings.LoadSettings(this.m_DefaultSettingType);
            this.players_WarptoCenter();
        }

        private void game_Start()
        {
            this.m_Status = Misc.BaseGameStatus.InProgress;
            psyGame.Send(msg.team_pm(this.m_AlphaFreq, "?|warpto " + this.m_CurrentBase.AlphaStartX + " " + this.m_CurrentBase.AlphaStartY + "|shipreset"));
            psyGame.Send(msg.team_pm(this.m_BravoFreq, "?|warpto " + this.m_CurrentBase.BravoStartX + " " + this.m_CurrentBase.BravoStartY + "|shipreset"));

            sendBotSpam("- Current Base        : " + this.m_CurrentBase.Number.ToString().PadLeft(2,'0'));
            sendBotSpam("- Current Base Size   : " + this.m_CurrentBase.Size.ToString());
            if (this.m_AllPoints.Count == 0)
            {
                sendBotSpam("- Team vs Team Rules  : " + (this.m_Settings.SafeWin ? "BaseClear and Safes" : "BaseClear"));
                sendBotSpam("- Auto Scoring        :  - On -");
            }

            if (this.m_InactiveTimerActivated)
            {
                this.m_InactiveTimer = new Timer();
                this.m_InactiveTimer.Elapsed += new ElapsedEventHandler(timer_InactiveTimer);
                this.m_InactiveTimer.Interval = 500;
                this.m_InactiveTimer.Start();

                this.m_ActivityTimeStamp = DateTime.Now;
            }

            this.m_CurrentPoint.startPoint();
        }

        private void point_Reset()
        {
            sendBotSpam("- Restarting point in " + m_StartGameDelay + " seconds! -");
            this.m_CurrentPoint.resetPoint();
            timer_Setup(Misc.TimerType.GameStart); 
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
                    sendBotSpam(("- " + (!AlphaWon ? this.m_ATeamName : this.m_BTeamName) + " is all dead! --  Team " + (AlphaWon ? this.m_ATeamName : this.m_BTeamName) + " scores! -"));
                }
                else if (winType == Misc.WinType.SafeWin)
                {
                    this.m_CurrentPoint.setSafeWinner(PlayerName);
                    sendBotSpam("- [ " + PlayerName + " ] has breached the enemy's defense! (SafeWin) " + (AlphaWon ? "[ " + this.m_ATeamName + " Team Win ]" : "[ " + this.m_BTeamName + " Team Win ]"));
                }
                // display score
                sendBotSpam("- " + this.m_ATeamName + " [ " + this.m_AlphaPoints.ToString() + " ]     Score     [ " + this.m_BravoPoints.ToString() + " ] " + this.m_BTeamName + " -");
                sendBotSpam("- ----------------------------------------------- -");

                sendPlayerSpam("   " + this.m_ATeamName + " " + this.m_AlphaPoints.ToString() + " - " + this.m_BravoPoints.ToString() + " " + this.m_BTeamName + "      WinType:" + winType + "   Match#:" + (this.m_AllPoints.Count + 1) + " -");
            }
            else
            {
                sendBotSpam("- Both teams out -- No count -");
                sendPlayerSpam("   " + this.m_ATeamName + " " + this.m_AlphaPoints.ToString() + " - " + this.m_BravoPoints.ToString() + " " + this.m_BTeamName + "      -- No count --    Match#:" + (this.m_AllPoints.Count + 1) + " -");
            }

            this.m_AllPoints.Add(this.m_CurrentPoint.GetCopy(AlphaWon, winType));
            this.m_CurrentPoint.resetPoint();

            if ((this.m_AlphaPoints >= this.m_Settings.MinimumPoints || this.m_BravoPoints >= this.m_Settings.MinimumPoints)
                && Math.Abs(this.m_AlphaPoints - this.m_BravoPoints) >= this.m_Settings.WinBy)
            {
                this.doEndGamePrintOut();
                this.game_Save();
                this.game_Reset();
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
            this.m_ActivityTimeStamp = DateTime.Now;

            this.m_CurrentPoint.removePlayer(p.PlayerName);
            this.player_TeamCountUpdate();
        }

        public void player_Join(SSPlayer p)
        {
            bool InAlpha;
            BasePlayer b = this.m_CurrentPoint.getPlayer(p.PlayerName, out InAlpha);

            this.m_ActivityTimeStamp = DateTime.Now;

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

        public void players_WarptoCenter()
        {
            this.sendAllTeamMessage("?|prize warp|prize fullcharge");
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void Event_TurretEvent(SSPlayer a, SSPlayer h)
        {
            if (this.m_Status != Misc.BaseGameStatus.InProgress) return;

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
            this.m_ActivityTimeStamp = DateTime.Now;
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
                    if (this.m_Locked || (p.Frequency != this.m_AlphaFreq && p.Frequency != this.m_BravoFreq))
                    {
                        psyGame.Send(msg.pm(p.PlayerName, "?prize warp"));
                    }
                    else
                    {
                        BaseTeam team = p.Frequency == this.m_AlphaFreq ? this.m_CurrentPoint.AlphaTeam():this.m_CurrentPoint.BravoTeam();
                        team.playerJoin(p.PlayerName);
                    }
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

        public void Event_PlayerKilled(SSPlayer Attacker, SSPlayer Victim)
        {
            if (!this.player_InRegion(Attacker.Position, this.m_CurrentBase.BaseDimension)) return;
            bool attacker_InAlpha, victim_InAlpha;
            BasePlayer attacker = this.m_CurrentPoint.getPlayer(Attacker.PlayerName, out attacker_InAlpha);
            BasePlayer victim = this.m_CurrentPoint.getPlayer(Victim.PlayerName, out victim_InAlpha);

            if (attacker == null || victim == null) return; // wtf shouldnt happen

            attacker.Stats.Kills++;
            victim.Stats.Deaths++;
        }

        public void Event_PlayerFiredWeapon(SSPlayer p)
        {
            this.m_ActivityTimeStamp = DateTime.Now;
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

            // next four lines a temp fix for freq change bug during timer - maybe change logic psy, wtf?
            this.m_CurrentPoint.AlphaTeam().resetTeam();
            this.m_CurrentPoint.BravoTeam().resetTeam();
            this.freqDump(this.m_AlphaFreq, this.m_CurrentPoint.AlphaTeam(), true);
            this.freqDump(this.m_BravoFreq, this.m_CurrentPoint.BravoTeam(), false);

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
            // Code shouldnt get here but ill make a safe guard
            if (this.m_Status == Misc.BaseGameStatus.NotStarted)
            {
                this.m_InactiveTimer.Stop();
                this.m_InactiveTimer = new Timer();
                return;
            }

            if ((DateTime.Now - this.m_ActivityTimeStamp).TotalMinutes < this.m_InactiveTimeLimit) return;

            this.m_Timer.Stop();
            this.m_InactiveTimer.Stop();
            this.game_Reset();
            string message = "- Inactive timer triggered: Game Reset. -";
            this.sendAllTeamMessage(message);
            this.sendPlayerSpam(this.m_GameTag + " " + message);
        }

        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        private void sendAllTeamMessage(string message)
        {
            psyGame.Send(msg.team_pm(this.m_AlphaFreq, message));
            psyGame.Send(msg.team_pm(this.m_BravoFreq, message));
        }
        private void sendPlayerSpam(string Message)
        {
            List<SSPlayer> spamPlayers = new List<SSPlayer>();
            
            for (int i = 0; i < m_SpamMeList.Count; i++)
            {
                SSPlayer p = m_Players.PlayerList.Find(item => item.PlayerName == m_SpamMeList[i] && item.Frequency == 7265);

                if (p != null)
                { psyGame.SafeSend(msg.pm(m_SpamMeList[i], this.m_GameTag + Message)); }
            }
        }

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
                if (team.teamCount() == 0 && m_AllPoints.Count == 0)
                {
                    if (IsAlpha) this.m_ATeamName = pList[0].SquadName == "~ no squad ~" ? "Alpha" : pList[0].SquadName;
                    else this.m_BTeamName = pList[0].SquadName == "~ no squad ~" ? "Bravo" : pList[0].SquadName;
                }

                team.playerJoin(pList[0].PlayerName);
                pList.RemoveAt(0);
            }

            if (IsAlpha) return;

            if (this.m_ATeamName == this.m_BTeamName)
            {
                this.m_ATeamName = m_DefaultATeamName;
                this.m_BTeamName = m_DefaultBTeamName;
            }

            if (m_AllPoints.Count == 0)
                this.m_CurrentPoint.SetTeamNames(this.m_ATeamName, this.m_BTeamName);
        }
        // send printouts according to setting
        private void sendBotSpam(string Message)
        {
            ChatEvent spam = new ChatEvent();
            spam.Message = Message;

            if (this.m_Settings.SpamSetting == ChatTypes.Team)
            {
                psyGame.Send(msg.team_pm(this.m_AlphaFreq, Message));
                psyGame.Send(msg.team_pm(this.m_BravoFreq, Message));
                return;
            }

            if (this.m_Settings.SpamSetting == ChatTypes.TeamPrivate)
            {
                //if (Message.StartsWith("-")) Message = Message.Remove(0, 1);
                psyGame.Send(msg.team_pm(this.m_AlphaFreq, ("?a " + Message).PadRight(55)));
                psyGame.Send(msg.team_pm(this.m_BravoFreq, ("?a " + Message).PadRight(55)));
            }


            spam.ChatType = this.m_Settings.SpamSetting;
            psyGame.Send(spam);
            return;
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
            m_BaseManager.ReleaseBase(this.m_CurrentBase,"BaseDuel");
            this.m_CurrentBase = m_BaseManager.getNextBase("BaseDuel", this.m_BaseSize);
            this.m_CurrentPoint.setBaseNumber(this.m_CurrentBase.Number);
        }
        
        private string getFormattedTime(TimeSpan ts)
        {
            string ms = ts.Milliseconds.ToString();
            ms = ms.Length > 3 ? ms.Substring(0, 3) : ms.PadRight(3, '0');
            return (ts.Hours > 0 ? ts.Hours.ToString().PadLeft(2, '0') + "h:" : "") + ts.Minutes.ToString().PadLeft(2, '0') + "m:" + ts.Seconds.ToString().PadLeft(2, '0') + "s:" + ms + "ms";
        }
        //----------------------------------------------------------------------//
        //                           PrintOuts                                  //
        //----------------------------------------------------------------------//
        private void doEndGamePrintOut()
        {
            int AlphaScore = this.m_AlphaPoints;
            int BravoScore = this.m_BravoPoints;
            bool alphaWon = AlphaScore > BravoScore;
            string WinTeam = alphaWon ? this.m_ATeamName : this.m_BTeamName;
            string score = alphaWon ? "Score " + this.m_AlphaPoints + " - " + this.m_BravoPoints : "Score " + this.m_BravoPoints + " - " + this.m_AlphaPoints;
            string freq = alphaWon ? "[Freq " + this.m_AlphaFreq + "]" : "[Freq " + this.m_BravoFreq + "]";

            List<BasePlayer> AlphaPlayers = new List<BasePlayer>();
            List<BasePlayer> BravoPlayers = new List<BasePlayer>();
            TimeSpan TotalTime = new TimeSpan();

            int[] Akills = new int[] { 0, 0 };
            int[] Bkills = new int[] { 0, 0 };

            foreach (BasePoint bp in this.m_AllPoints)
            {
                List<BasePlayer> tempA = bp.AlphaTeam().Players;
                if (bp.AlphaTeam().Inactives.Count > 0)
                    tempA.AddRange(bp.AlphaTeam().Inactives);

                List<BasePlayer> tempB = bp.BravoTeam().Players;
                if (bp.BravoTeam().Inactives.Count > 0)
                    tempB.AddRange(bp.BravoTeam().Inactives);
                
                TotalTime += bp.TotalTime;
                foreach (BasePlayer player in tempA)
                {
                    BasePlayer nextp = AlphaPlayers.Count > 0 ? AlphaPlayers.Find(item => item.PlayerName == player.PlayerName) : null;

                    if (nextp == null)
                    {
                        AlphaPlayers.Add(player);
                    }
                    else
                    {
                        nextp.Stats.Kills += player.Stats.Kills;
                        nextp.Stats.Deaths += player.Stats.Deaths;
                    }

                    Akills[0] += player.Stats.Kills;
                    Akills[1] += player.Stats.Deaths;
                }
                foreach (BasePlayer player in tempB)
                {
                    BasePlayer nextp = BravoPlayers.Count > 0 ? BravoPlayers.Find(item => item.PlayerName == player.PlayerName) : null;

                    if (nextp == null)
                    {
                        BravoPlayers.Add(player);
                    }
                    else
                    {
                        nextp.Stats.Kills += player.Stats.Kills;
                        nextp.Stats.Deaths += player.Stats.Deaths;
                    }

                    Bkills[0] += player.Stats.Kills;
                    Bkills[1] += player.Stats.Deaths;
                }

            }

            this.sendBotSpam(WinTeam + " wins! " + freq + "  " + score);
            this.sendBotSpam(".----------------------------------.");
            this.sendBotSpam("|  " + this.m_GameTag.PadRight(20) + "K      L    |");
            this.sendBotSpam("|                                  |");
            this.sendBotSpam("|----------------------------------+");
            this.sendBotSpam("|" + ("Freq [ " + this.m_AlphaFreq + " ]").PadRight(13) + this.m_ATeamName.PadRight(21) + "|");
            this.sendBotSpam("|-------------------+------+-------+");

            foreach (BasePlayer bp in AlphaPlayers)
            {
                this.sendBotSpam("|  " + bp.PlayerName.PadRight(17) + "| " + bp.Stats.Kills.ToString().PadRight(5) + "| " + bp.Stats.Deaths.ToString().PadRight(6) + "|");
            }
            this.sendBotSpam("|                   |------|-------|");
            this.sendBotSpam("|" + ("Team Total").PadRight(19) + "| " + Akills[0].ToString().PadRight(5) + "| " + Akills[1].ToString().PadRight(6) + "|");
            this.sendBotSpam("|----------------------------------+");
            this.sendBotSpam("|" + ("Freq [ " + this.m_BravoFreq + " ]").PadRight(13) + this.m_BTeamName.PadRight(21) + "|");
            this.sendBotSpam("|-------------------+------+-------+");
            foreach (BasePlayer bp in BravoPlayers)
            {
                this.sendBotSpam("|  " + bp.PlayerName.PadRight(17) + "| " + bp.Stats.Kills.ToString().PadRight(5) + "| " + bp.Stats.Deaths.ToString().PadRight(6) + "|");
            }
            this.sendBotSpam("|                   |------|-------|");
            this.sendBotSpam("|" + ("Team Total").PadRight(19) + "| " + Bkills[0].ToString().PadRight(5) + "| " + Bkills[1].ToString().PadRight(6) + "|");
            this.sendBotSpam("`-------------------+------+-------'");
            this.sendBotSpam("Total Game Time:  " + this.getFormattedTime(TotalTime));
            this.psyGame.Send(msg.arena(this.m_GameTag + "- GameEnd LogID# (Not implemented yet)"));
        }

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
            psyGame.Send(msg.pm(p.PlayerName, "Safe Win      :".PadRight(leftOffset) + ((this.m_Settings.SafeWin?"- On -":"- Off -").PadLeft(2, '0')).PadLeft(rightOffset)));
            psyGame.Send(msg.pm(p.PlayerName, "Locked        :".PadRight(leftOffset) + ((this.m_Locked ? "Locked" : "Unlocked").PadLeft(2, '0')).PadLeft(rightOffset)));
            if (this.m_CurrentBase.BaseID > 0)
                psyGame.Send(msg.pm(p.PlayerName, "Current BaseID:".PadRight(leftOffset) + (this.m_CurrentBase.BaseID).ToString().PadLeft(rightOffset)));
            if (this.m_CurrentBase.BaseCreator != null)
                psyGame.Send(msg.pm(p.PlayerName, "Base Creator  :".PadRight(leftOffset) + (this.m_CurrentBase.BaseCreator).PadLeft(rightOffset)));
            if (this.m_CurrentBase.DateCreated != null)
                psyGame.Send(msg.pm(p.PlayerName, "Date Created  :".PadRight(leftOffset) + (this.m_CurrentBase.DateCreated).PadLeft(rightOffset)));
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
/*
 * How to handle not enough players on game start - game reset?
 * !score
 * player specs - no one on team - reset game?
 */ 
