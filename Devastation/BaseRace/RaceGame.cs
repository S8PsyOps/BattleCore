using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

using System.Timers;

namespace Devastation.BaseRace
{
    class RaceGame
    {
        public RaceGame(ShortChat msg, MyGame psyGame, FileDataBase fakeDB, SSPlayerManager Players, BaseManager BaseManager, bool Multi, int GameNum)
        {
            this.msg = msg;
            this.psyGame = psyGame;
            this.m_FakeDB = fakeDB;
            this.m_MultiOn = Multi;
            this.m_GameNum = GameNum;
            this.m_Players = Players;
            this.m_BaseManager = BaseManager;
            this.m_Lobby = m_BaseManager.Lobby;
            this.m_Racers = new List<string>();
            this.m_Status = RaceState.NotStarted;
            this.m_Timer = new Timer();
            this.loadNextBase();
        }

        private ShortChat msg;
        private MyGame psyGame;
        private FileDataBase m_FakeDB;
        private SSPlayerManager m_Players;
        private BaseManager m_BaseManager;

        private List<BRPlayer> m_RacerRecords;
        private List<string> m_Racers;
        private int m_GameNum;
        private bool m_MultiOn;
        private Base m_Lobby;
        private Base m_CurrentBase;
        private DateTime m_StartTime;

        private Timer m_Timer;
        private int m_TimerCountdown;
        private ushort m_Freq;
        private BaseSize m_BaseSize;
        private RaceState m_Status;

        public Base loadedBase()
        { return this.m_CurrentBase; }

        public void raceFreq(ushort freq)
        { this.m_Freq = freq; }
        public ushort raceFreq()
        { return this.m_Freq; }

        //----------------------------------------------------------------------//
        //                       Commands                                       //
        //----------------------------------------------------------------------//
        public void command_PlayGame(SSPlayer p, ChatEvent e)
        {
            if (!this.allowedCommandUsage(p)) return;
            
            if (e.Message.Contains(' '))
            {
                string[] data = e.Message.Split(' ');
                int num;

                if (int.TryParse(data[1], out num) || int.TryParse(data[1], out num))
                {
                    if (!this.m_BaseManager.CheckBaseSafe(num))
                    {
                        psyGame.Send(msg.pm(p.PlayerName, "Base[ "+num+" ] is an invalid base. It is either too close to another base that is in use or the base number is too high. Please select a base from 1 to " + m_BaseManager.Bases.Count()  + "."));
                        return;
                    }

                    this.loadBase(num);
                    e.Message = ".race";
                    psyGame.CoreSend(e);
                }
                return;
            }

            if (!this.m_Timer.Enabled) timer_startRaceTimer();
            if (this.m_Status == RaceState.InProgress)
            {
                psyGame.Send(msg.pm(p.PlayerName, "?|setfreq " + m_Freq + (p.Ship == ShipTypes.Spectator ? "|setship 1" : "|prize warp") + "|prize fullcharge|a A race is currently in progress. Please wait here. Once next race starts you will be inlcuded and warped."));
                return;
            }

            psyGame.Send(msg.pm(p.PlayerName, "?|setfreq " + m_Freq + (p.Ship == ShipTypes.Spectator ? "|setship 1" : "|prize warp") + "|prize fullcharge|a Please select the ship you wish to race in. A race will begin shortly."));
        }

        //----------------------------------------------------------------------//
        //                             Event                                    //
        //----------------------------------------------------------------------//
        public void Event_PlayerPosition(SSPlayer ssp)
        {
            if (this.m_Status != RaceState.InProgress)
            {
                if (m_Timer.Enabled && this.m_TimerCountdown < 10 && this.player_InRegion(ssp.Position, this.m_Lobby.BaseDimension))
                {
                    string warpto = "|warpto " + this.m_CurrentBase.AlphaStartX + " " + this.m_CurrentBase.AlphaStartY;
                    string prepPlayer = "|prize fullcharge|prize -engineshutdown";

                    psyGame.Send(msg.pm(ssp.PlayerName, "?" + warpto + prepPlayer));
                }
                return;
            }

            if (this.player_InRegion(ssp.Position, this.m_Lobby.BaseDimension))
            {
                this.race_PlayerLeaving(ssp);
            }
            else if (this.player_InRegion(ssp.Position,this.m_CurrentBase.BravoSafe, 10) && this.m_Racers.Contains(ssp.PlayerName))
            {
                if (this.m_RacerRecords == null) this.m_RacerRecords = new List<BRPlayer>();

                // Store player info
                BRPlayer racer = new BRPlayer();
                racer.PlayerName = ssp.PlayerName;
                racer.Ship = ssp.Ship;
                racer.Time = DateTime.Now - this.m_StartTime;
                this.m_RacerRecords.Add(racer);

                psyGame.Send(msg.pm(ssp.PlayerName,"?|prize warp|a " + ("Congratulations you placed: "+this.player_getRank(racer)+"- Time " + this.getFormattedTime(racer.Time)).PadRight(57)));

                // Remove him from race list
                this.m_Racers.Remove(ssp.PlayerName);

                if (this.m_Racers.Count <= 0) race_EndRace();
            }
        }
        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host)
        {
            psyGame.Send(msg.pm(attacher.PlayerName, "?|setship 9|setship " + ((int)attacher.Ship + 1).ToString()));

            if (this.m_Status == RaceState.InProgress) this.m_Racers.Remove(attacher.PlayerName);
        }

        //----------------------------------------------------------------------//
        //                       Race Game Function                             //
        //----------------------------------------------------------------------//
        private void race_EndRace()
        {
            if (this.m_Timer.Enabled) this.m_Timer.Stop();

            if (this.m_RacerRecords != null && this.m_RacerRecords.Count > 0)
            {
                psyGame.Send(msg.team_pm(this.m_Freq, "?a Saving Race Scores . . . . ".PadRight(60)));
                this.m_FakeDB.BaseRace_SavePlayerTimes(this.m_RacerRecords, this.m_CurrentBase);
                psyGame.Send(msg.team_pm(this.m_Freq, "?a Scores Saved. ".PadRight(60)));

                if (this.m_RacerRecords.Count > 1)
                {
                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "|   BaseID [ " + this.m_CurrentBase.BaseID.ToString().PadLeft(2, '0') + " ]                                                     |"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                    for (int i = 0; i < this.m_RacerRecords.Count; i++)
                    {
                        psyGame.Send(msg.team_pm(this.m_Freq, "| " + (i + 1).ToString().PadLeft(2, '0') + ". " +
                            m_RacerRecords[i].PlayerName.PadRight(22) + this.getFormattedTime(m_RacerRecords[i].Time).PadRight(27) +
                            m_RacerRecords[i].Ship.ToString().PadRight(15) + "|"));
                    }
                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "|   To get all times for this base type :             .br baseinfo " + this.m_CurrentBase.BaseID.ToString().PadLeft(2, '0') + " |"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                }
                else
                {
                    List<BRPlayer> baseInfo = this.m_FakeDB.GetBaseRecords(m_CurrentBase.BaseID);

                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "|              Base ID Number " + this.m_CurrentBase.BaseID.ToString().PadLeft(2, '0') + "                 Top Times            |"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "|   Player".PadRight(25) + "Time".PadRight(22) + "Ship".PadRight(13) + "Date".PadRight(10) + "|"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "+----------------------^---------------------^------------^-----------+"));
                    int i = 1;
                    foreach (BRPlayer b in baseInfo)
                    {
                        psyGame.Send(msg.team_pm(this.m_Freq, (i++).ToString().PadLeft(2, '0') + ". " + b.PlayerName.PadRight(20) + this.getFormattedTime(b.Time).ToString().PadRight(22) + b.Ship.ToString().PadRight(13) + b.Date.ToShortDateString()));
                    }
                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "| To race in the next base,            type:  !race   or   .race      |"));
                    psyGame.Send(msg.team_pm(this.m_Freq, "+---------------------------------------------------------------------+"));
                }
            }
            else
            {
                psyGame.Send(msg.arena("Race cancelled."));
            }
            this.race_Reset();
        }

        private void race_Reset()
        {
            this.m_Racers = new List<string>();
            this.m_RacerRecords = new List<BRPlayer>();
            this.loadNextBase();
            this.m_Status = RaceState.NotStarted;
            if (this.m_Timer.Enabled) this.m_Timer.Stop();
            this.m_Timer = new Timer();
            //this.timer_startRaceTimer();
        }

        private void race_SendPlayersToStart()
        {
            // Put all players on a list
            this.m_Racers = new List<string>();
            List<SSPlayer> temp = this.m_Players.PlayerList.FindAll(item => item.Frequency == m_Freq && item.Ship != ShipTypes.Spectator);
            foreach (SSPlayer p in temp)
                this.m_Racers.Add(p.PlayerName);

            string warpto = "|warpto " + this.m_CurrentBase.AlphaStartX + " " + this.m_CurrentBase.AlphaStartY;
            string prepPlayer = "|prize fullcharge|prize -engineshutdown";

            psyGame.Send(msg.team_pm(this.m_Freq,"?" + warpto + prepPlayer));
        }

        public void race_PlayerLeaving(SSPlayer ssp)
        {
            if (this.m_Status != RaceState.InProgress) return;
            this.m_Racers.Remove(ssp.PlayerName);

            if (this.m_Racers.Count == 0) this.race_EndRace();
        }

        //----------------------------------------------------------------------//
        //                             Timer                                    //
        //----------------------------------------------------------------------//
        private void timer_startRaceTimer()
        {
            this.m_TimerCountdown = 30;
            this.m_Timer = new Timer();
            m_Timer.Elapsed += new ElapsedEventHandler(timer_StartGame);
            m_Timer.Interval = 1000;
            psyGame.Send(msg.arena("A race will begin in " + m_TimerCountdown + " seconds in Base ID#[ "+this.m_CurrentBase.BaseID+" ] . Type !race to join in."));
            this.m_Timer.Start();
        }

        private void timer_GameTimeOut(object source, ElapsedEventArgs e)
        {
            m_Timer.Stop();
            psyGame.Send(msg.arena("Game TimeOut activated."));

            while (this.m_Racers.Count > 0)
            {
                psyGame.Send(msg.pm(m_Racers[0],"?|setship 9|a "+("You have timed out of race. If you wish to join next race type !race").PadRight(60)));
                m_Racers.RemoveAt(0);
            }

            this.race_EndRace();
        }

        private void timer_StartGame(object source, ElapsedEventArgs e)
        {
            this.m_TimerCountdown--;

            if (this.m_TimerCountdown == 10) this.race_SendPlayersToStart();

            if (this.m_TimerCountdown <= 5)
            {
                psyGame.Send(msg.team_pm(this.m_Freq, "?|objon " + this.m_TimerCountdown));
                if (this.m_TimerCountdown != 0)
                    psyGame.Send(msg.arena("", SoundCodes.MessageAlarm));
                else
                    psyGame.Send(msg.arena("", SoundCodes.Goal));
            }

            if (this.m_TimerCountdown > 0) return;

            m_Timer.Stop();

            // Make sure there is enough ppl in team to start
            if (this.m_Players.PlayerList.FindAll(item => item.Frequency == m_Freq && item.Ship != ShipTypes.Spectator).Count > 0)
            {
                this.m_Status = RaceState.InProgress;
                this.psyGame.Send(msg.team_pm(this.m_Freq, "?|shipreset"));
                this.m_Timer.Elapsed += new ElapsedEventHandler(timer_GameTimeOut);
                this.m_Timer.Interval = 1000 * 60 * 5;
                this.m_StartTime = DateTime.Now;
                this.m_Timer.Start();
            }
            else
            {
                this.loadNextBase();
                psyGame.Send(msg.team_pm(7265,"Race cancelled - Not enough players. Next base loaded. Base[ "+this.m_CurrentBase.Number+" ]"));
            }
        }
        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        private bool allowedCommandUsage(SSPlayer ssp)
        {
            if (ssp.Ship == ShipTypes.Spectator) return true;
            if (ssp.Frequency > 1 && ssp.Frequency < 10) return true;
            if (ssp.Frequency == this.m_Freq) return true;
            psyGame.Send(msg.pm(ssp.PlayerName,"You must be in spec, or in freqs 2 through 9 to use this command."));
            return false;
        }
        private string player_getRank(BRPlayer racer)
        {
            string[] m_Places = new string[] {"st","nd","rd","th"};
            int rIndex = m_RacerRecords.IndexOf(racer);
            return (rIndex + 1).ToString() + m_Places[rIndex > 3?3:rIndex];
        }

        private string getFormattedTime(TimeSpan ts)
        {
            string ms = ts.Milliseconds.ToString();
            ms = ms.Length > 3? ms.Substring(0,3):ms.PadRight(3,'0');
            return ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0') + ":" + ms + "ms";
        }

        // Simple collision check
        private bool player_InRegion(PlayerPositionEvent p, ushort[] region)
        {
            return player_InRegion(p, region, 0);
        }
        private bool player_InRegion(PlayerPositionEvent p, ushort[] region, ushort pad)
        {
            int x = p.MapPositionX;
            int y = p.MapPositionY;
            return (x >= region[0] - pad && x <= region[2] + pad && y >= region[1] - pad && y <= region[3] + pad);
        }

        // must turn in old base before you can get a new one - basemanager rules no exceptions
        private void loadNextBase()
        {
            this.m_BaseManager.ReleaseBase(this.m_CurrentBase, "BaseRace");
            this.m_CurrentBase = m_BaseManager.getNextBase("BaseRace");
        }

        private void loadBase(int num)
        {
            this.m_BaseManager.ReleaseBase(this.m_CurrentBase, "BaseRace");
            this.m_CurrentBase = m_BaseManager.getBaseNumber(num,"BaseRace");
        }
    }
    enum RaceState
    {
        NotStarted,
        InProgress
    }
}
