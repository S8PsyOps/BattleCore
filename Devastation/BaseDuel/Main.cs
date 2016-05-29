using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

// Needed for timers
using System.Timers;

namespace Devastation.BaseDuel
{
    class Main
    {
        public Main( byte[] MapData, SSPlayerManager PlayerManager, ShortChat msg, MyGame myGame)
        {
            this.m_Timer = new Timer();
            this.m_BaseManager = new BaseManager(MapData);
            this.m_Base = m_BaseManager.CurrentBase;
            this.m_Lobby = m_BaseManager.Lobby;

            this.m_BaseGame = new BaseGame();

            this.m_AlphaFreq = 887;
            this.m_BravoFreq = 889;
            this.m_AlphaStartFreq = 0;
            this.m_BravoStartFreq = 1;

            this.m_BaseGame.AlphaFreq = m_AlphaFreq;
            this.m_BaseGame.BravoFreq = m_BravoFreq;
            this.m_BaseGame.AlphaStartFreq = m_AlphaStartFreq;
            this.m_BaseGame.BravoStartFreq = m_BravoStartFreq;

            this.m_MaxArenaFreq = 100;
            this.m_BlockedList = new List<string>();
            this.m_StartGameDelay = 10;
            this.m_StartGameShow = 5;
            this.m_TeamClearDelay = 5;
            this.m_GamesPlayed = new List<BaseGame>();
            

            this.m_Players = PlayerManager;
            this.msg = msg;
            this.psyGame = myGame;
            this.m_BotSpamSetting = ChatTypes.Arena;
            this.m_SpamZoneTimeLimit = 5;
            this.m_SpamZoneTimeStamp = DateTime.Now;
        }

        private ShortChat msg;                      // my Module to make sending chat messages easier
        private MyGame psyGame;                      // my method to send events from sub-modules
        private BaseGame m_BaseGame;                // This holds all the baseduel game info
        
        private Timer m_Timer;                      // Main timer for any needed timed events
        private double m_StartGameDelay;            // Delay in start of game (seconds)
        private double m_StartGameDelay_Elapsed;    // Used for start timer - not to be set
        private double m_StartGameShow;             // How many seconds to display countdown as txt or gfx ( 3..2..1 )
        private double m_TeamClearDelay;            // How many seconds before BaseClear Takes effect - helps with lag attach
        
        private BaseManager m_BaseManager;          // Controls bases and how they are loaded
        private Base m_Base;                        // Local variable to hold loaded base
        private Base m_Lobby;                       // Hold lobby dimensions and info

        private ushort m_AlphaFreq;                 // Keeping these local - may help creating multiple games playable at once later
        private ushort m_BravoFreq;                 // They are stored inside BaseGame as well, so you can create more games with more designated freqs
        private ushort m_AlphaStartFreq;                 // Keeping these local - may help creating multiple games playable at once later
        private ushort m_BravoStartFreq;                 // They are stored inside BaseGame as well, so you can create more games with more designated freqs
        private ushort m_MaxArenaFreq;              // I need to find a way to get this info from server on startup - in case it changes

        private List<string> m_BlockedList;         // Users to ignore - use this for a ban/block or for any diff games that may go on (baserace etc)
        
        private List<BaseGame> m_GamesPlayed;       // List of games that have been recorded
        private SSPlayerManager m_Players;          // Player tracker code

        private ChatTypes m_BotSpamSetting;         // What type of chat the bot will spam in
        private int m_SpamZoneTimeLimit;            // Minutes before a user can !spam
        private DateTime m_SpamZoneTimeStamp;       // Timestamp for !spam usage

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        /// <summary>
        /// <para>All commands used for baseduel send through here.</para>
        /// <para>Syntax: !baseduel command    Example !baseduel settings  or  .baseduel start</para>
        /// <para>If you want to make compatible commands, register them in devastation main</para>
        /// <para>and change the command to make it compatible with !baseduel command</para>
        /// <para>Example: RegisterCommand("!startbd") , then change incoming message e.Message = "!baseduel start"</para>
        /// <para>then send it here. BaseDuelCommands(e);</para>
        /// </summary>
        /// <param name="p">Deva Player</param>
        /// <param name="e">Command sent</param>
        /// <returns></returns>
        public void Commands( ChatEvent e)
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
                    case ".baseduel":
                        if (!moduleIsOn(e)) return;
                        e.Message = "!help baseduel";
                        psyGame.CoreSend(e);
                        return;

                    case "commands":
                        if (!moduleIsOn(e)) return;
                        e.Message = "!help baseduel commands";
                        psyGame.CoreSend(e);
                        return;

                    case "startbd":
                    case "start":
                        command_Start(e);
                        return;

                    case "toggle":
                        command_ModuleToggle(e);
                        return;

                    case "settings":
                        if (!moduleIsOn(e)) return;
                        show_GameSettings(p.PlayerName);
                        return;

                    case "shuffleteam":
                    case "shuffle":
                        if (!moduleIsOn(e)) return;
                        command_Shuffle(e);
                        return;

                    case "switch":

                        return;

                    case "spam":
                        command_SpamZone(e);
                        return;

                    case "restart":
                        if (!moduleIsOn(e)) return;
                        if (m_BaseGame.Status != BaseGameStatus.GameIdle)
                        round_Restart(e);
                        return;

                    case "reset":
                        if (!moduleIsOn(e)) return;
                        game_Reset(e);
                        return;

                    case "rounds":
                        if (!moduleIsOn(e)) return;
                        show_Rounds(p.PlayerName);
                        return;
                }
            }
        }

        private void command_ModuleToggle(ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Mod)) return;

            // Turn On module
            if (m_BaseGame == null)
            {
                // reset vars
                module_Reset();

                // Send Arena Message
                show_StartSpam();
                psyGame.Send(msg.arena("[ BaseDuel ] Module loaded by staff - " + e.PlayerName));
                show_GameSettings(e.PlayerName);
                return;
            }

            if (m_Timer.Enabled) m_Timer.Stop();
            // Turn Module Off
            m_BaseGame = null;
            psyGame.Send(msg.arena("[ BaseDuel ] Module unloaded by staff - " + e.PlayerName));
        }

        // send spam to devastation chat and zone
        private void command_SpamZone(ChatEvent e)
        {
            if ((DateTime.Now - m_SpamZoneTimeStamp).TotalMinutes < m_SpamZoneTimeLimit)
            {
                psyGame.Send(msg.pm(e.PlayerName, "This command can only be used every " + m_SpamZoneTimeLimit + " minutes. You have " + Math.Floor(m_SpamZoneTimeLimit - (DateTime.Now - m_SpamZoneTimeStamp).TotalMinutes) + "m:" + Math.Floor((double)60 - (DateTime.Now - m_SpamZoneTimeStamp).Seconds).ToString().PadLeft(2,'0') + "s before it can use it again."));
                return;
            }

            m_SpamZoneTimeStamp = DateTime.Now;

            string message = "A BaseDuel game is about to begin. Come to Devastation and join the battle! -" + e.PlayerName;

            psyGame.Send(msg.zone(message));
            psyGame.SafeSend(msg.chan(2, message));
        }

        private void command_Start(ChatEvent e)
        {
            // Module is off
            if (!moduleIsOn(e)) return;

            // only start game if pre-game
            if (m_Timer.Enabled || m_BaseGame.Status != BaseGameStatus.GameIdle)
            {
                psyGame.Send(msg.pm(e.PlayerName, "There is currently a game in progress. Please wait until game is over before using this command."));
                return;
            }

            // start game - enough players
            if (allow_GameStart(true))
            {
                psyGame.Send(msg.arena("- Starting BaseDuel in " + m_StartGameDelay + " seconds - " + e.PlayerName));
                //sendBotSpam("- Starting BaseDuel in " + m_StartGameDelay + " seconds - " + e.PlayerName);
                players_MoveToTeams();

                psyGame.Send(msg.team_pm(m_BaseGame.Round.AlphaTeam.TeamMembers[0].PlayerName, "- Welcome to team: " + m_BaseGame.Round.AlphaTeam.TeamName + "! Good luck!"));
                psyGame.Send(msg.team_pm(m_BaseGame.Round.BravoTeam.TeamMembers[0].PlayerName, "- Welcome to team: " + m_BaseGame.Round.BravoTeam.TeamName + "! Good luck!"));

                timer_Setup(TimerType.GameStart);
                return;
            }
            else // Not enough players
            {
                psyGame.Send(msg.pm(e.PlayerName, "In order to start a game you must have at least 1 player on each team."));
                return;
            }
        }

        // This command will be mod only until i can code a voting system - maybe
        private void command_Shuffle(ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Mod)) return;

            if (m_BaseGame.Status != BaseGameStatus.GameIdle)
            {
                psyGame.Send(msg.pm(e.PlayerName, "This command can only be used before a game. If you wish to reshuffle you must end current game and reset."));
                return;
            }
            List<SSPlayer> all = m_Players.PlayerList.FindAll(item => (item.Frequency == m_BaseGame.AlphaStartFreq || item.Frequency == m_BaseGame.BravoStartFreq) && !m_BlockedList.Contains(item.PlayerName));

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

                if (all[0].Frequency != (alternate ? 0 : 1))
                    psyGame.Send(msg.pm(all[0].PlayerName, "?|setfreq " + (alternate ? 0 : 1) + "|shipreset"));
                all.RemoveAt(0);
            }
        }

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        // reset all vars to baseduel
        private void module_Reset()
        {
            m_BaseGame = new BaseGame();
            m_BaseGame.AlphaFreq = m_AlphaFreq;
            m_BaseGame.BravoFreq = m_BravoFreq;
            m_BaseGame.AlphaStartFreq = m_AlphaStartFreq;
            m_BaseGame.BravoStartFreq = m_BravoStartFreq;
            m_BaseGame.Status = BaseGameStatus.GameIdle;
        }

        private void game_Start()
        {
            // get next round rdy
            if (m_BaseGame.Status == BaseGameStatus.GameIntermission)
                round_ResetPLayers();

            // send players to start and reset ships
            psyGame.Send(msg.team_pm(m_AlphaFreq,"?|warpto " + m_Base.AlphaStartX + " " + m_Base.AlphaStartY + "|shipreset"));
            psyGame.Send(msg.team_pm(m_BravoFreq, "?|warpto " + m_Base.BravoStartX + " " + m_Base.BravoStartY + "|shipreset"));

            int baseNum = m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1;
            //sendBotSpam("- Go Go Go Go Go Go! -");

            if (m_BaseGame.AllRounds.Count == 0)
            {
                sendBotSpam("- Current Base        : " + baseNum);
                sendBotSpam("- Team vs Team Rules  : " + (m_BaseGame.AllowSafeWin ? "BaseClear and Safes" : "BaseClear"));
                sendBotSpam("- Auto Scoring        : Not implemented yet.");
            }

            // Record start time
            m_BaseGame.Round.StartTime = DateTime.Now;
            // set status
            m_BaseGame.Status = BaseGameStatus.GameOn;
            psyGame.SafeSend(msg.debugChan("GameStatus change to GameOn."));
        }

        private void game_Save()
        {
            BaseGame saved = new BaseGame();
            saved.AllowAfterStartJoin = m_BaseGame.AllowAfterStartJoin;
            saved.AllowSafeWin = m_BaseGame.AllowSafeWin;
            saved.AllRounds = m_BaseGame.AllRounds;
            saved.AlphaFreq = m_BaseGame.AlphaFreq;
            saved.AlphaScore = m_BaseGame.AlphaScore;
            saved.BravoFreq = m_BaseGame.BravoFreq;
            saved.BravoScore = m_BaseGame.BravoScore;
            saved.MinimumWin = m_BaseGame.MinimumWin;
            saved.Round = m_BaseGame.Round;
            saved.Status = m_BaseGame.Status;
            saved.WinBy = m_BaseGame.WinBy;
            m_GamesPlayed.Add(saved);
        }

        private void game_Reset(ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Mod)) return;
            game_Reset();
        }
        private void game_Reset()
        {
            m_BaseGame = new BaseGame();
            m_BaseGame.AlphaFreq = m_AlphaFreq;
            m_BaseGame.BravoFreq = m_BravoFreq;
            m_BaseGame.Status = BaseGameStatus.GameIdle;

            psyGame.Send(msg.team_pm(m_AlphaFreq, "?|prize warp|setfreq "+m_BaseGame.AlphaStartFreq+"|shipreset|"));
            psyGame.Send(msg.team_pm(m_BravoFreq, "?|prize warp|setfreq " + m_BaseGame.BravoStartFreq + "|shipreset|"));

            // Stop timer just in case the all out timer started
            if (m_Timer.Enabled) m_Timer.Stop();
        }

        private void round_Finished(WinType winType, bool AlphaWon)
        {
            round_Finished(winType,AlphaWon,"");
        }
        private void round_Finished(WinType winType, bool AlphaWon, string PlayerName)
        {
            m_BaseGame.Status = BaseGameStatus.GameIntermission;

            if (winType != WinType.NoCount)
            {
                // assign points
                if (AlphaWon) m_BaseGame.AlphaScore++;
                else m_BaseGame.BravoScore++;

                if (winType == WinType.BaseClear)
                {
                    sendBotSpam(("- " + (!AlphaWon ? m_BaseGame.Round.AlphaTeam.TeamName : m_BaseGame.Round.BravoTeam.TeamName) + " is all dead! --  Team " + (AlphaWon ? m_BaseGame.Round.AlphaTeam.TeamName : m_BaseGame.Round.BravoTeam.TeamName) + " scores! -"));
                }
                else if (winType == WinType.SafeWin)
                {
                    m_BaseGame.Round.SafeWinner = PlayerName;
                    sendBotSpam("- [ " + PlayerName + " ] has breached the enemy's defense! (SafeWin) " + (AlphaWon ? "[ " + m_BaseGame.Round.AlphaTeam.TeamName + " Team Win ]" : "[ " + m_BaseGame.Round.BravoTeam.TeamName + " Team Win ]"));
                }

                int aNameCount = m_BaseGame.Round.AlphaTeam.TeamName.Length;
                int bNameCount = m_BaseGame.Round.BravoTeam.TeamName.Length;
                //- Score: Obsidian 2:2 BattleZone -
                //sendBotSpam("- " + m_BaseGame.Round.AlphaTeam.TeamName + " [ " + m_BaseGame.AlphaScore.ToString() + " - " + m_BaseGame.BravoScore.ToString() + " ] " + m_BaseGame.Round.BravoTeam.TeamName + " -");
                //sendBotSpam("- Score: " + m_BaseGame.Round.AlphaTeam.TeamName + " [ " + m_BaseGame.AlphaScore.ToString() + " - " + m_BaseGame.BravoScore.ToString() + " ] " + m_BaseGame.Round.BravoTeam.TeamName + " -");
                //sendBotSpam("- Score: " + m_BaseGame.Round.AlphaTeam.TeamName + " " + m_BaseGame.AlphaScore.ToString() + ":" + m_BaseGame.BravoScore.ToString() + " " + m_BaseGame.Round.BravoTeam.TeamName + " -");
                sendBotSpam("- " + m_BaseGame.Round.AlphaTeam.TeamName + " [ " + m_BaseGame.AlphaScore.ToString() + " ]     Score     [ " + m_BaseGame.BravoScore.ToString() + " ] " + m_BaseGame.Round.BravoTeam.TeamName + " -");
            }
            else
                sendBotSpam("- Both teams out -- No count -");

            m_BaseGame.Round.AlphaWon = AlphaWon;
            m_BaseGame.Round.WinType = winType;
            round_Save();

            //only load next base if it wasnt nocount
            if (winType != WinType.NoCount) loadNextBase();

            // Check for end of game
            if ((m_BaseGame.AlphaScore >= m_BaseGame.MinimumWin || m_BaseGame.BravoScore >= m_BaseGame.MinimumWin) && Math.Abs(m_BaseGame.AlphaScore - m_BaseGame.BravoScore) >= 2)
            {
                psyGame.Send(msg.arena((m_BaseGame.AlphaScore > m_BaseGame.BravoScore ? m_BaseGame.Round.AlphaTeam.TeamName : m_BaseGame.Round.BravoTeam.TeamName) + " wins the game. Final Score Alpha[ " + m_BaseGame.AlphaScore + " ] [ " + m_BaseGame.BravoScore + " ]Bravo.   -Print out here."));
                game_Save();
                game_Reset();
                return;
            }

            if (winType == WinType.NoCount)
                sendBotSpam("- Restarting point in " + m_StartGameDelay + " seconds! -");
            else
                sendBotSpam("- Starting next base in " + m_StartGameDelay + " seconds! -");
            
            timer_Setup(TimerType.GameStart);
        }

        private void round_Save()
        {
            // Copy over round so we can save it
            BaseRound saved = new BaseRound();
            saved.AlphaTeam = m_BaseGame.Round.AlphaTeam;
            saved.BravoTeam = m_BaseGame.Round.BravoTeam;
            saved.AlphaWon = m_BaseGame.Round.AlphaWon;
            saved.BaseNumber = (short)(m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1);
            saved.StartTime = m_BaseGame.Round.StartTime;
            saved.TotalTime = DateTime.Now - m_BaseGame.Round.StartTime;
            saved.WinType = m_BaseGame.Round.WinType;
            // Save it
            m_BaseGame.AllRounds.Add(saved);
        }

        private void round_Restart(ChatEvent e)
        {
            if (!player_isMod(e, ModLevels.Mod)) return;

            m_BaseGame.Status = BaseGameStatus.GameIntermission;
            if (m_Timer.Enabled) m_Timer.Stop();
            round_ResetPLayers();
            timer_Setup(TimerType.GameStart);
            sendBotSpam("- Round has been reset! - " + e.PlayerName);
            sendBotSpam("- Starting same round in " + m_StartGameDelay + " seconds! -");
        }

        private void round_ResetPLayers()
        {
            // Update players
            for (int i = m_BaseGame.Round.AlphaTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_BaseGame.Round.AlphaTeam.TeamMembers[i].Active)
                    m_BaseGame.Round.AlphaTeam.TeamMembers.Remove(m_BaseGame.Round.AlphaTeam.TeamMembers[i]);
                else
                {
                    string name = m_BaseGame.Round.AlphaTeam.TeamMembers[i].PlayerName;
                    m_BaseGame.Round.AlphaTeam.TeamMembers[i] = new BasePlayer(name);
                }
            }
            for (int i = m_BaseGame.Round.BravoTeam.TeamMembers.Count; i-- > 0; )
            {
                if (!m_BaseGame.Round.BravoTeam.TeamMembers[i].Active)
                    m_BaseGame.Round.BravoTeam.TeamMembers.Remove(m_BaseGame.Round.BravoTeam.TeamMembers[i]);
                else
                {
                    string name = m_BaseGame.Round.BravoTeam.TeamMembers[i].PlayerName;
                    m_BaseGame.Round.BravoTeam.TeamMembers[i] = new BasePlayer(name);
                }
            }
        }

        private void players_MoveToTeams()
        {
            // Move players from pub to private freqs and add them to teams
            List<SSPlayer> aTeam = m_Players.PlayerList.FindAll(item => item.Frequency == m_BaseGame.AlphaStartFreq && !m_BlockedList.Contains(item.PlayerName));
            while (aTeam.Count > 0)
            {
                // Auto assign TeamName to the first player joined
                if (m_BaseGame.Round.AlphaTeam.TeamMembers.Count == 0)
                    m_BaseGame.Round.AlphaTeam.TeamName = (aTeam[0].SquadName == "~ None Assigned ~" ? "Alpha" : aTeam[0].SquadName);

                m_BaseGame.Round.AlphaTeam.TeamMembers.Add(new BasePlayer(aTeam[0].PlayerName));
                psyGame.Send(msg.pm(aTeam[0].PlayerName, "?|setfreq " + m_AlphaFreq + "|shipreset"));
                aTeam.RemoveAt(0);
            }
            List<SSPlayer> bTeam = m_Players.PlayerList.FindAll(item => item.Frequency == m_BaseGame.BravoStartFreq && !m_BlockedList.Contains(item.PlayerName));
            while (bTeam.Count > 0)
            {
                // Auto assign TeamName to the first player joined
                if (m_BaseGame.Round.BravoTeam.TeamMembers.Count == 0)
                    m_BaseGame.Round.BravoTeam.TeamName = (bTeam[0].SquadName == "~ None Assigned ~" ? "Bravo" : bTeam[0].SquadName);

                m_BaseGame.Round.BravoTeam.TeamMembers.Add(new BasePlayer(bTeam[0].PlayerName));
                psyGame.Send(msg.pm(bTeam[0].PlayerName, "?|setfreq " + m_BravoFreq + "|shipreset"));
                bTeam.RemoveAt(0);
            }

            if (m_BaseGame.Round.AlphaTeam.TeamName == m_BaseGame.Round.BravoTeam.TeamName)
            {
                m_BaseGame.Round.AlphaTeam.TeamName = "Alpha";
                m_BaseGame.Round.BravoTeam.TeamName = "Bravo";
            }
        }

        private void player_Remove(SSPlayer p)
        {
            // If a game is in progress we want to set the player to inactive - this way we can save stats after player leaves
            if (m_BaseGame.Status != BaseGameStatus.GameIdle)
            {
                bool InAlpha;
                BasePlayer b = getPlayer(p, out InAlpha);

                if (b == null) return;

                b.Active = true;
            }
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
        public void Event_ShipChange(SSPlayer ssp){}
        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host) 
        {
            // Game is not turned on
            if (m_BaseGame == null) return;

            if (m_BaseGame.Status != BaseGameStatus.GameIdle)
            {
                // Grab player info
                bool h_InAlpha, a_InAlpha;
                // Host
                BasePlayer bHost = getPlayer(host, out h_InAlpha);
                // Attacher
                BasePlayer bAttach = getPlayer(host, out a_InAlpha);

                // do nothing they are not in game - wtf these better not mismatch
                if (bHost == null || bAttach == null) return;

                // Track host to see what region he is in
                if (player_InRegion(host.Position, m_Lobby.BaseDimension))
                {
                    bHost.InLobby = true;
                    bAttach.InLobby = true;
                    psyGame.SafeSend(msg.debugChan("Player[ " + attacher.PlayerName + " ] just attached to [ "+host.PlayerName+" ] - In lobby area. "));
                    return;
                }
                // I will not perform another check inside base dimension
                // assumption is if not in center, you are in base ( if player is somewhere else it can break bot)

                bHost.InLobby = false;
                bAttach.InLobby = false;
                psyGame.SafeSend(msg.debugChan("Player[ " + attacher.PlayerName + " ] just attached to [ " + host.PlayerName + " ] - In Base area. "));
            }
        }

        public void Event_PlayerTurretDetach(SSPlayer ssp) { }
        public void Event_PlayerEntered(SSPlayer ssp) {
            
            if (ssp.Frequency == m_AlphaFreq || ssp.Frequency == m_BravoFreq)
            {
                if (m_Players.PlayerList.FindAll(item => item.Frequency == 0).Count <= m_Players.PlayerList.FindAll(item => item.Frequency == 1).Count)
                    psyGame.Send(msg.pm(ssp.PlayerName, "?setfreq 0"));
                else
                    psyGame.Send(msg.pm(ssp.PlayerName, "?setfreq 1"));
            }
        }

        public void Event_PlayerPosition(SSPlayer ssp)
        {
            // Game is not turned on
            if (m_BaseGame == null) return;
            if (m_BlockedList.Contains(ssp.PlayerName)) return;

            if (m_BaseGame.Status == BaseGameStatus.GameOn)
            {
                // Grab player info
                bool InAlpha;
                BasePlayer b = getPlayer(ssp, out InAlpha);

                // do nothing they are not in game
                if (b == null)
                {
                    if ((ssp.Frequency == m_BaseGame.AlphaStartFreq || ssp.Frequency == m_BaseGame.BravoStartFreq) && m_BaseGame.AllowAfterStartJoin)
                    {
                        int aCount = m_BaseGame.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).Count;
                        int bCount = m_BaseGame.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).Count;

                        if (aCount <= bCount)
                        {
                            m_BaseGame.Round.AlphaTeam.TeamMembers.Add(new BasePlayer(ssp.PlayerName));
                            psyGame.Send(msg.pm(ssp.PlayerName, "?|setfreq " + m_AlphaFreq + "|shipreset"));
                        }
                        else
                        {
                            m_BaseGame.Round.BravoTeam.TeamMembers.Add(new BasePlayer(ssp.PlayerName));
                            psyGame.Send(msg.pm(ssp.PlayerName, "?|setfreq " + m_BravoFreq + "|shipreset"));
                        }
                    }
                    return;
                }

                // Player is out of team freq - put him back
                if (ssp.Frequency == m_BaseGame.AlphaStartFreq || ssp.Frequency == m_BaseGame.BravoStartFreq)
                {
                    psyGame.Send(msg.pm(ssp.PlayerName, "?|setfreq " + (InAlpha?m_AlphaFreq:m_BravoFreq) + "|shipreset"));
                    if (!b.Active) b.Active = true;
                    return;
                }

                // Only do collision on IsSafe - cuts down on checks
                if (ssp.Position.ShipState.IsSafe)
                {
                    // Player is in lobby
                    if (player_InRegion(ssp.Position,m_Lobby.BaseDimension))
                    {
                        b.InLobby = true;

                        // delay the BaseClear timer for start - it picks up everyone as out in the first second of play
                        if ((DateTime.Now - m_BaseGame.Round.StartTime).TotalMilliseconds < 1500) return;

                        // Check for BaseClear
                        bool aOut, bOut;
                        if (!m_Timer.Enabled && teamIsOut(out aOut, out bOut))
                        {
                            timer_Setup(TimerType.BaseClear);
                        }
                        // nothing else to check exit out
                        return;
                    }
                    // I will not perform another check inside base dimension
                    // assumption is if not in center, you are in base ( if player is somewhere else it can break bot)

                    // Change status if not updated
                    if (b.InLobby)
                    {
                        b.InLobby = false;
                        psyGame.SafeSend(msg.debugChan("Player[ " + ssp.PlayerName + " ]'s status has changed to InBase."));
                    }

                    // Check player against opposing team safe
                    if (m_BaseGame.AllowSafeWin && player_InRegion(ssp.Position,InAlpha?m_Base.BravoSafe:m_Base.AlphaSafe))
                    {
                        round_Finished(WinType.SafeWin, InAlpha, ssp.PlayerName);
                    }
                }
            }
        }

        public void Event_PlayerLeft(SSPlayer ssp)
        {
            // No need to deal with event if it is off
            if (m_BaseGame == null) return;
            player_Remove(ssp);
        }

        public void Event_PlayerFreqChange(SSPlayer ssp)
        {
            // No need to deal with event if it is off
            if (m_BaseGame == null) return;

            if (ssp.OldFrequency == m_AlphaFreq || m_BravoFreq == ssp.OldFrequency)
                player_Remove(ssp);
        }

        //----------------------------------------------------------------------//
        //                       Timer Stuff                                    //
        //----------------------------------------------------------------------//
        private void timer_Setup(TimerType t)
        {
            m_Timer = new Timer();

            // Set the timer for what we need it to do: Start or Clear Check
            if (t == TimerType.GameStart)
            {
                m_StartGameDelay_Elapsed = m_StartGameDelay;
                m_Timer.Elapsed += new ElapsedEventHandler(timer_StartGame);
                m_Timer.Interval = 1000;
            }
            else if (t == TimerType.BaseClear)
            {
                psyGame.Send(msg.debugChan("Starting Clear Timer"));
                m_Timer.Elapsed += new ElapsedEventHandler(timer_BaseClear);
                m_Timer.Interval = m_TeamClearDelay * 1000;
            }
            m_Timer.Start();
        }

        private void timer_StartGame(object source, ElapsedEventArgs e)
        {
            m_StartGameDelay_Elapsed--;

            if (m_StartGameDelay_Elapsed <= m_StartGameShow /*&& m_StartGameDelay_Elapsed != 0*/)
            {
                //myGame.Send(msg.macro("- " + m_StartGameDelay_Elapsed + " -", SoundCodes.MessageAlarm));
                psyGame.Send(msg.pub("*objon " + m_StartGameDelay_Elapsed));
                if (m_StartGameDelay_Elapsed != 0)
                    psyGame.Send(msg.arena("", SoundCodes.MessageAlarm));
                else
                    psyGame.Send(msg.arena("", SoundCodes.Goal));
            }

            if (m_StartGameDelay_Elapsed > 0) return;

            m_Timer.Stop();

            if (allow_GameStart(false)) game_Start();
            else sendBotSpam("- Not enough players. Start Game cancelled. -");
            //myGame.Send(msg.macro("- Not enough players. Start Game cancelled. -")); 

        }
        private void timer_BaseClear(object source, ElapsedEventArgs e)
        {
            psyGame.Send(msg.debugChan("- All out timer expired. -"));
            m_Timer.Stop();

            // make sure a team is still out
            bool aOut, bOut;

            if (teamIsOut(out aOut, out bOut))
            {
                round_Finished(aOut == bOut?WinType.NoCount:WinType.BaseClear, !aOut);
            }
        }

        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        private void sendBotSpam(string Message)
        {
            ChatEvent spam = new ChatEvent();
            spam.Message = Message;

            if (m_BotSpamSetting != ChatTypes.Team)
            {
                spam.ChatType = m_BotSpamSetting;
                psyGame.Send(spam);
                return;
            }

            psyGame.Send(msg.team_pm(m_BaseGame.AlphaStartFreq, Message));
            psyGame.Send(msg.team_pm(m_BaseGame.BravoStartFreq, Message));
            psyGame.Send(msg.team_pm(m_AlphaFreq, Message));
            psyGame.Send(msg.team_pm(m_BravoFreq, Message));
        }

        private bool teamIsOut(out bool alpha, out bool bravo)
        {
            // make sure a team is still out
            alpha = !(m_BaseGame.Round.AlphaTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            bravo = !(m_BaseGame.Round.BravoTeam.TeamMembers.FindAll(item => item.Active).FindAll(item => !item.InLobby).Count >= 1);
            return alpha || bravo;
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
                    FullMessage = FullMessage.Remove(0,1);

                    formattedCommand = FullMessage.Trim().ToLower();
                    return true;
                }
            }
            return false;
        }

        // Grab player from one of the teams
        private BasePlayer getPlayer( SSPlayer p, out bool InAlpha)
        {
            BasePlayer b = m_BaseGame.Round.AlphaTeam.TeamMembers.Find( item => item.PlayerName == p.PlayerName);

            if ( b != null)
            {
                InAlpha = true;
                return b;
            }

            b = m_BaseGame.Round.BravoTeam.TeamMembers.Find(item => item.PlayerName == p.PlayerName);
            InAlpha = false;
            return b;
        }

        // Simple collision check
        private bool player_InRegion(PlayerPositionEvent p, ushort[] region)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        // checking if player is mod - if not sends back message
        private bool player_isMod(ChatEvent e, ModLevels mod)
        {
            if (e.ModLevel >= mod)
                return true;

            psyGame.Send(msg.pm(e.PlayerName, "You do not have access to this command. This is a staff command. Required Moerator level: [ " + mod + " ]."));
            return false;
        }

        // Checks to see if module is loaded - returns message if not
        private bool moduleIsOn( ChatEvent e)
        {
            if (m_BaseGame != null) return true;

            psyGame.Send(msg.pm(e.PlayerName, "Module is currently not loaded."));
            return false;
        }

        // Player Count check to make sure we can start game
        private bool allow_GameStart(bool InPub)
        {
            int aFreq = InPub ? m_BaseGame.AlphaStartFreq : m_AlphaFreq;
            int bFreq = InPub ? m_BaseGame.BravoStartFreq : m_BravoFreq;

            int aCount = m_Players.PlayerList.FindAll(item => item.Frequency == aFreq && !m_BlockedList.Contains(item.PlayerName)).Count;
            int bCount = m_Players.PlayerList.FindAll(item => item.Frequency == bFreq && !m_BlockedList.Contains(item.PlayerName)).Count;

            return aCount > 0 && bCount > 0;
        }

        //----------------------------------------------------------------------//
        //                           PrintOuts                                  //
        //----------------------------------------------------------------------//
        private void show_GameSettings(string PlayerName)
        {
            // Show player how it was loaded
            psyGame.Send(msg.pm(PlayerName, "BaseDuel Settings ---------------------------"));
            psyGame.Send(msg.pm(PlayerName, "Status           :".PadRight(20) + m_BaseGame.Status.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "BotSpam Chat Type:".PadRight(20) + m_BotSpamSetting.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Join after Start :".PadRight(20) + m_BaseGame.AllowAfterStartJoin.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Safe win enabled :".PadRight(20) + m_BaseGame.AllowSafeWin.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Minimum Wins     :".PadRight(20) + m_BaseGame.MinimumWin.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Win By           :".PadRight(20) + m_BaseGame.WinBy.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "------------------------"));
            psyGame.Send(msg.pm(PlayerName, "Alpha Team Name  :".PadRight(20) + m_BaseGame.Round.AlphaTeam.TeamName.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_BaseGame.Round.AlphaTeam.TeamMembers.Count.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_BaseGame.AlphaFreq.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "------------------------"));
            psyGame.Send(msg.pm(PlayerName, "Bravo Team Name  :".PadRight(20) + m_BaseGame.Round.BravoTeam.TeamName.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Player Count     :".PadRight(20) + m_BaseGame.Round.BravoTeam.TeamMembers.Count.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Frequency        :".PadRight(20) + m_BaseGame.BravoFreq.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Base Loader Settings -----------------"));
            psyGame.Send(msg.pm(PlayerName, "Current Base     :".PadRight(20) + (m_BaseManager.Bases.IndexOf(m_BaseManager.CurrentBase) + 1).ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Total Bases      :".PadRight(20) + m_BaseManager.Bases.Count.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Sorting Mode     :".PadRight(20) + m_BaseManager.Mode.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "Size Restrictions:".PadRight(20) + m_BaseManager.SizeLimit.ToString().PadLeft(25)));
            psyGame.Send(msg.pm(PlayerName, "---------------------------------------------"));
        }
        private void show_Rounds(string PlayerName)
        {
            // Spit out round info from round list
            psyGame.Send(msg.pm(PlayerName, "All Rounds in Game --------------------------"));
            for (int i = 0; i < m_BaseGame.AllRounds.Count; i++)
            {
                psyGame.Send(msg.pm(PlayerName, "Round Number  :".PadRight(20) + (i + 1).ToString().PadLeft(25)));
                psyGame.Send(msg.pm(PlayerName, "Date          :".PadRight(20) + m_BaseGame.AllRounds[i].StartTime.ToShortDateString().PadLeft(25)));
                psyGame.Send(msg.pm(PlayerName, "Time          :".PadRight(20) + m_BaseGame.AllRounds[i].StartTime.ToShortTimeString().PadLeft(25)));
                psyGame.Send(msg.pm(PlayerName, "Duration      :".PadRight(20) + m_BaseGame.AllRounds[i].TotalTimeFormatted.ToString().PadLeft(25)));
                psyGame.Send(msg.pm(PlayerName, "Win Type      :".PadRight(20) + m_BaseGame.AllRounds[i].WinType.ToString().PadLeft(25)));
                if (m_BaseGame.AllRounds[i].WinType != WinType.NoCount)
                    psyGame.Send(msg.pm(PlayerName, "Winner        :".PadRight(20) + (m_BaseGame.AllRounds[i].AlphaWon ? m_BaseGame.Round.AlphaTeam.TeamName : m_BaseGame.Round.BravoTeam.TeamName).ToString().PadLeft(25)));
                psyGame.Send(msg.pm(PlayerName, "Base Number   :".PadRight(20) + m_BaseGame.AllRounds[i].BaseNumber.ToString().PadLeft(25)));
                psyGame.Send(msg.pm(PlayerName, "---------------------------------------------"));
            }
        }

        private void show_StartSpam()
        {
            psyGame.Send(msg.arena("._______       __        ________  _______  ________   ____  ____   _______  ___   BaseDuel Bot"));
            psyGame.Send(msg.arena("|   _  \"\\     /\"\"\\      /\"       )/\"     \"||\"      \"\\ (\"  _||_ \" | /\"     \"||\"  |  Written by: PsyOps"));
            psyGame.Send(msg.arena("(. |_)  :)   /    \\    (:   \\___/(: ______)(.  ___  :)|   (  ) : |(: ______)||  |          __"));
            psyGame.Send(msg.arena("|:     \\/   /' /\\  \\    \\___  \\   \\/    |  |: \\   ) ||(:  |  | . ) \\/    |  |:  |          | \\_"));
            psyGame.Send(msg.arena("(|  _  \\\\  //  __'  \\    __/  \\\\  // ___)_ (| (___\\ || \\\\ \\__/ //  // ___)_  \\  |___     =[_|  \\---.__ "));
            psyGame.Send(msg.arena("|: |_)  :)/   /  \\\\  \\  /\" \\   :)(:      \"||:       :) /\\\\ __ //\\ (:      \"|( \\_|:  \\    =[+  /-------'"));
            psyGame.Send(msg.arena("(_______/(___/    \\___)(_______/  \\_______)(________/ (__________) \\_______) \\_______)     |_/"));
        }
    }
}
/* add bd hold
 * 
 * 
*/