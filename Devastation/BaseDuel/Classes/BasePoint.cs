﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    public class BasePoint
    {
        public BasePoint( string AteamName, string BTeamName)
        {
            this.m_AlphaTeam = new BaseTeam();
            this.m_BravoTeam = new BaseTeam();
            this.m_ATeamName = AteamName;
            this.m_BTeamName = BTeamName;
        }

        private BaseTeam m_AlphaTeam;
        private BaseTeam m_BravoTeam;
        private string m_ATeamName, m_BTeamName;

        private DateTime m_StartTime;
        private TimeSpan m_TotalTime;
        public TimeSpan TotalTime
        { get { return this.m_TotalTime; } }

        private int m_BaseNumber;
        private string m_SafeWinner;
        private bool m_AlphaWon;
        private Misc.WinType m_WinType;

        public void SetTeamNames(string Ateam, string BTeam)
        {
            this.m_ATeamName = Ateam;
            this.m_BTeamName = BTeam;
        }

        public BasePoint GetCopy(bool AlphaWon, Misc.WinType winType)
        {
            BasePoint save = new BasePoint(this.m_ATeamName, this.m_BTeamName);
            save.m_AlphaWon = AlphaWon;
            save.m_WinType = winType;
            save.m_TotalTime = DateTime.Now - this.m_StartTime;
            save.m_StartTime = this.m_StartTime;
            save.m_BaseNumber = this.m_BaseNumber;
            save.m_AlphaTeam = this.m_AlphaTeam.getCopy();
            save.m_BravoTeam = this.m_BravoTeam.getCopy();
            save.m_SafeWinner = this.m_SafeWinner;
            return save;
        }

        public void startPoint()
        { this.m_StartTime = DateTime.Now; }

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

        // sloppy - clean this psy you nub
        public void removePlayer(string PlayerName)
        {
            if (m_AlphaTeam.getPlayer(PlayerName) != null)
            {
                this.m_AlphaTeam.removePlayer(PlayerName);
            }
            else if (m_BravoTeam.getPlayer(PlayerName) != null)
            {
                this.m_BravoTeam.removePlayer(PlayerName);
            }
        }

        public Queue<string> getPointInfo()
        {
            Queue<string> reply = new Queue<string>();
            int rightOffset = 27;
            int leftOffset = 20;

            reply.Enqueue("Team Count    :".PadRight(leftOffset) + ("[ " + this.AlphaCount().ToString()+" vs " + this.BravoCount().ToString() + " ]").PadLeft(rightOffset));
            reply.Enqueue("Winner        :".PadRight(leftOffset) + (this.m_AlphaWon ? "Alpha: " + this.m_ATeamName : "Bravo: " + this.m_BTeamName).PadLeft(rightOffset));
            reply.Enqueue("Base Number   :".PadRight(leftOffset) + (this.m_BaseNumber.ToString()).PadLeft(rightOffset));
            reply.Enqueue("Win Type      :".PadRight(leftOffset) + (this.m_WinType.ToString()).PadLeft(rightOffset));

            return reply;
        }
    }
}
