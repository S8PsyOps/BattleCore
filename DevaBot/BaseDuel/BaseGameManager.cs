using System;
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
    class BaseGameManager
    {
        public BaseGameManager( byte[] MapData, bool debugMode)
        {

            this.msg = new ShortChat();
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

            // Just an extra check to make sure we have a command to parse
            if (!e.Message.Contains(" ")) return q;

            if (data[1].Trim().ToLower() == "settings")
            {
                // Module is off
                if (!moduleIsOn(q, e)) return q;

                printOut_GameSettings(p.PlayerName, q);

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
                if (m_CurrentGame.Status != BaseGameStatus.GameIdle)
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

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
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
            if (m_BlockedList.Contains(p.PlayerName)) return null;

            // No need to deal with event if it is off
            if (m_CurrentGame == null) return null;

            Queue<EventArgs> q = new Queue<EventArgs>();

            if (m_CurrentGame.Status == BaseGameStatus.GameIdle)
            {
                if ( inRegion(p.Position,m_Lobby.BaseDimension) && ( p.Frequency <= m_MaxArenaFreq || inGameFreqs(p.Frequency)) && !isOnWaitList(p.PlayerName))
                {
                    addPlayerToWaitList(p, q);
                }
            }

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
            {
                m_CurrentGame.Status = BaseGameStatus.GameOn;
                m_BaseGameEvents.Enqueue(msg.macro("- Go Go Go! -    start code here."));
            }
            else
                m_BaseGameEvents.Enqueue(msg.macro("- Not enough players. Start Game cancelled. -"));
        }
        public void Timer_ClearCheck(object source, ElapsedEventArgs e)
        {
        }
        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        // Toggle debug
        public void setDebug(bool newMode)
        {
            msg.DebugMode = newMode;
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
                if (m_AlphaWaitList.Count >= 1 && m_BravoWaitList.Count >= 1)
                    return true;
                return false;
            }
            else if (m_CurrentGame.Status == BaseGameStatus.GameIntermission)
            {
                List<BasePlayer> a = m_CurrentGame.CurrentMatch.AlphaTeam.TeamMembers;
                List<BasePlayer> b = m_CurrentGame.CurrentMatch.BravoTeam.TeamMembers;

                int aCount = 0;
                int bCount = 0;

                for (int i = 0; i < a.Count; i++)
                    if (a[i].Active)
                        aCount++;
                for (int i = 0; i < b.Count; i++)
                    if (b[i].Active)
                        bCount++;

                if (aCount >= 1 && bCount >= 1)
                    return true;
                return false;
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
            q.Enqueue(msg.pm(PlayerName, "Alpha Team Name  :".PadRight(20) + m_CurrentGame.CurrentMatch.AlphaTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Alpha Frequency  :".PadRight(20) + m_CurrentGame.AlphaFreq.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "------------------------"));
            q.Enqueue(msg.pm(PlayerName, "Bravo Team Name  :".PadRight(20) + m_CurrentGame.CurrentMatch.BravoTeam.TeamName.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Bravo Frequency  :".PadRight(20) + m_CurrentGame.BravoFreq.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Base Loader Settings -----------------"));
            q.Enqueue(msg.pm(PlayerName, "Current Base     :".PadRight(20) + (m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1).ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Total Bases      :".PadRight(20) + m_BaseManager.Bases.Count.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Sorting Mode     :".PadRight(20) + m_BaseManager.Mode.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "Size Restrictions:".PadRight(20) + m_BaseManager.SizeLimit.ToString().PadLeft(25)));
            q.Enqueue(msg.pm(PlayerName, "---------------------------------------------"));
        }
    }
}
