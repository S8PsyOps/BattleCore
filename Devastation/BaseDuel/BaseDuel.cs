using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseDuel
{
    public class BaseDuel
    {
        public BaseDuel(BaseManager BaseManager, SSPlayerManager PlayerManager, ShortChat msg, MyGame psyGame, string ArenaName)
        {
            this.msg = msg;
            this.psyGame = psyGame;
            this.m_ArenaName = ArenaName;
            this.m_BaseManager = BaseManager;
            this.m_Players = PlayerManager;
            this.m_MultiGame = true;

            this.m_BlockedList = new List<string>();
            this.m_BlockedListFreq = 2;
            this.m_SpamMeList = new List<string>();
            this.m_CustomStaff = new List<string>();
            this.m_ArchivedGames = new List<Misc.ArchivedGames>();
            //this.m_BlockedList.Add("air con");

            this.m_SpamZoneTimeLimit = 5;
            this.m_SpamZoneTimeStamp = DateTime.Now;

            // Only set this if you want module loaded by default
            this.BaseDuel_Load(" * Auto Load *");

            this.m_CustomStaff.Add("Ahmad~");
            this.m_CustomStaff.Add("zxvf");
            this.m_CustomStaff.Add("Devastated");
            this.m_CustomStaff.Add("Neostar");
            this.m_CustomStaff.Add("jDs");
        }

        private ShortChat msg;
        private MyGame psyGame;
        private string m_ArenaName;
        private SSPlayerManager m_Players;
        private BaseManager m_BaseManager;

        private List<string> m_BlockedList;
        private ushort m_BlockedListFreq;

        private List<string> m_SpamMeList;

        private List<string> m_CustomStaff;
        private int m_SpamZoneTimeLimit;            // Minutes before a user can !spam
        private DateTime m_SpamZoneTimeStamp;       // Timestamp for !spam usage

        private List<Classes.BaseGame> m_Games;
        private List<Misc.ArchivedGames> m_ArchivedGames;
        private bool m_MultiGame;

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
                    case "status":  this.command_Status(p, e);          return;
                    case "toggle":  this.command_BaseDuelToggle(p, e);  return;
                }

                // Commands after this point need module to be loaded
                if (m_Games == null) return;

                switch (command)
                {
                    case "bdset":       this.command_SettingChanges(p, e);  return;
                    case "lock":        this.command_GameLock(p, e);        return;
                    case "games":       this.command_ShowGames(p, e);       return;
                    case "start":       this.command_GameStart(p, e);       return;
                    case "shuffle":     this.command_Shuffle(p, e);         return;
                    case "hold":        this.command_GameHold(p, e);        return;
                    case "spam":        this.command_SpamZone(e);           return;
                    case "updates":     this.command_SpamMe(p, e);          return;
                    case "restart":     this.command_PointRestart(p,e);     return;
                    case "reset":       this.command_GameReset(p, e);       return;
                    case "test":        this.doTest(p, e);                  return;
                    case ".baseduel":   this.command_BaseDuel(p, e);        return;
                    case "commands":    this.command_BaseCommands(p, e);    return;
                    case "stats":       this.command_GetPlayerStats(p, e);  return;
                    case "score":       this.command_GetGameScore(p,e);     return;
                }
            }
        }

        public void doTest(SSPlayer p, ChatEvent e)
        {
            //psyGame.Send(msg.pm(p.PlayerName, "?|setship 9|setship " + ((int)p.Ship + 1)));
            //List<Base> tempBases = new List<Base>();

            //for (int i = 0; i < 5; i++)
            //{
            //    Base nextBase = m_BaseManager.getNextBase();
            //    tempBases.Add(nextBase);
            //    Queue<EventArgs> bInfo = m_BaseManager.getBaseInfo(e.PlayerName, nextBase);
            //    while (bInfo.Count > 0)
            //        psyGame.Send(bInfo.Dequeue());
            //}

            //while (tempBases.Count > 0)
            //{
            //    m_BaseManager.ReleaseBase(tempBases[0], "BaseDuel");
            //    tempBases.RemoveAt(0);
            //}
        }

        private void command_GetGameScore(SSPlayer p, ChatEvent e)
        {
            if (p.Ship == ShipTypes.Spectator)
            {
                psyGame.Send(msg.pm(p.PlayerName, "+--------------------------------------------------+"));
                psyGame.Send(msg.pm(p.PlayerName, "| Game Number               Score                  |"));
                psyGame.Send(msg.pm(p.PlayerName, "+--------------------------------------------------+"));
                foreach (Classes.BaseGame b in this.m_Games)
                {
                    //psyGame.Send(msg.pm(p.PlayerName, "Game[ " + b.gameNum().ToString().PadLeft(2,'0') + " ]     Freq[ " + b.AlphaFreq.ToString().PadLeft(2,'0') + " ]   " + b.AlphaScore + " - " + b.BravoScore + "   [ " + b.BravoFreq + " ]Freq"));
                    psyGame.Send(msg.pm(p.PlayerName, "| [ " + b.gameNum().ToString().PadLeft(2, '0') + " ]       Freq[ " + b.AlphaFreq.ToString().PadLeft(2) + " ]  " + b.AlphaScore.ToString().PadLeft(2) + " - " + b.BravoScore.ToString().PadRight(2) + "   [ " + b.BravoFreq.ToString().PadLeft(2) + " ]Freq    |"));
                }
                psyGame.Send(msg.pm(p.PlayerName, "+--------------------------------------------------+"));
                return;
            }

            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.None);

            if (game != null)
            {
                psyGame.Send(msg.pm(p.PlayerName, game.AlphaName + "   " + game.AlphaScore.ToString().PadLeft(2) + " - " + game.BravoScore.ToString().PadRight(2) + "   " + game.BravoName));
            }
        }

        private void command_GetPlayerStats(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.None);

            if (game != null)
            { game.command_GetPlayerStats(p); }
        }

        private void command_SettingChanges(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.Mod);

            if (game != null)
            { game.command_SettingChange(p, e); }
        }

        private void command_BaseCommands(SSPlayer p, ChatEvent e)
        {
            e.Message = "!help baseduel commands";
            this.psyGame.CoreSend(e);
        }

        private void command_BaseDuel(SSPlayer p, ChatEvent e)
        {
            e.Message = "!help baseduel";
            this.psyGame.CoreSend(e);
        }

        private void command_SpamMe(SSPlayer p, ChatEvent e)
        {
            if (this.m_SpamMeList.Contains(p.PlayerName))
            {
                this.m_SpamMeList.Remove(p.PlayerName);
                psyGame.Send(msg.pm(p.PlayerName, "You have been removed from the personal game update list."));
                return;
            }

            this.m_SpamMeList.Add(p.PlayerName);
            psyGame.Send(msg.pm(p.PlayerName, "You have been added to the personal game update list. You will receive activity private messages for all games."));
        }

        private void command_GameLock(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.Mod);

            if (game != null)
            { game.command_GameLock(p); }
        }

        private void command_Shuffle(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.Mod);

            if (game != null)
            { game.command_ShuffleTeams(p,this.m_BlockedList); }
        }

        private void command_GameReset(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.Mod);

            if (game != null)
            { game.command_GameReset(p); }
        }

        private void command_GameHold(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.Mod);

            if (game != null)
            { game.command_GameHold(p); }
        }

        private void command_PointRestart(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.Mod);

            if (game != null)
            { game.command_PointReset(p); }
        }

        private void command_GameStart(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame_FromCommand(p, e, ModLevels.None);

            if (game != null)
            {
                if (game == this.getGame(p.Frequency))
                    game.command_GameStart(p);
                else if ( player_isMod(e,ModLevels.Mod))
                    game.command_GameStart(p);
            }
        }

        // send spam to devastation chat and zone
        private void command_SpamZone(ChatEvent e)
        {
            if (e.ModLevel != ModLevels.Sysop)
            if ((DateTime.Now - m_SpamZoneTimeStamp).TotalMinutes < m_SpamZoneTimeLimit)
            {
                psyGame.Send(msg.pm(e.PlayerName, "This command can only be used every " + m_SpamZoneTimeLimit + " minutes. You have " + Math.Floor(m_SpamZoneTimeLimit - (DateTime.Now - m_SpamZoneTimeStamp).TotalMinutes) + "m:" + Math.Floor((double)60 - (DateTime.Now - m_SpamZoneTimeStamp).Seconds).ToString().PadLeft(2, '0') + "s before it can use it again."));
                return;
            }

            // update timestamp
            m_SpamZoneTimeStamp = DateTime.Now;

            // Send message out - maybe add option to what gets sent out - option to change
            string message = "A BaseDuel game is about to begin. Come to Devastation and join the battle!   Arena:[ ?go "+m_ArenaName+" ]     -" + e.PlayerName;
            psyGame.Send(msg.zone(message));
            psyGame.SafeSend(msg.chan(2, message));
        }

        // Print out on all games in progress
        public void command_ShowGames(SSPlayer p, ChatEvent e)
        {
            psyGame.Send(msg.pm(p.PlayerName, "[ Baseduel ] Active Game List         MultiGame [ "+(m_MultiGame?"- On -":"- Off -")+" ]"));
            psyGame.Send(msg.pm(p.PlayerName, "--------------------------------------------------------"));
            for (int i = 0; i < m_Games.Count; i++)
            {
                psyGame.Send(msg.pm(p.PlayerName, "." + ("-<[  Game " + (i + 1).ToString().PadLeft(2, '0') + "  ]>-").PadLeft(33)));
                m_Games[i].getGameInfo(p);
                psyGame.Send(msg.pm(p.PlayerName, "--------------------------------------------------------"));
            }
        }

        // Checks surrent status of BaseDuel
        public void command_Status(SSPlayer p, ChatEvent e)
        {
            // show any print out for status
            psyGame.Send(msg.pm(p.PlayerName, "[ Baseduel ] Module is currently " + (m_Games == null?"Loaded":"Unloaded") + 
                (e.ModLevel >= ModLevels.Sysop?". To toggle on and off type: !baseduel toggle":".")));
        }

        // Toggle Baseduel On or Off
        public void command_BaseDuelToggle(SSPlayer p, ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Sysop)) return;

            // Baseduel is Unloaded: do Load
            if (m_Games == null)
            {
                BaseDuel_Load(p.PlayerName);
                return;
            }
            // Baseduel is Loaded: do unLoad
            BaseDuel_Unload(p.PlayerName);
        }

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        private void BaseDuel_Load(string PlayerName)
        {
            // Create the list
            m_Games = new List<Classes.BaseGame>();

            // Config main bd game
            Classes.BaseGame pubGame = new Classes.BaseGame(msg,psyGame,m_Players,m_BaseManager,m_MultiGame, m_Games.Count + 1);
            pubGame.setFreqs(0, 1);
            pubGame.setSpamMeList(this.m_SpamMeList);
            pubGame.setArchive(m_ArchivedGames);
            m_Games.Add(pubGame);

            if (this.m_MultiGame)
            {
                Classes.BaseGame pubGame2 = new Classes.BaseGame(msg, psyGame, m_Players, m_BaseManager, m_MultiGame, m_Games.Count + 1);
                pubGame2.setFreqs(10, 11);
                pubGame2.setSpamMeList(this.m_SpamMeList);
                pubGame2.setArchive(m_ArchivedGames);
                m_Games.Add(pubGame2);
            }

            // load and configure stuff to start baseduel module
            psyGame.Send(msg.arena("[ BaseDuel ] Module Loaded - " + PlayerName));
            psyGame.Send(msg.arena("[ BaseDuel ] MultiGame Toggle: [ "+( this.m_MultiGame?"On":"Off" )+" ]"));
        }

        private void BaseDuel_Unload(string PlayerName)
        {
            // Do all necessary stuff to unload module. Maybe record stuff, dunno
            int totalBases = m_Games.Count;
            // Make sure to release any bases that are being held by games
            while (m_Games.Count > 0)
            {
                m_BaseManager.ReleaseBase(m_Games[0].loadedBase(), "BaseDuel");
                m_Games.RemoveAt(0);
            }
            psyGame.Send(msg.debugChan("[ BaseDuel ] All basses released from BaseManager. Total Released:[ "+totalBases+" ]"));

            // Make game list null at end
            m_Games = null;
            psyGame.Send(msg.arena("[ BaseDuel ] Module Unloaded - " + PlayerName));
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void Event_PlayerPosition(SSPlayer p)
        {
            // Module isnt on
            if (m_Games == null ) return;

            // Entire game is based off of this event- Is Safe: either they are in center or in base
            if (p.Position.ShipState.IsSafe)
            {
                Classes.BaseGame game = getGame(p.Frequency);

                // Deal with blocked players first
                if (m_BlockedList.Contains(p.PlayerName) && game != null)
                {
                    psyGame.Send(msg.pm(p.PlayerName, "?|setfreq " + m_BlockedListFreq + "|prize fullcharge"));
                    return;
                }

                if (game == null || game.gameStatus() == Misc.BaseGameStatus.OnHold) return;

                game.Event_PlayerPosition(p);
            }
            else if (p.Position.Weapon.Type != WeaponTypes.NoWeapon)
            {
                Classes.BaseGame game = getGame(p.Frequency);

                if (game != null)
                    game.Event_PlayerFiredWeapon(p);
            }
        }

        public void Event_PlayerLeft(SSPlayer p)
        {
            // Module isnt on
            if (m_Games == null) return;
            Classes.BaseGame game = getGame(p.Frequency);

            if (game != null)
            {
                game.player_Remove(p);
            }
        }

        public void Event_PlayerFreqChange(SSPlayer p)
        {
            // Module isnt on
            if (m_Games == null) return;

            Classes.BaseGame joinGame = getGame(p.Frequency);
            Classes.BaseGame leaveGame = getGame(p.OldFrequency);

            if (leaveGame != null && leaveGame.gameStatus() != Misc.BaseGameStatus.NotStarted)
            { leaveGame.player_Remove(p); }

            if (joinGame != null && joinGame.gameStatus() != Misc.BaseGameStatus.NotStarted && !joinGame.lockedStatus())
            { joinGame.player_Join(p); }
        }

        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host)
        {
            // Module isnt on
            if (m_Games == null) return;

            Classes.BaseGame game = getGame(host.Frequency);

            if (game == null) return;

            game.Event_TurretEvent(attacher, host);
        }

        public void Event_PlayerKilled(SSPlayer Attacker, SSPlayer Victim)
        {
            // Module isnt on
            if (m_Games == null) return;

            Classes.BaseGame game = getGame(Attacker.Frequency);

            if (game == null) return;

            if (game.gameStatus() != Misc.BaseGameStatus.InProgress) return;

            game.Event_PlayerKilled(Attacker, Victim);
        }

        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        // Get game by freq
        private Classes.BaseGame getGame(ushort Freq)
        {
            return m_Games.Find(item => item.AlphaFreq == Freq || item.BravoFreq == Freq);
        }

        // get game from command
        private Classes.BaseGame getGame_FromCommand(SSPlayer p, ChatEvent e, ModLevels modLvl)
        {
            Classes.BaseGame game = this.getGame(p.Frequency);
            string[] data = e.Message.Split(' ');
            int num;
            
            if (game == null)
            {
                if (player_isMod(e, ModLevels.Mod))
                {
                    if (data.Length >= 2)
                    {
                        if (int.TryParse(data[1], out num) && num > 0 && num <= this.m_Games.Count)
                        {
                            return this.m_Games[num - 1];
                        }

                        if ( data.Length > 2 && int.TryParse(data[2], out num) && num > 0 && num <= this.m_Games.Count)
                        {
                            return this.m_Games[num - 1];
                        }

                        if (p.Ship == ShipTypes.Spectator)
                        {
                            psyGame.Send(msg.pm(p.PlayerName, "You are not on an active game freq. To see the list of active games type !bd games. For help on BD commands type: !bd commands."));
                            return null;
                        }
                    }
                    else
                    {
                        psyGame.Send(msg.pm(p.PlayerName, "You must be in the active game freq to use this command. To see the list of active games type !bd games. For help on BD commands type: !bd commands."));
                        return null;
                    }
                }
                return null;
            }
            else
            {
                if (data.Length == 3)
                {
                    if (int.TryParse(data[2], out num) && num > 0 && num <= this.m_Games.Count)
                    {
                        return this.m_Games[num - 1];
                    }
                }

                if (this.player_isMod(e, modLvl))
                {
                    return this.m_Games[game.gameNum() - 1];
                }
                return null;
            }
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
                    if (FullMessage.StartsWith("!bdset") || FullMessage.StartsWith(".bdset") || FullMessage.StartsWith("!baseduelset") || FullMessage.StartsWith(".baseduelset"))
                    {
                        if (!FullMessage.Contains(" ") || !FullMessage.Split(' ')[1].Contains(":")) return false;

                        formattedCommand = "bdset";

                        return true;
                    }
                    else if (FullMessage.StartsWith("!bd") || FullMessage.StartsWith(".bd") || FullMessage.StartsWith("!baseduel") || FullMessage.StartsWith(".baseduel"))
                    {
                        // If command isnt a multiple just send original ".baseduel"
                        if (FullMessage.Contains(" ")) formattedCommand = FullMessage.Split(' ')[1];

                        // Send back the attached command = .baseduel [command]
                        else formattedCommand = ".baseduel";

                        return true;
                    }

                    FullMessage = FullMessage.Contains(" ") ? FullMessage.Remove(0, 1).Trim().ToLower().Split(' ')[0] : FullMessage.Remove(0, 1).Trim().ToLower();

                    // Custom commands converted to standard
                    switch (FullMessage)
                    {
                        case "startbd":
                            formattedCommand = "start";
                            return true;
                        case "score":
                            formattedCommand = "score";
                            return true;
                        case "shuffleteam":
                            formattedCommand = "shuffle";
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
