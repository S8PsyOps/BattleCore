using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseDuel
{
    class PlayerManager
    {
        public PlayerManager()
        {
            m_PlayerList = new List<DevaPlayer>();
            m_PlayerListChat = new Queue<EventArgs>();
            msg = new ShortChat();
        }

        private Queue<EventArgs> m_PlayerListChat;      // This holds all messages that need to go out
        private ShortChat msg;                          // Custom Chat module
        private List<DevaPlayer> m_PlayerList;          // Holds all players in arena and status

        public List<DevaPlayer> List
        { get { return m_PlayerList; } }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void RemovePlayer(string PlayerName)
        {
            DevaPlayer BDplayer = m_PlayerList.Find(item => item.PlayerName.ToLower() == PlayerName.ToLower());

            if (BDplayer == null) return;
            m_PlayerList.Remove(BDplayer);
        }
        public DevaPlayer GetPlayer(PlayerLeftEvent e)
        {
            DevaPlayer BDplayer = m_PlayerList.Find(item => item.PlayerName.ToLower() == e.PlayerName.ToLower());
            return BDplayer;
        }
        public DevaPlayer GetPlayer(PlayerEnteredEvent e)
        {
            DevaPlayer BDplayer = m_PlayerList.Find(item => item.PlayerName.ToLower() == e.PlayerName.ToLower());

            if (BDplayer != null)
                m_PlayerListChat.Enqueue(msg.chan(1, "Error: Player[ " + e.PlayerName + " ] was found on list when it was supposed to be null."));

            // Create baseplayer and use playerenter to fill info in
            BDplayer = new DevaPlayer();
            BDplayer.PlayerEntered = e;

            //m_PlayerListChat.Enqueue(msg.chan(1, "Player[ " + BDplayer.PlayerName + " ] Squad[ " + BDplayer.SquadName + " ] added to playerlist."));
            m_PlayerList.Add(BDplayer);

            return BDplayer;
        }
        public DevaPlayer GetPlayer(PlayerPositionEvent e)
        {
            DevaPlayer BDplayer = m_PlayerList.Find(item => item.PlayerName.ToLower() == e.PlayerName.ToLower());

            if (BDplayer == null)
            {
                m_PlayerListChat.Enqueue(msg.chan(1, "Error: Player[ " + e.PlayerName + " ] was NOT found on list. (PlayerPositionEvent)"));
                return null;
            }
            ModUpdate(BDplayer, e.ModLevel);
            BDplayer.Position = e;
            return BDplayer;
        }
        public DevaPlayer GetPlayer(TeamChangeEvent e)
        {
            DevaPlayer BDplayer = m_PlayerList.Find(item => item.PlayerName.ToLower() == e.PlayerName.ToLower());

            if (BDplayer == null)
            {
                m_PlayerListChat.Enqueue(msg.chan(1, "Error: Player[ " + e.PlayerName + " ] was NOT found on list. (TeamChangeEvent)"));
                return null;
            }
            ModUpdate(BDplayer, e.ModLevel);
            if ((DateTime.Now - BDplayer.SCTimeStamp).TotalMilliseconds > 15) BDplayer.OldShip = BDplayer.Ship;

            BDplayer.OldFrequency = BDplayer.Frequency;
            BDplayer.Frequency = e.Frequency;

            return BDplayer;
        }
        public DevaPlayer GetPlayer(ShipChangeEvent e)
        {
            DevaPlayer BDplayer = m_PlayerList.Find(item => item.PlayerName.ToLower() == e.PlayerName.ToLower());

            if (BDplayer == null)
            {
                m_PlayerListChat.Enqueue(msg.chan(1, "Error: Player[ " + e.PlayerName + " ] was NOT found on list. (ShipChangeEvent)"));
                return null;
            }
            ModUpdate(BDplayer, e.ModLevel);
            BDplayer.SCTimeStamp = DateTime.Now;

            if (e.ModLevel != ModLevels.None && e.ModLevel != BDplayer.ModLevel)
            {
                BDplayer.ModLevel = e.ModLevel;
                m_PlayerListChat.Enqueue(msg.chan(1, "Staff Position Auto-Detected. Player[ " + BDplayer.PlayerName + " ] ModLevel[ " + BDplayer.ModLevel + " ]"));
            }

            if (BDplayer.Frequency != 7265) BDplayer.OldFrequency = BDplayer.Frequency;

            BDplayer.OldShip = e.PreviousShipType;
            BDplayer.Ship = e.ShipType;
            return BDplayer;
        }

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//

        //----------------------------------------------------------------------//
        //                         Misc Functions                               //
        //----------------------------------------------------------------------//
        public DevaPlayer GetPlayer(string PlayerName)
        {
            return m_PlayerList.Find(item => item.PlayerName.ToLower() == PlayerName.ToLower());
        }

        // Use PlayerInfoEvent to create our PlayerList
        public void UpdatePlayerList(PlayerInfoEvent pIE)
        {
            int Spec = 0, InShip = 0;

            for (int i = 0; i < pIE.PlayerList.Count; i += 1)
            {
                if (pIE.PlayerList[i].Ship == ShipTypes.Spectator) Spec++;
                else InShip++;

                DevaPlayer BDplayer = new DevaPlayer();
                BDplayer.PlayerInfo = pIE.PlayerList[i];
                m_PlayerList.Add(BDplayer);
            }

            //m_PlayerListChat.Enqueue(msg.chan(1, "Arena List stored. Spec[ " + Spec + " ] InGame[ " + InShip + " ]"));
        }
        public void UpdateModPlayerList(PlayerInfoEvent pIE)
        {
            for (int i = 0; i < pIE.PlayerList.Count; i += 1)
            {
                DevaPlayer match = m_PlayerList.Find(item => item.PlayerName.ToLower() == pIE.PlayerList[i].PlayerName.ToLower());
                if (match != null)  ModUpdate(match, pIE.PlayerList[i].ModeratorLevel);
            }
        }
        private void ModUpdate(DevaPlayer b, ModLevels m)
        {
            if (b.ModLevel == m) return;

            b.ModLevel = m;
            m_PlayerListChat.Enqueue(msg.chan(1,"ModCheckUpdate. Player[ "+b.PlayerName+" ]  ModLevel[ "+b.ModLevel+" ]."));
        }

        // Request for next message to send from Q
        public EventArgs MessageToSend()
        {
            if (m_PlayerListChat.Count > 0)
                return m_PlayerListChat.Dequeue();
            return null;
        }
    }
}
