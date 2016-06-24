using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using System.Timers;

namespace BattleCorePsyOps
{
    public class DisplayManager
    {
        public DisplayManager(ShortChat msg, MyGame psyGame, SSPlayerManager playerManager)
        {
            this.msg = msg;
            this.psyGame = psyGame;
            this.m_Players = playerManager;

            this.m_UpdateLimit = 250;

            this.m_PubDisplay_Old = new Dictionary<ushort, bool>();
            this.m_PubDisplay = new Dictionary<ushort, bool>();
            this.m_ScoreBoards = new Dictionary<string, ushort[]>();
            this.m_SingleImages = new Dictionary<string, ushort>();

            // Timer to update displays
            this.m_DisplayTimer = new Timer();
            this.m_DisplayTimer.Elapsed += new ElapsedEventHandler(displayTimer);
            this.m_DisplayTimer.Interval = 50;
            this.m_DisplayTimer.Start();
        }

        private ShortChat msg;
        private MyGame psyGame;
        private Timer m_DisplayTimer;
        private SSPlayerManager m_Players;

        // Limit of updates per packet
        private int m_UpdateLimit;

        // Pub display. A way to keep track of old and new state of lvz
        private Dictionary<ushort, bool> m_PubDisplay_Old,m_PubDisplay;
        // Scoreboards are list of lvz that represent a HUD score
        // new score will toggle old lvz off and new ones on
        private Dictionary<string, ushort[]> m_ScoreBoards;
        // simply a lvz asspciated with a name. like: "ScoreBoardBG" or something
        private Dictionary<string, ushort> m_SingleImages;

        //----------------------------------------------------------------------//
        //                     Display Functions                                //
        //----------------------------------------------------------------------//
        public void RegisterSingleImage(string ImageName, ushort LvzId)
        {
            // regardless if its on the list or not, this will create/change value
            this.m_SingleImages[ImageName] = LvzId;
        }

        public void RegisterScoreBoard(string BoardName, int StartIndex, int Digits)
        {
            // Temp list to store ids in
            List<ushort> lvz_ids = new List<ushort>();

            // Run through lvz range and add them to our list
            for (int i = StartIndex; i < StartIndex + Digits * 10; i += 1)
            { lvz_ids.Add((ushort)i); }

            // Register list to our local master list
            this.m_ScoreBoards.Add(BoardName, lvz_ids.ToArray());
            psyGame.SafeSend(msg.debugChan("[ Display Manager ] Scoreboard Registered. Scoreboard = [ " + BoardName + " ]"));
        }
        //--------------------------------//
        //              Public            //
        //--------------------------------//
        // Scoreboard MUST be registered before you load it
        public void LoadScoreBoard_Public(string BoardName)
        {
            this.loadScoreBoard(BoardName, this.m_PubDisplay, this.m_PubDisplay_Old);
        }

        public void UnloadScoreBoard_Public(string BoardName)
        {
            this.unloadScoreBoard(0xFFFF, BoardName, this.m_PubDisplay, this.m_PubDisplay_Old);
        }

        // Change of score 
        public void ScoreChange_Public(string BoardName, int NewScore)
        {
            // Container for lvz ids contained in our master list of scoreboards
            ushort[] LvzList;
             // Grab the list from our master list
            if (m_ScoreBoards.TryGetValue(BoardName, out LvzList))
            {
                // Use public as display to target, do scorechange
                this.scoreChange(NewScore, LvzList, this.m_PubDisplay);
                return;
            }
            psyGame.SafeSend(msg.debugChan("[ Display Manager ] A scorechange attempted on an unregistered Scoreboard. Scoreboard = [ " + BoardName + " ]"));
        }
        //--------------------------------//
        //              Player            //
        //--------------------------------//
        /// <summary>
        /// <para>Loads a scoreboard the the player's private display.</para>
        /// <para>The scoreboard MUST be registered before you try to load it.</para>
        /// <para>Use RegisterScoreBoard() to do so.</para>
        /// </summary>
        /// <param name="BoardName">The name the SB was registered with. Example:  LoadScoreBoard_Player("KillsSB", SSPlayer)</param>
        /// <param name="Player">SSPlayer that needs SB load.</param>
        public void LoadScoreBoard_Player(string BoardName, SSPlayer Player)
        {
            this.loadScoreBoard(BoardName, Player.Display, Player.Display_Old);
        }

        public void UnloadScoreBoard_Player(string BoardName, SSPlayer Player)
        {
            this.unloadScoreBoard(Player.PlayerId, BoardName, Player.Display, Player.Display_Old);
        }

        // Change of score 
        public void ScoreChange_Player(string BoardName, int NewScore, SSPlayer Player)
        {
            // Container for lvz ids contained in our master list of scoreboards
            ushort[] LvzList;
            // Grab the list from our master list
            if (m_ScoreBoards.TryGetValue(BoardName, out LvzList))
            {
                // Use public as display to target, do scorechange
                this.scoreChange(NewScore, LvzList, Player.Display);
                return;
            }
            psyGame.SafeSend(msg.debugChan("[ Display Manager ] A scorechange attempted on an unregistered (private) Scoreboard. Scoreboard = [ " + BoardName + " ]"));
        }
        //--------------------------------//
        //              Misc              //
        //--------------------------------//
        private void scoreChange(int NewScore, ushort[] LvzList, Dictionary<ushort, bool> TargetDisplay)
        {
            // Holds the individual digits
            List<int> listOfInts = new List<int>();
            // Holds the digits converted into lvz ids
            List<ushort> newscoreids = new List<ushort>();

            // Splits the score up into individual digits
            // and inverts them so we can start converting into 
            //lvz starting with ones,tens,hund...
            while (NewScore > 0)
            {
                listOfInts.Add(NewScore % 10);
                NewScore = NewScore / 10;
            }

            // We grab lvzList[0] and use it for our start index of lvz
            // and use it to convert to lvz from that point
            for (int i = 0; i < listOfInts.Count; i += 1)
                newscoreids.Add((ushort)(LvzList[0] + (10 * i) + listOfInts[i]));

            // First we toggle all lvz off
            for (int i = 0; i < LvzList.Length; i += 1)
                TargetDisplay[LvzList[i]] = false;

            // Then we turn on the lvz for our new score
            for (int i = 0; i < newscoreids.Count; i += 1)
                TargetDisplay[newscoreids[i]] = true;
        }

        private void loadSingleImage(string ImageName, Dictionary<ushort, bool> NewDisplay, Dictionary<ushort, bool> OldDisplay)
        {
            ushort LvzId;
            if (this.m_SingleImages.TryGetValue(ImageName, out LvzId))
            {
                this.Display_addLvz(LvzId, NewDisplay, OldDisplay);
            }
            psyGame.SafeSend(msg.debugChan("[ Display Manager ] An unregistered image attempted to load. ImageName = [ " + ImageName + " ]"));
        }

        // Load a scoreboard to target display
        private void loadScoreBoard(string BoardName, Dictionary<ushort, bool> NewDisplay, Dictionary<ushort, bool> OldDisplay)
        {
            ushort[] LvzList;

            if (m_ScoreBoards.TryGetValue(BoardName, out LvzList))
            {
                for (int i = 0; i < LvzList.Length; i += 1)
                { this.Display_addLvz(LvzList[i],NewDisplay,OldDisplay); }
                return;
            }
            psyGame.SafeSend(msg.debugChan("[ Display Manager ] An unregistered scoreboard attempted to load. Scoreboard = [ " + BoardName + " ]"));
        }

        private void unloadScoreBoard( ushort PlayerId, string BoardName , Dictionary<ushort, bool> NewDisplay, Dictionary<ushort, bool> OldDisplay)
        {
            ushort[] LvzList;

            if (m_ScoreBoards.TryGetValue(BoardName, out LvzList))
            {
                for (int i = 0; i < LvzList.Length; i++)
                { Display_removeLvz(LvzList[i], NewDisplay); }

                return;
            }
            psyGame.SafeSend(msg.debugChan("[ Display Manager ] An attempt to unload an unregistered scoreboard was made. Scoreboard = [ " + BoardName + " ]"));
        }

        private void Display_addLvz(ushort LvzId, Dictionary<ushort, bool> NewDisplay, Dictionary<ushort, bool> OldDisplay)
        {
            // Possible hole in logic here. If SB is loaded twice it will toggle any lvz that were left at true -> false

            // Lvz Id's shouldn't already be included 
            // but using this method we avoid possible crash
            NewDisplay[LvzId] = false;
            OldDisplay[LvzId] = false;
        }

        private void Display_removeLvz(ushort LvzId, Dictionary<ushort, bool> NewDisplay)
        {
            // Remove them from new display only, update will remove from old
            NewDisplay.Remove(LvzId);
        }

        public void RefreshDisplay_Player(SSPlayer Player)
        {
            List<ushort> AllLvz = new List<ushort>();

            foreach (ushort[] ids in this.m_ScoreBoards.Values)
            {
                foreach (ushort id in ids)
                { AllLvz.Add(id); }
            }

            foreach (ushort id in this.m_SingleImages.Values)
            { AllLvz.Add(id); }

            Dictionary<ushort, bool> update = new Dictionary<ushort, bool>();
            foreach (ushort id in AllLvz)
            {
                bool result;
                update[id] = this.m_PubDisplay.TryGetValue(id, out result) ? result : false;

                if (update.Count >= m_UpdateLimit)
                {
                    this.sendUpdate(Player.PlayerId, update);
                    update = new Dictionary<ushort, bool>();
                }
            }
            this.sendUpdate(Player.PlayerId, update);
        }

        //----------------------------------------------------------------------//
        //                       Timer Functions                                //
        //----------------------------------------------------------------------//
        private void displayTimer(object sender, ElapsedEventArgs e)
        {
            updatePublicDisplay();
            updateAllPlayers();
        }

        private void updateAllPlayers()
        {
            for (int i = 0; i < m_Players.PlayerList.Count; i++)
            {
                if (m_Players.PlayerList[i].NeedGfxUpdate)
                    this.updatePlayerDisplay(m_Players.PlayerList[i]);
            }
        }

        private void updatePlayerDisplay(SSPlayer Player)
        {
            this.updateDisplay(Player.PlayerId, Player.Display, Player.Display_Old);
        }

        private void updatePublicDisplay()
        {
            this.updateDisplay(0xFFFF, this.m_PubDisplay, this.m_PubDisplay_Old);
        }

        private void updateDisplay(ushort PlayerId, Dictionary<ushort, bool> NewDisplay, Dictionary<ushort, bool> OldDisplay)
        {
            // List of lvz to update
            Dictionary<ushort, bool> updateList = new Dictionary<ushort, bool>();

            // Itirate through entire list
            for (int i = OldDisplay.Count - 1; i >= 0; i--)
            {
                // Checking to make sure we dont pass limit per update
                if (updateList.Count >= m_UpdateLimit)
                {
                    // send the updates out to game - leave the rest till next update
                    // pub update = 0xFFFF
                    this.sendUpdate(PlayerId, updateList);
                    return;
                }

                // get next key to check against
                ushort key = OldDisplay.Keys.ElementAt(i);
                // First check to see if lvz id was deleted
                bool result;
                if (NewDisplay.TryGetValue(key, out result))
                {
                    // Only update for differences in new/old display
                    if (result != OldDisplay[key])
                    {
                        // add update to our list
                        updateList[key] = NewDisplay[key];
                        // make sure to update our display list as well
                        OldDisplay[key] = NewDisplay[key];
                    }
                }
                // The key wasnt found on new display - means it was deleted
                // We make sure the lvz wasnt left on here - if it was off when deleted ignore
                else 
                {
                    // turn the lvz off if it was deleted and left on
                    if (OldDisplay[key] == true)  updateList[key] = false;
                    // take off of old display
                    OldDisplay.Remove(key);
                }
            }
            // send the updates out to game - pub update = 0xFFFF
            this.sendUpdate(PlayerId, updateList);
        }

        //----------------------------------------------------------------------//
        //                         Misc                                         //
        //----------------------------------------------------------------------//
        private void sendUpdate(ushort PlayerId ,Dictionary<ushort, bool> updateList)
        {
            if (!(updateList.Count > 0)) return;
            // Pack it up and send it out
            LVZToggleEvent update = new LVZToggleEvent();
            update.TargetPlayerId = PlayerId;
            update.LVZObjects = updateList;
            psyGame.Send(update);
            //consoleShowUpdate(PlayerId, updateList);
        }

        private void consoleShowUpdate(ushort PlayerId,Dictionary<ushort, bool> list)
        {
            Console.WriteLine("--------------------------");
            Console.WriteLine("GFX Update      PID: " + PlayerId);
            Console.WriteLine("--------------------------");

            foreach (var lvz in list)
            {
                Console.WriteLine(("ID#:" + lvz.Key).PadRight(15) + "State:" + lvz.Value);
            }
        }
    }
}
