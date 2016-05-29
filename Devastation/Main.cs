using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Needed for timers
using System.Timers;
// Bot Core stuffs
using BattleCore;
using BattleCore.Events;
// My chat module
using BattleCorePsyOps;

namespace Devastation
{
    // Add the attribute and the base class
    [Behavior("DevaMain", "true", "0.01", "PsyOps", "Main Devastation Bot.")]
    public class Main:BotEventListener
    {
        public Main()
        {
            this.m_Players = new SSPlayerManager(7265);
            this.msg = new ShortChat(m_Players.PlayerList);
            this.msg.DebugMode = false;
            this.msg.IsASSS = true;
            this.myGame = new MyGame();

            this.m_GameTimer = new Timer();
            this.m_GameTimer.Elapsed += new ElapsedEventHandler(GameTimer);
            this.m_GameTimer.Interval = 5000;
            this.m_GameTimer.Start();

            RegisterCommand("!debug", doToggleDebug);               RegisterCommand(".debug", doToggleDebug);
        }

        private ShortChat msg;                          // Class to make sending messages easier
        private MyGame myGame;
        private Timer m_GameTimer;                      // Main timer for bot
        private bool m_StartIni, m_Initialized;         // Bool to help initialize bot
        private string m_BotName, m_ArenaName;          // Store bot info
        private byte[] m_MapInfo;                       // Byte array containing map info
        private BaseDuel.Main m_BaseDuel;               // BaseDuel Game
        SSPlayerManager m_Players;
        BaseRace m_BaseRace;

        

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        public void doToggleDebug(ChatEvent e)
        {
            if (!m_Initialized) return;

            if (!IsMod(e, ModLevels.Mod)) return;

            msg.DebugMode = !msg.DebugMode;

            Game(msg.arena("[ Deva Main ] Debug mode has been toggled " + (msg.DebugMode?"On":"Off") + " by staff - " + e.PlayerName));
        }

        //----------------------------------------------------------------------//
        //                         Event Monitor                                //
        //----------------------------------------------------------------------//
        public void MonitorAllChat(object sender, ChatEvent e)
        {
            if (!m_Initialized) return;
            
            m_BaseDuel.Commands(e);
            m_BaseRace.Commands(e);
        }

        public void MonitorPlayerEnteredEvent(object sender, PlayerEnteredEvent e)
        {
            Game(msg.pm(e.PlayerName, "?watchdamage"));

            if (!m_Initialized) return;

            SSPlayer ssp = m_Players.GetPlayer(e);

            m_BaseDuel.Event_PlayerEntered(ssp);
            m_BaseRace.Event_PlayerEntered(ssp);
        }
        public void MonitorPlayerLeftEvent(object sender, PlayerLeftEvent e)
        {
            if (!m_Initialized) return;
            // Grab player from list - need to make sure and remove at end of method
            SSPlayer ssp = m_Players.GetPlayer(e);
            
            // Update baseduel
            m_BaseDuel.Event_PlayerLeft(ssp);
            m_BaseRace.Event_PlayerLeft(ssp);
            
            // removing here allows you to pull all needed info inbetween, then you can delete
            m_Players.RemovePlayer(ssp);
        }
        public void MonitorTeamChangeEvent(object sender, TeamChangeEvent e)
        {
            if (!m_Initialized) return;

            SSPlayer ssp = m_Players.GetPlayer(e);
            
            m_BaseDuel.Event_PlayerFreqChange(ssp);
            m_BaseRace.Event_PlayerFreqChange(ssp);
        }
        public void MonitorShipChangeEvent(object sender, ShipChangeEvent e)
        {
            if (!m_Initialized) return;

            SSPlayer ssp = m_Players.GetPlayer(e);

            m_BaseDuel.Event_ShipChange(ssp);
            m_BaseRace.Event_ShipChange(ssp);
        }
        public void MonitorPlayerPositionEvent(object sender, PlayerPositionEvent e)
        {
            if (!m_Initialized) return;

            SSPlayer ssp = m_Players.GetPlayer(e);

            m_BaseDuel.Event_PlayerPosition(ssp);
            m_BaseRace.Event_PlayerPosition(ssp);
        }

        public void MonitorTurretEvent(object sender, ModifyTurretEvent e)
        {
            if (!m_Initialized) return;

            SSPlayer attacher = m_Players.GetPlayer(e.TurretAttacherName);
            SSPlayer host = m_Players.GetPlayer(e.TurretHostName);

            if (e.TurretHostId == 65535)
            {
                m_BaseDuel.Event_PlayerTurretDetach(attacher);
                m_BaseRace.Event_PlayerTurretDetach(attacher);
                return;
            }

            m_BaseDuel.Event_PlayerTurretAttach(attacher, host);
            m_BaseRace.Event_PlayerTurretAttach(attacher, host);
        }

        //----------------------------------------------------------------------//
        //                         Initialize Bot                               //
        //----------------------------------------------------------------------//
        public void StartInitialization()
        {
            if (m_StartIni) return;
            m_StartIni = true;
            
            // Ask for bot info
            Game(new BotInfoRequest());
        }
        public void GrabBotInfo(object sender, BotInfoRequest e)
        {
            if (!m_Initialized)
            {
                // Grab bot and map info and store it
                m_BotName = e.BotName;

                if (e.MapFile == null)
                {
                    Game(msg.arena("[ Deva Main ] Error: Map file received null, arena name set from hardcoded var."));
                    m_ArenaName = "devadev.lvl";
                }

                m_ArenaName = e.MapFile.Replace(".lvl", "");
                m_MapInfo = e.MapData;

                // Requesting Player's Info
                Game(new PlayerInfoEvent());
            }
        }
        public void GrabPlayerInfo(object sender, PlayerInfoEvent e)
        {
            if (!m_Initialized)
            {
                // Load all player info from the event and build list
                m_Players.PlayerInfoEvent = e;
                // Once we have mapdata we initialize baseduel
                m_BaseDuel = new BaseDuel.Main(m_MapInfo, m_Players, msg, myGame);
                m_BaseRace = new BaseRace(m_Players, m_MapInfo, msg,myGame);

                m_GameTimer.Stop();
                m_GameTimer = new Timer();
                m_GameTimer.Elapsed += new ElapsedEventHandler(GameTimer);
                m_GameTimer.Interval = 5;
                m_GameTimer.Start();

                Game(msg.arena("Bot [ " + m_BotName + " ] is initialized. Arena [ " + m_ArenaName + " ] MapInfoSize [ " + m_MapInfo.Length + " ]"));
                Game(msg.pub("?chat=devadev,devastation"));
                Game(msg.pub("?botfeature +seeallposn"));
                m_Initialized = true;
            }
        }

        //----------------------------------------------------------------------//
        //                         Game Timer Functions                         //
        //----------------------------------------------------------------------//
        public void GameTimer(object source, ElapsedEventArgs e)
        {
            StartInitialization();

            if (!m_Initialized) return;

            SendPsyEvent(myGame.EventQ);
            SendPsyEvent(myGame.CoreEventQ, true);
        }

        //----------------------------------------------------------------------//
        //                         Misc                                         //
        //----------------------------------------------------------------------//
        public void SendPsyEvent(EventArgs e)
        {
            SendPsyEvent(e, false);
        }
        public void SendPsyEvent(Queue<EventArgs> e)
        {
            SendPsyEvent(e, false);
        }
        // Helps check for numm events and also empties event queues
        public void SendPsyEvent(Queue<EventArgs> e, bool Core)
        {
            if (e == null) return;

            while (e.Count > 0)
                SendPsyEvent(e.Dequeue(), Core);
        }
        public void SendPsyEvent(EventArgs e, bool Core)
        {
            if (e == null) return;

            if (Core)
                SendCoreEvent(e);
            else
                Game(e);
        }

        // Mod check with an attached message in case they aren't authorized
        public bool IsMod(ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod) return true;

            Game(msg.pm(e.PlayerName, "You are not authorized to use this command. Required Mod Level [ "+mod+" ]."));
            return false;
        }

        //----------------------------------------------------------------------//
        //                         DEBUG STUFFS                                 //
        //----------------------------------------------------------------------//

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
