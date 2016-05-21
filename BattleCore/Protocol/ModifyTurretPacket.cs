//-----------------------------------------------------------------------
//
// NAME:        ModifyTurretPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Death Packet implementation.
//
// NOTES:       None.
//
// $History: ModifyTurretPacket.cs $
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
    /// ModifyTurretPacket object.  This packet is sent by the server
    /// to notify of players attaching or detaching.
    /// </summary>
    internal class ModifyTurretPacket : IPacket
    {
        /// <summary>Modify Turret Packet Event Data</summary>
        private ModifyTurretEvent m_event = new ModifyTurretEvent();

        ///<summary>Reliable Packet Property</summary>
        public Boolean Reliable { get { return false; } }

        /// <summary>
        /// Modify Turret Event Property
        /// </summary>
        public ModifyTurretEvent Event
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
                // Set the event data from the packet -- 0x0E s2c packet...notification of turret gunner de/attach
                m_event.TurretAttacherId = BitConverter.ToUInt16(value, 1);
                m_event.TurretHostId = BitConverter.ToUInt16(value, 3);
            }
            get
            {
                // Never going to actually send this packet

                // Create the packet memory stream
                MemoryStream packet = new MemoryStream(3);

                // Write the packet data to the memory stream -- 0x10 c2s packet...client attempting to link
                //packet.WriteByte(0x10);
                //packet.Write(BitConverter.GetBytes(m_event.TurretAttacherId), 0, 2);
                //packet.Write(BitConverter.GetBytes(m_event.TurretHostId), 0, 2);

                // return the packet data
                return packet.GetBuffer(); ;
            }
        }
    }
}
