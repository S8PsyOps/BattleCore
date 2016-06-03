using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Misc
{
    public class SettingsLoader
    {
        public void LoadGameSettings(Classes.BaseGame Game, GameSetting Settings)
        {
            if (Settings == GameSetting.Normal)
            {
                Game.gameSettings(GameSetting.Normal);
                Game.setMinimumPoint(5);
                Game.setWinBy(2);
                Game.setAllowSafeWin(true);
                Game.setLocked(false);
            }
        }
    }
}
