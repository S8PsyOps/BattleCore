using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

// Needed for timers
using System.Timers;

namespace DevaBot.BaseDuel
{
    class BaseGameManager
    {
        public BaseGameManager()
        {
            this.msg = new ShortChat();
            this.m_CurrentGame = new BaseGame();
            this.m_Timer = new Timer();
        }

        private ShortChat msg;
        private BaseGame m_CurrentGame;
        private Timer m_Timer;

        public void TransferWaitListToTeam( bool IsAlpha, List<string> WaitList)
        {
        }

        public void CommandStartGame()
        {
        }

    }
}
