using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    public class BasePlayerStats
    {
        public BasePlayerStats()
        {
            this.m_Deaths = 0;
            this.m_Kills = 0;
        }

        private int m_Kills;
        private int m_Deaths;

        public int Kills
        {
            get { return this.m_Kills; }
            set { this.m_Kills = value; }
        }

        public int Deaths
        { 
            get { return this.m_Deaths; }
            set { this.m_Deaths = value; }
        }

        public BasePlayerStats GetCopy()
        {
            BasePlayerStats copy = new BasePlayerStats();
            copy.Kills = this.m_Kills;
            copy.Deaths = this.m_Deaths;
            return copy;
        }
    }
}
