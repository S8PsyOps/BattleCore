using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel.Classes
{
    public class BasePlayer
    {
        public BasePlayer(string PlayerName)
        {
            this.m_PlayerName = PlayerName;
            this.m_InLobby = true;
        }

        private string m_PlayerName;
        private bool m_InLobby;
        
        public string PlayerName
        { get { return m_PlayerName; } }

        // Set or check if player is in lobby
        public bool inLobby()
        { return this.m_InLobby; }
        public void inLobby(bool InLobby)
        { this.m_InLobby = InLobby; }


    }
}
