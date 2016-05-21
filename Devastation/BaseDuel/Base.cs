using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devastation.BaseDuel
{
    class Base
    {
        private int m_BaseNumber;

        private String m_BaseName = "~none~";
        public String BaseName
        {
            get { return m_BaseName; }
        }

        private BaseSize m_Size = BaseSize.Small;
        public BaseSize Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        private int m_TileCount = 0;
        public int TileCount
        {
            get { return m_TileCount; }
            set { m_TileCount = value; }
        }

        private ushort[] m_AlphaSafe = new ushort[4];
        public ushort[] AlphaSafe
        {
            get { return m_AlphaSafe; }
            set { m_AlphaSafe = value; }
        }

        private ushort m_AlphaStartX;
        public ushort AlphaStartX
        {
            get { return m_AlphaStartX; }
            set { m_AlphaStartX = value; }
        }
        private ushort m_AlphaStartY;
        public ushort AlphaStartY
        {
            get { return m_AlphaStartY; }
            set { m_AlphaStartY = value; }
        }

        private ushort[] m_BravoSafe = new ushort[4];
        public ushort[] BravoSafe
        {
            get { return m_BravoSafe; }
            set { m_BravoSafe = value; }
        }

        private ushort m_BravoStartX;
        public ushort BravoStartX
        {
            get { return m_BravoStartX; }
            set { m_BravoStartX = value; }
        }

        private ushort m_BravoStartY;
        public ushort BravoStartY
        {
            get { return m_BravoStartY; }
            set { m_BravoStartY = value; }
        }

        private ushort[] m_BaseDimension = new ushort[4];
        public ushort[] BaseDimension
        {
            get { return m_BaseDimension; }
            set { m_BaseDimension = value; }
        }
    }
}
