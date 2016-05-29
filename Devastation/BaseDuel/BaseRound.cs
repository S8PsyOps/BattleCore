using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel
{
    class BaseRound
    {
        public BaseRound()
        {
            m_AlphaTeam = new BaseTeam();
            m_BravoTeam = new BaseTeam();
            m_StartTime = new DateTime();
            m_TotalTime = new TimeSpan();
        }

        private BaseTeam m_AlphaTeam;
        private BaseTeam m_BravoTeam;
        private DateTime m_StartTime;
        private TimeSpan m_TotalTime;
        private short m_BaseNumber;
        private WinType m_WinType;
        private bool m_AlphaWon;
        private string m_SafeWinner;

        private int m_AlphaDeaths = 0;
        private int m_BravoDeaths = 0;

        /// <summary>
        /// Alpha Team container
        /// </summary>
        public BaseTeam AlphaTeam
        {
            get { return m_AlphaTeam; }
            set { m_AlphaTeam = value; }
        }

        /// <summary>
        /// Bravo Team container
        /// </summary>
        public BaseTeam BravoTeam
        {
            get { return m_BravoTeam; }
            set { m_BravoTeam = value; }
        }

        public DateTime StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        /// <summary>
        /// Who won match. Alpha = true ; Bravo = false;
        /// </summary>
        public bool AlphaWon
        {
            get { return m_AlphaWon; }
            set { m_AlphaWon = value; }
        }

        /// <summary>
        /// How was match won.
        /// </summary>
        public WinType WinType
        {
            get { return m_WinType; }
            set { m_WinType = value; }
        }

        public short BaseNumber
        {
            set { m_BaseNumber = value; }
            get { return m_BaseNumber; }
        }

        public string SafeWinner
        {
            get { return m_SafeWinner; }
            set { m_SafeWinner = value; }
        }

        public TimeSpan TotalTime
        {
            get { return m_TotalTime; }
            set { m_TotalTime = value; }
        }

        public string TotalTimeFormatted
        {
            get { return
                m_TotalTime.Hours.ToString().PadLeft(2, '0') + "h:" 
                + m_TotalTime.Minutes.ToString().PadLeft(2, '0') + "m:"
                + m_TotalTime.Seconds.ToString().PadLeft(2, '0') + "s";
            }
        }

        public int AlphaDeaths
        {
            get { return m_AlphaDeaths; }
            set { m_AlphaDeaths = value; }
        }
        public int BravoDeaths
        {
            get { return m_BravoDeaths; }
            set { m_BravoDeaths = value; }
        }
    }
}
