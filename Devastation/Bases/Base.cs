using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation
{
    public class Base
    {
        private int m_BaseNumber;
        private String m_BaseName = "~none~";
        private BaseSize m_Size = BaseSize.Small;
        private int m_TileCount = 0;

        private ushort[] m_BaseDimension = new ushort[4];

        private ushort[] m_AlphaSafe = new ushort[4];
        private ushort m_AlphaStartX;
        private ushort m_AlphaStartY;

        private ushort[] m_BravoSafe = new ushort[4];
        private ushort m_BravoStartX;
        private ushort m_BravoStartY;

        /// <summary>
        /// Index number of base in master list
        /// </summary>
        public int Number
        {
            get { return m_BaseNumber; }
            set { m_BaseNumber = value; }
        }

        /// <summary>
        /// The name of the base, if you want to name bases.
        /// </summary>
        public String BaseName
        {
            get { return m_BaseName; }
            set { m_BaseName = value; }
        }

        /// <summary>
        /// What size the base is.
        /// </summary>
        public BaseSize Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        /// <summary>
        /// <para>How many tiles are inside of base.</para>
        /// <para>This is mainly used for base loader, it uses this to determine how big a base is.</para>
        /// </summary>
        public int TileCount
        {
            get { return m_TileCount; }
            set { m_TileCount = value; }
        }

        /// <summary>
        /// <para>Alpha Safe coords stored in a ushort array.</para>
        /// <para>AlphaSafe[0] = x coord from top left tile. (x1)</para>
        /// <para>AlphaSafe[1] = y coord from top left tile. (y1)</para>
        /// <para>AlphaSafe[2] = x coord from bottom right tile. (x2)</para>
        /// <para>AlphaSafe[3] = y coord from bottom right tile. (y2)</para>
        /// <para>THESE COORDS ARE SAVE AS PIXEL COORDINATES</para>
        /// <para>Use these for collision checks agains position packets from players as they are in pixels too</para>
        /// </summary>
        public ushort[] AlphaSafe
        {
            get { return m_AlphaSafe; }
            set { m_AlphaSafe = value; }
        }

        /// <summary>
        /// AlphaStartX and AlphaStartY is the center point inside AlphaSafe.
        /// <para>THESE COORDS ARE SAVE AS TILE COORDINATES</para>
        /// <para>This is used so you can warp a player inside the safe.</para>
        /// <para>usage: "*warpto " + AlphaStartX + " " + AlphaStartY</para>
        /// </summary>
        public ushort AlphaStartX
        {
            get { return m_AlphaStartX; }
            set { m_AlphaStartX = value; }
        }

        /// <summary>
        /// AlphaStartX and AlphaStartY is the center point inside AlphaSafe.
        /// <para>THESE COORDS ARE SAVE AS TILE COORDINATES</para>
        /// <para>This is used so you can warp a player inside the safe.</para>
        /// <para>usage: "*warpto " + AlphaStartX + " " + AlphaStartY</para>
        /// </summary>
        public ushort AlphaStartY
        {
            get { return m_AlphaStartY; }
            set { m_AlphaStartY = value; }
        }

        /// <summary>
        /// <para>Bravo Safe coords stored in a ushort array.</para>
        /// <para>BravoSafe[0] = x coord from top left tile. (x1)</para>
        /// <para>BravoSafe[1] = y coord from top left tile. (y1)</para>
        /// <para>BravoSafe[2] = x coord from bottom right tile. (x2)</para>
        /// <para>BravoSafe[3] = y coord from bottom right tile. (y2)</para>
        /// <para>THESE COORDS ARE SAVE AS PIXEL COORDINATES</para>
        /// <para>Use these for collision checks agains position packets from players as they are in pixels too</para>
        /// </summary>
        public ushort[] BravoSafe
        {
            get { return m_BravoSafe; }
            set { m_BravoSafe = value; }
        }

        /// <summary>
        /// BravoStartX and BravoStartY is the center point inside BravoSafe.
        /// <para>THESE COORDS ARE SAVE AS TILE COORDINATES</para>
        /// <para>This is used so you can warp a player inside the safe.</para>
        /// <para>usage: "*warpto " + BravoStartX + " " + BravoStartY</para>
        /// </summary>
        public ushort BravoStartX
        {
            get { return m_BravoStartX; }
            set { m_BravoStartX = value; }
        }

        /// <summary>
        /// BravoStartX and BravoStartY is the center point inside BravoSafe.
        /// <para>THESE COORDS ARE SAVE AS TILE COORDINATES</para>
        /// <para>This is used so you can warp a player inside the safe.</para>
        /// <para>usage: "*warpto " + BravoStartX + " " + BravoStartY</para>
        /// </summary>
        public ushort BravoStartY
        {
            get { return m_BravoStartY; }
            set { m_BravoStartY = value; }
        }

        /// <summary>
        /// <para>This is the dimension of the base.</para>
        /// <para>BaseDimension[0] = x coord from top left tile. (x1)</para>
        /// <para>BaseDimension[1] = y coord from top left tile. (y1)</para>
        /// <para>BaseDimension[2] = x coord from bottom right tile. (x2)</para>
        /// <para>BaseDimension[3] = y coord from bottom right tile. (y2)</para>
        /// <para>If the base is shaped irregular see below:</para>
        /// <para>min X coord and min Y coord is recorded as top left tile.</para>
        /// <para>max X coord and max Y coord is recorded as bottom right tile.</para>
        /// <para>THESE COORDS ARE SAVE AS PIXEL COORDINATES</para>
        /// <para>Use these for collision checks agains position packets from players as they are in pixels too</para>
        /// </summary>
        public ushort[] BaseDimension
        {
            get { return m_BaseDimension; }
            set { m_BaseDimension = value; }
        }
    }
}
