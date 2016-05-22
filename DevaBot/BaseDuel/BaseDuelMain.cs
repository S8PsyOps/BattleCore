﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

// Needed for timers
using System.Timers;

namespace DevaBot.BaseDuel
{
    class BaseDuelMain
    {
        public BaseDuelMain(byte[] MapData)
        {
            this.msg = new ShortChat();
            this.m_StartMatchTime = 10;
            this.m_ShowStartMatchTime = 0;

            this.m_BDTimer = new Timer();
            this.m_CurrentGame = new BaseGame();

            this.m_AllOutTimerSetting = 5; 
            this.m_AllOutTimer = new Timer();

            this.m_CurrentGame.AlphaFreq = 887;
            this.m_CurrentGame.BravoFreq = 889;
            this.m_CurrentGame.Status = BaseGameStatus.GameIdle;
            this.m_AlphaWaitList = new List<string>();
            this.m_BravoWaitList = new List<string>();
            this.m_BlockedList = new List<string>();
            this.m_BDEventQueue = new Queue<EventArgs>();
            this.m_BaseManager = new BaseManager(MapData);
        }

        private ShortChat msg;
        private bool m_AlternateWarp = true;
        private Timer m_BDTimer;
        
        private double m_StartMatchTime;
        private double m_StartMatchTime_Elapsed;
        private double m_ShowStartMatchTime;

        private double m_AllOutTimerSetting;
        private Timer m_AllOutTimer;

        private BaseGame m_CurrentGame;
        private BaseManager m_BaseManager;
        private bool m_BaseGameOn = false;
        private Queue<EventArgs> m_BDEventQueue;
        private List<string> m_AlphaWaitList, m_BravoWaitList, m_BlockedList;

        public bool BaseGameOn
        { get { return m_BaseGameOn; } }

        // Any messages needed to be sent or events just add to this queue
        public EventArgs BaseDuelEvents
        {
            get
            {
                if (m_BDEventQueue == null || m_BDEventQueue.Count == 0) return null;
                return m_BDEventQueue.Dequeue();
            }
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void Event_PlayerPosition(DevaPlayer dp)
        {
            // Check to see game is on and they are not on blocked list
            if (!m_BaseGameOn || m_BlockedList.Contains(dp.PlayerName)) return;

            // Player is in the center start box
            if (inRegion(dp.Position, m_BaseManager.Lobby.BaseDimension))
            {
                // Game hasnt started yet
                if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
                {
                    if (!inWaitList(dp.PlayerName) && canWarp(dp))
                        SendToWaitList(dp.PlayerName);
                    return;
                }
                // Game is in progress
                if (m_CurrentGame.Status == BaseGameStatus.GameOn)
                {
                    bool InAlpha;
                    BasePlayer b = getPlayer(dp.PlayerName, out InAlpha);

                    // Player join during game code here
                    if (b == null )
                    {
                        // Dont allow if option isnt set to true
                        if (!m_CurrentGame.AllowAfterStartJoin) return;
                        
                        NewPlayerJoin(dp.PlayerName);
                        b = getPlayer(dp.PlayerName, out InAlpha);
                    }
                    
                    // player is active
                    b.Active = true;
                    // and he is inside lobby area
                    b.InLobby = true;
                    // Grab the right freq
                    ushort freq = InAlpha ? m_CurrentGame.AlphaFreq : m_CurrentGame.BravoFreq;

                    // if they arent in right freq fix it
                    if (dp.Frequency != freq)
                        m_BDEventQueue.Enqueue(msg.pm(dp.PlayerName, "?|setfreq " + freq + "|shipreset"));
                    

                    bool aTeam, bTeam;
                    // Check for all out
                    checkAllOut(out aTeam, out bTeam);

                    // someone is out - run timer
                    if ((aTeam || bTeam) && !m_AllOutTimer.Enabled) StartAllOutTimer();
                }
            }
            else if (m_CurrentGame.Status == BaseGameStatus.GameOn)
            {
                bool InAlpha;
                BasePlayer b = getPlayer(dp.PlayerName, out InAlpha);

                // Player is not listed on teams and he isnt in center safe ?!?! how?
                if (b == null) return;

                // Player is in base
                if (inRegion(dp.Position, m_BaseManager.CurrentBase.BaseDimension))
                {
                    b.InLobby = false;

                    // Player is on opposing teams safe - Win ( if allowed by setting )
                    if (m_CurrentGame.AllowSafeWin && inRegion(dp.Position, InAlpha ? m_BaseManager.CurrentBase.BravoSafe : m_BaseManager.CurrentBase.AlphaSafe))
                    {
                        EndGame(WinType.SafeWin, InAlpha);
                    }
                }
            }
        }

        public void Event_TeamChange(DevaPlayer dp)
        {
            // Check to see game is on and they are not on blocked list
            if (!m_BaseGameOn || m_BlockedList.Contains(dp.PlayerName)) return;

            if (inFreq(dp.OldFrequency))
            {   RemovePlayer(dp.PlayerName);    }
        }

        public void Event_PlayerLeft(DevaPlayer dp)
        {
            // Check to see game is on and they are not on blocked list
            if (!m_BaseGameOn || m_BlockedList.Contains(dp.PlayerName)) return;

            if (inFreq(dp.Frequency))
            { RemovePlayer(dp.PlayerName); }
        }

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        // Toggle base game on or off
        public void Command_ToggleBaseDuel()
        {
            m_BaseGameOn = !m_BaseGameOn;
            m_BDEventQueue.Enqueue(msg.arena("BaseDuel Module is now set to [ "+m_BaseGameOn+" ]."));

            if (m_BaseGameOn)
            {
                LoadBaseDuel();
                return;
            }
        }

        // Start using the !startbd command
        public void Command_StartGame(string PlayerName)
        {
            if (m_CurrentGame.Status != BaseGameStatus.GameIdle || m_BDTimer.Enabled)
            {
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "There is a game currently in progress. Please wait till it is over to use this command."));
                return;
            }
            if (m_AlphaWaitList.Count >= 1 && m_BravoWaitList.Count >= 1 && m_AlphaWaitList.Count + m_BravoWaitList.Count >= 2)
            {
                m_BDEventQueue.Enqueue(msg.macro("- Game will begin in " + m_StartMatchTime + " seconds -", SoundCodes.BassBeep));
                ConfigGameStartTimer();
            }
            else
                m_BDEventQueue.Enqueue(msg.pm(PlayerName,"To start a game you need at least 2 players. (1 on each team)"));
        }

        public void Command_GetGameInfo(string PlayerName)
        {
            getGameInfo(PlayerName);
        }
        public void Command_GetMatchInfo(string PlayerName)
        {
            getMatchInfo(PlayerName);
        }
        //----------------------------------------------------------------------//
        //                    Game Functions                                    //
        //----------------------------------------------------------------------//
        public void getTeamCounts(out int AlphaCount, out int BravoCount)
        {
            int a = 0;
            int b = 0;
            for (int i = 0; i < m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Count; i++)
                if (m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i].Active) a++;

            for (int i = 0; i < m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Count; i++)
                if (m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i].Active) b++;

            AlphaCount = a;
            BravoCount = b;
        }
        public void NewPlayerJoin(string PlayerName)
        {
            int a, b;
                getTeamCounts(out a, out b);
            if (a <= b)
                addPlayerToTeam(PlayerName, true);
            else
                addPlayerToTeam(PlayerName, false);
        }
        private void prepareGameStart(List<string> Alpha, List<string> Bravo)
        {
            for (int i = 0; i < Alpha.Count; i++)

                addPlayerToTeam(Alpha[i], true);
            for (int i = 0; i < Bravo.Count; i++)
                addPlayerToTeam(Bravo[i], false);
        }
        private void addPlayerToTeam(string PlayerName, bool InAlpha)
        {
            BasePlayer b = new BasePlayer();
            b.PlayerName = PlayerName;
            if (InAlpha) m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Add(b);
            else m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Add(b);
        }
        private void StartGame()
        {
            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                // Move players over to Teams
                prepareGameStart(m_AlphaWaitList, m_BravoWaitList);

                // Clear the wait list
                m_AlphaWaitList = new List<string>();
                m_BravoWaitList = new List<string>();
            }

            string aName = m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[0].PlayerName;
            string bName = m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[0].PlayerName;

            // Grab coords from base and name of first person on team, to send team pm
            string TeamName1 = m_AlternateWarp ? aName : bName;
            string TeamName2 = !m_AlternateWarp ? aName : bName;
            ushort x1 = m_AlternateWarp? m_BaseManager.CurrentBase.AlphaStartX: m_BaseManager.CurrentBase.BravoStartX;
            ushort y1 = m_AlternateWarp ? m_BaseManager.CurrentBase.AlphaStartY : m_BaseManager.CurrentBase.BravoStartY;
            ushort x2 = !m_AlternateWarp ? m_BaseManager.CurrentBase.AlphaStartX : m_BaseManager.CurrentBase.BravoStartX;
            ushort y2 = !m_AlternateWarp ? m_BaseManager.CurrentBase.AlphaStartY : m_BaseManager.CurrentBase.BravoStartY;

            // Warp players to start - alternate each time you warp using the alternateWarp bool
            m_BDEventQueue.Enqueue(msg.teamPM(TeamName1,"?|warpto " + x1 + " " + y1 + "|shipreset"));
            m_BDEventQueue.Enqueue(msg.teamPM(TeamName2, "?|warpto " + x2 + " " + y2 + "|shipreset"));
            // change bool to alternate team warps again
            m_AlternateWarp = !m_AlternateWarp;

            m_CurrentGame.CurrentMatch.StartTime = DateTime.Now;
            m_CurrentGame.Status = BaseGameStatus.GameOn;
        }

        private void EndGame(WinType winType, bool AlphaWon)
        {
            string TotalTime;
            m_CurrentGame.Status = BaseGameStatus.GameIntermission;
            matchEnded(winType, AlphaWon, (short)m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase), out TotalTime);
            //Devastated> [Team] scores through [win method] in [time]!
            //Devastated> Score: [score]

            if (winType == WinType.NoCount)
            {
                m_BDEventQueue.Enqueue(msg.macro("- [ No winner for match - Match Reset ] -"));
            }
            else
            {
                m_BDEventQueue.Enqueue(msg.macro("- [" + ( AlphaWon ? "AlphaTeam" : "BravoTeam" ) + "] wins the match. ["+winType+"] - Time["+TotalTime+"] -", SoundCodes.VictoryBell));
                m_BDEventQueue.Enqueue(msg.macro("- Alpha [" + m_CurrentGame.AlphaScore + "] vs Bravo [" + m_CurrentGame.BravoScore + "] -"));
                m_BaseManager.LoadNextBase();
            }

            m_BDEventQueue.Enqueue(msg.macro("- Next match begins in "+m_StartMatchTime+" seconds. -"));
            //m_BDEventQueue.Enqueue(msg.macro(""));
            ConfigGameStartTimer();
        }

        // Reset vars and prep for start of game
        // ### Dont reset BaseManager. You want to keep the queue of bases into
        // ### memory. This way all bases get played out evenly
        private void LoadBaseDuel()
        {
            m_CurrentGame = new BaseGame();
            m_CurrentGame.AlphaFreq = 887;
            m_CurrentGame.BravoFreq = 889;
            m_CurrentGame.Status = BaseGameStatus.GameIdle;
        }

        // Remove player from wait list or from game
        private void RemovePlayer(string PlayerName)
        {
            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                bool inAlpha = m_AlphaWaitList.Contains(PlayerName);
                if (inAlpha) m_AlphaWaitList.Remove(PlayerName);
                else m_BravoWaitList.Remove(PlayerName);

                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "You have been removed from " + (inAlpha ? "AlphaTeam" : "BravoTeam") + "'s wait list."));
                return;
            }
            // Check if player is on a team
            bool InAlpha;
            BasePlayer b = getPlayer(PlayerName, out InAlpha);
            // Player isnt on list
            if (b == null) return;
            b.Active = false;
            int aCount, bCount;
            getTeamCounts(out aCount, out bCount);

            // Check for uneven teams here
            if (aCount != bCount)
                m_BDEventQueue.Enqueue(msg.arena("Teams are uneven. Active Count: Alpha[ " + aCount + " ] Bravo[ " + bCount + " ]"));
        }

        // send player to the wait list and set to freq - balance team as we put in
        private void SendToWaitList(string PlayerName)
        {
            // Make even teams by either going to one or other
            if (m_AlphaWaitList.Count <= m_BravoWaitList.Count)
            {
                m_AlphaWaitList.Add(PlayerName);
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "?|setfreq " + m_CurrentGame.AlphaFreq + "|shipreset"));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "You have been added to AlphaTeam's wait list."));
                return;
            }
            m_BravoWaitList.Add(PlayerName);
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "?|setfreq " + m_CurrentGame.BravoFreq + "|shipreset"));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "You have been added to BravoTeam's wait list."));
        }

        //----------------------------------------------------------------------//
        //                         Timers                                       //
        //----------------------------------------------------------------------//
        private void StartAllOutTimer()
        {
            m_AllOutTimer = new Timer();
            m_AllOutTimer.Elapsed += new ElapsedEventHandler(AllOutTimerFunc);
            m_AllOutTimer.Interval = m_AllOutTimerSetting * 1000;
            m_AllOutTimer.Start();
        }
        private void AllOutTimerFunc(object source, ElapsedEventArgs e)
        {
            // Do a final check for all out.
            bool aTeam, bTeam;
            // Check for all out
            checkAllOut(out aTeam, out bTeam);

            // One team is out
            if (aTeam || bTeam)
            {
                if (aTeam == bTeam) EndGame(WinType.NoCount, false);
                else EndGame(WinType.AllOut, !aTeam);
            }

            m_AllOutTimer.Stop();
        }
        private void ConfigGameStartTimer()
        {
            m_StartMatchTime_Elapsed = m_StartMatchTime;
            m_BDTimer = new Timer();
            m_BDTimer.Elapsed += new ElapsedEventHandler(CountDownTimer);
            m_BDTimer.Interval = 1000;
            m_BDTimer.Start();
        }

        private void CountDownTimer(object source, ElapsedEventArgs e)
        {
            // Decrease time display count
            m_StartMatchTime_Elapsed--;
            // Check to see if the count will be displayed
            if (m_StartMatchTime_Elapsed <= m_ShowStartMatchTime && m_StartMatchTime_Elapsed != 0)
            {    m_BDEventQueue.Enqueue(msg.macro("- " + m_StartMatchTime_Elapsed + " -",SoundCodes.MessageAlarm)); }

            if (m_StartMatchTime_Elapsed > 0) return;

            // Stop timer
            m_BDTimer.Stop();
            
            // Count for each team/wait list
            int aCount = 0, bCount = 0;

            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                aCount = m_AlphaWaitList.Count;
                bCount = m_BravoWaitList.Count;
            }
            else if (m_CurrentGame.Status == BaseGameStatus.GameIntermission)
            {   getTeamCounts(out aCount, out bCount);   }

            if (!(aCount >= 1 && bCount >= 1) || aCount == 0 || bCount == 0) // if code gets here there wasnt enough players to start
            {
                m_BDEventQueue.Enqueue(msg.macro(" ** Not enough players to start a game. Game start cancelled. **"));
                m_BDEventQueue.Enqueue(msg.macro("- Alpha Count [ " + aCount + " ] --------  Bravo Count [ " + bCount + " ] -"));
                return;
            }
            m_BDEventQueue.Enqueue(msg.macro("- Go Go Go! -".PadRight(30) + ("BaseNumber: " + m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase)).PadLeft(25),SoundCodes.Goal));
            StartGame();
        }

        //----------------------------------------------------------------------//
        //                         Misc                                         //
        //----------------------------------------------------------------------//
        // Simple collision check
        private bool inRegion(PlayerPositionEvent p, ushort[] region)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        // Player is in alpha or bravo wait list
        private bool inWaitList(string PlayerName)
        {
            return m_AlphaWaitList.Contains(PlayerName) || m_BravoWaitList.Contains(PlayerName);
        }

        // Player is on alpha/bravo freq
        private bool inFreq(ushort freq)
        {
            return m_CurrentGame.AlphaFreq == freq || m_CurrentGame.BravoFreq == freq;
        }

        // Check to see if player is allowed to warp, update warp stamp if so
        private bool canWarp(DevaPlayer bp)
        {
            if ((DateTime.Now - bp.WarpStamp).TotalMilliseconds < 1200) return false;

            bp.WarpStamp = DateTime.Now;
            return true;
        }
        private BasePlayer getPlayer(string PlayerName, out bool InAlpha)
        {
            InAlpha = true;
            BasePlayer b = m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Find(item => item.PlayerName == PlayerName);
            // Player is in alpha team - return player
            if (b != null) return b;

            // even if not in bravo team we still return a null
            b = m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Find(item => item.PlayerName == PlayerName);
            InAlpha = false;
            return b;
        }
        private void checkAllOut(out bool Alpha, out bool Bravo)
        {
            bool a = true;
            bool b = true;
            for (int i = 0; i < m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Count; i++)
                if (m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i].Active && !m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i].InLobby) a = false;

            for (int i = 0; i < m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Count; i++)
                if (m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i].Active && !m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i].InLobby) b = false;

            Alpha = a;
            Bravo = b;
        }
       private void matchEnded(WinType winType, bool AlphaWon, short BaseNumber, out string TotalTime)
        {
            m_CurrentGame.CurrentMatch.AlphaWon = AlphaWon;
            m_CurrentGame.CurrentMatch.WinType = winType;
            m_CurrentGame.CurrentMatch.BaseNumber = BaseNumber;

            if (winType != WinType.NoCount)
            {
                if (AlphaWon) m_CurrentGame.AlphaScore++;
                else m_CurrentGame.BravoScore++;
            }

            m_CurrentGame.CurrentMatch.MatchEnded();
            saveMatch();

            for (int i = m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i].Active)
                    m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Remove(m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i]);
                else
                {
                    //m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i].ResetPlayer();
                }
            }
            for (int i = m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i].Active)
                    m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Remove(m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i]);
                else
                {
                    //m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i].ResetPlayer();
                }
            }

            // make sure to send back time
            TotalTime = m_CurrentGame.CurrentMatch.TotalTimeFormatted;
        }

        private void saveMatch()
        {
            BaseMatch newMatch = new BaseMatch();
            newMatch.AlphaTeam = m_CurrentGame.CurrentMatch.AlphaTeam;
            newMatch.AlphaWon = m_CurrentGame.CurrentMatch.AlphaWon;
            newMatch.BaseNumber = m_CurrentGame.CurrentMatch.BaseNumber;
            newMatch.BravoTeam = m_CurrentGame.CurrentMatch.BravoTeam;
            newMatch.TotalTime = m_CurrentGame.CurrentMatch.TotalTime;
            newMatch.WinType = m_CurrentGame.CurrentMatch.WinType;
            m_CurrentGame.Matches.Add(newMatch);
        }
        private void getGameInfo(string PlayerName)
        {
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Game Info -----------------------------------"));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "GameStatus      :".PadRight(20) + m_CurrentGame.Status.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "----------------"));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Alpha Team Name :".PadRight(20) + m_CurrentGame.CurrentMatch.AlphaTeam.TeamName.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Alpha Freq      :".PadRight(20) + m_CurrentGame.AlphaFreq.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Team Count      :".PadRight(20) + m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Alpha Score     :".PadRight(20) + m_CurrentGame.AlphaScore.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Players --"));
            for (int i = 0; i < m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers.Count; i++)
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i].PlayerName.PadRight(20) + ("Active :" + m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers[i].Active).PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "----------------"));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Bravo Team Name :".PadRight(20) + m_CurrentGame.CurrentMatch.BravoTeam.TeamName.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Bravo Freq      :".PadRight(20) + m_CurrentGame.BravoFreq.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Team Count      :".PadRight(20) + m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Bravo Score     :".PadRight(20) + m_CurrentGame.BravoScore.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Players --"));
            for (int i = 0; i < m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers.Count; i++)
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i].PlayerName.PadRight(20) + ("Active :" + m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers[i].Active).PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "----------------"));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Allow Safe Win  :".PadRight(20) + m_CurrentGame.AllowSafeWin.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Join After Start:".PadRight(20) + m_CurrentGame.AllowSafeWin.ToString().PadLeft(25)));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Matches stored  :".PadRight(20) + m_CurrentGame.Matches.Count.ToString().PadLeft(25)));
        }
        private void getMatchInfo(string PlayerName)
        {
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Match Info ----------------------------------"));
            m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Matches Stored    :".PadRight(20) + m_CurrentGame.Matches.Count.ToString().PadLeft(25)));
            for (int i = 0; i < m_CurrentGame.Matches.Count; i++)
            {
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "----------------"));
                if (m_CurrentGame.Matches[i].WinType != WinType.NoCount)
                    m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Winner          :".PadRight(20) + (m_CurrentGame.Matches[i].AlphaWon ? "Alpha Team" : "Bravo Team").ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Win Type        :".PadRight(20) + m_CurrentGame.Matches[i].WinType.ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Match Time      :".PadRight(20) + m_CurrentGame.Matches[i].TotalTimeFormatted.ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Match Number    :".PadRight(20) + (i + 1).ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Base Number     :".PadRight(20) + m_CurrentGame.Matches[i].BaseNumber.ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Alpha Team Name :".PadRight(20) + m_CurrentGame.Matches[i].AlphaTeam.TeamName.ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Alpha Team Count:".PadRight(20) + m_CurrentGame.Matches[i].AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Bravo Team Name :".PadRight(20) + m_CurrentGame.Matches[i].BravoTeam.TeamName.ToString().PadLeft(25)));
                m_BDEventQueue.Enqueue(msg.pm(PlayerName, "Bravo Team Count:".PadRight(20) + m_CurrentGame.Matches[i].BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            }
        }
    }
}
