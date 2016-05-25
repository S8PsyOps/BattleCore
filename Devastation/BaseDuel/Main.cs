using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

// Needed for timers
using System.Timers;

namespace Devastation.BaseDuel
{
    class Main
    {
        public Main( byte[] MapData, SSPlayerManager PlayerManager, ShortChat msg, MyGame myGame)
        {
            this.m_Timer = new Timer();
            this.m_BaseManager = new BaseManager(MapData);
            this.m_Base = m_BaseManager.CurrentBase;
            this.m_Lobby = m_BaseManager.Lobby;
            this.m_AlphaFreq = 887;
            this.m_BravoFreq = 889;
            this.m_MaxArenaFreq = 100;
            this.m_BlockedList = new List<string>();
            this.m_StartGameDelay = 10;
            this.m_StartGameShow = 3;
            this.m_TeamClearDelay = 5;
            this.m_GamesPlayed = new List<BaseGame>();
            this.m_BaseGame = new BaseGame();
            this.m_BaseGame.AlphaFreq = m_AlphaFreq;
            this.m_BaseGame.BravoFreq = m_BravoFreq;
            this.m_Players = PlayerManager;
            this.msg = msg;
            this.myGame = myGame;
        }

        private ShortChat msg;                      // Module to make sending chat messages easier
        private BaseGame m_BaseGame;                    // This holds all the baseduel game info
        
        private Timer m_Timer;                      // Main timer for any needed timed events
        private double m_StartGameDelay;            // Delay in start of game (seconds)
        private double m_StartGameDelay_Elapsed;    // Used for start timer - not to be set
        private double m_StartGameShow;             // How many seconds to display countdown as txt or gfx ( 3..2..1 )
        private double m_TeamClearDelay;            // How many seconds before BaseClear Takes effect - helps with lag attach
        
        private BaseManager m_BaseManager;          // Controls bases and how they are loaded
        private Base m_Base;                        // Local variable to hold loaded base
        private Base m_Lobby;                       // Hold lobby dimensions and info

        private ushort m_AlphaFreq;                 // Keeping these local - may help creating multiple games playable at once later
        private ushort m_BravoFreq;                 // They are stored inside BaseGame as well, so you can create more games with more designated freqs
        private ushort m_MaxArenaFreq;              // I need to find a way to get this info from server on startup - in case it changes

        private List<string> m_BlockedList;         // Users to ignore - use this for a ban/block or for any diff games that may go on (baserace etc)
        
        private List<BaseGame> m_GamesPlayed;       // List of games that have been recorded
        private SSPlayerManager m_Players;          // Player tracker code
        private MyGame myGame;

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        /// <summary>
        /// <para>All commands used for baseduel send through here.</para>
        /// <para>Syntax: !baseduel command    Example !baseduel settings  or  .baseduel start</para>
        /// <para>If you want to make compatible commands, register them in devastation main</para>
        /// <para>and change the command to make it compatible with !baseduel command</para>
        /// <para>Example: RegisterCommand("!startbd") , then change incoming message e.Message = "!baseduel start"</para>
        /// <para>then send it here. BaseDuelCommands(e);</para>
        /// </summary>
        /// <param name="p">Deva Player</param>
        /// <param name="e">Command sent</param>
        /// <returns></returns>
        public void BaseDuelCommands(SSPlayer p, ChatEvent e)
        {
            if (m_BlockedList.Contains(p.PlayerName)) return;

            string[] data = e.Message.Split(' ');

            if (data[0].Trim().ToLower() == ".baseduel" && data.Length <= 1)
            {
                // Module is off
                if (!moduleIsOn(e)) return ;

                return;
            }

            string command = data[1].Trim().ToLower();

            switch (command)
            {
                case "start":
                    command_Start(e);
                    return;

                case "toggle":
                    command_Toggle(p);
                    return;

                case "settings":
                    if (!moduleIsOn(e)) return;
                    show_GameSettings(p.PlayerName);
                    return;

                case "shuffle":
                    if (!moduleIsOn(e)) return;
                    command_Shuffle(e);
                    return;

                case "switch":

                    return;

                case "reset":
                    return;

                case "rounds":

                    return;
            }
            return;
        }

        private void command_Toggle(SSPlayer p)
        {
            // Turn On module
            if (m_BaseGame == null)
            {
                // reset vars
                baseduel_Reset();

                // Send Arena Message
                myGame.Send(msg.arena("[ BaseDuel ] Module loaded by staff - " + p.PlayerName));
                show_GameSettings(p.PlayerName);
                return;
            }

            if (m_Timer.Enabled) m_Timer.Stop();
            // Turn Module Off
            m_BaseGame = null;
            myGame.Send(msg.arena("[ BaseDuel ] Module unloaded by staff - " + p.PlayerName));
        }

        private void command_Start(ChatEvent e)
        {
            // Module is off
            if (!moduleIsOn(e)) return;

            // only start game if pre-game
            if (m_Timer.Enabled || m_BaseGame.Status != BaseGameStatus.GameIdle)
            {
                myGame.Send(msg.pm(e.PlayerName, "There is currently a game in progress. Please wait until game is over before using this command."));
                return;
            }

            // start game - enough players
            if (allow_GameStart(true))
            {
                myGame.Send(msg.macro("- Game starting in " + m_StartGameDelay + " seconds -"));

                // Move players from pub to private freqs and add them to teams
                List<SSPlayer> aTeam = m_Players.PlayerList.FindAll(item => item.Frequency == 0 && !m_BlockedList.Contains(item.PlayerName));
                while (aTeam.Count > 0)
                {
                    m_BaseGame.Round.AlphaTeam.TeamMembers.Add(new BasePlayer(aTeam[0].PlayerName));
                    myGame.Send(msg.pm(aTeam[0].PlayerName, "?|setfreq " + m_AlphaFreq + "|shipreset"));
                    aTeam.RemoveAt(0);
                }
                List<SSPlayer> bTeam = m_Players.PlayerList.FindAll(item => item.Frequency == 1 && !m_BlockedList.Contains(item.PlayerName));
                while (bTeam.Count > 0)
                {
                    m_BaseGame.Round.BravoTeam.TeamMembers.Add(new BasePlayer(bTeam[0].PlayerName));
                    myGame.Send(msg.pm(bTeam[0].PlayerName, "?|setfreq " + m_BravoFreq + "|shipreset"));
                    bTeam.RemoveAt(0);
                }

                timer_Setup(TimerType.GameStart);
                return;
            }

            else // Not enough players
            {
                myGame.Send(msg.pm(e.PlayerName, "In order to start a game you must have at least 1 player on each team."));
                return;
            }
        }

        private void command_Shuffle(ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Mod)) return;

            if (m_BaseGame.Status != BaseGameStatus.GameIdle)
            {
                myGame.Send(msg.pm(e.PlayerName, "This command can only be used before a game. If you wish to reshuffle you must end current game and reset."));
                return;
            }
            List<SSPlayer> all = m_Players.PlayerList.FindAll(item => (item.Frequency == 0 || item.Frequency == 1) && !m_BlockedList.Contains(item.PlayerName));

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

                if (all[0].Frequency != (alternate ? 0 : 1))
                    myGame.Send(msg.pm(all[0].PlayerName, "?|setfreq " + (alternate ? 0 : 1) + "|shipreset"));
                all.RemoveAt(0);
            }
        }

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        // reset all vars to baseduel
        private void baseduel_Reset()
        {
            m_BaseGame = new BaseGame();
            m_BaseGame.AlphaFreq = m_AlphaFreq;
            m_BaseGame.BravoFreq = m_BravoFreq;
            m_BaseGame.Status = BaseGameStatus.GameIdle;
        }

        private void game_Start()
        {
            // get next round rdy
            if (m_BaseGame.Status == BaseGameStatus.GameIntermission)
                round_ResetPLayers();

            // send players to start and reset ships
            myGame.Send(msg.team_pm(m_AlphaFreq,"?|warpto " + m_Base.AlphaStartX + " " + m_Base.AlphaStartY + "|shipreset"));
            myGame.Send(msg.team_pm(m_BravoFreq, "?|warpto " + m_Base.BravoStartX + " " + m_Base.BravoStartY + "|shipreset"));

            int baseNum = m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1;
            myGame.Send(msg.macro("- Go Go Go! -    BaseNum[ " + baseNum + " ]"));
            // Record start time
            m_BaseGame.Round.StartTime = DateTime.Now;
            // set status
            m_BaseGame.Status = BaseGameStatus.GameOn;
            myGame.SafeSend(msg.debugChan("GameStatus change to GameOn."));
        }

        private void game_Save()
        {
            BaseGame saved = new BaseGame();
            saved.AllowAfterStartJoin = m_BaseGame.AllowAfterStartJoin;
            saved.AllowSafeWin = m_BaseGame.AllowSafeWin;
            saved.AllRounds = m_BaseGame.AllRounds;
            saved.AlphaFreq = m_BaseGame.AlphaFreq;
            saved.AlphaScore = m_BaseGame.AlphaScore;
            saved.BravoFreq = m_BaseGame.BravoFreq;
            saved.BravoScore = m_BaseGame.BravoScore;
            saved.MinimumWin = m_BaseGame.MinimumWin;
            saved.Round = m_BaseGame.Round;
            saved.Status = m_BaseGame.Status;
            saved.WinBy = m_BaseGame.WinBy;
            m_GamesPlayed.Add(saved);
        }

        private void game_Reset()
        {
            // Stop timer just in case the all out timer started
            if (m_Timer.Enabled) m_Timer.Stop();
        }

        private void round_Finished(WinType winType, bool AlphaWon)
        {
            m_BaseGame.Status = BaseGameStatus.GameIntermission;

            if (winType != WinType.NoCount)
            {
                // assign points
                if (AlphaWon) m_BaseGame.AlphaScore++;
                else m_BaseGame.BravoScore++;

                myGame.Send(msg.macro("- " + (AlphaWon ? "Alpha Team" : "Bravo Team") + " wins the round by: " + winType + ".  - Alpha[ " + m_BaseGame.AlphaScore + " ][ " + m_BaseGame.BravoScore + " ]Bravo -"));
            }
            else
                myGame.Send(msg.macro("- No Count Round. -"));

            m_BaseGame.Round.AlphaWon = AlphaWon;
            m_BaseGame.Round.WinType = winType;
            round_Save();

            //only load next base if it wasnt nocount
            if (winType != WinType.NoCount) loadNextBase();

            // Check for end of game
            if ((m_BaseGame.AlphaScore >= m_BaseGame.MinimumWin || m_BaseGame.BravoScore >= m_BaseGame.MinimumWin) && Math.Abs(m_BaseGame.AlphaScore - m_BaseGame.BravoScore) >= 2)
            {
                myGame.Send(msg.arena((m_BaseGame.AlphaScore > m_BaseGame.BravoScore ? "Alpha Team" : "Bravo Team") + " wins the game. Final Score Alpha[ " + m_BaseGame.AlphaScore + " ] [ " + m_BaseGame.BravoScore + " ]Bravo.   -Print out here."));
                game_Save();
                game_Reset();
                return;
            }

            myGame.Send(msg.macro("- Next round starts in " + m_StartGameDelay + " seconds. Change ships now if needed. -"));
            timer_Setup(TimerType.GameStart);
        }

        private void round_Save()
        {
            // Copy over round so we can save it
            BaseRound saved = new BaseRound();
            saved.AlphaTeam = m_BaseGame.Round.AlphaTeam;
            saved.BravoTeam = m_BaseGame.Round.BravoTeam;
            saved.AlphaWon = m_BaseGame.Round.AlphaWon;
            saved.BaseNumber = (short)(m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1);
            saved.StartTime = m_BaseGame.Round.StartTime;
            saved.TotalTime = DateTime.Now - m_BaseGame.Round.StartTime;
            saved.WinType = m_BaseGame.Round.WinType;
            // Save it
            m_BaseGame.AllRounds.Add(saved);
        }

        private void round_ResetPLayers()
        {
        }

        private void player_Remove(SSPlayer p)
        {
            // If a game is in progress we want to set the player to inactive - this way we can save stats after player leaves
            if (m_BaseGame.Status == BaseGameStatus.GameOn)
            {
            }
        }

        // Load next base using BaseManager but store it locally - just to cut down on some code length
        private void loadNextBase()
        {
            m_BaseManager.LoadNextBase();
            m_Base = m_BaseManager.CurrentBase;
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void Event_ShipChange(SSPlayer ssp){}
        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host) { }
        public void Event_PlayerTurretDetach(SSPlayer ssp) { }
        public void Event_PlayerEntered(SSPlayer ssp) { }

        public void Event_PlayerPosition(SSPlayer ssp)
        {
            // Game is not turned on
            if (m_BaseGame == null) return;

            if (m_BaseGame.Status != BaseGameStatus.GameIdle)
            {
                // Grab player info
                bool InAlpha;
                BasePlayer b = getPlayer(ssp, out InAlpha);

                // do nothing they are not in game
                if (b == null) return;

                // Only do collision on IsSafe - cuts down on checks
                if (ssp.Position.ShipState.IsSafe)
                {
                    // Player is in lobby
                    if (player_InRegion(ssp.Position,m_Lobby.BaseDimension))
                    {
                        b.InLobby = true;

                        // delay the BaseClear timer for start - it picks up everyone as out in the first second of play
                        if ((DateTime.Now - m_BaseGame.Round.StartTime).TotalMilliseconds < 1500) return;

                        // Check for BaseClear
                        bool aOut, bOut;
                        if (!m_Timer.Enabled && teamIsOut(out aOut, out bOut))
                        {
                            timer_Setup(TimerType.BaseClear);
                        }
                        // nothing else to check exit out
                        return;
                    }
                    // I will not perform another check inside base dimension
                    // assumption is if not in center, you are in base ( if player is somewhere else it can break bot)

                    // Change status if not updated
                    if (b.InLobby)
                    {
                        b.InLobby = false;
                        myGame.SafeSend(msg.debugChan("Player[ " + ssp.PlayerName + " ]'s status has changed to InBase."));
                    }

                    // Check player against opposing team safe
                    if (m_BaseGame.AllowSafeWin && player_InRegion(ssp.Position,InAlpha?m_Base.BravoSafe:m_Base.AlphaSafe))
                    {
                        round_Finished(WinType.SafeWin, InAlpha);
                    }
                }
            }
        }

        public void Event_PlayerLeft(SSPlayer ssp)
        {
            // No need to deal with event if it is off
            if (m_BaseGame == null) return;


            player_Remove(ssp);
        }

        public void Event_PlayerFreqChange(SSPlayer ssp)
        {
            // No need to deal with event if it is off
            if (m_BaseGame == null) return;


        }

        //----------------------------------------------------------------------//
        //                       Timer Stuff                                    //
        //----------------------------------------------------------------------//
        private void timer_Setup(TimerType t)
        {
            m_Timer = new Timer();

            // Set the timer for what we need it to do: Start or Clear Check
            if (t == TimerType.GameStart)
            {
                m_StartGameDelay_Elapsed = m_StartGameDelay;
                m_Timer.Elapsed += new ElapsedEventHandler(timer_StartGame);
                m_Timer.Interval = 1000;
            }
            else if (t == TimerType.BaseClear)
            {
                myGame.Send(msg.debugChan("Starting Clear Timer"));
                m_Timer.Elapsed += new ElapsedEventHandler(timer_BaseClear);
                m_Timer.Interval = m_TeamClearDelay * 1000;
            }
            m_Timer.Start();
        }

        private void timer_StartGame(object source, ElapsedEventArgs e)
        {
            m_StartGameDelay_Elapsed--;

            if (m_StartGameDelay_Elapsed <= m_StartGameShow && m_StartGameDelay_Elapsed != 0)
            {
                myGame.Send(msg.macro("- " + m_StartGameDelay_Elapsed + " -", SoundCodes.MessageAlarm));
            }

            if (m_StartGameDelay_Elapsed > 0) return;

            m_Timer.Stop();

            if (allow_GameStart(false)) game_Start();
            else    myGame.Send(msg.macro("- Not enough players. Start Game cancelled. -")); 
        }

        private void timer_BaseClear(object source, ElapsedEventArgs e)
        {
            myGame.Send(msg.debugChan("- All out timer expired. -"));
            m_Timer.Stop();

            // make sure a team is still out
            bool aOut, bOut;

            if (teamIsOut(out aOut, out bOut))
            {
                round_Finished(aOut == bOut?WinType.NoCount:WinType.BaseClear, !aOut);
            }
        }

        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        private bool teamIsOut(out bool alpha, out bool bravo)
        {
            // make sure a team is still out
            alpha = !(m_BaseGame.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            bravo = !(m_BaseGame.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            return alpha || bravo;
        }

        // Grab player from one of the teams
        private BasePlayer getPlayer( SSPlayer p, out bool InAlpha)
        {
            BasePlayer b = m_BaseGame.Round.AlphaTeam.TeamMembers.Find( item => item.PlayerName == p.PlayerName);

            if ( b != null)
            {
                InAlpha = true;
                return b;
            }

            b = m_BaseGame.Round.BravoTeam.TeamMembers.Find(item => item.PlayerName == p.PlayerName);
            InAlpha = false;
            return b;
        }

        // Simple collision check
        private bool player_InRegion(PlayerPositionEvent p, ushort[] region)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        // checking if player is mod - if not sends back message
        private bool player_isMod(ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod)
                return true;

            myGame.Send(msg.pm(e.PlayerName, "You do not have access to this command. This is a staff command. Required Moerator level: [ " + mod + " ]."));
            return false;
        }

        // Checks to see if module is loaded - returns message if not
        private bool moduleIsOn( ChatEvent e)
        {
            if (m_BaseGame != null) return true;

            myGame.Send(msg.pm(e.PlayerName, "Module is currently not loaded."));
            return false;
        }

        // Player Count check to make sure we can start game
        private bool allow_GameStart(bool InPub)
        {
            int aFreq = InPub ? 0 : m_AlphaFreq;
            int bFreq = InPub ? 1 : m_BravoFreq;

            int aCount = m_Players.PlayerList.FindAll(item => item.Frequency == aFreq && !m_BlockedList.Contains(item.PlayerName)).Count;
            int bCount = m_Players.PlayerList.FindAll(item => item.Frequency == bFreq && !m_BlockedList.Contains(item.PlayerName)).Count;

            return aCount > 0 && bCount > 0;
        }

        //----------------------------------------------------------------------//
        //                           PrintOuts                                  //
        //----------------------------------------------------------------------//
        private void show_GameSettings(string PlayerName)
        {
            // Show player how it was loaded
            myGame.Send(msg.pm(PlayerName, "BaseDuel Settings ---------------------------"));
            myGame.Send(msg.pm(PlayerName, "Status           :".PadRight(20) + m_BaseGame.Status.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Join after Start :".PadRight(20) + m_BaseGame.AllowAfterStartJoin.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Safe win enabled :".PadRight(20) + m_BaseGame.AllowSafeWin.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Minimum Wins     :".PadRight(20) + m_BaseGame.MinimumWin.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Win By           :".PadRight(20) + m_BaseGame.WinBy.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "------------------------"));
            myGame.Send(msg.pm(PlayerName, "Alpha Team Name  :".PadRight(20) + m_BaseGame.Round.AlphaTeam.TeamName.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_BaseGame.Round.AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_BaseGame.AlphaFreq.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "------------------------"));
            myGame.Send(msg.pm(PlayerName, "Bravo Team Name  :".PadRight(20) + m_BaseGame.Round.BravoTeam.TeamName.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_BaseGame.Round.BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_BaseGame.BravoFreq.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Base Loader Settings -----------------"));
            myGame.Send(msg.pm(PlayerName, "Current Base     :".PadRight(20) + (m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1).ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Total Bases      :".PadRight(20) + m_BaseManager.Bases.Count.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Sorting Mode     :".PadRight(20) + m_BaseManager.Mode.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "Size Restrictions:".PadRight(20) + m_BaseManager.SizeLimit.ToString().PadLeft(25)));
            myGame.Send(msg.pm(PlayerName, "---------------------------------------------"));
        }
        private void show_Rounds(string PlayerName)
        {
            // Spit out round info from round list
            myGame.Send(msg.pm(PlayerName, "All Rounds in Game --------------------------"));
            for (int i = 0; i < m_BaseGame.AllRounds.Count; i++)
            {
                myGame.Send(msg.pm(PlayerName, "Round Number  :".PadRight(20) + (i + 1).ToString().PadLeft(25)));
                myGame.Send(msg.pm(PlayerName, "Date          :".PadRight(20) + m_BaseGame.AllRounds[i].StartTime.ToShortDateString().PadLeft(25)));
                myGame.Send(msg.pm(PlayerName, "Time          :".PadRight(20) + m_BaseGame.AllRounds[i].StartTime.ToShortTimeString().PadLeft(25)));
                myGame.Send(msg.pm(PlayerName, "Duration      :".PadRight(20) + m_BaseGame.AllRounds[i].TotalTimeFormatted.ToString().PadLeft(25)));
                myGame.Send(msg.pm(PlayerName, "Win Type      :".PadRight(20) + m_BaseGame.AllRounds[i].WinType.ToString().PadLeft(25)));
                if (m_BaseGame.AllRounds[i].WinType != WinType.NoCount)
                    myGame.Send(msg.pm(PlayerName, "Winner        :".PadRight(20) + (m_BaseGame.AllRounds[i].AlphaWon?"Alpha Team":"Bravo Team").ToString().PadLeft(25)));
                myGame.Send(msg.pm(PlayerName, "Base Number   :".PadRight(20) + m_BaseGame.AllRounds[i].BaseNumber.ToString().PadLeft(25)));
                myGame.Send(msg.pm(PlayerName, "---------------------------------------------"));
            }
        }
    }
}
/*
 *  Tracking every position packet and running collision on each packet is extremely inefficient.
 *  Since I am tracking other events too I need to change InBase status based on key events that would remove a player from base.
 *  Small summary of events, tasks to do, and what to track.
 *      InBase Tracking ---------------------------
 *          *-Detach/Attach     - run collision on host, Toggle InBase for both
 *          *-FreqChange        - Probably only a spectate event. Any freq/ship will result in center spawn
 *          *-Shipchange        - Probably only a spectate event. Any freq/ship will result in center spawn
 *          *-PlayerDeath       - Toggle player out of base - May not need this, spawn in center and trips InSafe
 *          *-PlayerPosition    - InSafe will trigger collision check. I can see if player is in center or in an enemy safe.
 */
