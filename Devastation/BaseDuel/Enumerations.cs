using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel
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

    public enum GameMode
    {
        AhmadMode,
        BaseOpsMode
    }

    public enum GameStatus
    {
        Disabled,
        GameIdle,
        GameInProgress,
        GameIntermission,
        GameOnHold
    }

    public enum WinType
    {
        SafeWin,
        AllOut,
        NoCount
    }
}
