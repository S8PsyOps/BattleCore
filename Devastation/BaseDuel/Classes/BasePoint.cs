using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    class BasePoint
    {
        public BasePoint()
        {
            this.m_AlphaTeam = new BaseTeam();
            this.m_BravoTeam = new BaseTeam();
        }

        private BaseTeam m_AlphaTeam;
        private BaseTeam m_BravoTeam;

        private DateTime m_StartTime;
        private TimeSpan m_TotalTime;

        private int m_BaseNumber;
        private string m_SafeWinner;
        private bool m_AlphaWon;
        private WinType m_WinType;

        public BasePoint getSavedPoint(bool AlphaWon, WinType winType)
        {
            BasePoint save = new BasePoint();
            save.m_AlphaWon = AlphaWon;
            save.m_WinType = winType;
            save.m_TotalTime = DateTime.Now - this.m_StartTime;
            save.m_StartTime = this.m_StartTime;
            save.m_BaseNumber = this.m_BaseNumber;
            save.m_AlphaTeam = this.m_AlphaTeam;
            save.m_BravoTeam = this.m_BravoTeam;
            save.m_SafeWinner = this.m_SafeWinner;
            return save;
        }

        public void resetPoint()
        {
            this.m_AlphaTeam.flushTeam();
            this.m_BravoTeam.flushTeam();
            this.m_SafeWinner = null;
        }

        public void setSafeWinner(string PlayerName)
        { this.m_SafeWinner = PlayerName; }

        public void setBaseNumber(int baseNum)
        { this.m_BaseNumber = baseNum; }

        public BaseTeam AlphaTeam()
        { return this.m_AlphaTeam; }
        public BaseTeam BravoTeam()
        { return this.m_BravoTeam; }

        public int AlphaCount()
        { return this.m_AlphaTeam.teamCount(); }
        public int BravoCount()
        { return this.m_BravoTeam.teamCount(); }

        public BasePlayer getPlayer(string PlayerName, out bool InAlpha)
        {
            BasePlayer b = this.m_AlphaTeam.getPlayer(PlayerName);
            if (b != null)
            {
                InAlpha = true;
                return b;
            }
            InAlpha = false;
            return this.m_BravoTeam.getPlayer(PlayerName);
        }

        public void removePlayer(string PlayerName, bool InAlpha)
        {
            if (InAlpha)
            {
                this.m_AlphaTeam.removePlayer(PlayerName);
            }
            else
            {
                this.m_BravoTeam.removePlayer(PlayerName);
            }
        }
    }
}
