//-----------------------------------------------------------------------
//
// NAME:        PlayerUpdatePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Update Packet implementation.
//
// NOTES:       None.
//
// $History: PlayerUpdatePacket.cs $
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
   /// PlayerUpdatePacket Object.  This packet is received when the 
   /// player ship and frequency information is updated is set.
   /// </summary>
   internal class PlayerUpdatePacket : IPacket
   {
      private ShipChangeEvent m_shipEvent = new ShipChangeEvent();
      private TeamChangeEvent m_teamEvent = new TeamChangeEvent();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Ship Change Event Property
      /// </summary>
      public ShipChangeEvent ShipEvent 
      {
         set { m_shipEvent = value; }
         get { return m_shipEvent; }
      }

      /// <summary>
      /// Team Change Event Property
      /// </summary>
      public TeamChangeEvent TeamEvent
      {
         set { m_teamEvent = value; }
         get { return m_teamEvent; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            if (value[0] == 0x0D)
            {
               m_teamEvent.PlayerId = BitConverter.ToUInt16 (value, 1);
               m_teamEvent.Frequency = BitConverter.ToUInt16 (value, 3);
               m_shipEvent.ShipType = (ShipTypes)value[5];
            }
            else if (value[0] == 0x1D)
            {
               m_shipEvent.ShipType = (ShipTypes)value[1];
               m_teamEvent.PlayerId = BitConverter.ToUInt16 (value, 2);
               m_teamEvent.Frequency = BitConverter.ToUInt16 (value, 4);
            }

            // Set the ship event player identifier
            m_shipEvent.PlayerId = m_teamEvent.PlayerId;
         }
         get
         {
            // return a new Byte
            return new Byte[1];
         }
      }
   }
}
