using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseRace
{
    class BaseRace
    {
        public BaseRace(BaseManager BaseManager, SSPlayerManager PlayerManager, ShortChat msg, MyGame psyGame,FileDataBase fakeDB, DisplayManager gfx, string ArenaName)
        {
            this.m_FakeDB = fakeDB;
            this.m_GFX = gfx;
            m_GFX.RegisterScoreBoard("TestBoard", 20, 4);
            m_GFX.LoadScoreBoard_Public("TestBoard");
            m_GFX.ScoreChange_Public("TestBoard", 0);
            this.msg = msg;
            this.psyGame = psyGame;
            this.m_ArenaName = ArenaName;
            this.m_BaseManager = BaseManager;
            this.m_Players = PlayerManager;
            this.m_MultiGame = false;

            this.m_FreqStartIndex = 1337;

            this.m_BlockedList = new List<string>();
            this.m_CustomStaff = new List<string>();

            // Only set this if you want module loaded by default
            this.BaseRace_Load(" * Auto Load *");

            this.m_CustomStaff.Add("Ahmad~");
            this.m_CustomStaff.Add("zxvf");
            this.m_CustomStaff.Add("Devastated");
            this.m_CustomStaff.Add("Neostar");
            this.m_CustomStaff.Add("jDs");
        }

        private FileDataBase m_FakeDB;
        //private DisplayManager2 m_GFX;
        private DisplayManager m_GFX;
        private ShortChat msg;
        private MyGame psyGame;
        private string m_ArenaName;
        private SSPlayerManager m_Players;
        private BaseManager m_BaseManager;
        private bool m_MultiGame;
        private List<string> m_BlockedList;
        private List<string> m_CustomStaff;
        private List<RaceGame> m_Races;
        private ushort m_FreqStartIndex;

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        public void Commands(ChatEvent e)
        {
            if (m_BlockedList.Contains(e.PlayerName)) return;

            // make everything lower case
            e.Message = e.Message.ToLower();

            // store command here if all checks out
            string command;
            // making sure command is formatted properly
            if (isCommand(e, out command))
            {
                SSPlayer p = m_Players.GetPlayer(e.PlayerName);

                switch (command)
                {
                    case "status":                                          return;
                    case "baseinfo":    this.doBaseInfo(p,e);               return;
                    case "toggle":      this.command_BaseRaceToggle(p, e);  return;
                    case "test":        this.doTest(p, e);                  return;
                }

                // Commands after this point need module to be loaded
                if (m_Races == null) return;

                switch (command)
                {
                    case "brset":  return;
                    case "race": this.command_Race(p, e); return;
                }
            }
        }

        private void doTest(SSPlayer p, ChatEvent e)
        {
            int num;
            if (e.Message.Contains(' ') && e.Message.Split(' ').Count() == 3 && int.TryParse(e.Message.Split(' ')[2], out num))
            {
                if (num == 666)
                {
                    m_GFX.RefreshDisplay_Player(p);
                    return;
                }

                m_GFX.ScoreChange_Public("TestBoard", num);
                return;
            }

            psyGame.Send(msg.arena("Parse fail. [ " + e.Message.Split(' ')[2] + " ]"));
        }

        private void doBaseInfo(SSPlayer p, ChatEvent e)
        {
            int num;
            if (int.TryParse(e.Message.Split(' ')[2], out num))
            {
                List<BRPlayer> listRequested = this.m_FakeDB.GetBaseRecords(num);

                if (listRequested == null)
                {
                    psyGame.Send(msg.pm(p.PlayerName, "There are no records for Base Number[ " + num + " ]."));
                    return;
                }

                psyGame.Send(msg.pm(p.PlayerName, "+----------------------------------------------------------------------+"));
                psyGame.Send(msg.pm(p.PlayerName, "|  Base Times for:                                     BaseID #[ " + num.ToString().PadLeft(2, '0') + " ]  |"));
                psyGame.Send(msg.pm(p.PlayerName, "+----------------------------------------------------------------------+"));
                psyGame.Send(msg.pm(p.PlayerName, "|    Player".PadRight(25) + "Time".PadRight(22) + "Ship".PadRight(13) + "Date".PadRight(11) + "|"));
                psyGame.Send(msg.pm(p.PlayerName, "+----------------------------------------------------------------------+"));
                int i = 1;
                foreach (BRPlayer b in listRequested)
                {
                    psyGame.Send(msg.pm(p.PlayerName,"| "+ (i++).ToString().PadLeft(2,'0') + "." + b.PlayerName.PadRight(20) + b.Time.ToString().PadRight(22) + b.Ship.ToString().PadRight(13) + b.Date.ToShortDateString().PadRight(11) + "|"));
                }
                psyGame.Send(msg.pm(p.PlayerName, "+----------------------------------------------------------------------+"));
                psyGame.Send(msg.pm(p.PlayerName, "| For any help with commands type :                     .br commands   |"));
                psyGame.Send(msg.pm(p.PlayerName, "+----------------------------------------------------------------------+"));
                return;
            }
            psyGame.Send(msg.pm(p.PlayerName, "The correct format for this command is : .br baseinfo [ID#]      EXAMPLE:   .br baseinfo 23"));
        }

        private void command_Race(SSPlayer p, ChatEvent e)
        {
            this.m_Races[0].command_PlayGame(p,e);
        }

        // Toggle BaseRace On or Off
        private void command_BaseRaceToggle(SSPlayer p, ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Sysop)) return;

            // Baseduel is Unloaded: do Load
            if (m_Races == null)
            {
                BaseRace_Load(p.PlayerName);
                return;
            }
            // Baseduel is Loaded: do unLoad
            BaseRace_Unload(p.PlayerName);
        }

        //----------------------------------------------------------------------//
        //                           Events                                     //
        //----------------------------------------------------------------------//
        public void Event_PlayerPosition(SSPlayer p)
        {
            if (m_Races == null) return;
            RaceGame race = getRace(p);

            if (race == null) return;

            race.Event_PlayerPosition(p);
        }
        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host)
        {
            if (m_Races == null) return;
            RaceGame race = getRace(attacher);
            if (race == null) return;
            race.Event_PlayerTurretAttach(attacher, host);
        }
        public void Event_PlayerLeft(SSPlayer p)
        {
            if (m_Races == null) return;
            RaceGame race = getRace(p);
            if (race == null) return;
            race.race_PlayerLeaving(p);
        }
        public void Event_PlayerFreqChange(SSPlayer p) 
        {
            if (m_Races == null) return;
            RaceGame race = getRace(p.OldFrequency);
            if (race == null) return;

            race.race_PlayerLeaving(p);
        }

        //----------------------------------------------------------------------//
        //                     Race Functions                                   //
        //----------------------------------------------------------------------//
        private void BaseRace_Load(string PlayerName)
        {
            // Create the list
            m_Races = new List<RaceGame>();

            // Config main bd game
            RaceGame pubGame = new RaceGame(msg, psyGame, m_FakeDB, m_Players, m_BaseManager, m_MultiGame, 1);
            pubGame.raceFreq(this.m_FreqStartIndex);
            m_Races.Add(pubGame);

            if (this.m_MultiGame)
            {
            }

            // load and configure stuff to start baseduel module
            psyGame.Send(msg.arena("[ BaseRace ] Module Loaded - " + PlayerName));
            psyGame.Send(msg.arena("[ BaseRace ] MultiGame Toggle: [ " + (this.m_MultiGame ? "On" : "Off") + " ]"));
        }
        private void BaseRace_Unload(string PlayerName)
        {
            // Do all necessary stuff to unload module. Maybe record stuff, dunno
            int totalBases = m_Races.Count;
            // Make sure to release any bases that are being held by games
            while (m_Races.Count > 0)
            {
                m_BaseManager.ReleaseBase(m_Races[0].loadedBase(), "BaseRace");
                m_Races.RemoveAt(0);
            }
            psyGame.Send(msg.debugChan("[ BaseRace ] All basses released from BaseManager. Total Released:[ " + totalBases + " ]"));

            // Make game list null at end
            m_Races = null;
            psyGame.Send(msg.arena("[ BaseRace ] Module Unloaded - " + PlayerName));
        }
        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        private RaceGame getRace(SSPlayer p)
        {
            if (p.Ship == ShipTypes.Spectator) return null;
            return this.m_Races.Find(item => item.raceFreq() == p.Frequency);
        }
        private RaceGame getRace(ushort freq)
        {
            return this.m_Races.Find(item => item.raceFreq() == freq);
        }

        // checking if player is mod - if not sends back message
        private bool player_isMod(ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod || m_CustomStaff.Contains(e.PlayerName))
                return true;

            psyGame.Send(msg.pm(e.PlayerName, "You do not have access to this command. This is a staff command. Required Moderator level: [ " + mod + " ]."));
            return false;
        }

        // Series of checks to make sure the command is in proper format
        private bool isCommand(ChatEvent e, out string formattedCommand)
        {
            string FullMessage = e.Message;
            formattedCommand = "Doesnt matter if code sends this back, because it isnt a proper command";

            // Making sure its the right type of message
            if (e.ChatType == ChatTypes.Public || e.ChatType == ChatTypes.Private || e.ChatType == ChatTypes.Team || e.ChatType == ChatTypes.Macro)
            {
                // making sure our command is in message with a [!] or a [.]
                if (FullMessage.StartsWith("!") || FullMessage.StartsWith("."))
                {
                    if (FullMessage.StartsWith("!brset") || FullMessage.StartsWith(".brset") || FullMessage.StartsWith("!baseraceset") || FullMessage.StartsWith(".baseraceset"))
                    {
                        if (!FullMessage.Contains(" ") || !FullMessage.Split(' ')[1].Contains(":")) return false;

                        formattedCommand = "brset";

                        return true;
                    }
                    else if (FullMessage.StartsWith("!br") || FullMessage.StartsWith(".br") || FullMessage.StartsWith("!baserace") || FullMessage.StartsWith(".baserace"))
                    {
                        // If command isnt a multiple just send original ".baseduel"
                        if (FullMessage.Contains(" ")) formattedCommand = FullMessage.Split(' ')[1];

                        // Send back the attached command = .baseduel [command]
                        else formattedCommand = ".baserace";

                        return true;
                    }

                    FullMessage = FullMessage.Contains(" ") ? FullMessage.Remove(0, 1).Trim().ToLower().Split(' ')[0] : FullMessage.Remove(0, 1).Trim().ToLower();

                    // Custom commands converted to standard
                    switch (FullMessage)
                    {
                        case "race":
                            formattedCommand = "race";
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
