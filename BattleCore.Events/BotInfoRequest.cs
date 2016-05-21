// Namespace usage
using System;
using BattleCore;

namespace BattleCore.Events
{
    public class BotInfoRequest : EventArgs
    {
        private String m_BotName;
        public String BotName
        {
            get { return m_BotName; }
            set { m_BotName = value; }
        }
        private String m_MapFile;
        public String MapFile
        {
            get { return m_MapFile; }
            set { m_MapFile = value; }
        }
        private Byte[] m_MapData;
        public Byte[] MapData
        {
            get { return m_MapData; }
            set { m_MapData = value; }
        }
    }
}
