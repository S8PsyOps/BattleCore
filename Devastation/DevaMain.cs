using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Needed for timers
using System.Timers;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation
{
    // Add the attribute and the base class
    [Behavior("DevaMain", "false", "0.01", "PsyOps", "Main Devastation Bot.")]
    public class DevaMain: BotEventListener
    {
        public DevaMain()
        {
            // set vars
            this.msg = new ShortChat();
            this.m_ModuleOn = false;
            this.m_AutoLoad = true;
            this.m_Initialized = false;
            this.m_Biller = new BillerCommands();
            this.m_SafeGameTimeStamp = DateTime.Now;
            this.m_SafeGameTime = 25;
            this.m_GameTimer = new Timer();
            this.m_GameTimerDelay = 5;
            this.m_ModCheckInterval = 60;
            this.m_ModCheckTS = DateTime.Now;

            // Commands
            RegisterCommand("!bdtoggle", ToggleModuleCommand);
            RegisterCommand("!bdstatus", BDStatus);
            RegisterCommand("!modcheck", ModCheck);
            
            // Debug commands
            RegisterCommand("!pinfo", GetPlayerInfo);
            RegisterCommand("!tinfo", GetTeamInfo);
            RegisterCommand("!test", Test);
            RegisterCommand("!lp", LP); RegisterCommand(".lp", LP);
            RegisterCommand("!startbd", StartBD);
            RegisterCommand("!shuffleteam", Shuffle);
            
            // Timer
            RegisterTimedEvent("InitializeTimer",5000, InitializeBot);
        }

        private ShortChat msg;                      // My module to make sending messages easier
        private bool m_AutoLoad;                    // Autoload set true/false
        private bool m_ModuleOn;                    // Turns the module on and off
        private bool m_Initialized;                 // Once bot has everything loaded and ready to go
        private BillerCommands m_Biller;            // Helps send/monitor biller commands to avoid spam kick
        private string m_BotName, m_ArenaName;      // Vars to store bot info
        private byte[] m_MapInfo;                   // Stores map data in raw form
        private BaseDuel.PlayerManager m_Players;   // Manages and updates player info
        private BaseDuel.BaseManager m_BaseManager; // Controls loading and anything related to bases
        private Timer m_GameTimer;                  // Main Game Timer
        private double m_GameTimerDelay;            // Delay for game timer
        private DateTime m_SafeGameTimeStamp;       // self explanatory
        private double m_SafeGameTime;              // milliseconds
        private int m_ModCheckInterval;             // Interval between modcheck command - seconds
        private DateTime m_ModCheckTS;              // Time stamp from last command

        private BaseDuel.BaseDuel m_BDGame;

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        // Turns baseduel module on and off
        public void ToggleModuleCommand(ChatEvent e)
        {
            if (IsMod(e,ModLevels.Mod))   ToggleModule(); 
        }
        // Displays current status of module: setings etc
        public void BDStatus(ChatEvent e)
        {
            if (IsMod(e, ModLevels.Mod)) DisplayBDStatus(e);
        }
        public void ModCheck(ChatEvent e)
        {
            if (!m_Initialized) return;

            int elapsed = (int)(DateTime.Now - m_ModCheckTS).TotalSeconds;

            if (elapsed <= m_ModCheckInterval)
            {
                Game(msg.pm(e.PlayerName, "You can not use this command at this time. Please wait [ " + (m_ModCheckInterval - elapsed) + " ] more seconds."));
                return;
            }

            m_ModCheckTS = DateTime.Now;
            Game(msg.pm(e.PlayerName, "Refreshing Mod List."));
            Game(msg.pub("*listmod"));
            Game(new PlayerInfoEvent());
        }
        // ## Debug stuff
        public void LP(ChatEvent e)
        {
            e.Message = "!help poo";
            SendCoreEvent(e);
        }
        public void StartBD(ChatEvent e)
        {
            m_BDGame.StartBDCommand();
        }
        public void Shuffle(ChatEvent e)
        {
            m_BDGame.Shuffle(e);
        }
        public void Test(ChatEvent e)
        {
            MsgDumper(m_BDGame.GetTeamList(e.PlayerName));

            for (int i = 0; i < m_Players.List.Count; i++)
            {
                if (m_Players.List[i].Ship != ShipTypes.Spectator)
                    Game(msg.pm(m_Players.List[i].PlayerName,"?|prize warp|setfreq 0|shipreset"));
            }
        }
        public void GetPlayerInfo(ChatEvent e)
        {
            BaseDuel.DevaPlayer b = m_Players.GetPlayer(e.PlayerName);

            if (b == null)
                Game(msg.arena("Player returned null."));
            else
            {
                Game(msg.pm(b.PlayerName,"Player info -------"));
                Game(msg.pm(b.PlayerName,"Name:".PadRight(20) + b.PlayerName.PadLeft(20)));
                Game(msg.pm(b.PlayerName,"Squad:".PadRight(20) + b.SquadName.PadLeft(20)));
                Game(msg.pm(b.PlayerName,"ModLevel:".PadRight(20) + b.ModLevel.ToString().PadLeft(20)));
                Game(msg.pm(b.PlayerName,"OldFreq:".PadRight(20) + b.OldFrequency.ToString().PadLeft(20)));
                Game(msg.pm(b.PlayerName,"Freq:".PadRight(20) + b.Frequency.ToString().PadLeft(20)));
                Game(msg.pm(b.PlayerName,"OldShip:".PadRight(20) + b.OldShip.ToString().PadLeft(20)));
                Game(msg.pm(b.PlayerName,"Ship:".PadRight(20) + b.Ship.ToString().PadLeft(20)));
                Game(msg.pm(b.PlayerName,"Loc:".PadRight(20) + ("X:" + b.Position.MapPositionX + "  Y:" + b.Position.MapPositionY).PadLeft(20)));
            }
        }
        public void GetTeamInfo(ChatEvent e)
        {
            MsgDumper( m_BDGame.PrintTeams(e.PlayerName));
        }
        //----------------------------------------------------------------------//
        //                         Event Monitors                               //
        //----------------------------------------------------------------------//
        // watch for incoming Bot info
        public void MonitorIncomingBotInfo(object sender, BotInfoRequest e)
        {
            // Grab bot and map info and store it
            m_BotName = e.BotName;
            m_ArenaName = e.MapFile.Replace(".lvl", "");
            m_MapInfo = e.MapData;
            //Game(msg.arena("Bot Data Stored. BotName[ "+m_BotName+" ]  Arena[ "+m_ArenaName+" ]  MapDataLength[ "+e.MapData.Length+" ]"));

            m_Biller.SendMessage(msg.pub("?chat=devadev,devastation"));
            // Make sure bot gets all position packets
            MsgDumper(msg.pub("?botfeature +seeallposn"));

            // Set main game timer
            m_GameTimer = new Timer();
            m_GameTimer.Elapsed += new ElapsedEventHandler(GameTimer);
            m_GameTimer.Interval = m_GameTimerDelay;
            m_GameTimer.Start();

            if (!m_AutoLoad) return;
            //Game(msg.arena("AutoLoad Module Activated."));
            ToggleModule();
        }

        public void MonitorPlayerInfoEvent(object sender, PlayerInfoEvent pIE)
        {
            if (!m_Initialized)
            {
                m_Players = new BaseDuel.PlayerManager();
                m_Players.UpdatePlayerList(pIE); // I like pie

                m_Initialized = true;
            }
            else { m_Players.UpdateModPlayerList(pIE); }
        }

        public void MonitorChatMessage(object sender, ChatEvent c)
        {
            //if (c.ChatType != ChatTypes.Channel && c.Message.Contains("Damage watching on") && c.Message.Contains("turned"))
            //{ MsgDumper(msg.chan(1, c.Message)); }
        }

        public void MonitorPlayerEnteredEvent(object sender, PlayerEnteredEvent e)
        {
            // Toggle on watchdamage for every player that enters
            MsgDumper(msg.pm(e.PlayerName, "?watchdamage"));

            if (!m_Initialized) return;

            BaseDuel.DevaPlayer BDplayer = m_Players.GetPlayer(e);
        }

        public void MonitorPlayerLeftEvent(object sender, PlayerLeftEvent e)
        {
            if (!m_Initialized) return;
            BaseDuel.DevaPlayer BDplayer = m_Players.GetPlayer(e);

            m_BDGame.PlayerLeftEvent(BDplayer);

            // Leave this to last so we can grab info first
            m_Players.RemovePlayer(e.PlayerName);
        }

        public void MonitorFrequencyChange(object sender, TeamChangeEvent e)
        {
            if (!m_Initialized) return;
            BaseDuel.DevaPlayer BDplayer = m_Players.GetPlayer(e);

            m_BDGame.PlayerFreqChange(BDplayer);
        }

        public void MonitorShipChange(object sender, ShipChangeEvent e)
        {
            if (!m_Initialized) return;
            BaseDuel.DevaPlayer BDplayer = m_Players.GetPlayer(e);

            MsgDumper(m_BDGame.PlayerShipChange(BDplayer));
        }

        public void MonitorPLayerPosition(object sender, PlayerPositionEvent e)
        {
            if (!m_Initialized) return;
            BaseDuel.DevaPlayer BDplayer = m_Players.GetPlayer(e);

            m_BDGame.PlayerPositionUpdate(BDplayer);
        }

        //----------------------------------------------------------------------//
        //                         Load/Unload                                  //
        //----------------------------------------------------------------------//
        public void InitializeBot()
        {
            RemoveTimedEvent("InitializeTimer");
            // ask core for bot info with mapdata attached
            Game(new BotInfoRequest());
        }

        public void ToggleModule()
        {
            m_ModuleOn = !m_ModuleOn;
            //Game(msg.arena("BaseDuel Module - set to: [ " + m_ModuleOn + " ]"));

            if (m_ModuleOn) ModuleOn();
            else ModuleOff();
        }

        public void ModuleOn()
        {
            m_BaseManager = new BaseDuel.BaseManager(m_MapInfo);
            m_BDGame = new BaseDuel.BaseDuel(m_BaseManager);
            Game(msg.arena("Initializing. Scanning map....BasesLoaded[ " + m_BaseManager.Bases.Count + " ] - Module On"));
            //Game(msg.arena("BasesLoaded[ "+m_BaseManager.Bases.Count+" ]"));
            //request player info from core
            Game(new PlayerInfoEvent());
        }

        public void ModuleOff()
        {
            m_Initialized = false;
            m_Players = null;
            m_BaseManager = null;
            Game(msg.arena("Reset or remove vars here. Turn game off."));
        }

        //----------------------------------------------------------------------//
        //                         Timer                                        //
        //----------------------------------------------------------------------//
        public void GameTimer(object source, ElapsedEventArgs e)
        {
            // Send any q'ed Biller commands
            SafeGame(m_Biller.MessageToSend);       

            if (!m_Initialized) return;

            MsgDumper(m_Players.MessageToSend());
            MsgDumperNoSafe(m_BDGame.BaseTimer());
        }

        //----------------------------------------------------------------------//
        //                         Misc Stuffs                                  //
        //----------------------------------------------------------------------//
        // MsgDumper takes a single event or a Queue of events: 
        // Checks to see if they are valid and also checks to see if its a biller message, then sends it out
        private void MsgDumper(Queue<EventArgs> q)
        {
            if (q == null || q.Count == 0) return;

            while (q.Count > 0)
                MsgDumper(q.Dequeue());
        }

        private void MsgDumper(EventArgs e)
        {
            if (e == null) return;

            if (e is ChatEvent)
            {
                ChatEvent c = e as ChatEvent;

                if (c.ChatType == ChatTypes.Channel || c.Message.StartsWith("?") || c.Message.StartsWith("\\"))
                    m_Biller.SendMessage(c);
                else Game(e);
            }
            else Game(e);
        }

        private void MsgDumperNoSafe(Queue<EventArgs> q)
        {
            if (q == null || q.Count == 0) return;

            while (q.Count > 0)
                MsgDumperNoSafe(q.Dequeue());
        }

        private void MsgDumperNoSafe(EventArgs e)
        {
            if (e == null) return;
            Game(e);
        }

        // Added security to biller module to avoid flooding
        private void SafeGame(EventArgs e)
        {
            if (e == null || (DateTime.Now - m_SafeGameTimeStamp).TotalMilliseconds < m_SafeGameTime) return;
            m_SafeGameTimeStamp = DateTime.Now;
            Game(e);
        }
        // Checks to see if you are mod - if not sends back message with required mod lvl to operate
        public bool IsMod(ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod)  return true;

            Game(msg.pm(e.PlayerName, "You are not authorized to use this command. Required moderator level: " + mod + "."));
            return false;
        }

        public void DisplayBDStatus( ChatEvent e)
        {
            Game(msg.pm(e.PlayerName,"Base Duel Status"));
            Game(msg.pm(e.PlayerName, "----------------------------------------------"));
            Game(msg.pm(e.PlayerName, "Module Loaded:".PadRight(20) + m_ModuleOn.ToString().PadLeft(25)));
            Game(msg.pm(e.PlayerName, "AutoLoad set to:".PadRight(20) + m_AutoLoad.ToString().PadLeft(25)));
             
            if (m_BaseManager != null) MsgDumper(m_BaseManager.GetBaseManagerSettings(e.PlayerName));
            MsgDumper(m_BDGame.GetGameInfo(e.PlayerName));
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
// Base Manager should go into base game ?!?!
// Playermanager - when player is returned null, create player?!?!
// 69.164.220.203 7022
