using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation.BaseDuel
{
    class BaseManager
    {

        private ShortChat msg;                  // Custom chat module
        private Random ran;                     // Random num generator
        private List<Base> my_Bases;            // Main list of bases
        private Base my_Lobby;                  // Safe area saved as a base
        private Base my_LoadedBase;             // Base that is loaded
        private BaseMode my_Mode;               // Way to sort bases
        private BaseSize my_SizeMode;           // Limit bases loaded, by size
        private Queue<int> my_ShuffleModeQ;     // Stores the shuffle
        private int my_ShuffleModeQCount;       // Min amount of bases to keep on Q
        private int my_RoundRobinCount;         // where round robin picks up at

        public BaseManager(Byte[] mapData)
        {
            // Load all vars
            my_Lobby = new Base();
            my_Bases = new List<Base>();
            msg = new ShortChat();
            ran = new Random();
            my_Mode = BaseMode.Shuffle;
            my_SizeMode = BaseSize.Off;
            my_ShuffleModeQ = new Queue<int>();
            my_ShuffleModeQCount = 10;
            my_RoundRobinCount = 0;

            // Byte array tunrned into arrays i can work with
            MapData my_MapInfo = new MapData(mapData);

            // Map scanner. Loads/Configures/Saves Bases
            BaseLoader my_BaseLoader = new BaseLoader(my_MapInfo.TileIDs, my_MapInfo.TileTypes);
            my_BaseLoader.LoadBases(my_Bases, my_Lobby);

            // Load next base from default random
            ReShuffleQ(true);
            LoadNextBase();
        }
        public Base Lobby
        { get { return my_Lobby; } }

        public List<Base> Bases
        { get { return my_Bases; } }

        public Base LoadedBase
        { get { return my_LoadedBase; } }

        // Mode for choosing what base to chose next
        //-------------------------------------------
        // Shuffle  :   Bases get shuffled and put into a queue
        //              once every base has been played they get reshuffled again
        // Random   :   Every base is chosen at random. So it is possible to get the same
        //              base twice
        //RoundRobin:  Play from base 1 to last one and just restart at end
        // Custom   :  Not used yet, but maybe for custom base loading like player chosen/voted on
        public void ChangeBaseMode(BaseMode mode)
        {
            my_Mode = mode;
        }

        public void SetBaseSize(BaseSize newSize)
        {
            // Ignore if same size
            if (my_SizeMode == newSize)
                return;
            // Set new size
            my_SizeMode = newSize;
            // Load next base
            LoadNextBase();
        }

        // Load next base- depends on mode
        public void LoadNextBase()
        {
            bool reload = true;
            while (reload)
            {
                if (my_SizeMode == BaseSize.Off)
                    reload = false;

                string oldName = my_LoadedBase == null ? "~none~" : my_LoadedBase.BaseName;
                int newBase = 0;

                switch (my_Mode)
                {
                    case BaseMode.Shuffle:
                        newBase = my_ShuffleModeQ.Dequeue();
                        // keep our q at a minimum of [m_ModeZeroMinCount] - add to shuffle if we fall below
                        if (my_ShuffleModeQ.Count < my_ShuffleModeQCount)
                            ReShuffleQ(false);
                        break;
                    case BaseMode.RoundRobin:
                        my_RoundRobinCount = (my_RoundRobinCount + 1) >= my_Bases.Count ? 0 : my_RoundRobinCount + 1;
                        newBase = my_RoundRobinCount;
                        break;
                    case BaseMode.Random:
                        newBase = ran.Next(0, my_Bases.Count);
                        break;
                }

                // Assign new mode
                my_LoadedBase = my_Bases[newBase];
                // If the base is set to a certain size
                if (my_SizeMode != BaseSize.Off && my_SizeMode == my_LoadedBase.Size)
                    reload = false;
            }
        }

        // Shuffle bases for ShuffleMode - Option to clear old, if not we add to existing old shuffle
        private void ReShuffleQ(bool ClearOld)
        {
            List<int> baseIndexes = new List<int>();

            // add all indexes of all bases into temp list
            for (int i = 0; i < my_Bases.Count; i += 1)
                baseIndexes.Add(i);

            // shuffle 10 times to try and get the most random shuffle
            for (int h = 0; h < 10; h += 1)
            {
                // Shuffles bases by switching the next with a random one that's ahead in the deck
                // Continues till end
                for (int i = 0; i < baseIndexes.Count; i += 1)
                {
                    int temp = baseIndexes[i];
                    int r = ran.Next(i, baseIndexes.Count);
                    baseIndexes[i] = baseIndexes[r];
                    baseIndexes[r] = temp;
                }
            }

            // If we want a raw shuffle we clear old one here
            if (ClearOld) my_ShuffleModeQ = new Queue<int>();

            // Dump the new shuffle into our master queue
            for (int i = 0; i < my_Bases.Count; i += 1)
                my_ShuffleModeQ.Enqueue(baseIndexes[i]);
        }

        public Queue<EventArgs> LoadedBaseInfo(string PlayerName, int BaseNumber)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();

            int offset = 38;

            reply.Enqueue(msg.pm(PlayerName, "Base Information ----------------------------"));      //BaseNumber
            reply.Enqueue(msg.pm(PlayerName, "Base Name".PadRight(20) + my_Bases[BaseNumber].BaseName.PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Base Number".PadRight(20) + BaseNumber.ToString().PadLeft(2, '0').PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Tile Count".PadRight(20) + my_Bases[BaseNumber].TileCount.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Base Size".PadRight(20) + my_Bases[BaseNumber].Size.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, ("+--------  Dimension   --------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x1 [ " + my_Bases[BaseNumber].BaseDimension[0].ToString().PadLeft(4) + " ]    y1 [ " + my_Bases[BaseNumber].BaseDimension[1].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x2 [ " + my_Bases[BaseNumber].BaseDimension[2].ToString().PadLeft(4) + " ]    y2 [ " + my_Bases[BaseNumber].BaseDimension[3].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+--------  Alpha Safe  --------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x1 [ " + my_Bases[BaseNumber].AlphaSafe[0].ToString().PadLeft(4) + " ]    y1 [ " + my_Bases[BaseNumber].AlphaSafe[1].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x2 [ " + my_Bases[BaseNumber].AlphaSafe[2].ToString().PadLeft(4) + " ]    y2 [ " + my_Bases[BaseNumber].AlphaSafe[3].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+--Warp Point     -------------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x  [ " + my_Bases[BaseNumber].AlphaStartX.ToString().PadLeft(4) + " ]    y  [ " + my_Bases[BaseNumber].AlphaStartY.ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|--------  Bravo Safe  --------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x1 [ " + my_Bases[BaseNumber].BravoSafe[0].ToString().PadLeft(4) + " ]    y1 [ " + my_Bases[BaseNumber].BravoSafe[1].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x2 [ " + my_Bases[BaseNumber].BravoSafe[2].ToString().PadLeft(4) + " ]    y2 [ " + my_Bases[BaseNumber].BravoSafe[3].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+--Warp Point     -------------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x  [ " + my_Bases[BaseNumber].BravoStartX.ToString().PadLeft(4) + " ]    y  [ " + my_Bases[BaseNumber].BravoStartY.ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+------------------------------+").PadLeft(offset)));

            return reply;
        }

        public Queue<EventArgs> GetBaseManagerSettings(string PlayerName)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();

            reply.Enqueue(msg.pm(PlayerName, "Base Manager Settings -----------------------"));
            reply.Enqueue(msg.pm(PlayerName, ("Loading Mode").PadRight(35) + my_Mode.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Size Limit").PadRight(35) + my_SizeMode.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Shuffle Q Min").PadRight(35) + my_ShuffleModeQCount.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Bases Currently in Shuffle Q").PadRight(35) + my_ShuffleModeQ.Count.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Next Base in Shuffle Q").PadRight(35) + ("Base [" + my_Bases.IndexOf(my_LoadedBase) + "]").PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Round Robin Mode Count").PadRight(35) + my_RoundRobinCount.ToString().PadLeft(10)));

            return reply;
        }
    }
}
