using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

/*
 * Need to make a list of bases being used. The hope is to allow multiple base games to be played.
 * The problem will come in to play at the end of a shuffle. If the the last base picked was from the bottom of the "deck"
 * then the new game created could possibly pick the same base on the first base after reshuffle
 * this will also be useful when we play multiple types of games. Like playing baserace when baseduel is going on.
 * 
 * rep range = 400 pixels
 */ 

namespace Devastation
{
    public class BaseManager
    {

        private ShortChat msg;                  // Custom chat module
        private MyGame psyGame;
        private Random ran;                     // Random num generator
        private List<Base> m_Bases;             // Main list of bases
        private Base m_Lobby;                   // Safe area saved as a base
        private BaseMode m_Mode;                // Way to sort bases
        private BaseSize m_SizeMode;            // Limit bases loaded, by size
        private Queue<int> m_ShuffleModeQ;      // Stores the shuffle
        private int m_ShuffleModeQCount;        // Min amount of bases to keep on Q
        private int m_RoundRobinCount;          // where round robin picks up at

        private List<int> m_BasesInUse;

        public BaseManager(Byte[] mapData, ShortChat msg, MyGame psyGame)
        {
            // Load all vars
            m_Lobby = new Base();
            m_Bases = new List<Base>();
            this.msg = msg;
            this.psyGame = psyGame;
            ran = new Random();
            m_Mode = BaseMode.Shuffle;
            m_SizeMode = BaseSize.Off;
            m_ShuffleModeQ = new Queue<int>();
            m_ShuffleModeQCount = 10;
            m_RoundRobinCount = 0;
            m_BasesInUse = new List<int>();

            // Byte array tunrned into arrays i can work with
            MapData my_MapInfo = new MapData(mapData);

            // Map scanner. Loads/Configures/Saves Bases
            BaseLoader my_BaseLoader = new BaseLoader(my_MapInfo.TileIDs, my_MapInfo.TileTypes);
            my_BaseLoader.LoadBasesFromMap(m_Bases, m_Lobby);

            // Load next base from default random
            ReShuffleQ(true);
            //getNextBase();
        }

        /// <summary>
        /// Center spawn area. The big safe in center for Deva.
        /// </summary>
        public Base Lobby
        { get { return m_Lobby; } }

        /// <summary>
        /// Main List of bases.
        /// </summary>
        public List<Base> Bases
        { get { return m_Bases; } }

        /// <summary>
        ///<para>Mode for choosing what base to chose next</para>
        ///<para>-------------------------------------------</para>
        ///<para> Shuffle   :   Bases get shuffled and put into a queue, once every base has been played they get reshuffled again</para>       
        ///<para> Random    :   Every base is chosen at random. So it is possible to get the same base twice</para>             
        ///<para> RoundRobin:   Play from base 1 to last one and just restart at end</para>
        ///<para> Custom    :   Not used yet, but maybe for custom base loading like player chosen/voted on</para>
        /// </summary>
        public BaseMode Mode
        {
            get { return m_Mode; }
            set { m_Mode = value; }
        }

        /// <summary>
        /// <para>If you want to limit the next base loaded to a size</para>
        /// <para>Sizes are : BaseSize.Small BaseSize.Medium BaseSize.Large</para>
        /// <para> ** CALL  LoadNextBase(); after so that the new setting takes effect **</para>
        /// </summary>
        public BaseSize SizeLimit
        {
            get { return m_SizeMode; }
            set { m_SizeMode = value; }
        }

        /// <summary>
        /// <para>Load a specific base.</para>
        /// <para>Base will not load if you choose number higher than available.</para>
        /// </summary>
        /// <param name="BaseToLoad"></param>
        public Base getBaseNumber(int BaseToLoad)
        {
            if (BaseToLoad < m_Bases.Count && !m_BasesInUse.Contains(BaseToLoad))
            {
                m_BasesInUse.Add(BaseToLoad);
                return m_Bases[BaseToLoad];
            }
            return null;
        }

        // Collision check between 2 bases to see if they are within rep distance
        private bool inRepProximity(Base one, Base two)
        {
            ushort oneWidth = (ushort)(one.BaseDimension[2] - one.BaseDimension[0]);
            ushort oneHeight = (ushort)(one.BaseDimension[3] - one.BaseDimension[1]);
            ushort oneCenterX = (ushort)(one.BaseDimension[0] + oneWidth);
            ushort oneCenterY = (ushort)(one.BaseDimension[1] + oneHeight);

            ushort twoWidth = (ushort)(two.BaseDimension[2] - two.BaseDimension[0]);
            ushort twoHeight = (ushort)(two.BaseDimension[3] - two.BaseDimension[1]);
            ushort twoCenterX = (ushort)(two.BaseDimension[0] + twoWidth);
            ushort twoCenterY = (ushort)(two.BaseDimension[1] + twoHeight);
            
            // the lesser value is the distance of the square's width and that of the lenght of the
            // other square
            return Math.Abs(twoCenterX - oneCenterX) < (twoWidth + 200) / 2 + (oneWidth + 200) / 2
                    && Math.Abs(twoCenterY - oneCenterY) < (twoHeight + 200) / 2 + (oneWidth + 200) / 2;
        }

        public Base getNextBase()
        {
            return this.getNextBase("~ unknown ~");
        }
        /// <summary>
        /// <para>Load the next available Base.</para>
        /// <para>BaseMode is what determins what base to load next.</para>
        /// </summary>
        /// <returns>Next base in queue</returns>
        public Base getNextBase(string Requester)
        {
            bool reload = true;
            int newBase = 0;
            while (reload)
            {
                if (m_SizeMode == BaseSize.Off)
                    reload = false;
                
                switch (m_Mode)
                {
                    case BaseMode.Shuffle:
                        newBase = m_ShuffleModeQ.Dequeue();
                        // keep our q at a minimum of [m_ModeZeroMinCount] - add to shuffle if we fall below
                        if (m_ShuffleModeQ.Count < m_ShuffleModeQCount)
                            ReShuffleQ(false);
                        break;
                    case BaseMode.RoundRobin:
                        m_RoundRobinCount = (m_RoundRobinCount + 1) >= m_Bases.Count ? 0 : m_RoundRobinCount + 1;
                        newBase = m_RoundRobinCount;
                        break;
                    case BaseMode.Random:
                        newBase = ran.Next(0, m_Bases.Count);
                        break;
                }

                // make sure base isnt in use
                if (!m_BasesInUse.Contains(newBase))
                    reload = false;

                // If the base is set to a certain size
                if (m_SizeMode != BaseSize.Off && m_SizeMode == m_Bases[newBase].Size)
                    reload = false;

                // Rep range is 400 pixels - run a prox check to make sure we arent releasing a base within rep prox distance
                for (int i = 0; i < m_BasesInUse.Count; i++)
                {
                    if (inRepProximity(m_Bases[newBase], m_Bases[m_BasesInUse[i]]))
                    {
                        reload = true;
                    }
                }

                if (reload == false)
                {
                    // add base to in-use list
                    m_BasesInUse.Add(newBase);
                }
            }
            psyGame.Send(msg.debugChan("[ BaseManager ] - [ "+Requester+" ] has checked out Base[ "+newBase+" ]. Total bases checked out:[ "+this.m_BasesInUse.Count+" ]"));
            return m_Bases[newBase];
        }

        public void ReleaseBase(Base baseToRelease)
        {
            this.ReleaseBase(baseToRelease, "~ unknown ~");
        }
        /// <summary>
        /// <para></para>Releases the base to public use.</para>
        /// <para>You MUST release base after use or no one will be able to use it after.</para>
        /// </summary>
        /// <param name="baseToRelease"></param>
        public void ReleaseBase(Base baseToRelease , string Requester)
        {
            if (baseToRelease == null)
            {
                psyGame.Send(msg.debugChan("[ BaseManager ] - [ " + Requester + " ] has attempted to release a null Base. Total bases checked out:[ " + this.m_BasesInUse.Count + " ]"));
                return;
            }
            int relBase = m_Bases.IndexOf(baseToRelease);
            psyGame.Send(msg.debugChan("[ BaseManager ] - [ " + Requester + " ] has released Base[ " + relBase + " ]. Total bases checked out:[ " + this.m_BasesInUse.Count + " ]"));
            m_BasesInUse.Remove(relBase);
        }

        // Shuffle bases for ShuffleMode - Option to clear old, if not we add to existing old shuffle
        private void ReShuffleQ(bool ClearOld)
        {
            List<int> baseIndexes = new List<int>();

            // add all indexes of all bases into temp list
            for (int i = 0; i < m_Bases.Count; i += 1)
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
            if (ClearOld) m_ShuffleModeQ = new Queue<int>();

            // Dump the new shuffle into our master queue
            for (int i = 0; i < m_Bases.Count; i += 1)
                m_ShuffleModeQ.Enqueue(baseIndexes[i]);
        }

        public Queue<EventArgs> getBaseInfo(string PlayerName, Base lBase)
        {
            return this.getBaseInfo(PlayerName, m_Bases.IndexOf(lBase));
        }
        public Queue<EventArgs> getBaseInfo(string PlayerName, int BaseNumber)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();

            int offset = 38;

            reply.Enqueue(msg.pm(PlayerName, "Base Information ----------------------------"));      //BaseNumber
            reply.Enqueue(msg.pm(PlayerName, "Base Name".PadRight(20) + m_Bases[BaseNumber].BaseName.PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Base Number".PadRight(20) + BaseNumber.ToString().PadLeft(2, '0').PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Tile Count".PadRight(20) + m_Bases[BaseNumber].TileCount.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, "Base Size".PadRight(20) + m_Bases[BaseNumber].Size.ToString().PadLeft(25)));
            reply.Enqueue(msg.pm(PlayerName, ("+--------  Dimension   --------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x1 [ " + m_Bases[BaseNumber].BaseDimension[0].ToString().PadLeft(4) + " ]    y1 [ " + m_Bases[BaseNumber].BaseDimension[1].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x2 [ " + m_Bases[BaseNumber].BaseDimension[2].ToString().PadLeft(4) + " ]    y2 [ " + m_Bases[BaseNumber].BaseDimension[3].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+--------  Alpha Safe  --------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x1 [ " + m_Bases[BaseNumber].AlphaSafe[0].ToString().PadLeft(4) + " ]    y1 [ " + m_Bases[BaseNumber].AlphaSafe[1].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x2 [ " + m_Bases[BaseNumber].AlphaSafe[2].ToString().PadLeft(4) + " ]    y2 [ " + m_Bases[BaseNumber].AlphaSafe[3].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+--Warp Point     -------------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x  [ " + m_Bases[BaseNumber].AlphaStartX.ToString().PadLeft(4) + " ]    y  [ " + m_Bases[BaseNumber].AlphaStartY.ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|--------  Bravo Safe  --------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x1 [ " + m_Bases[BaseNumber].BravoSafe[0].ToString().PadLeft(4) + " ]    y1 [ " + m_Bases[BaseNumber].BravoSafe[1].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x2 [ " + m_Bases[BaseNumber].BravoSafe[2].ToString().PadLeft(4) + " ]    y2 [ " + m_Bases[BaseNumber].BravoSafe[3].ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+--Warp Point     -------------+").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("|  x  [ " + m_Bases[BaseNumber].BravoStartX.ToString().PadLeft(4) + " ]    y  [ " + m_Bases[BaseNumber].BravoStartY.ToString().PadLeft(4) + " ]  |").PadLeft(offset)));
            reply.Enqueue(msg.pm(PlayerName, ("+------------------------------+").PadLeft(offset)));

            return reply;
        }

        public Queue<EventArgs> GetBaseManagerSettings(string PlayerName)
        {
            Queue<EventArgs> reply = new Queue<EventArgs>();

            reply.Enqueue(msg.pm(PlayerName, "Base Manager Settings -----------------------"));
            reply.Enqueue(msg.pm(PlayerName, ("Loading Mode").PadRight(35) + m_Mode.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Size Limit").PadRight(35) + m_SizeMode.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Shuffle Q Min").PadRight(35) + m_ShuffleModeQCount.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Bases Currently in Shuffle Q").PadRight(35) + m_ShuffleModeQ.Count.ToString().PadLeft(10)));
            reply.Enqueue(msg.pm(PlayerName, ("Round Robin Mode Count").PadRight(35) + m_RoundRobinCount.ToString().PadLeft(10)));

            return reply;
        }
    }
}
