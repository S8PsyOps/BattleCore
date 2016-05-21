using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;
using System.Timers;

namespace Devastation.BaseDuel
{
    class BaseDuel
    {
        public BaseDuel(BaseManager bm)
        {
            this.m_GameMode = GameMode.AhmadMode;
            this.msg = new ShortChat();
            this.m_BaseManager = bm;
            this.m_BlockedList = new List<string>();
            this.m_GameStatus = GameStatus.GameIdle;
            this.m_CountDownSeconds = 10;
            this.m_CountDownShow = 3;
            this.m_GameQEvents = new Queue<EventArgs>();
            this.m_CountDown = new Timer();
            this.m_TeamOutTimer = new Timer();
            this.m_TeamOutTimerSetting = 5;
            this.m_CurrentGame = new BaseGame();
            this.m_AlphaWaitList = new List<string>();
            this.m_BravoWaitList = new List<string>();
            this.m_AlphaFreq = 887;
            this.m_BravoFreq = 889;
        }

        private ShortChat msg;                      // My module to make sending messages easier
        private GameMode m_GameMode;                // Think ill use this for preset settings
        private BaseManager m_BaseManager;          // Controls base loading
        private List<string> m_BlockedList;         // A way to prevent a player from interacting with baseduel
        private GameStatus m_GameStatus;            // Current status of game
        private Queue<EventArgs> m_GameQEvents;     // Any chat or commands that need to be sent using timer
        private Timer m_CountDown;                  // Count Down Timer
        private int m_CountDownSeconds;             // Seconds before game starts
        private int m_CountDownSeconds_Elapsed;     // Used to count down - no need to set
        private int m_CountDownShow;                // what part of the countdown you want to show. If set to (3) =>  3..2..1
        private Timer m_TeamOutTimer;               // TeamOut timer
        private int m_TeamOutTimerSetting;          // Seconds before TeamOut activates

        private BaseGame m_CurrentGame;
        private ushort m_AlphaFreq, m_BravoFreq;
        private List<string> m_AlphaWaitList, m_BravoWaitList;

        // Shuffle the wait list.
        public void Shuffle(ChatEvent c)
        {
            // Make sure the start timer isnt already running
            if (m_CountDown.Enabled)
            { 
                m_GameQEvents.Enqueue(msg.pm(c.PlayerName, "Game Start Timer has started. You can not use this command at this time."));
                return;
            }
            
            // Make sure command is used only before game
            if (m_GameStatus != GameStatus.GameIdle)
            { 
                m_GameQEvents.Enqueue(msg.pm(c.PlayerName, "Game has started. You can not use this command at this time."));
                return;
            }

            // Make sure there is at least 3 Players
            if (m_AlphaWaitList.Count + m_BravoWaitList.Count > 2)
            { 
                ShuffleTeams();
                return;
            }  

            m_GameQEvents.Enqueue(msg.pm(c.PlayerName, "You do not have enough players to use this command."));    
        }

        // Starts game
        private void StartGame()
        {
            // Game hasn't started yet
            if (m_GameStatus == GameStatus.GameIdle)
            {
                // Initialize the game container
                m_CurrentGame = new BaseGame();
                m_CurrentGame.CreateFreqs(m_AlphaFreq, m_BravoFreq);
                m_CurrentGame.CreateTeams(m_AlphaWaitList, m_BravoWaitList);
            }
            // Game is inbetween matches
            else if (m_GameStatus == GameStatus.GameIntermission){}

            // Warp players to start points
            m_CurrentGame.WarpPlayersToStart(m_BaseManager.LoadedBase, m_GameQEvents);

            // Turn game status to inprogress
            m_GameStatus = GameStatus.GameInProgress;
        }

        // End the game and assign the way it was won/lost
        private void EndGame(bool Alpha, WinType winType)
        {
            // Either alpha or bravo wins
            if (winType != WinType.NoCount)
            {
                m_GameQEvents.Enqueue(msg.macro(("[ " + (Alpha ? "AlphaTeam" : "Bravo Team") + " ] wins [ " + winType + " ]. Game PrintOut here -")));
                // Only load next base it is not NoCount
                m_BaseManager.LoadNextBase();
            }
            // No Count
            else
            {
                m_GameQEvents.Enqueue(msg.macro(("[ No Count ]. Game PrintOut here -")));
            }

            // Make sure you save match inside BaseGame
            m_CurrentGame.MatchOver(Alpha, winType);
            // Change status
            m_GameStatus = GameStatus.GameIntermission;
            m_GameQEvents.Enqueue(msg.macro("Match will begin in " + m_CountDownSeconds + " seconds."));
            // Start game again
            GameStartTimer();
            m_GameQEvents.Enqueue(msg.macro("Matches on List[ " + m_CurrentGame.MatchCount + " ]"));
        }

        private void ShuffleTeams()
        {
            // dump both wait lists into one list
            List<string> allPlayers = new List<string>();
            for (int i = 0; i < m_AlphaWaitList.Count; i++)
            {   allPlayers.Add(m_AlphaWaitList[i]);   }
            for (int i = 0; i < m_BravoWaitList.Count; i++)
            {   allPlayers.Add(m_BravoWaitList[i]);   }

            // Shuffle the list we just created
            Random ran = new Random();
            for (int h = 0; h < 10; h++ )
            {
                for (int i = 0; i < allPlayers.Count; i++ )
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
                m_GameQEvents.Enqueue(msg.pm(allPlayers[0], "?|setfreq 0|prize fullcharge"));
                allPlayers.RemoveAt(0);
            }
        }

        private void RemovePlayer(string PlayerName)
        {
            if (m_GameStatus == GameStatus.GameIdle)
            {
                m_GameQEvents.Enqueue(msg.macro("Game Status Idle: Remove from wait list."));
                m_AlphaWaitList.Remove(PlayerName);
                m_BravoWaitList.Remove(PlayerName);
                return;
            }
            m_CurrentGame.RemovePlayer(PlayerName);
            m_GameQEvents.Enqueue(msg.macro("Remove player- figure out how ot handle all game status."));
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void PlayerPositionUpdate(DevaPlayer d)
        {
            ushort pX = d.Position.MapPositionX;
            ushort pY = d.Position.MapPositionY;

            // Ignore if on block list
            if (m_BlockedList.Contains(d.PlayerName)) return;

            if (m_GameStatus == GameStatus.GameIdle)
            {
                // Player is inside lobby area
                if (InRegion(pX, pY, m_BaseManager.Lobby.BaseDimension))
                {
                    if (!InTeamFreq(d.Frequency))
                    {
                        if (!InWaitList(d.PlayerName))
                        {
                            // May need to move this - some tasks may not need warp timestamp
                            if ((DateTime.Now - d.WarpTimeStamp).TotalMilliseconds <= 1500) return;

                            d.WarpTimeStamp = DateTime.Now;
                            SendToWaitList(d.PlayerName);

                            return;
                        }
                    }
                }
                return;
            }

            if (m_GameStatus == GameStatus.GameInProgress)
            {
                bool alpha;
                BasePlayer b = m_CurrentGame.GetPlayer(d.PlayerName, out alpha);

                if (b == null) return;

                // In base area
                if (InRegion(pX, pY, m_BaseManager.LoadedBase.BaseDimension))
                {
                    b.InLobby = false;

                    if ((alpha && InRegion(pX, pY, m_BaseManager.LoadedBase.BravoSafe) && d.Position.ShipState.IsSafe) ||
                        (!alpha && InRegion(pX, pY, m_BaseManager.LoadedBase.AlphaSafe) && d.Position.ShipState.IsSafe))
                    { EndGame(alpha,WinType.SafeWin); }
                }
                else
                {
                    b.InLobby = true;
                    
                    if (!m_TeamOutTimer.Enabled)
                        StartTeamOutTimer();
                }
                return;
            }
        }

        public void PlayerLeftEvent(DevaPlayer b)
        {
            RemovePlayer(b.PlayerName);
        }

        public Queue<EventArgs> PlayerShipChange(DevaPlayer b)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();

            // NOT SURE I NEED THIS - SPECTATORS GET HANDLED BY FREQ CHANGE EVENT TOO
            //if (b.Ship == ShipTypes.Spectator)
            //{   RemovePlayer(b.PlayerName); }

            return reply;
        }

        public void PlayerFreqChange(DevaPlayer b)
        {
            if (InTeamFreq(b.OldFrequency))
            {   RemovePlayer(b.PlayerName); }
        }
        //----------------------------------------------------------------------//
        //                         Timers                                       //
        //----------------------------------------------------------------------//
        // Timer that sends events to main class
        public EventArgs BaseTimer()
        {
            // Send next message/event in q
            if (m_GameQEvents.Count > 0)
            {
                return m_GameQEvents.Dequeue();
            }
            return null;
        }

        private void StartTeamOutTimer()
        {
            m_TeamOutTimer = new Timer();
            m_TeamOutTimer.Elapsed += new ElapsedEventHandler(TeamOutTimer);
            m_TeamOutTimer.Interval = m_TeamOutTimerSetting * 1000;
            m_TeamOutTimer.Start();
        }

        private void TeamOutTimer(object source, ElapsedEventArgs e)
        {
            m_TeamOutTimer.Stop();
            bool alphaOut;
            bool bravoOut;

            if (m_CurrentGame.TeamIsOut(out alphaOut, out bravoOut))
            {
                if (alphaOut != bravoOut) EndGame(!alphaOut, WinType.AllOut);
                else EndGame(true,WinType.NoCount);
            }
        }

        public void StartBDCommand()
        {
            // Command can only be used before a game gets started
            if (m_GameStatus != GameStatus.GameIdle) return;
            m_GameQEvents.Enqueue(msg.macro("Game will begin in " + m_CountDownSeconds + " seconds."));
            GameStartTimer();
        }

        private void GameStartTimer()
        {
            m_CountDownSeconds_Elapsed = m_CountDownSeconds;
            m_CountDown = new Timer();
            m_CountDown.Elapsed += new ElapsedEventHandler(CountDownTimer);
            m_CountDown.Interval = 1000;
            m_CountDown.Start();
        }

        private void CountDownTimer(object source, ElapsedEventArgs e)
        {
            m_CountDownSeconds_Elapsed--;
            if (m_CountDownSeconds_Elapsed <= m_CountDownShow && m_CountDownSeconds_Elapsed != 0)
            {
                m_GameQEvents.Enqueue(msg.macro("- " + m_CountDownSeconds_Elapsed + " -"));
                m_GameQEvents.Enqueue(msg.arena("", SoundCodes.MessageAlarm));
            }

            if (m_CountDownSeconds_Elapsed > 0) return;

            m_GameQEvents.Enqueue(msg.macro("-Go Go Go-"));
            m_GameQEvents.Enqueue(msg.arena("", SoundCodes.Goal));
            m_CountDown.Stop();
            StartGame();
        }

        //----------------------------------------------------------------------//
        //                         Misc                                         //
        //----------------------------------------------------------------------//
        private void SendToWaitList(string PlayerName)
        {
            if (m_AlphaWaitList.Count <= m_BravoWaitList.Count)
            {
                m_AlphaWaitList.Add(PlayerName);
                m_GameQEvents.Enqueue(msg.pm(PlayerName, "?|setfreq " + m_AlphaFreq + "|shipreset"));
                return;
            }

            m_BravoWaitList.Add(PlayerName);
            m_GameQEvents.Enqueue(msg.pm(PlayerName, "?|setfreq " + m_BravoFreq + "|shipreset"));
        }

        private bool InRegion(ushort x, ushort y, ushort[] region)
        {
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        private bool InTeamFreq(ushort freq)
        {
            return freq == m_AlphaFreq || freq == m_BravoFreq;
        }

        private bool InWaitList(string PlayerName)
        {
            return m_AlphaWaitList.Contains(PlayerName) || m_AlphaWaitList.Contains(PlayerName);
        }

        public Queue<EventArgs> GetTeamList(string PlayerName)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();

            reply.Enqueue(msg.arena("Alpha Wait List -------"));
            for (int i = 0; i < m_AlphaWaitList.Count; i++)
            { reply.Enqueue(msg.arena("Player [ " + m_AlphaWaitList[i] + " ]")); }

            reply.Enqueue(msg.arena("Bravo Wait List -------"));
            for (int i = 0; i < m_BravoWaitList.Count; i++)
            { reply.Enqueue(msg.arena("Player [ " + m_BravoWaitList[i] + " ]")); }

            return reply;
        }

        public Queue<EventArgs> GetGameInfo(string PlayerName)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();
            reply.Enqueue(msg.pm(PlayerName, "Game Settings -------------------------------"));
            reply.Enqueue(msg.pm(PlayerName, "Current Game Status:".PadRight(20) + m_GameStatus.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Setting/Mode:".PadRight(20) + m_GameMode.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Alpha Team Freq:".PadRight(20) + m_AlphaFreq.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Bravo Team Freq:".PadRight(20) + m_BravoFreq.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Match Start Timer:".PadRight(20) + (m_CountDownSeconds.ToString() + " (seconds)").PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Start Timer Show:".PadRight(20) + (m_CountDownShow).ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "All Out timer:".PadRight(20) + (m_TeamOutTimerSetting + " (seconds)").PadLeft(25)));
            return reply;
        }

        public Queue<EventArgs> PrintTeams(string PlayerName)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();

            BaseTeam a,b;
            m_CurrentGame.GetTeams(out a, out b);

            reply.Enqueue(msg.pm(PlayerName, "Alpha Team ----------------------------------"));
            for (int i = 0; i < a.TeamList.Count; i++)
                reply.Enqueue(msg.pm(PlayerName, a.TeamList[i].PlayerName.PadRight(20) + a.TeamList[i].Active.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Bravo Team ----------------------------------"));
            for (int i = 0; i < b.TeamList.Count; i++)
                reply.Enqueue(msg.pm(PlayerName, b.TeamList[i].PlayerName.PadRight(20) + b.TeamList[i].Active.ToString().PadLeft(25)));

            return reply;
        }
    }
}
