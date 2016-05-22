using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace DevaBot
{
    class PlayerManager
    {
        public PlayerManager()
        {
            msg = new ShortChat();
            m_PMEventQueue = new Queue<EventArgs>();
            m_MasterList = new List<DevaPlayer>();
        }

        private Queue<EventArgs> m_PMEventQueue;
        private ShortChat msg;
        private List<DevaPlayer> m_MasterList;

        /// <summary>
        /// Master list of players
        /// </summary>
        public List<DevaPlayer> MasterList
        {   get { return m_MasterList; }    }

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        public void PrintPlayerInfo(string PlayerName)
        {
            DevaPlayer bp = getPlayer(PlayerName); 

            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "Player info -------"));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "Name:".PadRight(20) + bp.PlayerName.PadLeft(20)));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "Squad:".PadRight(20) + bp.SquadName.PadLeft(20)));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "ModLevel:".PadRight(20) + bp.ModLevel.ToString().PadLeft(20)));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "OldFreq:".PadRight(20) + bp.OldFrequency.ToString().PadLeft(20)));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "Freq:".PadRight(20) + bp.Frequency.ToString().PadLeft(20)));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "OldShip:".PadRight(20) + bp.OldShip.ToString().PadLeft(20)));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "Ship:".PadRight(20) + bp.Ship.ToString().PadLeft(20)));
            m_PMEventQueue.Enqueue(msg.pm(PlayerName, "Loc:".PadRight(20) + ("X:" + bp.Position.MapPositionX + "  Y:" + bp.Position.MapPositionY).PadLeft(20)));
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public DevaPlayer GetPlayer(PlayerLeftEvent e)
        { return getPlayer(e.PlayerName); }

        public DevaPlayer GetPlayer(ChatEvent e)
        { return getPlayer(e.PlayerName); }
        
        public DevaPlayer GetPlayer(PlayerEnteredEvent e)
        {
            // Grab player info
            DevaPlayer dp = getPlayer(e.PlayerName);
            // Update all info using player entered event
            dp.PlayerEntered = e;
            // return info
            return dp;
        }

        public DevaPlayer GetPlayer(TeamChangeEvent e)
        {
            DevaPlayer dp = getPlayer(e.PlayerName);

            // Update old ship to new ship if its not a spec event
            if ((DateTime.Now - dp.SCTimeStamp).TotalMilliseconds > 20)
            { dp.OldShip = dp.Ship; }
            // Update player freq info
            dp.OldFrequency = dp.Frequency;
            dp.Frequency = e.Frequency;
            // send it back
            return dp;
        }

        public DevaPlayer GetPlayer(ShipChangeEvent e)
        {
            // Grab player info
            DevaPlayer dp = getPlayer(e.PlayerName);
            // Update sc timestamp
            dp.SCTimeStamp = DateTime.Now;
            // Special case update for freq
            if (dp.Frequency != 7265) dp.OldFrequency = dp.Frequency;
            // Ship updates
            dp.OldShip = e.PreviousShipType;
            dp.Ship = e.ShipType;
            //return info
            return dp;
        }
        public DevaPlayer GetPlayer(PlayerPositionEvent e)
        {
            // Grab player info
            DevaPlayer dp = getPlayer(e.PlayerName);
            // Update PlayerPosition
            dp.Position = e;
            //return info
            return dp;
        }

        /// <summary>
        /// Use PlayerInfoEvent to fill in and populate PlayerList on start up.
        /// </summary>
        public PlayerInfoEvent PlayerInfoEvent
        {
            set
            {
                for (int i = 0; i < value.PlayerList.Count; i += 1)
                {
                    DevaPlayer dp = new DevaPlayer();
                    dp.PlayerInfo = value.PlayerList[i];
                    m_MasterList.Add(dp);
                }
            }
        }

        private DevaPlayer getPlayer(string PlayerName)
        {
            DevaPlayer dp = m_MasterList.Find(item => item.PlayerName == PlayerName);

            if (dp != null) return dp;

            dp = new DevaPlayer();
            dp.PlayerName = PlayerName;
            m_MasterList.Add(dp);
            return m_MasterList.Find(item => item.PlayerName == PlayerName);
        }

        // Any messages needed to be sent or events just add to this queue
        public EventArgs PlayerManagerEvents
        {
            get
            {
                if (m_PMEventQueue == null || m_PMEventQueue.Count == 0) return null;
                return m_PMEventQueue.Dequeue();
            }
        }

        //----------------------------------------------------------------------//
        //                         Misc Stuffs                                  //
        //----------------------------------------------------------------------//

        public void RemovePlayer(string PlayerName)
        {
            // Grab player info
            DevaPlayer dp = getPlayer(PlayerName);

            // UPDATE PLAYER INFO TO DB
            m_MasterList.Remove(dp);
        }
    }
}
