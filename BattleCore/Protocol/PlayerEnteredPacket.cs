//-----------------------------------------------------------------------
//
// NAME:        PlayerEnteredPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Entered Packet implementation.
//
// NOTES:       None.
//
// $History: PlayerEnteredPacket.cs $
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
   /// PlayerEnteredPacket object.  This object is used to receive player
   /// enter packets from the server.
   /// </summary>
   internal class PlayerEnteredPacket : IPacket
   {
      /// <summary>Player Enter Packet Event Data</summary>
      private PlayerEnteredEvent m_event = new PlayerEnteredEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Player Entered Event Property
      /// </summary>
      public PlayerEnteredEvent Event
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
            m_event.PlayerName = (new ASCIIEncoding ()).GetString (value, 3, 20);
            m_event.SquadName  = (new ASCIIEncoding ()).GetString (value, 23, 20);
            m_event.ShipType = (ShipTypes)value[1];
            m_event.AcceptsAudio = (value[2] == 1);
            m_event.KillPoints = BitConverter.ToUInt32 (value, 43);
            m_event.FlagPoints = BitConverter.ToUInt32 (value, 47);
            m_event.PlayerId = BitConverter.ToUInt16 (value, 51);
            m_event.Frequency = BitConverter.ToUInt16 (value, 53);
            m_event.Wins = BitConverter.ToUInt16 (value, 55);
            m_event.Losses = BitConverter.ToUInt16 (value, 57);
            m_event.TurretPlayerId = BitConverter.ToUInt16 (value, 59);
            m_event.FlagsCarried = BitConverter.ToUInt16 (value, 61);
            m_event.HasKOTH = (value[63] == 1);
         }
         get
         {
            // return a new Byte
            return new Byte[1];
         }
      }
   }
}
