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
            this.myGame = myGame;
            this.m_BaseRaceFreq = 1337;
        }

        private SSPlayerManager m_Players;
        private ShortChat msg;
        private BaseManager m_BaseManager;
        private MyGame myGame;
        private ushort m_BaseRaceFreq;


        //----------------------------------------------------------------------//
        //                         Commands                                     //
        //----------------------------------------------------------------------//
        public void BaseRaceCommands(ChatEvent e)
        {
            // split the command into 2 diff commands
            string[] data = e.Message.Split(' ');

            // code for command !baserace or .baserace or !br or .br
            if (data[0].Trim().ToLower() == ".baserace" && data.Length <= 1)
            {
                // ------------------ //
                return;
            }

            // get the second command
            string command = data[1].Trim().ToLower();

            // !baserace command
            // for example:   !baserace info   or    !br info
            //                .baserace test   or    .br test
            switch (command)
            {
                case "info":
                    myGame.Send(msg.arena("show info message"));
                    return;
                case "test":
                    doThis(e);
                    return;
                case "ahmad":
                    doAhmad(e);
                    return;
            }
        }

        public void doThis(ChatEvent e)
        {
            SSPlayer ssp = m_Players.GetPlayer(e);
        }
        public void doAhmad(ChatEvent e)
        {
            myGame.Send(msg.arena("Hi ahmad."));
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
    }
}
