using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Needed for timers
using System.Timers;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseRace
{
    public class RaceData
    {

        public class Race
        {
            public int time { get; set; }
            public string ctime { get; set; }
            public string name { get; set; }
            public Boolean reach { get; set; }
            public string reason { get; set; }
            public int rank { get; set; }
            public Race(int time2, string name2, Boolean reach2, string ctime2)
            {
                name = name2;
                time = time2;
                ctime = ctime2;
                reach = reach2;
                reason = "";
                rank = -1;
            }

        }
        public List<Race> RaceItems = new List<Race>();
        //Start position
        public int Start_x = 102;
        public int Start_y = 56;
        public string[] top10 = new string[11] { "", "", "", "", "", "", "", "", "", "", "" };
        //Finish position
        //32,56
        public int[] finish_x = new int[2] { 31, 33 };
        public int[] finish_y = new int[2] { 55, 57 };
        public Boolean isThereReach()
        {
            foreach (Race item in RaceItems)
            {
                if (item.reach == true)
                {
                    return true;
                }
            }
            return false;
        }
        public Boolean isInEnd(int x, int y)
        {
            return (finish_x[0] <= x && finish_y[0] <= y && finish_x[1] >= x && finish_y[1] >= y);
        }
        public int getTotalPlayers()
        {
            return RaceItems.Count;
        }
        public Boolean isInArea(int px, int py, int x, int y, int radius)
        {
            return (x + radius >= px && y + radius >= py && x + radius <= px && y + radius <= py);
        }
        public void loadBase(string mapname, string basename)
        {

        }
        public void RemoveRacer(Race e)
        {
            RaceItems.Remove(e);
        }

        public Race getRacer(string name)
        {
            Race n = new Race(-1, ".null", false, "");
            foreach (Race item in RaceItems)
            {
                if (item.name.Equals(name))
                {
                    n = item;
                    return n;
                }
            }
            return n;
        }
        public void CreateNewRacer(string name)
        {
            Race n = new Race(-1, name, false, "");
            n.name = name;
            n.time = -1;
            n.reach = false;
            n.ctime = "";
            n.reason = "";
            RaceItems.Add(n);

        }
    }
}
