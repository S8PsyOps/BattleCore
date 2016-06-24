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
using System.IO;
using System.Reflection;

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
            this.psyGame = new MyGame();
            this.m_GFX = new DisplayManager(msg, psyGame,m_Players);

            this.m_GameTimer = new Timer();
            this.m_GameTimer.Elapsed += new ElapsedEventHandler(GameTimer);
            this.m_GameTimer.Interval = 5000;
            this.m_GameTimer.Start();

            RegisterCommand("!debug", doToggleDebug);               RegisterCommand(".debug", doToggleDebug);
            RegisterCommand("!test", doTest);                       RegisterCommand(".test", doTest);
        }

        private ShortChat msg;                          // Class to make sending messages easier
        private MyGame psyGame;                          // Both classes come from BattleCorePsyOps
      
        private BaseManager m_BaseManager;
        private FileDataBase m_FakeDB;
        private DisplayManager m_GFX;

        private Timer m_GameTimer;                      // Main timer for bot
        private bool m_StartIni, m_Initialized;         // Bool to help initialize bot
        private string m_BotName, m_ArenaName;          // Store bot info
        private byte[] m_MapInfo;                       // Byte array containing map info              
        private BaseDuel.BaseDuel m_BaseDuel;           // BaseDuel Game
        private SSPlayerManager m_Players;
        //private BaseRace.BaseRace2 m_BaseRace2;
        private BaseRace.BaseRace m_BaseRace;

        DateTime ts = DateTime.Now;
        public void doTest(ChatEvent e)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location );
            psyGame.Send(msg.pm("PsyOps",path));
            psyGame.Send(msg.pm("PsyOps",  System.IO.Directory.GetParent(path).ToString()));
            //Game(msg.arena("Time: ["+(DateTime.Now - ts)+"]"));
            //ts = DateTime.Now;
        }
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

            if (e.ChatType == ChatTypes.Arena && e.Message.Contains(" has user id "))
            {
                string fullmsg = e.Message;
                string[] data = e.Message.Split(' ');
                string stringID = data[data.Length - 1];
                string user = fullmsg.Replace(stringID, "").Replace(" has user id ","").Trim();
                int id;

                SSPlayer p = this.m_Players.PlayerList.Find(item => item.PlayerName == user);

                if (p != null && int.TryParse(stringID, out id))
                {
                    p.SubspaceId = id;
                }

                return;
            }

            if (e.Message.StartsWith(".lp") || e.Message.StartsWith("!lp"))
            {
                e.Message = "!help poo";
                SendCoreEvent(e);
                return;
            }

            m_BaseRace.Commands(e);
            m_BaseDuel.Commands(e);
        }

        public void MonitorPlayerEnteredEvent(object sender, PlayerEnteredEvent e)
        {
            Game(msg.pm(e.PlayerName, "?watchdamage"));

            if (!m_Initialized) return;
            SSPlayer ssp = m_Players.GetPlayer(e);
            this.m_GFX.RefreshDisplay_Player(ssp);
            psyGame.SafeSend(msg.pm(e.PlayerName, "?userid"));
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

            //m_BaseRace2.Event_ShipChange(ssp);
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
                //m_BaseDuel2.Event_PlayerTurretDetach(attacher);
                //m_BaseRace2.Event_PlayerTurretDetach(attacher);
                return;
            }

            m_BaseDuel.Event_PlayerTurretAttach(attacher, host);
            m_BaseRace.Event_PlayerTurretAttach(attacher, host);
        }

        public void MonitorPlayerDeathEvent(object sender, PlayerDeathEvent e)
        {
            if (!m_Initialized) return;

            SSPlayer attacker = m_Players.GetPlayer(e.KillerName);
            SSPlayer victim = m_Players.GetPlayer(e.KilledName);

            m_BaseDuel.Event_PlayerKilled(attacker, victim);
        }

        public void MonitorLvzEvent(object sender, LVZToggleEvent e)
        {
            //Game(msg.arena("LvzToggleEvent fired - PlayerID [ "+e.TargetPlayerId+" ]  " + e.TargetPlayerName));
            //if (e.LVZObjects != null)
            //    foreach (var item in e.LVZObjects)
            //    {
            //        Game(msg.arena("LvzID[ " + item.Key.ToString().PadLeft(4, '0') + " ]".PadRight(15) + "State[ " + (item.Value ? " On" : "Off") + " ]"));
            //    }
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

                if (e.MapFile == null || e.MapFile.Length <= 0)
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
                foreach (PlayerInfo p in e.PlayerList) this.psyGame.SafeSend(msg.pm(p.PlayerName, "?userid"));

                // Load all player info from the event and build list
                this.m_Players.PlayerInfoEvent = e;
                // Once we have mapdata we initialize baseduel
                this.m_BaseManager = new BaseManager(m_MapInfo, msg, psyGame);

                this.m_FakeDB = new FileDataBase(msg, psyGame, m_Players);

                this.m_BaseRace = new BaseRace.BaseRace(m_BaseManager, m_Players, msg, psyGame, m_FakeDB,m_GFX, m_ArenaName);
                this.m_BaseDuel = new BaseDuel.BaseDuel(m_BaseManager, m_Players, msg, psyGame, m_ArenaName);

                this.m_GameTimer.Stop();
                this.m_GameTimer = new Timer();
                this.m_GameTimer.Elapsed += new ElapsedEventHandler(GameTimer);
                this.m_GameTimer.Interval = 5;
                this.m_GameTimer.Start();

                Game(msg.arena("Bot [ " + m_BotName + " ] is initialized. Arena [ " + m_ArenaName + " ] MapInfoSize [ " + m_MapInfo.Length + " ]"));
                Game(msg.pub("?chat=devadev,devastation"));
                Game(msg.pub("?botfeature +seeallposn"));
                this.m_Initialized = true;
                Console.WriteLine("Bot Initialized");
            }
        }

        //----------------------------------------------------------------------//
        //                         Game Timer Functions                         //
        //----------------------------------------------------------------------//
        public void GameTimer(object source, ElapsedEventArgs e)
        {
            StartInitialization();

            if (!m_Initialized) return;

            SendPsyEvent(psyGame.EventQ);
            SendPsyEvent(psyGame.CoreEventQ, true);
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
            if (e.ModLevel >= mod || e.PlayerName == "zxvf") return true;

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
