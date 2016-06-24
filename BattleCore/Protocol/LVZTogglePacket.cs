﻿//-----------------------------------------------------------------------
//
// NAME:        LVZTogglePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Death Packet implementation.
//
// NOTES:       None.
//
// $History: LVZTogglePacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BattleCore.Events;

// Namespace declaration
namespace BattleCore.Protocol
{
    /// <summary>
    /// LVZTogglePacket object.  This object is used to send/receive LVZ
    /// Toggle packets from the server.
    /// </summary>
    internal class LVZTogglePacket : IPacket
    {
        /// <summary>LVZ Toggle Packet Event Data</summary>
        private LVZToggleEvent m_event = new LVZToggleEvent();

        ///<summary>Reliable Packet Property</summary>
        public Boolean Reliable { get { return false; } }

        /// <summary>
        /// LVZ Toggle Event Property
        /// </summary>
        public LVZToggleEvent Event
        {
            set { m_event = value; }
            get { return m_event; }
        }

        /// <summary>
        /// Packet Data Property
        /// </summary>
        public Byte[] Packet
        {
            set
            {
                m_event = new LVZToggleEvent();
                m_event.LVZObjects = new Dictionary<ushort, bool>();

                bool is_delimiter = true;
                // (65,535 - lvz id) is delimiter when the lvz is being turned off - ganna skip every other toggle
                for (Int32 i = 1; i < value.Length; i += 2)
                {
                    bool state = true;

                    byte[] nexttwo = new byte[2] { value[i], value[i + 1] };
                    ushort result = (ushort)BitConverter.ToInt16(nexttwo, 0);
                    
                    if (result >= 32768)
                    {
                        state = false;
                        result = (ushort)(result - 32768);
                    }

                    //if (state == false && !is_delimiter || state == true)
                    //{
                    //    m_event.LVZObjects[result] = state;
                    //    if (!is_delimiter)is_delimiter = true;
                    //}
                    //else
                    //{
                    //    is_delimiter = false;
                    //}

                    m_event.LVZObjects[result] = state;
                }
            }
            get
            {
                // Offset  Length  Description
                // 0       1       Encapsulating Type Byte (0x0A)
                // 1       2       Player ID (-1 for all players)
                // 3       1       Real Type Byte (0x35)
                // 4       2*      Object Toggle Data
                //
                // WARNING: http://www.twcore.org/wiki/SubspaceProtocol#a0x35-LVZObjectToggle appears to describe wrong structure for Object Toggle Data
                // Real structure is as follows
                //
                // Bit Field   Bit Length   Description
                // 0           1            New state of LVZ object...0 denotes visible, 1 denotes hidden
                // 1           15           LVZ object ID
                //
                // Therefore, 0000 0000 0000 1111 shows LVZ object 15, and 1000 0000 0000 1111 hides LVZ object 15

                // Create the packet memory stream
                int size = 4 + 2 * m_event.LVZObjects.Count;
                MemoryStream packet = new MemoryStream(size);

                // Write the packet data to the memory stream
                packet.WriteByte(0x0A);
                packet.Write(BitConverter.GetBytes(m_event.TargetPlayerId), 0, 2);
                packet.WriteByte(0x35);

                foreach (var item in m_event.LVZObjects)
                {
                    UInt16 data = item.Key; // lvz ID occupies the right-most bits
                    if (!item.Value)
                        data += 32768;  // 1000 0000 0000 0000  hide object

                    packet.Write(BitConverter.GetBytes(data), 0, 2);
                }

                // return the packet data
                return packet.GetBuffer();
            }
        }
    }
}
