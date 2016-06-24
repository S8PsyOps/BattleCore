using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using System.Timers;

namespace BattleCorePsyOps
{
    public class DisplayManager2
    {
        public DisplayManager2(ShortChat msg, MyGame psyGame)
        {
            this.msg = msg;
            this.psyGame = psyGame;

            this.m_GFXTimer = new Timer();
            this.m_GFXTimer.Elapsed += new ElapsedEventHandler(GFXTimer);
            this.m_GFXTimer.Interval = 50;
            this.m_GFXTimer.Start();
        }

        private ShortChat msg;
        private MyGame psyGame;
        private Timer m_GFXTimer;

        // Old and new display - compare one to the other to see which lvz to toggle
        private Dictionary<ushort, bool> m_OldGlobalDisplayState = new Dictionary<ushort, bool>();
        private Dictionary<ushort, bool> m_GlobalDisplay = new Dictionary<ushort, bool>();
        /// <summary>
        /// Master list of all score boards created
        /// </summary>
        private Dictionary<string, ushort[]> m_ScoreBoardList = new Dictionary<string, ushort[]>();
        /// <summary>
        /// Single display is nothing more than a word associated with a lvz
        /// If you want to store: ScorboardBackground to be able to load/unload using "scoreBG" or something
        /// </summary>
        private Dictionary<string, ushort> m_SingleDisplayList = new Dictionary<string, ushort>();

        // Update timestamp duh
        private DateTime m_UpdateTimeStamp = DateTime.Now;
        // How often do we want to update gfx - ms
        //private double m_UpdateInterval = 50;

        // Packet allows for 250 updates - give or take
        private int m_PacketLimit = 250;

        /// <summary>
        /// Register a single lvz to the public lvz list.
        /// This goes directly into the public display: use this is you dont need any way of accessing other than lvz id
        /// If you prefer to name the lvz and access it that way use RegisterSingleDisplay and load it.
        /// </summary>
        /// <param name="LvzID">Lvz ID Number</param>
        public void RegisterLvz_Pub(ushort LvzID)
        {
            // Add lvz to both lists
            m_OldGlobalDisplayState.Add(LvzID, false);
            m_GlobalDisplay.Add(LvzID, false);
        }
        /// <summary>
        /// Score Board used for a player. Assumption is that it uses 0-9 and x amount of digits
        /// Scoreboards can be displays for points, currency etc.
        /// EXAMPLE: An example of a point scoreboard that has 
        /// points (1 - 1million) and the lvz start at 2000 
        /// would be configured as follows: 
        /// RegisterScoreBoard("PointScoreBoard", 2000 , 7 )
        /// </summary>
        /// <param name="sb_name">Name of your scoreboard: example= "Points"</param>
        /// <param name="start_index">The first lvz number in your display lvz range.
        /// Your lvz should start in the ones place and start from 0 and work their way up to 9.
        /// Starting at lvz id 2000, your 0 digit will be 2000, 1 = 2001, .. 10 = 2010 ... 100 = 2100 etc.
        /// </param>
        /// <param name="digits">
        /// How many places your scoreboard needs. For points ranging from 1 to 9999 it would be 4 digits.
        /// </param>
        public void RegisterScoreBoard(string sb_name, int start_index, int digits)
        {
            List<ushort> lvz_ids = new List<ushort>();

            // Run through lvz range and add them to our list
            for (int i = start_index; i < start_index + digits * 10; i += 1)
                lvz_ids.Add((ushort)i);

            // Register list to our local master list
            m_ScoreBoardList.Add(sb_name, lvz_ids.ToArray());
        }
        /// <summary>
        /// This gives you the ability to name a lvz in order to load and unload a display when you want.
        /// </summary>
        /// <param name="DisplayName">Name you want to give the lvz.
        /// Example: ScoreboardBackground</param>
        /// <param name="LvzID">Lvz ID Number</param>
        public void RegisterSingleDisplay(string DisplayName, ushort LvzID)
        {
            m_SingleDisplayList.Add(DisplayName, LvzID);
        }

        public void LoadSingleDisplay_Pub(string DisplayName)
        {
            ushort lvzid;

            if (m_SingleDisplayList.TryGetValue(DisplayName, out lvzid))
            {
                // if lvz isnt already included it will add it here
                m_GlobalDisplay[lvzid] = false;
                m_OldGlobalDisplayState[lvzid] = false;
            }
        }

        public void LoadSingleDisplay_Priv(string DisplayName, SSPlayer ssp)
        {
            ushort lvzid;

            if (m_SingleDisplayList.TryGetValue(DisplayName, out lvzid))
            {
                // if lvz isnt already included it will add it here
                ssp.Display[lvzid] = false;
                ssp.Display_Old[lvzid] = false;
            }
        }

        // Not sure if i need 2 methods yet - making this one just in case - may combine later
        public void ScoreChange_Priv(SSPlayer ssp, string listname, int newscore)
        {
            // Container for lvz ids contained in our master list of of scoreboards
            ushort[] alldisplaylvz;
            // Grab the list from our master list
            if (m_ScoreBoardList.TryGetValue(listname, out alldisplaylvz))
            {
                // Holds the individual digits
                List<int> listOfInts = new List<int>();
                // Holds the digits converted into lvz ids
                List<ushort> newscoreids = new List<ushort>();

                // Splits the score up into individual digits
                // and inverts them so we can start converting into 
                //lvz starting with ones,tens,hund...
                while (newscore > 0)
                {
                    listOfInts.Add(newscore % 10);
                    newscore = newscore / 10;
                }

                // We grab lvzList[0] and use it for our start index of lvz
                // and use it to convert to lvz from that point
                for (int i = 0; i < listOfInts.Count; i += 1)
                    newscoreids.Add((ushort)(alldisplaylvz[0] + (10 * i) + listOfInts[i]));

                // First we toggle all lvz off
                for (int i = 0; i < alldisplaylvz.Length; i += 1)
                    ssp.Display[alldisplaylvz[i]] = false;

                // Then we turn on the lvz for our new score
                for (int i = 0; i < newscoreids.Count; i += 1)
                    ssp.Display[newscoreids[i]] = true;
            }
        }

        public void ScoreChange_Pub(string listname, int newscore)
        {
            // Container for lvz ids contained in our master list of scoreboards
            ushort[] alldisplaylvz;
            // Grab the list from our master list
            if (m_ScoreBoardList.TryGetValue(listname, out alldisplaylvz))
            {
                // Holds the individual digits
                List<int> listOfInts = new List<int>();
                // Holds the digits converted into lvz ids
                List<ushort> newscoreids = new List<ushort>();

                // Splits the score up into individual digits
                // and inverts them so we can start converting into 
                //lvz starting with ones,tens,hund...
                while (newscore > 0)
                {
                    listOfInts.Add(newscore % 10);
                    newscore = newscore / 10;
                }

                // We grab lvzList[0] and use it for our start index of lvz
                // and use it to convert to lvz from that point
                for (int i = 0; i < listOfInts.Count; i += 1)
                    newscoreids.Add((ushort)(alldisplaylvz[0] + (10 * i) + listOfInts[i]));

                // First we toggle all lvz off
                for (int i = 0; i < alldisplaylvz.Length; i += 1)
                    m_GlobalDisplay[alldisplaylvz[i]] = false;

                // Then we turn on the lvz for our new score
                for (int i = 0; i < newscoreids.Count; i += 1)
                    m_GlobalDisplay[newscoreids[i]] = true;
            }
        }
        /// <summary>
        /// This will load a Scoreboard to a users profile so we can check update lvz as they are toggled.
        /// Add/Remove them as status changes.
        /// </summary>
        /// <param name="ssp">Player Profile</param>
        /// <param name="listname">Name of Lvz set to be loaded</param>
        /// <param name="q">Chat Q so we can add the lvz toggle commands.</param>
        public void LoadScoreboard_Priv(SSPlayer ssp, string listname)
        {
            ushort[] lvzList;

            if (m_ScoreBoardList.TryGetValue(listname, out lvzList))
            {
                for (int i = 0; i < lvzList.Length; i += 1)
                {
                    ssp.Display.Add(lvzList[i], false);
                    ssp.Display_Old.Add(lvzList[i], false);
                }
            }
        }

        /// <summary>
        /// This will unload a display from a users profile.
        /// </summary>
        /// <param name="ssp">Player Profile</param>
        /// <param name="listname">Name of Lvz set to be loaded</param>
        /// <param name="q">Chat Q so we can add the lvz toggle commands.</param>
        public void UnloadScoreboard_Priv(SSPlayer ssp, string listname)
        {
            ushort[] lvzList;

            if (m_ScoreBoardList.TryGetValue(listname, out lvzList))
                for (int i = 0; i < lvzList.Length; i += 1)
                {
                    if (ssp.Display.ContainsKey(lvzList[i]))
                    {
                        ssp.Display.Remove(lvzList[i]);
                        ssp.Display_Old.Remove(lvzList[i]);
                    }
                }
        }

        public void LoadScoreboard_Pub(string listname)
        {
            ushort[] lvzList;

            if (m_ScoreBoardList.TryGetValue(listname, out lvzList))
            {
                for (int i = 0; i < lvzList.Length; i += 1)
                {
                    // if lvz isnt already included it will add it here
                    m_GlobalDisplay[lvzList[i]] = false;
                    m_OldGlobalDisplayState[lvzList[i]] = false;
                }
            }
        }
        public void UnloadScoreboard_Pub(string listname)
        {
            ushort[] lvzList;

            if (m_ScoreBoardList.TryGetValue(listname, out lvzList))
                for (int i = 0; i < lvzList.Length; i += 1)
                {
                    if (m_GlobalDisplay.ContainsKey(lvzList[i]))
                    {
                        m_GlobalDisplay.Remove(lvzList[i]);
                        m_OldGlobalDisplayState.Remove(lvzList[i]);
                    }
                }
        }
        /// <summary>
        /// This sends the needed packet out to change lvz.
        /// This should be sent after you ave finished modifying the display.
        /// Example: Update Points, Update Currency, etc ... Then Refresh Display.
        /// </summary>
        /// <param name="ssp"></param>
        /// <param name="q"></param>
        public void RefreshDisplay_Priv(SSPlayer ssp)
        {
            // List of lvz to update
            Dictionary<ushort, bool> updateList = new Dictionary<ushort, bool>();

            // Itirate through entire list
            foreach (ushort id in ssp.Display.Keys)
            {
                // Check if lvz needs updating
                if (ssp.Display[id] != ssp.Display_Old[id])
                {
                    // add update to list
                    updateList.Add(id, ssp.Display[id]);
                    // update the users old display
                    ssp.Display_Old[id] = !ssp.Display_Old[id];
                }
            }

            // Pack the event into q
            if (updateList.Count > 0)
            {
                LVZToggleEvent update = new LVZToggleEvent();
                update.TargetPlayerId = ssp.PlayerId;
                update.LVZObjects = updateList;
                psyGame.Send(update);
            }
        }

        public void GFXTimer(object sender, ElapsedEventArgs e)
        {
            RefreshDisplay_Pub();
        }

        private void RefreshDisplay_Pub()
        {
            // List of lvz to update
            Dictionary<ushort, bool> updateList = new Dictionary<ushort, bool>();

            // Itirate through entire list
            for (int i = m_OldGlobalDisplayState.Count - 1; i >= 0; i--)
            {
                ushort key = m_OldGlobalDisplayState.Keys.ElementAt(i);

                // Check if update is needed
                if (m_OldGlobalDisplayState[key] != m_GlobalDisplay[key])
                {
                    // Reached limit for a packet
                    if (updateList.Count >= m_PacketLimit)
                    {
                        LVZToggleEvent update = new LVZToggleEvent();
                        update.TargetPlayerId = 0xFFFF; // pub
                        update.LVZObjects = updateList;
                        psyGame.Send(update);

                        consoleShowUpdate(update.LVZObjects);

                        return;
                    }

                    // add update to list
                    updateList[key] = m_GlobalDisplay[key]; ;
                    // update the users old display
                    m_OldGlobalDisplayState[key] = m_GlobalDisplay[key];
                }
            }

            // Update is needed - pack it up and send it out
            if (updateList.Count > 0)
            {
                // Pack it up and send it out
                LVZToggleEvent update = new LVZToggleEvent();
                update.TargetPlayerId = 0xFFFF; // pub
                update.LVZObjects = updateList;
                psyGame.Send(update);

                consoleShowUpdate(update.LVZObjects);

                // send it back
                return;
            }
        }

        private void consoleShowUpdate( Dictionary<ushort,bool> list)
        {
            Console.WriteLine("--------------------------");
            Console.WriteLine("GFX Update");
            Console.WriteLine("--------------------------");

            foreach (var lvz in list)
            {
                Console.WriteLine(("ID#:" + lvz.Key).PadRight(15) + "State:" + lvz.Value);
            }
        }
    }
}
