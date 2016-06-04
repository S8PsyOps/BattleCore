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
            this.m_CustomStaff = new List<string>();
            this.m_ArchivedGames = new List<Misc.ArchivedGames>();
            //this.m_BlockedList.Add("PsyOps");
            this.m_Settings = new Misc.SettingsLoader();

            this.m_SpamZoneTimeLimit = 5;
            this.m_SpamZoneTimeStamp = DateTime.Now;

            // Only set this if you want module loaded by default
            this.loadBaseDuel(" * Auto Load *");

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
        private Misc.SettingsLoader m_Settings;

        private List<string> m_BlockedList;
        private ushort m_BlockedListFreq;

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

            // store command here if all checks out
            string command;
            // making sure command is formatted properly
            if (isCommand(e, out command))
            {
                SSPlayer p = m_Players.GetPlayer(e.PlayerName);

                switch (command)
                {
                    case "status":
                        doStatus(p, e);
                        return;

                    case "toggle":
                        doToggle(p, e);
                        return;
                }

                // Commands after this point need module to be loaded
                if (m_Games == null) return;

                switch (command)
                {
                    case ".baseduel":
                        e.Message = "!help baseduel";
                        psyGame.CoreSend(e);
                        return;

                    case "games":
                        doGames(p, e);
                        return;

                    case "commands":
                        e.Message = "!help baseduel commands";
                        psyGame.CoreSend(e);
                        return;

                    case "start":
                        command_StartGame(p, e);
                        return;

                    case "toggle":
                        return;

                    case "settings":
                        return;

                    case "shuffle":
                        doShuffle(p,e);
                        return;

                    case "switch":
                        return;

                    case "hold":
                        return;

                    case "spam":
                        command_SpamZone(e);
                        return;

                    case "restart":
                        return;

                    case "reset":
                        return;

                    case "test":
                        doTest(p, e);
                        return;
                }
            }
        }

        public void doShuffle(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = this.getGame(p.Frequency);

            if (game != null && p.Ship != ShipTypes.Spectator)
            {
                if (game.gameStatus() == Misc.BaseGameStatus.NotStarted)
                {
                    List<SSPlayer> all = m_Players.PlayerList.FindAll(item => (item.Frequency == game.AlphaFreq || item.Frequency == game.BravoFreq) && !m_BlockedList.Contains(item.PlayerName));

                    // Shuffle the list we just created
                    Random ran = new Random();
                    for (int h = 0; h < 10; h++)
                    {
                        for (int i = 0; i < all.Count; i++)
                        {
                            SSPlayer temp = all[i];
                            int r = ran.Next(i, all.Count);
                            all[i] = all[r];
                            all[r] = temp;
                        }
                    }

                    bool alternate = false;

                    // Only setfreq to players that need to change
                    while (all.Count > 0)
                    {
                        alternate = !alternate;

                        if (all[0].Frequency != (alternate ? game.AlphaFreq : game.BravoFreq))
                            psyGame.Send(msg.pm(all[0].PlayerName, "?|setfreq " + (alternate ? game.AlphaFreq : game.BravoFreq) + "|shipreset"));
                        all.RemoveAt(0);
                    }
                }
                else
                {
                    psyGame.Send(msg.pm(e.PlayerName, "This command can only be used before a game. If you wish to reshuffle you must end current game and reset."));
                    return;
                }
            }
            else
            { psyGame.Send(msg.pm(p.PlayerName, "You need to be in a ship and on a game freq. Type [ !bd games ] for info on any active games.")); }
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
            //    m_BaseManager.ReleaseBase(tempBases[0],"BaseDuel");
            //    tempBases.RemoveAt(0);
            //}
        }

        // send spam to devastation chat and zone
        private void command_SpamZone(ChatEvent e)
        {
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
        public void doGames(SSPlayer p, ChatEvent e)
        {
            int rightOffset = 35;
            int leftOffset = 20;
            psyGame.Send(msg.pm(p.PlayerName, "[ Baseduel ] Active Game List         MultiGame [ "+(m_MultiGame?" On":"Off")+" ]"));
            psyGame.Send(msg.pm(p.PlayerName, "--------------------------------------------------------"));
            for (int i = 0; i < m_Games.Count; i++)
            {
                psyGame.Send(msg.pm(p.PlayerName, "Game          :".PadRight(leftOffset) + ((i + 1).ToString().PadLeft(2, '0')).PadLeft(rightOffset)));
                m_Games[i].getGameInfo(p);
                psyGame.Send(msg.pm(p.PlayerName, "--------------------------------------------------------"));
            }
        }

        // Checks surrent status of BaseDuel
        public void doStatus(SSPlayer p, ChatEvent e)
        {
            // show any print out for status
            psyGame.Send(msg.pm(p.PlayerName, "[ Baseduel ] Module is currently " + (m_Games == null?"Loaded":"Unloaded") + 
                (e.ModLevel >= ModLevels.Sysop?". To toggle on and off type: !baseduel toggle":".")));
        }

        // Toggle Baseduel On or Off
        public void doToggle(SSPlayer p, ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Sysop)) return;

            // Baseduel is Unloaded: do Load
            if (m_Games == null)
            {
                loadBaseDuel(p.PlayerName);
                return;
            }
            // Baseduel is Loaded: do unLoad
            unloadBaseDuel(p.PlayerName);
        }

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        private void command_StartGame(SSPlayer p, ChatEvent e)
        {
            Classes.BaseGame game = getGame(p.Frequency);

            if (p.Ship == ShipTypes.Spectator || game == null)
            {
                psyGame.Send(msg.pm(p.PlayerName, "You must be in a ship and in the freq of an open game. To see the list of active games type !bd games."));
            }
            else
            {
                game.command_Start(p);
            }
            return;
        }

        private void loadBaseDuel(string PlayerName)
        {
            // Create the list
            m_Games = new List<Classes.BaseGame>();

            // Config main bd game
            Classes.BaseGame pubGame = new Classes.BaseGame(msg,psyGame,m_Players,m_BaseManager,m_MultiGame);
            pubGame.setArchive(m_ArchivedGames);
            pubGame.setFreqs(0, 1);
            // Load normal settings to game
            this.m_Settings.LoadGameSettings(pubGame, Misc.GameSetting.Normal);
            // Add to game list
            m_Games.Add(pubGame);
            pubGame.setGameNum(this.m_Games.IndexOf(pubGame) + 1);

            Classes.BaseGame pubGame2 = new Classes.BaseGame(msg, psyGame, m_Players, m_BaseManager,m_MultiGame);
            pubGame2.setArchive(m_ArchivedGames);
            pubGame2.setFreqs(10, 11);
            // Load normal settings to game
            this.m_Settings.LoadGameSettings(pubGame2, Misc.GameSetting.Normal);
            // Add to game list
            m_Games.Add(pubGame2);
            pubGame2.setGameNum(this.m_Games.IndexOf(pubGame2) + 1);

            // load and configure stuff to start baseduel module
            psyGame.Send(msg.arena("[ BaseDuel ] Module Loaded - " + PlayerName));
            psyGame.Send(msg.arena("[ BaseDuel ] MultiGame Toggle: [ "+( this.m_MultiGame?"On":"Off" )+" ]"));
        }

        private void unloadBaseDuel(string PlayerName)
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
            if (m_Games == null) return;

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

                if (game == null) return;

                game.Event_PlayerPosition(p);
            }
        }

        public void Event_PlayerFreqChange(SSPlayer p)
        {
            // Module isnt on
            if (m_Games == null) return;

            Classes.BaseGame joinGame = getGame(p.Frequency);
            Classes.BaseGame leaveGame = getGame(p.OldFrequency);


            if (joinGame != null && joinGame.gameStatus() != Misc.BaseGameStatus.NotStarted)
            {
                joinGame.player_Join(p);
            }
            else if (leaveGame != null && joinGame.gameStatus() != Misc.BaseGameStatus.NotStarted)
            {
                leaveGame.player_Remove(p);
            }
        }

        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host)
        {
            // Module isnt on
            if (m_Games == null) return;

            Classes.BaseGame game = getGame(host.Frequency);

            if (game != null)
            {
                game.Event_TurretEvent(attacher, host);
            }
        }

        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        // Get game by freq
        private Classes.BaseGame getGame(ushort Freq)
        {
            return m_Games.Find(item => item.AlphaFreq == Freq || item.BravoFreq == Freq);
        }
        //// Check to see if player is in a game
        //private bool player_InGame( SSPlayer p, out int GameIndex, out Classes.BasePlayer basePlayer, out bool InAlpha)
        //{
        //    for (int i = 0; i < m_Games.Count; i++)
        //    {
        //        if (m_Games[i].playerInGame(p, out basePlayer, out InAlpha))
        //        {
        //            GameIndex = i;
        //            return true;
        //        }
        //    }

        //    GameIndex = 0;
        //    basePlayer = null;
        //    InAlpha = false;
        //    return false;
        //}

        //// Player is in a game freq
        //private bool player_inGameFreq(ushort Freq)
        //{
        //    return m_Games.Find(item => item.AlphaFreq == Freq || item.BravoFreq == Freq) != null;
        //}

        // checking if player is mod - if not sends back message
        private bool player_isMod(ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod || m_CustomStaff.Contains(e.PlayerName))
                return true;

            psyGame.Send(msg.pm(e.PlayerName, "You do not have access to this command. This is a staff command. Required Moerator level: [ " + mod + " ]."));
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
                    if (FullMessage.StartsWith("!bd") || FullMessage.StartsWith(".bd") || FullMessage.StartsWith("!baseduel") || FullMessage.StartsWith(".baseduel"))
                    {
                        // If command isnt a multiple just send original ".baseduel"
                        if (FullMessage.Contains(" ")) formattedCommand = FullMessage.Split(' ')[1];

                        // Send back the attached command = .baseduel [command]
                        else formattedCommand = ".baseduel";

                        return true;
                    }
                    FullMessage = FullMessage.Remove(0, 1).Trim().ToLower();

                    // Custom commands converted to standard
                    switch (FullMessage)
                    {
                        case "startbd":
                            formattedCommand = "start";
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
