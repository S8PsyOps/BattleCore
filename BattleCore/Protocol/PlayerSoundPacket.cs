//-----------------------------------------------------------------------
//
// NAME:        PlayerSoundPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Sound Packet implementation.
//
// NOTES:       None.
//
// $History: PlayerSoundPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using BattleCore.Events;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// PlayerSoundPacket object.  This object is used to player sounds.
   /// </summary>
   internal class PlayerSoundPacket : IPacket
   {
      /// <summary>Player Sound Packet Event Data</summary>
      PlayerSoundEvent m_event = new PlayerSoundEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Player Sound Event Property
      /// </summary>
      public PlayerSoundEvent Event
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
            // Set the event data from the packet
            m_event.PlayerId = BitConverter.ToUInt16 (value, 2);
            Array.Copy(value, 3, m_event.SoundFile, 0, value.Length - 3);
         }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (m_event.SoundFile.Length + 3);

            // Write the packet data to the memory stream
            packet.WriteByte (0x0C);
            packet.Write (BitConverter.GetBytes (m_event.PlayerId), 0, 2);
            packet.Write (m_event.SoundFile, 0, m_event.SoundFile.Length);

            // return the packet data
            return packet.GetBuffer ();  
         }
      }   
   }
}
