//-----------------------------------------------------------------------
//
// NAME:        SecurityRequestPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Security Request Packet implementation.
//
// NOTES:       None.
//
// $History: SecurityRequestPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// SecurityRequestPacket object.  This object is received to update the
   /// prize and door seeds and request a security response.
   /// </summary>
   internal class SecurityRequestPacket : IPacket
   {
      private UInt32 m_nPrizeSeed;   // Prize seed value
      private UInt32 m_nDoorSeed;    // Door Seed value
      private UInt32 m_nServerTime;  // Current server time
      private UInt32 m_nChecksumKey; // Checksum generation key

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Prize Seed Property
      /// </summary>
      public UInt32 PrizeSeed
      {
         get { return m_nPrizeSeed; }
      }

      /// <summary>
      /// Door Seed Property
      /// </summary>
      public UInt32 DoorSeed
      {
         get { return m_nDoorSeed; }
      }

      /// <summary>
      /// Server Time Property
      /// </summary>
      public UInt32 ServerTime
      {
         get { return m_nServerTime; }
      }

      /// <summary>
      /// Checksum Key Property
      /// </summary>
      public UInt32 ChecksumKey
      {
         get { return m_nChecksumKey; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Set the event data from the packet
            m_nPrizeSeed = BitConverter.ToUInt32 (value, 1);
            m_nDoorSeed = BitConverter.ToUInt32 (value, 5);
            m_nServerTime = BitConverter.ToUInt32 (value, 9);
            m_nChecksumKey = BitConverter.ToUInt32 (value, 13);
         }

         // Return an empty array
         get { return new Byte[1]; }
      }

   }
}
