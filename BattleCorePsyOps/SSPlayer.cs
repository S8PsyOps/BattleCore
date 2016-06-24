﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleCore;
using BattleCore.Events;

namespace BattleCorePsyOps
{
    public class SSPlayer
    {
        private String m_PlayerName;
        private ushort m_PlayerId;
        private int m_SubspaceId;
        private ushort m_OldFrequency = 7265;
        private ushort m_Frequency = 7265;
        private ModLevels m_ModLevel = ModLevels.None;
        private String m_Squadname;
        private PlayerPositionEvent m_Position = new PlayerPositionEvent();
        private DateTime m_ShipChangeTS = DateTime.Now;
        private DateTime m_WarpStamp = DateTime.Now;
        private ShipTypes m_Ship;
        private ShipTypes m_OldShip;

        /// <summary>
        /// <para>Use this to fill in the info of player</para>
        /// </summary>
        public PlayerEnteredEvent PlayerEntered
        {
            set
            {
                m_PlayerName = value.PlayerName;
                m_PlayerId = value.PlayerId;
                m_Frequency = value.Frequency;
                m_Ship = value.ShipType;
                m_Squadname = value.SquadName.IndexOf("\0") == 0 ? "~ no squad ~" : value.SquadName;
            }
        }

        /// <summary>
        /// <para>Use this to fill in the info of player</para>
        /// </summary>
        public PlayerInfo PlayerInfo
        {
            set
            {
                m_PlayerName = value.PlayerName;
                m_ModLevel = value.ModeratorLevel;
                m_PlayerId = value.PlayerId;
                m_Frequency = value.Frequency;
                m_Ship = value.Ship;
                m_Squadname = value.SquadName.IndexOf("\0") == 0 ? "~ no squad ~" : value.SquadName;
                m_Position = value.Position;
            }
        }

        public String PlayerName
        {
            get { return m_PlayerName; }
            set { m_PlayerName = value; }
        }

        public ushort PlayerId
        {
            get { return m_PlayerId; }
            set { m_PlayerId = value; }
        }

        public int SubspaceId
        {
            get { return m_SubspaceId; }
            set { m_SubspaceId = value; }
        }

        public ushort OldFrequency
        {
            get { return m_OldFrequency; }
            set { m_OldFrequency = value; }
        }

        public ushort Frequency
        {
            get { return m_Frequency; }
            set { m_Frequency = value; }
        }

        public ModLevels ModLevel
        {
            get { return m_ModLevel; }
            set { m_ModLevel = value; }
        }

        public String SquadName
        {
            get { return m_Squadname; }
            set { m_Squadname = value; }
        }

        public DateTime SCTimeStamp
        {
            get { return m_ShipChangeTS; }
            set { m_ShipChangeTS = value; }
        }

        public DateTime WarpStamp
        {
            get { return m_WarpStamp; }
            set { m_WarpStamp = value; }
        }

        public ShipTypes Ship
        {
            get { return m_Ship; }
            set { m_Ship = value; }
        }

        public ShipTypes OldShip
        {
            get { return m_OldShip; }
            set { m_OldShip = value; }
        }

        public PlayerPositionEvent Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        private bool m_NeedGfxUpdate = false;
        public bool NeedGfxUpdate
        {
            get { return m_NeedGfxUpdate; }
            set { m_NeedGfxUpdate = value; }
        }

        private Dictionary<ushort, bool> m_Display = new Dictionary<ushort, bool>();
        public Dictionary<ushort, bool> Display
        {
            get { return m_Display; }
            set { m_Display = value; }
        }

        private Dictionary<ushort, bool> m_DisplayOld = new Dictionary<ushort, bool>();
        public Dictionary<ushort, bool> Display_Old
        {
            get { return m_DisplayOld; }
            set { m_DisplayOld = value; }
        }
    }
}
