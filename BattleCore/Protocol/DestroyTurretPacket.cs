//-----------------------------------------------------------------------
//
// NAME:        DestroyTurretPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Death Packet implementation.
//
// NOTES:       None.
//
// $History: DestroyTurretPacket.cs $
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
    /// DestroyTurretPacket object.  This object is used to send or receive
    /// packets that signify a turret driver detaching from his gunners.
    /// </summary>
    internal class DestroyTurretPacket : IPacket
    {
        /// <summary>Create Turret Packet Event Data</summary>
        private DestroyTurretEvent m_event = new DestroyTurretEvent();

        ///<summary>Reliable Packet Property</summary>
        public Boolean Reliable { get { return false; } }

        /// <summary>
        /// Create Turret Event Property
        /// </summary>
        public DestroyTurretEvent Event
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
                // Set the event data from the packet -- 0x15 s2c packet...turret host detach
                m_event.TurretHostId = BitConverter.ToUInt16(value, 1);
            }
            get
            {
                // Create the packet memory stream
                MemoryStream packet = new MemoryStream(3);
                
                // Write the packet data to the memory stream -- needs to be hardcoded for some reason
                packet.WriteByte(0x10);
                packet.WriteByte(0xFF);
                packet.WriteByte(0xFF);
                //packet.Write(BitConverter.GetBytes(m_event.TurretHostId), 0, 2);
                
                // return the packet data
                return packet.GetBuffer(); ;          
            }
        }
    }
}
