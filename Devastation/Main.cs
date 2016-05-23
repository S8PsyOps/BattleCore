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
    [CommandHelp("!toggle baserace","Turns baserace Module on and off", ModLevels.Mod)]
    public class Main:BotEventListener
    {
        public Main()
        {
            this.msg = new ShortChat();
            msg.DebugMode = true;
            this.m_Players = new PlayerManager();

            this.m_GameTimer = new Timer();
            this.m_GameTimer.Elapsed += new ElapsedEventHandler(GameTimer);
            this.m_GameTimer.Interval = 5000;
            this.m_GameTimer.Start();

            // Registered commands
            RegisterCommand("!pinfo", getPlayerInfo);               RegisterCommand(".pinfo", getPlayerInfo);
            RegisterCommand("!startbd", StartBD);                   RegisterCommand(".startbd", StartBD);
            RegisterCommand("!baseduel", doBaseDuelCommand);        RegisterCommand(".baseduel", doBaseDuelCommand);
            RegisterCommand("!debug", doToggleDebug);               RegisterCommand(".debug", doToggleDebug);
        }

        private ShortChat msg;                          // Class to make sending messages easier
        private Timer m_GameTimer;                      // Main timer for bot
        private bool m_StartIni, m_Initialized;         // Bool to help initialize bot
        private string m_BotName, m_ArenaName;          // Store bot info
        private byte[] m_MapInfo;                       // Byte array containing map info
        private BaseDuel.Main m_BaseDuel;           // BaseDuel Game
        private PlayerManager m_Players;

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        public void doToggleDebug(ChatEvent e)
        {
            if (!m_Initialized) return;

            if (!IsMod(e, ModLevels.Mod)) return;

            msg.DebugMode = !msg.DebugMode;
            m_BaseDuel.setDebug(msg.DebugMode);

            Game(msg.arena("[ Deva Main ] Debug mode has been toggled " + (msg.DebugMode?"On":"Off") + " by staff - " + e.PlayerName));
        }
        public void doBaseDuelCommand(ChatEvent e)
        {
            if (!m_Initialized) return;

            DevaPlayer dp = m_Players.GetPlayer(e);

            SendPsyEvent(m_BaseDuel.BaseDuelCommands(dp, e));
        }

        // Print out player info from playermanager
        public void getPlayerInfo(ChatEvent e)
        {
            if (!m_Initialized) return;
            m_Players.PrintPlayerInfo(e.PlayerName);
        }

        public void StartBD(ChatEvent e)
        {
            if (!m_Initialized) return;
            //m_BaseDuel.Command_StartGame(e.PlayerName);

            DevaPlayer dp = m_Players.GetPlayer(e);

            // change command format to make compatible with older command syntax
            e.Message = "!baseduel start";
            SendPsyEvent(m_BaseDuel.BaseDuelCommands(dp, e));
        }

        //----------------------------------------------------------------------//
        //                         Event Monitor                                //
        //----------------------------------------------------------------------//
        public void MonitorPlayerEnteredEvent(object sender, PlayerEnteredEvent e)
        {
            Game(msg.pm(e.PlayerName, "?watchdamage"));

            if (!m_Initialized) return;

            DevaPlayer dp = m_Players.GetPlayer(e);
        }
        public void MonitorPlayerLeftEvent(object sender, PlayerLeftEvent e)
        {
            if (!m_Initialized) return;

            DevaPlayer dp = m_Players.GetPlayer(e);
            m_BaseDuel.Event_PlayerLeft(dp);
            SendPsyEvent(m_BaseDuel.Event_PlayerLeft(dp));
        }
        public void MonitorTeamChangeEvent(object sender, TeamChangeEvent e)
        {
            if (!m_Initialized) return;

            DevaPlayer dp = m_Players.GetPlayer(e);

            SendPsyEvent(m_BaseDuel.Event_PlayerFreqChange(dp));
        }
        public void MonitorShipChangeEvent(object sender, ShipChangeEvent e)
        {
            if (!m_Initialized) return;

            DevaPlayer dp = m_Players.GetPlayer(e);
            
        }
        public void MonitorPlayerPositionEvent(object sender, PlayerPositionEvent e)
        {
            if (!m_Initialized) return;

            DevaPlayer dp = m_Players.GetPlayer(e);

            SendPsyEvent(m_BaseDuel.Event_PlayerPosition(dp));

            m_BaseDuel.Event_PlayerPosition(dp);
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
                m_ArenaName = e.MapFile.Replace(".lvl", "");
                m_MapInfo = e.MapData;

                // Once we have mapdata we initialize baseduel
                m_BaseDuel = new BaseDuel.Main(e.MapData, msg.DebugMode);

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

                m_GameTimer.Stop();
                m_GameTimer = new Timer();
                m_GameTimer.Elapsed += new ElapsedEventHandler(GameTimer);
                m_GameTimer.Interval = 5;
                m_GameTimer.Start();

                Game(msg.debugArena("Bot [ " + m_BotName + " ] is initialized. Arena [ " + m_ArenaName + " ] MapInfoSize [ " + m_MapInfo.Length + " ]"));
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

            SendPsyEvent(m_Players.PlayerManagerEvents);
            SendPsyEvent(m_BaseDuel.Events);
        }

        //----------------------------------------------------------------------//
        //                         Misc                                         //
        //----------------------------------------------------------------------//
        // Helps check for numm events and also empties event queues
        public void SendPsyEvent(Queue<EventArgs> e)
        {
            if (e == null) return;

            while (e.Count > 0)
                SendPsyEvent(e.Dequeue());
        }
        public void SendPsyEvent(EventArgs e)
        {
            if (e == null) return;
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
