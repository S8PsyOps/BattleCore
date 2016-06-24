using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;

namespace Devastation.BaseRace
{
    public class BRPlayer
    {
        public string PlayerName { get; set; }
        public ShipTypes Ship { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime Date { get; set; }
    }
}
