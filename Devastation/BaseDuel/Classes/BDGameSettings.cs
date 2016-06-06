using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    public enum BDSettingType
    {
        Normal,
        OneVsOne,
        Custom
    }
    public class BDGameSettings
    {
        private BDSettingType m_Type;
        private bool m_SafeWinOn;
        private int m_MinimumPoint;
        private int m_WinBy;

        public BDSettingType Type
        {
            get { return this.m_Type; }
            set { this.m_Type = value; }
        }

        public bool SafeWin
        {
            get { return this.m_SafeWinOn; }
            set { this.m_SafeWinOn = value; }
        }

        public int MinimumPoints
        {
            get { return this.m_MinimumPoint; }
            set { this.m_MinimumPoint = value; }
        }

        public int WinBy
        {
            get { return this.m_WinBy; }
            set { this.m_WinBy = value; }
        }

        public void LoadSettings(BDSettingType type)
        {
            switch (type)
            {
                case BDSettingType.Normal:
                    this.loadNormal();
                    break;
                case BDSettingType.OneVsOne:
                    this.loadNormal();
                    this.loadOneVsOne();
                    break;
            }
        }

        private void loadNormal()
        {
            this.MinimumPoints = 5;
            this.WinBy = 2;
            this.SafeWin = true;
            this.Type = BDSettingType.Normal;
        }
        private void loadOneVsOne()
        {
            this.SafeWin = false;
            this.Type = BDSettingType.OneVsOne;
        }
    }
}
