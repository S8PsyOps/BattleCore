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
        public BaseGameManager( byte[] MapData)
        {

            this.msg = new ShortChat();
            this.m_Timer = new Timer();
            this.m_BaseManager = new BaseManager(MapData);
            this.m_Base = m_BaseManager.CurrentBase;
            this.m_AlphaFreq = 887;
            this.m_BravoFreq = 889;
            this.m_AlphaWaitList = new List<string>();
            this.m_BravoWaitList = new List<string>();
        }

        private ShortChat msg;
        
        private BaseGame m_CurrentGame;
        private Timer m_Timer;
        
        private BaseManager m_BaseManager;
        private Base m_Base;

        private ushort m_AlphaFreq, m_BravoFreq;
        private List<string> m_AlphaWaitList, m_BravoWaitList;

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        public Queue<EventArgs> BaseDuelCommands(DevaPlayer p, ChatEvent e)
        {
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

            if (data[1].Trim().ToLower() == "toggle")
            {
                // Player isnt mod
                if (!isMod(q, e, ModLevels.Mod)) return q;

                toggleBaseDuel(p, q);

                return q;
            }

            return q;
        }

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        private void toggleBaseDuel(DevaPlayer p, Queue<EventArgs> q)
        {
            // Turn On module
            if (m_CurrentGame == null)
            {
                m_CurrentGame = new BaseGame();
                m_CurrentGame.AlphaFreq = m_AlphaFreq;
                m_CurrentGame.BravoFreq = m_BravoFreq;
                m_CurrentGame.Status = BaseGameStatus.GameIdle;

                m_AlphaWaitList = new List<string>();
                m_BravoWaitList = new List<string>();

                // Send Arena Message
                q.Enqueue(msg.arena("[ BaseDuel ] Module loaded by staff - " + p.PlayerName));
                printOut_GameSettings(p.PlayerName, q);
                return;
            }

            // Turn Module Off
            m_CurrentGame = null;
            q.Enqueue(msg.arena("[ BaseDuel ] Module unloaded by staff - " + p.PlayerName));
        }

        // Load next base to our local base
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

            Queue<EventArgs> q = new Queue<EventArgs>();



            return q;
        }

        //----------------------------------------------------------------------//
        //                       Timer Stuff                                    //
        //----------------------------------------------------------------------//
        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        // Simple collision check
        private bool inRegion(PlayerPositionEvent p, ushort[] region)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        // checking if player is mod - if not sends bac message
        private bool isMod(Queue<EventArgs> q, ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod)
                return true;

            q.Enqueue(msg.pm(e.PlayerName,"You do not have access to this command. This is a staff command. Required Moerator level: [ "+mod+" ]."));
            return false;
        }

        // Checks to see if module is loaded - returns message if not
        private bool moduleIsOn(Queue<EventArgs> q, ChatEvent e)
        {
            if (m_CurrentGame != null) return true;

            q.Enqueue(msg.pm(e.PlayerName,"Module is currently not loaded."));
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
