//-----------------------------------------------------------------------
//
// NAME:        FlagDropPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Flag Drop Packet implementation.
//
// NOTES:       None.
//
// $History: FlagDropPacket.cs $
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
   /// FlagDropPacket object.  This object is used to receive 
   /// flag drop notifications from the server.
   /// </summary>
   internal class FlagDropPacket : IPacket
   {
      /// <summary>Flag Drop Packet Event Data</summary>
      private FlagDropEvent m_event = new FlagDropEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Flag Drop Event Property
      /// </summary>
      public FlagDropEvent Event
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
            m_event.PlayerId = BitConverter.ToUInt16 (value, 1);
         }

         // Return an empty array
         get { return new Byte[1]; }
      }


   }
}
