// Namespace usage
using System;
using System.Collections.Generic;
using BattleCore;

namespace BattleCore.Events
{
    public class BotProfileEvent : EventArgs
    {
        private String m_BotName;
        private String m_ArenaName;

        public string BotName
        {
            get { return m_BotName; }
            set { m_BotName = value; }
        }

        public string ArenaName
        {
            get { return m_ArenaName; }
            set { m_ArenaName = value; }
        }
    }
}
