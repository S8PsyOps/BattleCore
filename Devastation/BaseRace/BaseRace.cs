using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation
{
    public class BaseRace
    {
        public BaseRace(SSPlayerManager PlayerManager, byte[] MapData, ShortChat msg, MyGame myGame)
        {
            this.m_Players = PlayerManager;
            this.msg = msg;
            this.m_BaseManager = new BaseManager(MapData);
            this.psyGame = myGame;
            this.m_BaseRaceFreq = 1337;
            this.m_BlockedList = new List<string>();
        }

        private SSPlayerManager m_Players;
        private ShortChat msg;
        private BaseManager m_BaseManager;
        private MyGame psyGame;
        private ushort m_BaseRaceFreq;
        private List<string> m_BlockedList;

        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        /// <summary>
        /// <para>All commands used for baserace send through here.</para>
        /// <para>Syntax: !baserace command    Example !baserace settings  or  .baserace start</para>
        /// <para>If you want to make compatible commands, register them in devastation main</para>
        /// <para>and change the command to make it compatible with !baseduel command</para>
        /// <para>then send it here. BaseRace.Commands(e);</para>
        /// </summary>
        /// <param name="p">Deva Player</param>
        /// <param name="e">Command sent</param>
        /// <returns></returns>
        public void Commands(ChatEvent e)
        {
            if (m_BlockedList.Contains(e.PlayerName)) return;

            // store command here if all checks out
            string command;
            // making sure command is formatted properly
            if (isCommand(e, out command))
            {

                SSPlayer p = m_Players.GetPlayer(e.PlayerName);

                switch (command)
                {
                    case ".baserace":
                        e.Message = "!help baserace";
                        psyGame.CoreSend(e);
                        return;

                    case "commands":
                        e.Message = "!help baserace commands";
                        psyGame.CoreSend(e);
                        return;
                    
                    case "info":
                        psyGame.Send(msg.arena("show info message"));
                        return;
                    
                    case "test":
                        doThis(e);
                        return;
                    
                    case "ahmad":
                        doAhmad(e);
                        return;
                }
            }
        }

        public void doThis(ChatEvent e)
        {
            SSPlayer ssp = m_Players.GetPlayer(e);
        }
        public void doAhmad(ChatEvent e)
        {
            psyGame.Send(msg.arena("Hi ahmad."));
        }

        //----------------------------------------------------------------------//
        //                         Events                                       //
        //----------------------------------------------------------------------//
        public void Event_PlayerPosition(SSPlayer ssp)
        {

        }

        public void Event_PlayerTurretAttach(SSPlayer attacher, SSPlayer host) { }
        public void Event_PlayerTurretDetach(SSPlayer ssp) { }
        public void Event_PlayerEntered(SSPlayer ssp){ }
        public void Event_PlayerLeft(SSPlayer ssp){ }
        public void Event_PlayerFreqChange(SSPlayer ssp){   }
        public void Event_ShipChange(SSPlayer ssp){ }


        //----------------------------------------------------------------------//
        //                             Misc                                     //
        //----------------------------------------------------------------------//
        // Series of checks to make sure the command is in proper format
        private bool isCommand(ChatEvent e, out string formattedCommand)
        {
            string FullMessage = e.Message;
            formattedCommand = "Doesnt matter if code sends this back, because it isnt a proper command";

            // Making sure its the right type of message
            if (e.ChatType == ChatTypes.Public || e.ChatType == ChatTypes.Private || e.ChatType == ChatTypes.Team || e.ChatType == ChatTypes.Macro)
            {
                // making sure our command is in message with a [!] or a [.]
                if (FullMessage.StartsWith("!") || FullMessage.StartsWith("."))
                {
                    if (FullMessage.StartsWith("!br") || FullMessage.StartsWith(".br") || FullMessage.StartsWith("!baserace") || FullMessage.StartsWith(".baserace"))
                    {
                        // If command isnt a multiple just send original ".baseduel"
                        if (FullMessage.Contains(" ")) formattedCommand = FullMessage.Split(' ')[1];

                        // Send back the attached command = .baseduel [command]
                        else formattedCommand = ".baserace";

                        return true;
                    }
                    FullMessage = FullMessage.Remove(0, 1);

                    formattedCommand = FullMessage.Trim().ToLower();
                    return true;
                }
            }
            return false;
        }
    }
}
