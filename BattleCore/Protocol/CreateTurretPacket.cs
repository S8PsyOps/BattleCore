//-----------------------------------------------------------------------
//
// NAME:        CreateTurretPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Death Packet implementation.
//
// NOTES:       None.
//
// $History: CreateTurretPacket.cs $
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
    /// CreateTurretPacket object.  This packet is sent to the server by a
    /// turret gunner to attach to somebody else.
    /// </summary>
    internal class CreateTurretPacket : IPacket
    {
        /// <summary>Create Turret Packet Event Data</summary>
        private CreateTurretEvent m_event = new CreateTurretEvent();

        ///<summary>Reliable Packet Property</summary>
        public Boolean Reliable { get { return false; } }

        /// <summary>
        /// Create Turret Event Property
        /// </summary>
        public CreateTurretEvent Event
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
                // Never going to receive this packet from the server
                //m_event.TurretHostId = BitConverter.ToUInt16(value, 1);
            }
            get
            {
                // Create the packet memory stream
                MemoryStream packet = new MemoryStream(3);

                // Write the packet data to the memory stream -- 0x10 c2s packet...client attempting to link
                packet.WriteByte(0x10);
                packet.Write(BitConverter.GetBytes(m_event.TurretHostId), 0, 2);

                // return the packet data
                return packet.GetBuffer(); ;
            }
        }
    }
}
