using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Misc
{
    public enum BaseGameStatus
    {
        NotStarted,
        InProgress,
        BetweenPoints,
        OnHold
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

    public enum GameSetting
    {
        Normal,
        OneVsOne,
        Custom
    }
}
