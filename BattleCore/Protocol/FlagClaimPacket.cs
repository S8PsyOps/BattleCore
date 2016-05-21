//-----------------------------------------------------------------------
//
// NAME:        FlagClaimPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Flag Claim Packet implementation.
//
// NOTES:       None.
//
// $History: FlagClaimPacket.cs $
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
   /// FlagClaimPacket object.  This object is used to receive player
   /// flag claim packets from the server.
   /// </summary>
   internal class FlagClaimPacket : IPacket
   {
      /// <summary>Flag Claim Packet Event Data</summary>
      private FlagClaimEvent m_event = new FlagClaimEvent ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Flag Claim Event Property
      /// </summary>
      public FlagClaimEvent Event
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
            m_event.PlayerId = BitConverter.ToUInt16 (value, 3);
         }

         // Return an empty array
         get { return new Byte[1]; }
      }
   }
}
