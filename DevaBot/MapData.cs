using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevaBot
{
    class MapData
    {
        // Holds tileID using a multi dimensional array m_MapGrid[xCoor][yCoor]
        private byte[][] m_MapTileIDs;
        public byte[][] TileIDs
        {
            get { return m_MapTileIDs; }
        }

        // Holds tileType
        private byte[][] m_MapTileType;
        public byte[][] TileTypes
        {
            get { return m_MapTileType; }
        }
        // Saves a linear array of tile info into a multi dimensional array
        public MapData(Byte[] mapData)
        {
            // temp array to make sure we dont double check tiles > 1x1
            byte[][] m_MapTileIDs_temp;

            // Create arrays and initialize them
            m_MapTileIDs_temp = new byte[1024][];
            m_MapTileIDs = new byte[1024][];
            m_MapTileType = new byte[1024][];
            for (int i = 0; i < 1024; i += 1)
            {
                m_MapTileIDs_temp[i] = new byte[1024];

                m_MapTileIDs[i] = new byte[1024];
                m_MapTileType[i] = new byte[1024];

                for (int j = 0; j < 1024; j += 1)
                {
                    m_MapTileIDs_temp[i][j] = 0;

                    m_MapTileIDs[i][j] = 0;
                    m_MapTileType[i][j] = 1;
                }
            }

            // Convert from bit info into our array
            int remainingBytes = mapData.Length;
            for (Int32 i = 0; i < mapData.Length; i += 4)
            {
                // Gathering the next 4 bytes and converting to Int32
                byte[] nextFour = new byte[4] { mapData[i], mapData[i + 1], mapData[i + 2], mapData[i + 3] };
                int mi = BitConverter.ToInt32(nextFour, 0);

                int tile = mi >> 24 & 0x00ff;
                int y = (mi >> 12) & 0x03FF;
                int x = mi & 0x03FF;

                // Sort Type here
                byte type = 1;

                if (tile == 0) type = 1;
                else if (tile < 162) type = 0;
                else if (tile < 170) type = 2;
                else if (tile < 192) type = 1;
                else if (tile < 220) type = 0;
                else if (tile == 220) type = 3;
                else if (tile < 241) type = 0;
                else if (tile == 241) type = 1;
                else if (tile == 242) type = 3;
                else if (tile < 252) type = 0;
                else if (tile == 252) type = 3;

                m_MapTileIDs[x][y] = (byte)tile;
                m_MapTileType[x][y] = type;

                // Fill main array with missing tile info- see m_fillTile description
                switch (tile)
                {
                    case 217:
                        m_fillTile(x, y, 217, 2, type);
                        break;
                    case 219:
                        m_fillTile(x, y, 219, 6, type);
                        break;
                    case 220:
                        m_fillTile(x, y, 220, 5, type);
                        break;
                }
            }
        }

        /*  The map info saved in a lvl file stores a tile bigger than 1x1
         *  by only recording the top left tile coord. This will fill in the
         *  empty tile locations that weren't recorded on the initial save. 
         *  Tiles: Station, Wormhole, and Large Ateroid       */
        private void m_fillTile(int x, int y, byte ID, int places, byte type)
        {
            for (int i = 1; i < places; i++)
            {
                for (int j = 1; j < places; j++)
                {
                    m_MapTileIDs[x + i][y + j] = ID;
                    m_MapTileType[x + i][y + j] = type;
                }
            }
        }
    }
}
