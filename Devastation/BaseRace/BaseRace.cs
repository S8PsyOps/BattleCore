using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;
// Needed for timers
using System.Timers;

namespace Devastation.BaseRace
{
    public class BaseRace
    {
        public BaseRace(SSPlayerManager PlayerManager,BaseManager BaseManager, ShortChat msg, MyGame myGame)
        {
            this.m_Players = PlayerManager;
            this.msg = msg;
            this.m_BaseManager = BaseManager;
            this.psyGame = myGame;
            this.m_BaseRaceFreq = 1337;
            this.m_BlockedList = new List<string>();

            this.loadNextBase();

            m_Timer = new Timer();
            m_Timer.Elapsed += new ElapsedEventHandler(MyTimer);
            m_Timer.Interval = 1000;
        }

        private SSPlayerManager m_Players;
        private ShortChat msg;
        private BaseManager m_BaseManager;
        private MyGame psyGame;
        private ushort m_BaseRaceFreq;
        private List<string> m_BlockedList;

        Timer m_Timer;

        int second = -1;
        RaceData m_RaceData = new RaceData();
        public Boolean game_status = false;
        public int freq_race = 1337;
        public int ship = 1;
        public int rank = 0;
        public int[] center_x = new int[2] { 448, 448 };
        public int[] center_y = new int[2] { 575, 575 };
        public Boolean WarpAtStart = false;
        public int ms, sec, min = 0;
        public DateTime m_TimeStart = new DateTime();
        public DateTime m_TimeGame = new DateTime();
        public Boolean BaseRace_toggle = true;

        private Base m_Base;

        //----------------------------------------------------------------------//
        //                     Game Functions                                   //
        //----------------------------------------------------------------------//
        public void GameOver(Boolean msgz)
        {
            this.loadNextBase();
            if (msgz == true)
            {
                psyGame.Send(msg.arena("Game Over", SoundCodes.Hallellula));
                psyGame.Send(msg.arena("Type !baserace to join again!"));
                //   }
            }

            if (m_RaceData.isThereReach() == true)
            {
                psyGame.Send(msg.arena("+------+--------------+-----------------+"));
                psyGame.Send(msg.arena("| Rank | Player       |     Time        |"));
                psyGame.Send(msg.arena("+------+--------------+-----------------+"));
                // getTopPlayers();
                for (int i = 0; i < 11; ++i)
                {
                    if (i >= 1 && i <= 10)
                    {
                        if (m_RaceData.top10[i] != "")
                        {
                            RaceData.Race racer = m_RaceData.getRacer(m_RaceData.top10[i]);
                            string rankz = "" + racer.rank;
                            string name = racer.name;
                            string time = racer.ctime;
                            psyGame.Send(msg.arena(String.Format("| {0,-4} | {1,-12} | {2,15} |", rankz, name, time)));
                            //Game(msg.arena("| " + rankz.PadRight(4) + " | " + name.PadRight((getStringLen("+--------------+")-(name.Length+1))) + "|" + time.PadRight(getStringLen("+------+--------------+-----------------")) + "|"));
                        }
                    }
                }
                psyGame.Send(msg.arena("+------+--------------+-----------------+"));

            }
            game_status = false;
            if (m_Timer.Enabled)
            {
                m_Timer.Stop();
            }
            second = -1;
            m_RaceData.top10 = new string[11] { "", "", "", "", "", "", "", "", "", "", "" };
            rank = 0;
            m_RaceData.RaceItems.Clear();
        }

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        /// <summary>
        /// <para>All commands used for baserace send through here.</para>
        /// <para>Syntax: !baserace command    Example !baserace settings  or  .baserace start</para>
        /// <para>If you want to make compatible commands, register them in devastation main</para>
        /// <para>and change the command to make it compatible with !baseduel command</para>
        /// <para>then send it here. BaseRace.Commands(e);</para>
        /// </summary>
        /// <param name="p">Deva Player</param>
        /// <param name="e">Command sent</param>
        /// <returns></returns>
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
                    case ".baserace":
                        e.Message = "!help baserace";
                        psyGame.CoreSend(e);
                        return;

                    case "start":
                        cmd_RaceFreq(e);
                        return;

                    case "commands":
                        e.Message = "!help baserace commands";
                        psyGame.CoreSend(e);
                        return;
                    
                    case "info":
                        psyGame.Send(msg.arena("show info message"));
                        return;

                    case "left":
                        cmd_left(e);
                        return;

                    case "toggle":
                        cmd_toggle(e);
                        return;
                }
            }
        }

        public void cmd_timeleft(ChatEvent e)
        {
            if (game_status == true)
            {
                TimeSpan m_TimeGameTimeSpan = DateTime.Now - m_TimeGame;
                psyGame.Send(msg.pm(e.PlayerName, "timeleft: " + m_TimeGameTimeSpan.Minutes.ToString().PadLeft(2, '0') + ":" + m_TimeGameTimeSpan.Seconds.ToString().PadLeft(2, '0') + "." + m_TimeGameTimeSpan.Milliseconds));
            }
        }


        public void cmd_toggle(ChatEvent e)
        {
            string[] data = e.Message.Split(' ');
            if (e.ModLevel >= ModLevels.Mod)
            {
                if (data[1] == "baserace")
                {
                    if (BaseRace_toggle == false)
                    {
                        psyGame.Send(msg.pm(e.PlayerName, "Toggle Baserace: ON"));
                        BaseRace_toggle = true;
                    }
                    else
                    {
                        if (game_status == true)
                        {
                            foreach (RaceData.Race racer in m_RaceData.RaceItems)
                            {
                                psyGame.Send(msg.pm(racer.name, "*specall"));
                            }
                            GameOver(false);
                            psyGame.Send(msg.arena("Mod " + e.PlayerName + " toggle off baserace!"));

                        }
                        else
                        {
                            if (second == -1)
                            {
                                GameOver(false);
                            }
                            else
                            {
                                foreach (RaceData.Race racer in m_RaceData.RaceItems)
                                {
                                    psyGame.Send(msg.pm(racer.name, "*specall"));
                                }
                                psyGame.Send(msg.arena("Mod " + e.PlayerName + " toggle off baserace!"));
                                GameOver(false);
                            }

                        }
                        psyGame.Send(msg.pm(e.PlayerName, "Toggle Baserace: OFF"));

                        BaseRace_toggle = false;
                    }
                }
            }
        }

        public void cmd_RaceFreq(ChatEvent e)
        {
            if (BaseRace_toggle == true)
            {
                if (game_status == false)
                {
                    if (m_RaceData.getRacer(e.PlayerName).name.Equals(".null"))
                    {
                        psyGame.Send(msg.pm(e.PlayerName, "*setfreq " + freq_race));
                        psyGame.Send(msg.pm(e.PlayerName, "*setship " + ship));
                        if (m_RaceData.getTotalPlayers() == 0)
                        {
                            if (game_status == false)
                            {
                                if (second == -1)
                                {
                                    if (!m_Timer.Enabled)
                                    { m_Timer.Start(); }
                                    second = 20;
                                }
                            }
                            m_RaceData.Start_x = m_Base.AlphaStartX;
                            m_RaceData.Start_y = m_Base.AlphaStartY;
                            psyGame.Send(msg.macro("type !baserace to join race!! will start in 20 seconds!!"));
                        }

                        m_RaceData.CreateNewRacer(e.PlayerName);
                    }
                }
            }
            else
            {
                psyGame.Send(msg.pm(e.PlayerName, "Baserace currently off!"));
            }

        }

        public void cmd_left(ChatEvent e)
        {
            psyGame.Send(msg.pm(e.PlayerName, "Players left: " + getTotalRacers()));
        }

        //----------------------------------------------------------------------//
        //                         Timer                                        //
        //----------------------------------------------------------------------//
        public void MyTimer(object source, ElapsedEventArgs a)
        {
            if (second <= 20 && second >= 0)
            {
                if (second == 10)
                {
                    psyGame.Send(msg.macro("type !baserace to join race!! 10 seconds left"));
                    /*foreach (AhmadClass.RaceData.Race e in m_RaceData.RaceItems)
                    {
 
                        Game(msg.warp(e.name, m_RaceData.Start_x, m_RaceData.Start_y));
                        Game(msg.pm(e.name, "*prize shutdown"));
                    }*/
                }
                if (second <= 10 && second >= 0)
                {
                    foreach (RaceData.Race e in m_RaceData.RaceItems)
                    {
                        psyGame.Send(msg.pm(e.name, "*objon " + second));
                    }
                }
                if (second == 0)
                {
                    foreach (RaceData.Race e in m_RaceData.RaceItems)
                    {
                        if (WarpAtStart == true)
                        {
                            psyGame.Send(msg.warp(e.name, m_RaceData.Start_x, m_RaceData.Start_y));
                        }
                        psyGame.Send(msg.pm(e.name, "*shipreset"));
                        psyGame.Send(msg.pm(e.name, "Go Go Go Go Go!", SoundCodes.Goal));
                        psyGame.Send(msg.pm(e.name, "*objon 0"));
                    }
                    game_status = true;
                    m_TimeStart = DateTime.Now;
                    m_TimeGame = DateTime.Now;
                }

                --second;
            }

        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void Event_PlayerPosition(SSPlayer ssp)
        {
            int tilex = ssp.Position.MapPositionX / 16;
            int tiley = ssp.Position.MapPositionY / 16;
            int pixelx = ssp.Position.MapPositionX;
            int pixely = ssp.Position.MapPositionY;/// 16;
            //Game(msg.macro("X: " + tilex + " Y:" + tiley));
            if (game_status == false)
            {
                if (second <= 20 && second >= 0)
                {
                    if (!m_RaceData.getRacer(ssp.PlayerName).name.Equals(".null"))
                    {
                        if (InRegion(ssp.Position, m_BaseManager.Lobby.BaseDimension))
                        {
                            psyGame.Send(msg.warp(ssp.PlayerName, m_RaceData.Start_x, m_RaceData.Start_y));
                            psyGame.Send(msg.pm(ssp.PlayerName, "*prize -engineshutdown"));
                            //   Game(msg.pm(e.PlayerName, "*setship " + ship));
                        }
                        if (!InRegion(ssp.Position, m_Base.AlphaSafe))
                        {
                            psyGame.Send(msg.warp(ssp.PlayerName, m_RaceData.Start_x, m_RaceData.Start_y));
                            psyGame.Send(msg.pm(ssp.PlayerName, "*prize -engineshutdown"));
                            //   Game(msg.pm(e.PlayerName, "*setship " + ship));
                        }
                        if (!ssp.Position.ShipState.IsSafe)
                        {
                            psyGame.Send(msg.warp(ssp.PlayerName, m_RaceData.Start_x, m_RaceData.Start_y));
                            psyGame.Send(msg.pm(ssp.PlayerName, "*prize -engineshutdown"));
                        }
                    }

                }
            }
            else
            {

                RaceData.Race v = m_RaceData.getRacer(ssp.PlayerName);
                if (!v.name.Equals(".null"))
                {
                    if (InRegion(ssp.Position, m_BaseManager.Lobby.BaseDimension))
                    {
                        if (v.reason == "" && v.reach == false)
                        {
                            v.reason = "BackToCenter";
                        }
                        //Game(msg.warp(e.PlayerName, m_RaceData.Start_x, m_RaceData.Start_y));
                        //   Game(msg.pm(e.PlayerName, "*setship " + ship));
                    }

                    //  AhmadClass.RaceData.Race e = v;//foreach list only readable cann't write i create this line to set value for ctime
                    m_RaceData.finish_x[0] = m_Base.BravoSafe[0];
                    m_RaceData.finish_y[0] = m_Base.BravoSafe[1];
                    m_RaceData.finish_x[1] = m_Base.BravoSafe[2];
                    m_RaceData.finish_y[1] = m_Base.BravoSafe[3];
                    if (m_RaceData.isInEnd(pixelx, pixely))
                    {
                        if (v.reason == "" && v.reach == false)
                        {
                            TimeSpan m_MatchTotalTime = DateTime.Now - m_TimeStart;
                            //m_TimeStart
                            v.ctime = m_MatchTotalTime.Minutes.ToString().PadLeft(2, '0') + ":" + m_MatchTotalTime.Seconds.ToString().PadLeft(2, '0') + "." + m_MatchTotalTime.Milliseconds;
                            v.reach = true;
                            string mm_rank = "";
                            ++rank;
                            m_RaceData.getRacer(ssp.PlayerName).rank = rank;
                            if (rank == 1)
                            {
                                mm_rank = "1st";
                            }
                            else if (rank == 2)
                            {
                                mm_rank = "2nd";
                            }
                            else if (rank == 3)
                            {
                                mm_rank = "3rd";
                            }
                            else if (rank >= 4)
                            {
                                mm_rank = rank + "th";
                            }
                            if (rank >= 1 && rank <= 10)
                            {
                                m_RaceData.top10[rank] = ssp.PlayerName;
                            }
                            psyGame.Send(msg.arena(mm_rank + "- " + ssp.PlayerName + " Time:" + v.ctime, SoundCodes.VictoryBell));
                        }
                    }
                }
                if (getTotalRacers() == 0)
                {
                    GameOver(true);
                }
                TimeSpan m_TimeGameTimeSpan = DateTime.Now - m_TimeGame;
                if (m_TimeGameTimeSpan.Minutes >= 5)
                {
                    foreach (RaceData.Race racer in m_RaceData.RaceItems)
                    {
                        psyGame.Send(msg.pm(racer.name, "*specall"));
                    }
                    GameOver(true);
                    psyGame.Send(msg.arena("Game has been timed out. Baserace Reset"));
                }
            }
            //  Game(msg.pub("tick"));
        }

        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host)
        {
            if (game_status == true)
            {
                RaceData.Race racer = m_RaceData.getRacer(attacher.PlayerName);
                if (!racer.Equals(".null"))
                {
                    if (racer.reason == "" && racer.reach == false)
                    {
                        racer.reason = "Attaching to another player";
                        psyGame.Send(msg.pm(racer.name, "*specall"));
                        psyGame.Send(msg.pm(racer.name, "Removed you from race! Reason: " + racer.reason));
                    }
                }
            }
            else
            {
                if (second >= 0 && second <= 20)
                {
                    RaceData.Race racer = m_RaceData.getRacer(attacher.PlayerName);
                    if (!racer.Equals(".null"))
                    {
                        psyGame.Send(msg.pm(racer.name, "*setship 8"));
                        psyGame.Send(msg.pm(racer.name, "please don't attach to another player!"));
                    }
                }
            }
        }
        public void Event_PlayerTurretDetach(SSPlayer ssp) { }
        public void Event_PlayerEntered(SSPlayer ssp){ }
        public void Event_PlayerLeft(SSPlayer ssp)
        {
            RaceData.Race racer = m_RaceData.getRacer(ssp.PlayerName);
            if (!racer.name.Equals(".null"))
            {
                racer.reason = "left arena";
            }
        }
        public void Event_PlayerFreqChange(SSPlayer ssp){   }
        public void Event_ShipChange(SSPlayer ssp)
        {
            //SSPlayer myPlayer = this.m_Players.PlayerList.Find(item => item.PlayerName == "PsyOps");
            //SSPlayer myPlayer2 = this.m_Players.PlayerList.Find(item => item.Frequency == 1234);

            //int freq0count = this.m_Players.PlayerList.FindAll(item => item.Frequency == 0 && item.Ship != ShipTypes.Spectator).Count;

            //foreach (SSPlayer player in this.m_Players.PlayerList){ }

            if (game_status == false)
            {
                if (second <= 20 && second >= 0)
                {
                    RaceData.Race racer = m_RaceData.getRacer(ssp.PlayerName);// AhmadClass.RaceData.Race racer = m_RaceData.getRacer(e.PlayerName);
                    if (!racer.name.Equals(".null"))
                    {
                        if (ssp.Ship != ShipTypes.Warbird)
                        {

                            if (ssp.Ship != ShipTypes.Spectator)
                            {

                                psyGame.Send(msg.warp(ssp.PlayerName, m_RaceData.Start_x, m_RaceData.Start_y));
                                psyGame.Send(msg.pm(ssp.PlayerName, "*setship " + ship));
                                psyGame.Send(msg.pm(ssp.PlayerName, "*prize -engineshutdown"));
                            }
                            else if (ssp.Ship == ShipTypes.Spectator)
                            {
                                m_RaceData.RemoveRacer(racer);
                            }
                        }
                    }
                }

            }
            else
            {
                if (ssp.Ship != ShipTypes.Warbird)
                {
                    RaceData.Race v = m_RaceData.getRacer(ssp.PlayerName);
                    if (!v.name.Equals(".null"))
                    {
                        if (v.reason == "" && v.reach == false)
                        {
                            v.reason = "ShipChange";
                        }
                    }
                }
            }
        }


        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        public int getTotalRacers()
        {
            int count = 0;
            foreach (RaceData.Race v in m_RaceData.RaceItems)
            {
                if (v.reach == false && v.reason == "")
                {
                    ++count;
                }
            }
            return count;
        }
        private bool InRegion(PlayerPositionEvent p, ushort[] region)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] && x <= region[2] && y >= region[1] && y <= region[3]);
        }

        public Boolean isInCenter(int x, int y)
        {
            return (m_BaseManager.Lobby.BaseDimension[0] <= x &&
                m_BaseManager.Lobby.BaseDimension[1] <= y && 
                m_BaseManager.Lobby.BaseDimension[2] <= x && 
                m_BaseManager.Lobby.BaseDimension[3] <= y); // (center_x[0] <= x && center_y[0] <= y && center_x[1] >= x && center_y[1] >= y);
        }

        public int getStringLen(string ms)
        { return ms.Length - 1; }

        private void loadNextBase()
        {
            m_BaseManager.ReleaseBase(m_Base,"BaseRace");
            m_Base = m_BaseManager.getNextBase("BaseRace");
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
                    if (FullMessage.StartsWith("!br") || FullMessage.StartsWith(".br") || FullMessage.StartsWith("!baserace") || FullMessage.StartsWith(".baserace"))
                    {
                        // If command isnt a multiple just send original ".baseduel"
                        if (FullMessage.Contains(" ")) formattedCommand = FullMessage.Split(' ')[1];

                        // Send back the attached command = .baseduel [command]
                        else formattedCommand = ".baserace";

                        return true;
                    }
                    FullMessage = FullMessage.Remove(0, 1).Trim().ToLower();

                    // Shorcut commands go here
                    switch (FullMessage)
                    {
                        // makes !startbr the same as !baserace start
                        case "brstart":
                            formattedCommand = "start";
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
