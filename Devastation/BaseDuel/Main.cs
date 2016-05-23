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
            this.m_CurrentGame = new BaseGame();
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
        }

        private ShortChat msg;
        
        private BaseGame m_CurrentGame;
        private Timer m_Timer;
        private double m_StartGameDelay;
        private double m_StartGameDelay_Elapsed;
        private double m_StartGameShow;
        private double m_TeamClearDelay;
        
        private BaseManager m_BaseManager;
        private Base m_Base;
        private Base m_Lobby;

        private ushort m_AlphaFreq, m_BravoFreq, m_MaxArenaFreq;
        private List<string> m_AlphaWaitList, m_BravoWaitList, m_BlockedList;
        private Queue<EventArgs> m_BaseGameEvents;
        private List<BaseGame> m_GamesPlayed;

        public Queue<EventArgs> Events
        {
            get { return m_BaseGameEvents; }
        }

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
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
                toggleBaseDuel(p, q);
                return q;
            }

            // Start baseduel
            if (data[1].Trim().ToLower() == "start")
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;

                // only start game if pre-game
                if (m_Timer.Enabled || m_CurrentGame.Status != BaseGameStatus.GameIdle)
                {
                    q.Enqueue(msg.pm(e.PlayerName, "There is currently a game in progress. Please wait until game is over before using this command."));
                    return q;
                }

                // start game - enough players
                if (canStartGame())
                {
                    m_BaseGameEvents.Enqueue(msg.macro("- Game starting in "+ m_StartGameDelay+" seconds -"));
                    setupTimer(true);
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
        private void startGame()
        {
            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                // Move players from wait list to teams
                while (m_AlphaWaitList.Count > 0)
                {
                    BasePlayer b = new BasePlayer();
                    b.PlayerName = m_AlphaWaitList[0];
                    b.Active = true;
                    b.InLobby = false;
                    m_CurrentGame.Round.AlphaTeam.TeamMembers.Add(b);
                    m_AlphaWaitList.RemoveAt(0);
                }
                while (m_BravoWaitList.Count > 0)
                {
                    BasePlayer b = new BasePlayer();
                    b.PlayerName = m_BravoWaitList[0];
                    b.Active = true;
                    b.InLobby = false;
                    m_CurrentGame.Round.BravoTeam.TeamMembers.Add(b);
                    m_BravoWaitList.RemoveAt(0);
                }
            }
            // get next round rdy
            else if (m_CurrentGame.Status == BaseGameStatus.GameIntermission)
                prepForNextRound();

            // find the first active player to use as reference on a team pm
            string a_name = m_CurrentGame.Round.AlphaTeam.TeamMembers.Find(item => item.Active).PlayerName;
            string b_name = m_CurrentGame.Round.BravoTeam.TeamMembers.Find(item => item.Active).PlayerName;

            // send players to start and reset ships
            m_BaseGameEvents.Enqueue(msg.teamPM(a_name,"?|warpto " + m_Base.AlphaStartX + " " + m_Base.AlphaStartY + "|shipreset"));
            m_BaseGameEvents.Enqueue(msg.teamPM(b_name, "?|warpto " + m_Base.BravoStartX + " " + m_Base.BravoStartY + "|shipreset"));

            int baseNum = m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1;
            m_BaseGameEvents.Enqueue(msg.macro("- Go Go Go! -    BaseNum[ "+baseNum+" ]"));
            // Record start time
            m_CurrentGame.Round.StartTime = DateTime.Now;
            // set status
            m_CurrentGame.Status = BaseGameStatus.GameOn;
        }

        private void toggleBaseDuel(DevaPlayer p, Queue<EventArgs> q)
        {
            // Turn On module
            if (m_CurrentGame == null)
            {
                // reset vars
                resetBaseDuel();

                // Send Arena Message
                q.Enqueue(msg.arena("[ BaseDuel ] Module loaded by staff - " + p.PlayerName));
                printOut_GameSettings(p.PlayerName, q);
                return;
            }

            // Turn Module Off
            m_CurrentGame = null;
            q.Enqueue(msg.arena("[ BaseDuel ] Module unloaded by staff - " + p.PlayerName));
        }

        // reset all vars to baseduel
        private void resetBaseDuel()
        {
            m_CurrentGame = new BaseGame();
            m_CurrentGame.AlphaFreq = m_AlphaFreq;
            m_CurrentGame.BravoFreq = m_BravoFreq;
            m_CurrentGame.Status = BaseGameStatus.GameIdle;

            m_AlphaWaitList = new List<string>();
            m_BravoWaitList = new List<string>();
        }

        // Add Player to wait list
        private void addPlayerToWaitList(DevaPlayer p, Queue<EventArgs> q)
        {
            // Join them to right list to keep it even
            if (m_AlphaWaitList.Count <= m_BravoWaitList.Count)
                m_AlphaWaitList.Add(p.PlayerName);
            else    m_BravoWaitList.Add(p.PlayerName);

            // What team are they on
            bool alpha = m_AlphaWaitList.Contains(p.PlayerName);

            // set them to right freq
            q.Enqueue(msg.pm(p.PlayerName, "?|setfreq " + (alpha ? m_AlphaFreq.ToString() : m_BravoFreq.ToString()) + "|shipreset"));
            q.Enqueue(msg.debugChan("Player[ " + p.PlayerName + " ] has been added to " + (alpha ? "Alpha" : "Bravo") + " wait list. AlphaCount[" + m_AlphaWaitList.Count + "] BravoCount[" + m_BravoWaitList.Count + "]"));
        }

        private void removePlayer(DevaPlayer p, Queue<EventArgs> q)
        {
            // dont know if i need anything more than just this - this will do for now
            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                if (isOnWaitList(p.PlayerName))
                    q.Enqueue(msg.debugChan("Removing Player[ " + p.PlayerName + " ] from wait list."));
                m_AlphaWaitList.Remove(p.PlayerName);
                m_BravoWaitList.Remove(p.PlayerName);
                return;
            }

            // If a game is in progress we want to set the player to inactive - this way we can save stats after player leaves
            if (m_CurrentGame.Status == BaseGameStatus.GameOn || m_CurrentGame.Status == BaseGameStatus.GameIntermission)
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

        private void RoundFinished( WinType winType, bool AlphaWon)
        {
            m_CurrentGame.Status = BaseGameStatus.GameIntermission;

            if (winType == WinType.NoCount)
            {
                m_BaseGameEvents.Enqueue(msg.macro("- No Count Round. -"));
            }
            else
            {
                // assign points
                if (AlphaWon) m_CurrentGame.AlphaScore++;
                else m_CurrentGame.BravoScore++;

                m_BaseGameEvents.Enqueue(msg.macro("- " + (AlphaWon ? "Alpha Team" : "Bravo Team") + " wins the round by: " + winType + ".  - Alpha[ " + m_CurrentGame.AlphaScore + " ][ " + m_CurrentGame.BravoScore + " ]Bravo -"));
            }

            m_CurrentGame.Round.AlphaWon = AlphaWon;
            m_CurrentGame.Round.WinType = winType;
            saveRound();

            //only load next base if it wasnt nocount
            if (winType != WinType.NoCount) loadNextBase();

            // Check for end of game
            if ((m_CurrentGame.AlphaScore >= m_CurrentGame.MinimumWin || m_CurrentGame.BravoScore >= m_CurrentGame.MinimumWin) && Math.Abs(m_CurrentGame.AlphaScore - m_CurrentGame.BravoScore) >= 2)
            {
                m_BaseGameEvents.Enqueue(msg.arena((m_CurrentGame.AlphaScore > m_CurrentGame.BravoScore ? "Alpha Team" : "Bravo Team") + " wins the game. Final Score Alpha[ " + m_CurrentGame.AlphaScore + " ] [ " + m_CurrentGame.BravoScore + " ]Bravo.   -Print out here."));
                m_CurrentGame.Status = BaseGameStatus.GameHold;
                saveGame();

                List<BasePlayer> aTeam = m_CurrentGame.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active);
                List<BasePlayer> bTeam = m_CurrentGame.Round.BravoTeam.TeamMembers.FindAll(item => item.Active);

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

                m_CurrentGame = new BaseGame();

                return;
            }

            m_BaseGameEvents.Enqueue(msg.macro("- Next round starts in " + m_StartGameDelay + " seconds. Change ships now if needed. -"));
            setupTimer(true);
        }

        private void saveRound()
        {
            // Copy over round so we can save it
            BaseRound saved = new BaseRound();
            saved.AlphaTeam = m_CurrentGame.Round.AlphaTeam;
            saved.BravoTeam = m_CurrentGame.Round.BravoTeam;
            saved.AlphaWon = m_CurrentGame.Round.AlphaWon;
            saved.BaseNumber = (short)(m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1);
            saved.StartTime = m_CurrentGame.Round.StartTime;
            saved.TotalTime = DateTime.Now - m_CurrentGame.Round.StartTime;
            saved.WinType = m_CurrentGame.Round.WinType;
            // Save it
            m_CurrentGame.AllRounds.Add(saved);
        }

        private void saveGame()
        {
            BaseGame saved = new BaseGame();
            saved.AllowAfterStartJoin = m_CurrentGame.AllowAfterStartJoin;
            saved.AllowSafeWin = m_CurrentGame.AllowSafeWin;
            saved.AllRounds = m_CurrentGame.AllRounds;
            saved.AlphaFreq = m_CurrentGame.AlphaFreq;
            saved.AlphaScore = m_CurrentGame.AlphaScore;
            saved.BravoFreq = m_CurrentGame.BravoFreq;
            saved.BravoScore = m_CurrentGame.BravoScore;
            saved.MinimumWin = m_CurrentGame.MinimumWin;
            saved.Round = m_CurrentGame.Round;
            saved.Status = m_CurrentGame.Status;
            saved.WinBy = m_CurrentGame.WinBy;
            m_GamesPlayed.Add(saved);
        }

        private void prepForNextRound()
        {
            // Update players
            for (int i = m_CurrentGame.Round.AlphaTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_CurrentGame.Round.AlphaTeam.TeamMembers[i].Active)
                    m_CurrentGame.Round.AlphaTeam.TeamMembers.Remove(m_CurrentGame.Round.AlphaTeam.TeamMembers[i]);
                else
                {
                    string name = m_CurrentGame.Round.AlphaTeam.TeamMembers[i].PlayerName;
                    m_CurrentGame.Round.AlphaTeam.TeamMembers[i] = new BasePlayer();
                    m_CurrentGame.Round.AlphaTeam.TeamMembers[i].PlayerName = name;
                    m_CurrentGame.Round.AlphaTeam.TeamMembers[i].Active = true;
                    m_CurrentGame.Round.AlphaTeam.TeamMembers[i].InLobby = true;
                }
            }
            for (int i = m_CurrentGame.Round.BravoTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_CurrentGame.Round.BravoTeam.TeamMembers[i].Active)
                    m_CurrentGame.Round.BravoTeam.TeamMembers.Remove(m_CurrentGame.Round.BravoTeam.TeamMembers[i]);
                else
                {
                    string name = m_CurrentGame.Round.BravoTeam.TeamMembers[i].PlayerName;
                    m_CurrentGame.Round.BravoTeam.TeamMembers[i] = new BasePlayer();
                    m_CurrentGame.Round.BravoTeam.TeamMembers[i].PlayerName = name;
                    m_CurrentGame.Round.BravoTeam.TeamMembers[i].Active = true;
                    m_CurrentGame.Round.BravoTeam.TeamMembers[i].InLobby = true;
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
            if (m_CurrentGame == null) return null;

            if (m_BlockedList.Contains(p.PlayerName)) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                if ( inRegion(p.Position,m_Lobby.BaseDimension) && ( p.Frequency <= m_MaxArenaFreq || inGameFreqs(p.Frequency)) && !isOnWaitList(p.PlayerName))
                {
                    if ((DateTime.Now - p.WarpStamp).TotalMilliseconds < 1500) return null;

                    p.WarpStamp = DateTime.Now;
                    addPlayerToWaitList(p, q);
                }
            }
            if (m_CurrentGame.Status == BaseGameStatus.GameOn)
            {
                bool InAlpha;
                BasePlayer b = getPlayer(p, out InAlpha);

                if (b == null) return q;

                if (inRegion(p.Position, m_Lobby.BaseDimension))
                {
                    if (inGameFreqs(p.Frequency))
                    {
                        b.InLobby = true;

                        //ignore if timer is already running
                        if (m_Timer.Enabled) return q;

                        // Check for BaseClear and start timer
                        bool alpha, bravo;
                        if (teamIsOut( out alpha, out bravo))   setupTimer(false);

                        m_BaseGameEvents.Enqueue(msg.debugChan("Starting Clear Timer"));
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
                        if ((DateTime.Now - m_CurrentGame.Round.StartTime).TotalMilliseconds >= 3000)
                            RoundFinished(WinType.SafeWin, InAlpha);
                    }
                }
            }
            return q;
        }

        public Queue<EventArgs> Event_PlayerLeft(DevaPlayer p)
        {
            // No need to deal with event if it is off
            if (m_CurrentGame == null) return null;

            if (m_BlockedList.Contains(p.PlayerName)) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            removePlayer(p, q);

            return q;
        }

        public Queue<EventArgs> Event_PlayerFreqChange(DevaPlayer p)
        {
            // No need to deal with event if it is off
            if (m_CurrentGame == null) return null;

            // Maybe leave this out? if player gets blocked and not removed?
            if (m_BlockedList.Contains(p.PlayerName)) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            if (inGameFreqs(p.OldFrequency))
                removePlayer(p, q);

            return q;
        }

        //----------------------------------------------------------------------//
        //                       Timer Stuff                                    //
        //----------------------------------------------------------------------//
        public void setupTimer(bool GameStart)
        {
            m_Timer = new Timer();

            // Set the timer for what we need it to do: Start or Clear Check
            if (GameStart)
            {
                m_StartGameDelay_Elapsed = m_StartGameDelay;
                m_Timer.Elapsed += new ElapsedEventHandler(CountDown_StartGame);
                m_Timer.Interval = 1000;
            }
            else
            {
                m_Timer.Elapsed += new ElapsedEventHandler(Timer_ClearCheck);
                m_Timer.Interval = m_TeamClearDelay * 1000;
            }
            m_Timer.Start();
        }

        public void CountDown_StartGame(object source, ElapsedEventArgs e)
        {
            m_StartGameDelay_Elapsed--;

            if (m_StartGameDelay_Elapsed <= m_StartGameShow && m_StartGameDelay_Elapsed != 0)
            {
                m_BaseGameEvents.Enqueue(msg.macro("- " + m_StartGameDelay_Elapsed + " -", SoundCodes.MessageAlarm));
            }

            if (m_StartGameDelay_Elapsed > 0) return;

            m_Timer.Stop();

            if (canStartGame())
            { startGame(); }
            else
            { m_BaseGameEvents.Enqueue(msg.macro("- Not enough players. Start Game cancelled. -")); }
        }

        public void Timer_ClearCheck(object source, ElapsedEventArgs e)
        {
            m_BaseGameEvents.Enqueue(msg.debugChan("- All out timer expired. -"));
            m_Timer.Stop();

            // make sure a team is still out
            bool alpha, bravo;

            if (teamIsOut(out alpha, out bravo))
            {
                RoundFinished(alpha == bravo?WinType.NoCount:WinType.BaseClear, !alpha);
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
            alpha = !(m_CurrentGame.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            bravo = !(m_CurrentGame.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            return alpha || bravo;
        }

        // Grab player from one of the teams
        private BasePlayer getPlayer( DevaPlayer p, out bool InAlpha)
        {
            BasePlayer b = m_CurrentGame.Round.AlphaTeam.TeamMembers.Find( item => item.PlayerName == p.PlayerName);

            if ( b != null)
            {
                InAlpha = true;
                return b;
            }

            b = m_CurrentGame.Round.BravoTeam.TeamMembers.Find(item => item.PlayerName == p.PlayerName);
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
            if (m_CurrentGame != null) return true;

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
            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                return m_AlphaWaitList.Count >= 1 && m_BravoWaitList.Count >= 1;
            }
            else if (m_CurrentGame.Status == BaseGameStatus.GameIntermission)
            {
                return  m_CurrentGame.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).Count >= 1 &&
                    m_CurrentGame.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).Count >= 1;
            }
            return false;
        }

        //----------------------------------------------------------------------//
        //                           PrintOuts                                  //
        //----------------------------------------------------------------------//
        private void printOut_GameSettings(string PlayerName, Queue<EventArgs> q)
        {
            // Show player how it was loaded
            q.Enqueue(msg.pm(PlayerName, "BaseDuel Settings ---------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Status           :".PadRight(20) + m_CurrentGame.Status.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Join after Start :".PadRight(20) + m_CurrentGame.AllowAfterStartJoin.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Safe win enabled :".PadRight(20) + m_CurrentGame.AllowSafeWin.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Alpha Team Name  :".PadRight(20) + m_CurrentGame.Round.AlphaTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_CurrentGame.Round.AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_CurrentGame.AlphaFreq.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Bravo Team Name  :".PadRight(20) + m_CurrentGame.Round.BravoTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_CurrentGame.Round.BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_CurrentGame.BravoFreq.ToString().PadLeft(25)));
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
            for (int i = 0; i < m_CurrentGame.AllRounds.Count; i++)
            {
                q.Enqueue(msg.pm(PlayerName, "Round Number  :".PadRight(20) + (i + 1).ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Date          :".PadRight(20) + m_CurrentGame.AllRounds[i].StartTime.ToShortDateString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Time          :".PadRight(20) + m_CurrentGame.AllRounds[i].StartTime.ToShortTimeString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Duration      :".PadRight(20) + m_CurrentGame.AllRounds[i].TotalTimeFormatted.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Win Type      :".PadRight(20) + m_CurrentGame.AllRounds[i].WinType.ToString().PadLeft(25)));
                if (m_CurrentGame.AllRounds[i].WinType != WinType.NoCount)
                    q.Enqueue(msg.pm(PlayerName, "Winner        :".PadRight(20) + (m_CurrentGame.AllRounds[i].AlphaWon?"Alpha Team":"Bravo Team").ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "Base Number   :".PadRight(20) + m_CurrentGame.AllRounds[i].BaseNumber.ToString().PadLeft(25)));
                q.Enqueue(msg.pm(PlayerName, "---------------------------------------------"));
            }
        }
    }
}
