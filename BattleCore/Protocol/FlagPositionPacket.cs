//-----------------------------------------------------------------------
//
// NAME:        FlagPositionPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Flag Position Packet implementation.
//
// NOTES:       None.
//
// $History: FlagPositionPacket.cs $
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
   /// FlagPositionPacket object.  This object is used to receive 
   /// flag positions from the server.
   /// </summary>
   internal class FlagPositionPacket : IPacket
   {
      /// <summary>Flag Position Packet Event Data</summary>
      private FlagPositionEvent m_event = new FlagPositionEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Flag Position Event Property
      /// </summary>
      public FlagPositionEvent Event
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
            m_event.FlagId = BitConverter.ToUInt16 (value, 1);
            m_event.MapPositionX = BitConverter.ToUInt16 (value, 3);
            m_event.MapPositionY = BitConverter.ToUInt16 (value, 5);
            m_event.Frequency = BitConverter.ToUInt16 (value, 7);
         }

         // Return an empty array
         get { return new Byte[1]; }
      }
   }
}
