using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation
{
    public enum BaseGameStatus
    {
        GameIdle,
        GameOn,
        GameIntermission,
        GameHold
    }

    public enum TimerType
    {
        GameStart,
        BaseClear,
        InactiveReset
    }

    public enum WinType
    {
        SafeWin,
        BaseClear,
        NoCount
    }
}
