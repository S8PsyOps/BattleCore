using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;
using Devastation.BaseRace;

namespace Devastation
{
    class FileDataBase
    {
        public FileDataBase(ShortChat msg, MyGame psyGame, SSPlayerManager Players)
        {
            this.msg = msg;
            this.psyGame = psyGame;
            this.m_Players = Players;
            this.m_HomeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "DataBase" + Path.DirectorySeparatorChar;
            this.m_RecordedBases = new List<BaseInfo>();
        }

        private string m_HomeDir; //   :/BotName/DataBase/
        private ShortChat msg;
        private MyGame psyGame;
        private SSPlayerManager m_Players;
        private List<BaseInfo> m_RecordedBases;

        class BaseInfo
        {
            public int BaseID { get; set; }
            public List<BRPlayer> Times { get; set; }
        }

        public void BaseRace_SavePlayerTimes(List<BRPlayer> RaceTimes, Base Base)
        {
            foreach (BRPlayer p in RaceTimes) this.baserace_savePLayer(p, Base);
        }

        public ShipTypes getShip(string ship)
        {
            foreach (ShipTypes s in Enum.GetValues(typeof(ShipTypes)))
            {
                if (s.ToString().Equals(ship)) return s;
            }
            return ShipTypes.Spectator;
        }

        public List<BRPlayer> GetBaseRecords(int BaseID)
        {
            return this.getBaseRecords(BaseID);
        }

        private List<BRPlayer> getBaseRecords(int BaseID)
        {
            string baseFileName = "base" + BaseID.ToString().PadLeft(3, '0') + ".times";
            string baseDir = this.m_HomeDir + "BaseRace" + Path.DirectorySeparatorChar + "Bases" + Path.DirectorySeparatorChar;

            if (!File.Exists(baseDir + baseFileName)) { return null; }

            List<BRPlayer> savedTimes = new List<BRPlayer>();
            string name = "";
            string ship = "";
            TimeSpan time = new TimeSpan();

            using (StreamReader sr = File.OpenText(baseDir + baseFileName))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.StartsWith("Player:")) name = s.Replace("Player:", "").Trim();
                    else if (s.StartsWith("Ship:")) ship = s.Replace("Ship:", "").Trim();
                    else if (s.StartsWith("Time:")) time = TimeSpan.Parse(s.Replace("Time:", "").Trim());
                    else if (s.StartsWith("Date:"))
                    {
                        BRPlayer p = new BRPlayer();
                        p.PlayerName = name;
                        p.Ship = this.getShip(ship);
                        p.Time = time;
                        p.Date = DateTime.Parse(s.Replace("Date:", "").Trim());
                        savedTimes.Add(p);
                    }
                }
            }

            if (savedTimes.Count <= 0) return null;

            int a,b =  0;
            for (a = 0; a < savedTimes.Count; a++)
            {
                for (b = a + 1; b < savedTimes.Count; b++)
                {
                    if (savedTimes[a].Time > savedTimes[b].Time)
                    {
                        BRPlayer temp = savedTimes[a];
                        savedTimes[a] = savedTimes[b];
                        savedTimes[b] = temp;
                    }
                }
            }

            return savedTimes;
        }

        private void baserace_savePLayer(BRPlayer racer, Base Base)
        {
            SSPlayer ssp = this.m_Players.PlayerList.Find(item => item.PlayerName == racer.PlayerName);

            if (ssp == null || !(ssp.SubspaceId > 0))
            {
                psyGame.SafeSend(msg.chan(1,"SSPlayer not found - Filedatabase - baserace_savePLayer."));
                return;
            }

            // Create Deva player profile folder if it isnt created
            System.IO.Directory.CreateDirectory(this.m_HomeDir + "Players" + Path.DirectorySeparatorChar + ssp.SubspaceId.ToString());

            // Check deva profile - create profile if not created
            if (!this.playerHasDevaProfile(ssp)) this.createDevaPlayerProfile(ssp);

            string racerDir = this.m_HomeDir + "BaseRace" + Path.DirectorySeparatorChar + "Racers" + Path.DirectorySeparatorChar + ssp.SubspaceId.ToString();

            // Create racer folder if it isnt created
            System.IO.Directory.CreateDirectory(racerDir);

            // Check Race profile - create profile if not created
            if (!this.playerHasRaceProfile(ssp, racerDir)) this.createRaceProfile(ssp, racerDir);

            // Store race time for player
            using (StreamWriter w = File.AppendText(racerDir + Path.DirectorySeparatorChar  + "base" + Base.BaseID.ToString().PadLeft(3, '0') + ".times"))
            {
                w.WriteLine("Player:" + ssp.PlayerName);
                w.WriteLine("Ship:" + racer.Ship);
                w.WriteLine("Time:" + racer.Time);
                w.WriteLine("Date:" + DateTime.Now.ToShortDateString());
            }

            string baseDir = this.m_HomeDir + "BaseRace" + Path.DirectorySeparatorChar + "Bases" + Path.DirectorySeparatorChar;
            using (StreamWriter w = File.AppendText(baseDir + "base" + Base.BaseID.ToString().PadLeft(3, '0') + ".times"))
            {
                w.WriteLine("Player:" + ssp.PlayerName);
                w.WriteLine("Ship:" + racer.Ship);
                w.WriteLine("Time:" + racer.Time);
                w.WriteLine("Date:" + DateTime.Now.ToShortDateString());
            }

            //need to store match if player count > 1
        }

        private bool playerHasDevaProfile(SSPlayer ssp)
        {
            return File.Exists(this.m_HomeDir + "Players" + Path.DirectorySeparatorChar + ssp.SubspaceId.ToString() + "/profile.info");
        }

        private void createDevaPlayerProfile(SSPlayer ssp)
        {
            string DevaPlayerDir = this.m_HomeDir + "Players" + Path.DirectorySeparatorChar + ssp.SubspaceId.ToString() + Path.DirectorySeparatorChar;

            using (StreamWriter writer = File.AppendText(DevaPlayerDir + "profile.info"))
            {
                writer.WriteLine("[Player Information]");
                writer.WriteLine("PlayerName:" + ssp.PlayerName);
                writer.WriteLine("DateCreated:" + DateTime.Now.ToShortDateString());
                psyGame.SafeSend(msg.chan(1,"Player[ "+ssp.PlayerName+" ] not found, new player profile created."));
            }
        }

        private void loadRaceAllRecords()
        {

        }

        private bool playerHasRaceProfile(SSPlayer ssp, string racerDir)
        {
            return File.Exists(racerDir + Path.DirectorySeparatorChar + "profile.info");
        }

        private void createRaceProfile(SSPlayer ssp, string racerDir)
        {
            using (StreamWriter writer = File.AppendText(racerDir + Path.DirectorySeparatorChar + "profile.info"))
            {
                writer.WriteLine("[Player Information]");
                writer.WriteLine("PlayerName:" + ssp.PlayerName);
                writer.WriteLine("DateCreated:" + DateTime.Now.ToShortDateString());
                psyGame.SafeSend(msg.chan(1, "Player[ " + ssp.PlayerName + " ] not found in BaseRace profiles, new racer profile created."));
            }
        }
    }
}
