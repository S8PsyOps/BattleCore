using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore.Events;

namespace BattleCorePsyOps
{
    public class SSPlayerManager
    {
        public SSPlayerManager()
        {
            this.m_PlayerList = new List<SSPlayer>();
            this.m_SpecFreq = (ushort) 8025;
        }

        public SSPlayerManager(ushort SpecFreq)
        {
            this.m_PlayerList = new List<SSPlayer>();
            this.m_SpecFreq = SpecFreq;
        }

        private List<SSPlayer> m_PlayerList;
        private ushort m_SpecFreq;

        /// <summary>
        /// <para>This is a master list of players and all their updated info.</para>
        /// <para>As long as you track the right events this will update as needed.</para>
        /// <para>There is no need to modify this.</para>
        /// </summary>
        public List<SSPlayer> PlayerList
        {
            get { return m_PlayerList; }
            set { m_PlayerList = value; }
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
                    SSPlayer ssp = GetPlayer(value.PlayerList[i].PlayerName);
                    ssp.PlayerInfo = value.PlayerList[i];
                }
            }
        }

        /// <summary>
        /// <para>Sends back ssplayer using the PlayerPositionEvent.</para>
        /// </summary>
        /// <param name="e">Player Name</param>
        /// <returns>SSPlayer with all updated info</returns>
        public SSPlayer GetPlayer(PlayerPositionEvent e)
        {
            // Grab player info
            SSPlayer ssp = GetPlayer(e.PlayerName);
            ssp.ModLevel = e.ModLevel;
            // Update PlayerPosition
            ssp.Position = e;
            //return info
            return ssp;
        }

        /// <summary>
        /// <para>Sends back ssplayer using the ShipChangeEvent.</para>
        /// </summary>
        /// <param name="e">Player Name</param>
        /// <returns>SSPlayer with all updated info</returns>
        public SSPlayer GetPlayer(ShipChangeEvent e)
        {
            // Grab player info
            SSPlayer ssp = GetPlayer(e.PlayerName);
            ssp.ModLevel = e.ModLevel;
            // Update sc timestamp
            ssp.SCTimeStamp = DateTime.Now;
            // Special case update for freq
            if (ssp.Frequency != m_SpecFreq) ssp.OldFrequency = ssp.Frequency;
            // Ship updates
            ssp.OldShip = e.PreviousShipType;
            ssp.Ship = e.ShipType;
            //return info
            return ssp;
        }

        /// <summary>
        /// Sends back ssplayer using the TeamChangeEvent.
        /// </summary>
        /// <param name="e">Player Name</param>
        /// <returns>SSPlayer with all updated info</returns>
        public SSPlayer GetPlayer(TeamChangeEvent e)
        {
            // Grab player
            SSPlayer ssp = GetPlayer(e.PlayerName);
            ssp.ModLevel = e.ModLevel;
            // Update old ship to new ship if its not a spec event
            if ((DateTime.Now - ssp.SCTimeStamp).TotalMilliseconds > 20)
            { ssp.OldShip = ssp.Ship; }
            // Update player freq info
            ssp.OldFrequency = ssp.Frequency;
            ssp.Frequency = e.Frequency;
            // send it back
            return ssp;
        }

        /// <summary>
        /// Creates and stores player using the PlayerEnteredEvent.
        /// </summary>
        /// <param name="e">Player Name</param>
        /// <returns>SSPlayer with all updated info</returns>
        public SSPlayer GetPlayer(PlayerEnteredEvent e)
        {
            // Grab player - most likely not on list but a new will be created
            SSPlayer ssp = GetPlayer(e.PlayerName);
            // update all info using player entered
            ssp.PlayerEntered = e;
            //return it
            return ssp;
        }

        /// <summary>
        /// Sends back ssplayer using ChatEvent. Updates modlvl too.
        /// </summary>
        /// <param name="e">Player Name</param>
        /// <returns>SSPlayer with all updated info</returns>
        public SSPlayer GetPlayer(ChatEvent e)
        {
            SSPlayer ssp = GetPlayer(e.PlayerName);
            ssp.ModLevel = e.ModLevel;
            return ssp;
        }

        /// <summary>
        /// Sends back SSPlayer using PlayerLeftEvent.
        /// </summary>
        /// <param name="e">Player Name</param>
        /// <returns>SSPlayer with all updated info</returns>
        public SSPlayer GetPlayer(PlayerLeftEvent e)
        {
            return GetPlayer(e.PlayerName);
        }

        /// <summary>
        /// <para>Use this after you getPlayer(PlayerLeftEvent) to delete from list.</para>
        /// <para>That way you can get all the necessary info from it before its deleted.</para>
        /// </summary>
        /// <param name="ssPlayer"></param>
        public void RemovePlayer(SSPlayer ssPlayer)
        {
            m_PlayerList.Remove(ssPlayer);
        }

        // Grab player from list. If not in list create a player and put into list
        public SSPlayer GetPlayer(string PlayerName)
        {
            SSPlayer ssp = m_PlayerList.Find(player => player.PlayerName == PlayerName);

            if (ssp != null) return ssp;

            ssp = new SSPlayer();
            ssp.PlayerName = PlayerName;
            m_PlayerList.Add(ssp);

            return m_PlayerList.Find(player => player.PlayerName == PlayerName);
        }
    }
}
