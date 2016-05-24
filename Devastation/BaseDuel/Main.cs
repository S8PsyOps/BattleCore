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
        public Main( byte[] MapData, bool debugMode)
        {
            this.msg = new ShortChat();
            this.msg.DebugMode = debugMode;
            this.m_Timer = new Timer();
            this.m_BaseManager = new BaseManager(MapData);
            this.m_Base = m_BaseManager.CurrentBase;
            this.m_Lobby = m_BaseManager.Lobby;
            this.m_AlphaFreq = 887;
            this.m_BravoFreq = 889;
            this.m_MaxArenaFreq = 100;
            this.m_AlphaWaitList = new List<string>();
            this.m_BravoWaitList = new List<string>();
            this.m_BlockedList = new List<string>();
            this.m_StartGameDelay = 10;
            this.m_StartGameShow = 3;
            this.m_TeamClearDelay = 5;
            this.m_BaseGameEvents = new Queue<EventArgs>();
            this.m_GamesPlayed = new List<BaseGame>();
            this.m_Game = new BaseGame();
            this.m_Game.AlphaFreq = m_AlphaFreq;
            this.m_Game.BravoFreq = m_BravoFreq;
        }

        private ShortChat msg;                      // Module to make sending chat messages easier
        private BaseGame m_Game;                    // This holds all the baseduel game info
        
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
        
        private List<string> m_AlphaWaitList;       // Wait lists to join game
        private List<string> m_BravoWaitList;       // ----
        private List<string> m_BlockedList;         // Users to ignore - use this for a ban/block or for any diff games that may go on (baserace etc)
        
        private Queue<EventArgs> m_BaseGameEvents;  // This is a queue for events that need to be sent - deva main will send them out as soon as its used
        
        private List<BaseGame> m_GamesPlayed;       // List of games that have been recorded

        /// <summary>
        /// Any chat message or event that you need to send out from BaseDuel.Main stack it in here
        /// </summary>
        public Queue<EventArgs> Events
        {
            get { return m_BaseGameEvents; }
        }

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
        public Queue<EventArgs> BaseDuelCommands(DevaPlayer p, ChatEvent e)
        {
            if (m_BlockedList.Contains(p.PlayerName)) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            string[] data = e.Message.Split(' ');

            if (data[0].Trim().ToLower() == "!baseduel" && data.Length < 2)
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;


                return q;
            }

            if (data[1].Trim().ToLower() == "settings")
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;

                printOut_GameSettings(p.PlayerName, q);

                return q;
            }

            if (data[1].Trim().ToLower() == "shuffle")
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;

                doShuffleTeams();

                return q;
            }

            if (data[1].Trim().ToLower() == "reset")
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;

                if (m_Game.Status == BaseGameStatus.GameOn)
                    game_Reset();

                return q;
            }

            if (data[1].Trim().ToLower() == "rounds")
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;

                printOut_Rounds(p.PlayerName, q);

                return q;
            }

            // Toggle baseduel on and off
            if (data[1].Trim().ToLower() == "toggle" && isMod(q, e, ModLevels.Mod))
            {
                baseduel_Toggle(p, q);
                return q;
            }

            // Start baseduel
            if (data[1].Trim().ToLower() == "start")
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;

                // only start game if pre-game
                if (m_Timer.Enabled || m_Game.Status != BaseGameStatus.GameIdle)
                {
                    q.Enqueue(msg.pm(e.PlayerName, "There is currently a game in progress. Please wait until game is over before using this command."));
                    return q;
                }

                // start game - enough players
                if (canStartGame())
                {
                    m_BaseGameEvents.Enqueue(msg.macro("- Game starting in "+ m_StartGameDelay+" seconds -"));
                    timer_Setup(true);
                    return q;
                }
                else // Not enough players
                {
                    q.Enqueue(msg.pm(e.PlayerName,"In order to start a game you must have at least 1 player on each team."));
                    return q;
                }
            }

            return null;
        }
        private void doShuffleTeams()
        {
            // dump both wait lists into one list
            List<string> allPlayers = new List<string>();
            for (int i = 0; i < m_AlphaWaitList.Count; i++)
            { allPlayers.Add(m_AlphaWaitList[i]); }
            for (int i = 0; i < m_BravoWaitList.Count; i++)
            { allPlayers.Add(m_BravoWaitList[i]); }

            // Shuffle the list we just created
            Random ran = new Random();
            for (int h = 0; h < 10; h++)
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    string temp = allPlayers[i];
                    int r = ran.Next(i, allPlayers.Count);
                    allPlayers[i] = allPlayers[r];
                    allPlayers[r] = temp;
                }
            }

            // Clear both lists
            m_AlphaWaitList = new List<string>();
            m_BravoWaitList = new List<string>();

            // Kick them to center in random order
            //and let bot reAdd them
            while (allPlayers.Count > 0)
            {
                m_BaseGameEvents.Enqueue(msg.pm(allPlayers[0], "?|setfreq 0|prize fullcharge"));
                allPlayers.RemoveAt(0);
            }
        }
        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        private void baseduel_Toggle(DevaPlayer p, Queue<EventArgs> q)
        {
            // Turn On module
            if (m_Game == null)
            {
                // reset vars
                baseduel_Reset();

                // Send Arena Message
                q.Enqueue(msg.arena("[ BaseDuel ] Module loaded by staff - " + p.PlayerName));
                printOut_GameSettings(p.PlayerName, q);
                return;
            }

            // Turn Module Off
            m_Game = null;
            q.Enqueue(msg.arena("[ BaseDuel ] Module unloaded by staff - " + p.PlayerName));
        }

        // reset all vars to baseduel
        private void baseduel_Reset()
        {
            m_Game = new BaseGame();
            m_Game.AlphaFreq = m_AlphaFreq;
            m_Game.BravoFreq = m_BravoFreq;
            m_Game.Status = BaseGameStatus.GameIdle;

            m_AlphaWaitList = new List<string>();
            m_BravoWaitList = new List<string>();
        }

        private void game_Start()
        {
            if (m_Game.Status == BaseGameStatus.GameIdle)
            {
                // Move players from wait list to teams
                while (m_AlphaWaitList.Count > 0)
                {
                    BasePlayer b = new BasePlayer();
                    b.PlayerName = m_AlphaWaitList[0];
                    b.Active = true;
                    b.InLobby = false;
                    m_Game.Round.AlphaTeam.TeamMembers.Add(b);
                    m_AlphaWaitList.RemoveAt(0);
                }
                while (m_BravoWaitList.Count > 0)
                {
                    BasePlayer b = new BasePlayer();
                    b.PlayerName = m_BravoWaitList[0];
                    b.Active = true;
                    b.InLobby = false;
                    m_Game.Round.BravoTeam.TeamMembers.Add(b);
                    m_BravoWaitList.RemoveAt(0);
                }
            }
            // get next round rdy
            else if (m_Game.Status == BaseGameStatus.GameIntermission)
                round_ResetPLayers();

            // find the first active player to use as reference on a team pm
            string a_name = m_Game.Round.AlphaTeam.TeamMembers.Find(item => item.Active).PlayerName;
            string b_name = m_Game.Round.BravoTeam.TeamMembers.Find(item => item.Active).PlayerName;

            // send players to start and reset ships
            m_BaseGameEvents.Enqueue(msg.teamPM(a_name,"?|warpto " + m_Base.AlphaStartX + " " + m_Base.AlphaStartY + "|shipreset"));
            m_BaseGameEvents.Enqueue(msg.teamPM(b_name, "?|warpto " + m_Base.BravoStartX + " " + m_Base.BravoStartY + "|shipreset"));

            int baseNum = m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1;
            m_BaseGameEvents.Enqueue(msg.macro("- Go Go Go! -    BaseNum[ "+baseNum+" ]"));
            // Record start time
            m_Game.Round.StartTime = DateTime.Now;
            // set status
            m_Game.Status = BaseGameStatus.GameOn;
        }

        private void game_Save()
        {
            BaseGame saved = new BaseGame();
            saved.AllowAfterStartJoin = m_Game.AllowAfterStartJoin;
            saved.AllowSafeWin = m_Game.AllowSafeWin;
            saved.AllRounds = m_Game.AllRounds;
            saved.AlphaFreq = m_Game.AlphaFreq;
            saved.AlphaScore = m_Game.AlphaScore;
            saved.BravoFreq = m_Game.BravoFreq;
            saved.BravoScore = m_Game.BravoScore;
            saved.MinimumWin = m_Game.MinimumWin;
            saved.Round = m_Game.Round;
            saved.Status = m_Game.Status;
            saved.WinBy = m_Game.WinBy;
            m_GamesPlayed.Add(saved);
        }

        private void game_Reset()
        {
            List<BasePlayer> aTeam = m_Game.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active);
            List<BasePlayer> bTeam = m_Game.Round.BravoTeam.TeamMembers.FindAll(item => item.Active);

            while (aTeam.Count > 0)
            {
                m_AlphaWaitList.Add(aTeam[0].PlayerName);
                aTeam.RemoveAt(0);
            }

            while (bTeam.Count > 0)
            {
                m_BravoWaitList.Add(bTeam[0].PlayerName);
                bTeam.RemoveAt(0);
            }

            m_Game = new BaseGame();
            m_Game.AlphaFreq = m_AlphaFreq;
            m_Game.BravoFreq = m_BravoFreq;
            m_Game.Status = BaseGameStatus.GameIdle;
            m_BaseGameEvents.Enqueue(msg.pm(m_AlphaWaitList[0], "?prize warp"));
            m_BaseGameEvents.Enqueue(msg.pm(m_BravoWaitList[0], "?prize warp"));

            if (m_Timer.Enabled) m_Timer.Stop();
        }

        private void round_Finished(WinType winType, bool AlphaWon)
        {
            m_Game.Status = BaseGameStatus.GameIntermission;

            if (winType == WinType.NoCount)
            {
                m_BaseGameEvents.Enqueue(msg.macro("- No Count Round. -"));
            }
            else
            {
                // assign points
                if (AlphaWon) m_Game.AlphaScore++;
                else m_Game.BravoScore++;

                m_BaseGameEvents.Enqueue(msg.macro("- " + (AlphaWon ? "Alpha Team" : "Bravo Team") + " wins the round by: " + winType + ".  - Alpha[ " + m_Game.AlphaScore + " ][ " + m_Game.BravoScore + " ]Bravo -"));
            }

            m_Game.Round.AlphaWon = AlphaWon;
            m_Game.Round.WinType = winType;
            round_Save();

            //only load next base if it wasnt nocount
            if (winType != WinType.NoCount) loadNextBase();

            // Check for end of game
            if ((m_Game.AlphaScore >= m_Game.MinimumWin || m_Game.BravoScore >= m_Game.MinimumWin) && Math.Abs(m_Game.AlphaScore - m_Game.BravoScore) >= 2)
            {
                m_BaseGameEvents.Enqueue(msg.arena((m_Game.AlphaScore > m_Game.BravoScore ? "Alpha Team" : "Bravo Team") + " wins the game. Final Score Alpha[ " + m_Game.AlphaScore + " ] [ " + m_Game.BravoScore + " ]Bravo.   -Print out here."));
                //m_BDGame.Status = BaseGameStatus.GameHold;
                game_Save();
                game_Reset();
                return;
            }

            m_BaseGameEvents.Enqueue(msg.macro("- Next round starts in " + m_StartGameDelay + " seconds. Change ships now if needed. -"));
            timer_Setup(true);
        }

        private void round_Save()
        {
            // Copy over round so we can save it
            BaseRound saved = new BaseRound();
            saved.AlphaTeam = m_Game.Round.AlphaTeam;
            saved.BravoTeam = m_Game.Round.BravoTeam;
            saved.AlphaWon = m_Game.Round.AlphaWon;
            saved.BaseNumber = (short)(m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1);
            saved.StartTime = m_Game.Round.StartTime;
            saved.TotalTime = DateTime.Now - m_Game.Round.StartTime;
            saved.WinType = m_Game.Round.WinType;
            // Save it
            m_Game.AllRounds.Add(saved);
        }

        private void round_ResetPLayers()
        {
            // Update players
            for (int i = m_Game.Round.AlphaTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_Game.Round.AlphaTeam.TeamMembers[i].Active)
                    m_Game.Round.AlphaTeam.TeamMembers.Remove(m_Game.Round.AlphaTeam.TeamMembers[i]);
                else
                {
                    string name = m_Game.Round.AlphaTeam.TeamMembers[i].PlayerName;
                    m_Game.Round.AlphaTeam.TeamMembers[i] = new BasePlayer();
                    m_Game.Round.AlphaTeam.TeamMembers[i].PlayerName = name;
                    m_Game.Round.AlphaTeam.TeamMembers[i].Active = true;
                    m_Game.Round.AlphaTeam.TeamMembers[i].InLobby = true;
                }
            }
            for (int i = m_Game.Round.BravoTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_Game.Round.BravoTeam.TeamMembers[i].Active)
                    m_Game.Round.BravoTeam.TeamMembers.Remove(m_Game.Round.BravoTeam.TeamMembers[i]);
                else
                {
                    string name = m_Game.Round.BravoTeam.TeamMembers[i].PlayerName;
                    m_Game.Round.BravoTeam.TeamMembers[i] = new BasePlayer();
                    m_Game.Round.BravoTeam.TeamMembers[i].PlayerName = name;
                    m_Game.Round.BravoTeam.TeamMembers[i].Active = true;
                    m_Game.Round.BravoTeam.TeamMembers[i].InLobby = true;
                }
            }
        }

        // Add Player to wait list
        private void player_AddToWaitList(DevaPlayer p, Queue<EventArgs> q)
        {
            // Join them to right list to keep it even
            if (m_AlphaWaitList.Count <= m_BravoWaitList.Count)
                m_AlphaWaitList.Add(p.PlayerName);
            else    m_BravoWaitList.Add(p.PlayerName);

            // What team are they on
            bool alpha = m_AlphaWaitList.Contains(p.PlayerName);

            // set them to right freq
            q.Enqueue(msg.pm(p.PlayerName, "?|setfreq " + (alpha ? m_Game.AlphaFreq.ToString() : m_Game.BravoFreq.ToString()) + "|shipreset"));
            q.Enqueue(msg.debugChan("Player[ " + p.PlayerName + " ] has been added to " + (alpha ? "Alpha" : "Bravo") + " wait list. AlphaCount[" + m_AlphaWaitList.Count + "] BravoCount[" + m_BravoWaitList.Count + "]"));
        }

        private void player_Remove(DevaPlayer p, Queue<EventArgs> q)
        {
            // dont know if i need anything more than just this - this will do for now
            if (m_Game.Status == BaseGameStatus.GameIdle)
            {
                if (isOnWaitList(p.PlayerName))
                    q.Enqueue(msg.debugChan("Removing Player[ " + p.PlayerName + " ] from wait list."));
                m_AlphaWaitList.Remove(p.PlayerName);
                m_BravoWaitList.Remove(p.PlayerName);
                return;
            }

            // If a game is in progress we want to set the player to inactive - this way we can save stats after player leaves
            if (m_Game.Status == BaseGameStatus.GameOn || m_Game.Status == BaseGameStatus.GameIntermission)
            {
                bool InAlpha;
                BasePlayer b = getPlayer(p, out InAlpha);

                if (b != null)
                {
                    q.Enqueue(msg.debugChan("Player[ " + p.PlayerName + " ] found in "+(InAlpha?"Alpha":"Bravo")+" Team. Player set to inactive."));
                    b.Active = false;
                }
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
        public Queue<EventArgs> Event_PlayerPosition(DevaPlayer p)
        {
            // No need to deal with event if it is off
            if (m_Game == null) return null;

            if (m_BlockedList.Contains(p.PlayerName)) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            if (m_Game.Status == BaseGameStatus.GameIdle)
            {
                if ( inRegion(p.Position,m_Lobby.BaseDimension) && ( p.Frequency <= m_MaxArenaFreq || inGameFreqs(p.Frequency)) && !isOnWaitList(p.PlayerName))
                {
                    if ((DateTime.Now - p.WarpStamp).TotalMilliseconds < 1500) return null;

                    p.WarpStamp = DateTime.Now;
                    player_AddToWaitList(p, q);
                }
            }
            if (m_Game.Status == BaseGameStatus.GameOn)
            {
                bool InAlpha;
                BasePlayer b = getPlayer(p, out InAlpha);

                // Allow players to join after game start
                if (b == null)
                {
                    if (m_Game.AllowAfterStartJoin && inRegion(p.Position, m_Lobby.BaseDimension))
                    {
                        b = new BasePlayer();
                        b.PlayerName = p.PlayerName;
                        b.InLobby = true;
                        b.Active = true;

                        int aCount = m_Game.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).Count;
                        int bCount = m_Game.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).Count;

                        if (aCount <= bCount)
                            m_Game.Round.AlphaTeam.TeamMembers.Add(b);
                        else
                            m_Game.Round.BravoTeam.TeamMembers.Add(b);

                        q.Enqueue(msg.pm(p.PlayerName, "?|setfreq " + (aCount <= bCount ? m_AlphaFreq.ToString() : m_BravoFreq.ToString()) + "|shipreset"));

                        return q;
                    }
                    return q;
                }

                if (inRegion(p.Position, m_Lobby.BaseDimension))
                {
                    if (inGameFreqs(p.Frequency))
                    {
                        b.InLobby = true;

                        //ignore if timer is already running
                        if (m_Timer.Enabled) return q;

                        // Check for BaseClear and start timer
                        bool alpha, bravo;
                        if (teamIsOut( out alpha, out bravo))   timer_Setup(false);
                    }
                    else
                    {
                        b.Active = true;
                        b.InLobby = true;
                        q.Enqueue(msg.pm(p.PlayerName,"?|setfreq " + (InAlpha?m_AlphaFreq.ToString():m_BravoFreq.ToString()) + "|shipreset"));
                    }
                }
                else if (inRegion(p.Position, m_Base.BaseDimension))
                {
                    b.InLobby = false;

                    if (inRegion(p.Position, (InAlpha ? m_BaseManager.CurrentBase.BravoSafe : m_BaseManager.CurrentBase.AlphaSafe)))
                    {
                        // adding an extra 3 second start for BaseClear check - if not timer gets set auto at start
                        if ((DateTime.Now - m_Game.Round.StartTime).TotalMilliseconds >= 3000)
                            round_Finished(WinType.SafeWin, InAlpha);
                    }
                }
            }
            return q;
        }

        public Queue<EventArgs> Event_PlayerLeft(DevaPlayer p)
        {
            // No need to deal with event if it is off
            if (m_Game == null) return null;

            if (m_BlockedList.Contains(p.PlayerName)) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            player_Remove(p, q);

            return q;
        }

        public Queue<EventArgs> Event_PlayerFreqChange(DevaPlayer p)
        {
            // No need to deal with event if it is off
            if (m_Game == null) return null;

            // Maybe leave this out? if player gets blocked and not removed?
            if (m_BlockedList.Contains(p.PlayerName)) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            if (inGameFreqs(p.OldFrequency))
                player_Remove(p, q);

            return q;
        }

        //----------------------------------------------------------------------//
        //                       Timer Stuff                                    //
        //----------------------------------------------------------------------//
        private void timer_Setup(bool GameStart)
        {
            m_Timer = new Timer();

            // Set the timer for what we need it to do: Start or Clear Check
            if (GameStart)
            {
                m_StartGameDelay_Elapsed = m_StartGameDelay;
                m_Timer.Elapsed += new ElapsedEventHandler(timer_StartGame);
                m_Timer.Interval = 1000;
            }
            else
            {
                m_BaseGameEvents.Enqueue(msg.debugChan("Starting Clear Timer"));
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
                m_BaseGameEvents.Enqueue(msg.macro("- " + m_StartGameDelay_Elapsed + " -", SoundCodes.MessageAlarm));
            }

            if (m_StartGameDelay_Elapsed > 0) return;

            m_Timer.Stop();

            if (canStartGame())
            { game_Start(); }
            else
            { m_BaseGameEvents.Enqueue(msg.macro("- Not enough players. Start Game cancelled. -")); }
        }

        private void timer_BaseClear(object source, ElapsedEventArgs e)
        {
            m_BaseGameEvents.Enqueue(msg.debugChan("- All out timer expired. -"));
            m_Timer.Stop();

            // make sure a team is still out
            bool alpha, bravo;

            if (teamIsOut(out alpha, out bravo))
            {
                round_Finished(alpha == bravo?WinType.NoCount:WinType.BaseClear, !alpha);
            }
        }

        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        // Toggle debug
        public void setDebug(bool newMode)
        {
            msg.DebugMode = newMode;
        }

        private bool teamIsOut(out bool alpha, out bool bravo)
        {
            // make sure a team is still out
            alpha = !(m_Game.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            bravo = !(m_Game.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            return alpha || bravo;
        }

        // Grab player from one of the teams
        private BasePlayer getPlayer( DevaPlayer p, out bool InAlpha)
        {
            BasePlayer b = m_Game.Round.AlphaTeam.TeamMembers.Find( item => item.PlayerName == p.PlayerName);

            if ( b != null)
            {
                InAlpha = true;
                return b;
            }

            b = m_Game.Round.BravoTeam.TeamMembers.Find(item => item.PlayerName == p.PlayerName);
            InAlpha = false;
            return b;
        }

        // Simple collision check
        private bool inRegion(PlayerPositionEvent p, ushort[] region)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        // checking if player is mod - if not sends back message
        private bool isMod(Queue<EventArgs> q, ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod)
                return true;

            q.Enqueue(msg.pm(e.PlayerName,"You do not have access to this command. This is a staff command. Required Moerator level: [ "+mod+" ]."));
            return false;
        }

        // Player is registered on wait list
        private bool isOnWaitList(string PlayerName)
        {
            return m_AlphaWaitList.Contains(PlayerName) || m_BravoWaitList.Contains(PlayerName);
        }

        // Checks to see if module is loaded - returns message if not
        private bool moduleIsOn(Queue<EventArgs> q, ChatEvent e)
        {
            if (m_Game != null) return true;

            q.Enqueue(msg.pm(e.PlayerName,"Module is currently not loaded."));
            return false;
        }

        // player is in a game freq
        private bool inGameFreqs(int freq)
        {
            return freq == m_AlphaFreq || freq == m_BravoFreq;
        }

        // Player Count check to make sure we can start game
        private bool canStartGame()
        {
            if (m_Game.Status == BaseGameStatus.GameIdle)
               return m_AlphaWaitList.Count >= 1 && m_BravoWaitList.Count >= 1;    
            
            if (m_Game.Status == BaseGameStatus.GameIntermission)
                return  m_Game.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).Count >= 1 &&
                    m_Game.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).Count >= 1;
            
            return false;
        }

        //----------------------------------------------------------------------//
        //                           PrintOuts                                  //
        //----------------------------------------------------------------------//
        private void printOut_GameSettings(string PlayerName, Queue<EventArgs> q)
        {
            // Show player how it was loaded
            q.Enqueue(msg.pm(PlayerName, "BaseDuel Settings ---------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Status           :".PadRight(20) + m_Game.Status.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Join after Start :".PadRight(20) + m_Game.AllowAfterStartJoin.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Safe win enabled :".PadRight(20) + m_Game.AllowSafeWin.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Alpha Team Name  :".PadRight(20) + m_Game.Round.AlphaTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_Game.Round.AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_Game.AlphaFreq.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Bravo Team Name  :".PadRight(20) + m_Game.Round.BravoTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_Game.Round.BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_Game.BravoFreq.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Base Loader Settings -----------------"));
            q.Enqueue(msg.pm(PlayerName, "Current Base     :".PadRight(20) + (m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1).ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Total Bases      :".PadRight(20) + m_BaseManager.Bases.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Sorting Mode     :".PadRight(20) + m_BaseManager.Mode.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Size Restrictions:".PadRight(20) + m_BaseManager.SizeLimit.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "---------------------------------------------"));
        }
        private void printOut_Rounds(string PlayerName, Queue<EventArgs> q)
        {
            // Spit out round info from round list
            q.Enqueue(msg.pm(PlayerName, "All Rounds in Game --------------------------"));
            for (int i = 0; i < m_Game.AllRounds.Count; i++)
            {
                q.Enqueue(msg.pm(PlayerName, "Round Number  :".PadRight(20) + (i + 1).ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Date          :".PadRight(20) + m_Game.AllRounds[i].StartTime.ToShortDateString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Time          :".PadRight(20) + m_Game.AllRounds[i].StartTime.ToShortTimeString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Duration      :".PadRight(20) + m_Game.AllRounds[i].TotalTimeFormatted.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Win Type      :".PadRight(20) + m_Game.AllRounds[i].WinType.ToString().PadLeft(25)));
                if (m_Game.AllRounds[i].WinType != WinType.NoCount)
                    q.Enqueue(msg.pm(PlayerName, "Winner        :".PadRight(20) + (m_Game.AllRounds[i].AlphaWon?"Alpha Team":"Bravo Team").ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Base Number   :".PadRight(20) + m_Game.AllRounds[i].BaseNumber.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "---------------------------------------------"));
            }
        }
    }
}
