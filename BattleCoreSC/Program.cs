using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleCore;

namespace BattleCoreSC
{
    class Program
    {
        public static void Main()
        {
            BattleCore.BattleCore m_core = new BattleCore.BattleCore();
            m_core.OnStart(null);
            Console.ReadLine();
            m_core.OnStop ();
        }
    }
}
