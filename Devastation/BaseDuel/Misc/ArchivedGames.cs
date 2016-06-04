using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Misc
{
    public class ArchivedGames
    {
        public ArchivedGames(List<Classes.BasePoint> Archive)
        {
            this.m_SavedGame = Archive;
        }
        
        private List<Classes.BasePoint> m_SavedGame;
        public List<Classes.BasePoint> getGame()
        { return this.m_SavedGame; }
    }
}
