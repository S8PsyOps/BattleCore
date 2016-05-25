using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation
{
    public enum BaseSize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
        Off = 3
    }

    public enum BaseMode
    {
        Shuffle,
        Random,
        RoundRobin,
        Custom
    }

    public enum BaseGameStatus
    {
        GameIdle,
        GameOn,
        GameIntermission,
        GameHold
    }

    public enum WinType
    {
        SafeWin,
        BaseClear,
        NoCount
    }
}
