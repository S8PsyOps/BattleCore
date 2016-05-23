using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel
{
    class BaseTeam
    {
        public BaseTeam()
        {
            m_TeamMembers = new List<BasePlayer>();
            m_TeamName = "~ None Assigned ~";
        }

        private List<BasePlayer> m_TeamMembers;
        private string m_TeamName;

        /// <summary>
        /// Name of team.
        /// </summary>
        public string TeamName
        {
            get { return m_TeamName; }
            set { m_TeamName = value; }
        }

        /// <summary>
        /// All player's on team. Active or Inactive.
        /// </summary>
        public List<BasePlayer> TeamMembers
        {
            get { return m_TeamMembers; }
            set { m_TeamMembers = value; }
        }
    }
}
