//-----------------------------------------------------------------------
//
// NAME:        ArenaListPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Arena List Packet implementation.
//
// NOTES:       None.
//
// $History: ArenaListPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BattleCore.Events;

namespace BattleCore.Protocol
{
    internal class ArenaListPacket : IPacket
    {
        /// <summary>Arenas List Event Data</summary>
        private ArenaListEvent m_event = new ArenaListEvent();
        /// <summary>
        /// LVZ Toggle Event Property
        /// </summary>
        public ArenaListEvent Event
        {
            set { m_event = value; }
            get { return m_event; }
        }

        ///<summary>Reliable Packet Property</summary>
        public Boolean Reliable { get { return false; } }

        /// <summary>
        /// Packet Data Property
        /// </summary>
        public Byte[] Packet
        {
            set
            {
                // Create list to hold arenas
                System.Collections.Generic.Dictionary<String, UInt16> m_ArenasList = new Dictionary<string, ushort>();
                // Keeps track of next start position for Arena name
                int startIndex = 1;
                // Go through all bytes
                for (int i = 1; i < value.Length; i += 1)
                {
                    // Check to see if name is finished - null
                    if (value[i] == 0x00)
                    {
                        // Parse info
                        string name = Encoding.ASCII.GetString(value, startIndex, i - startIndex);
                        ushort c = (ushort)Math.Abs(BitConverter.ToInt16(value, i + 1));
                        // Add to list
                        m_ArenasList.Add(name, c);
                        // Skip over the next bytes that were included in arena count
                        i = i + 2;
                        // update index to next spot
                        startIndex = i + 1;
                    }
                }
                // Attach list to event
                m_event.ArenasList = m_ArenasList;
            }
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
