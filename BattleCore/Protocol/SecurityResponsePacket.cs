//-----------------------------------------------------------------------
//
// NAME:        SecurityResponsePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Security Response Packet implementation.
//
// NOTES:       None.
//
// $History: SecurityResponsePacket.cs $
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
   /// SecurityResponsePacket object.  This object is sent is response 
   /// to the security request packet.
   /// </summary>
   internal class SecurityResponsePacket : IPacket
   {
      private UInt32 m_nExeChecksum;       // Executable Checksum Value
      private UInt32 m_nParameterChecksum; // Parameter Checksum Value
      private UInt32 m_nLevelChecksum;     // Level Checksum Value
      private UInt16 m_nReliableCount;     // Number of reliable messages received
      private UInt16 m_nCurrentPing;       // Current Ping Value
      private UInt16 m_nAveragePing;       // Average Ping Value
      private UInt16 m_nLowPing;           // Low Ping Value
      private UInt16 m_nHighPing;          // High Ping Value
      private UInt32 m_nWeaponCount;       // Number of weapon packets received
      private UInt16 m_nS2CSlowCurrent;    // Current S2C Slow Packets
      private UInt16 m_nS2CFastCurrent;    // Current S2C Fast Packets
      private UInt32 m_nS2CSlowTotal;      // Total S2C Slow Packets
      private UInt32 m_nS2CFastTotal;      // Total S2C Fast Packets
      private Byte   m_bSlowFrame;         // Slow Frame

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// Executable Checksum Property
      /// </summary>
      public UInt32 ExecutableChecksum { set { m_nExeChecksum = value; } }

      /// <summary>
      /// Parameter Checksum Property
      /// </summary>
      public UInt32 ParameterChecksum { set { m_nParameterChecksum = value; } }

      /// <summary>
      /// Level Checksum Property
      /// </summary>
      public UInt32 LevelChecksum { set { m_nLevelChecksum = value; } }

      /// <summary>
      /// Reliable Message Count Property
      /// </summary>
      public UInt16 ReliableCount { set { m_nReliableCount = value; } }
       
      /// <summary>
      /// Current Ping Property
      /// </summary>
      public UInt16 CurrentPing { set { m_nCurrentPing = value; } }

      /// <summary>
      /// Average Ping Property
      /// </summary>
      public UInt16 AveragePing { set { m_nAveragePing = value; } }

      /// <summary>
      /// Low Ping Property
      /// </summary>
      public UInt16 LowPing { set { m_nLowPing = value; } }

      /// <summary>
      /// High Ping Property
      /// </summary>
      public UInt16 HighPing { set { m_nHighPing = value; } }

      /// <summary>
      /// Weapon Count Property
      /// </summary>
      public UInt32 WeaponCount { set { m_nWeaponCount = value; } }

      /// <summary>
      /// S2C Slow Current Property
      /// </summary>
      public UInt16 S2CSlowCurrent { set { m_nS2CSlowCurrent = value; } }

      /// <summary>
      /// S2C Fast Current Property
      /// </summary>
      public UInt16 S2CFastCurrent { set { m_nS2CFastCurrent = value; } }

      /// <summary>
      /// S2C Slow Total Property
      /// </summary>
      public UInt32 S2CSlowTotal { set { m_nS2CSlowTotal = value; } }

      /// <summary>
      /// S2C Fast Total Property
      /// </summary>
      public UInt32 S2CFastTotal { set { m_nS2CFastTotal = value; } }

      /// <summary>
      /// Slow Frame Property
      /// </summary>
      public Byte SlowFrame { set { m_bSlowFrame = value; } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }
         get
         {
            // Create the disconnect packet
            MemoryStream packet = new MemoryStream (40);

            // Compose the password packet
            packet.WriteByte (0x1A);
            packet.Write (BitConverter.GetBytes (m_nWeaponCount), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nParameterChecksum), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nExeChecksum), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nLevelChecksum), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nS2CSlowTotal), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nS2CFastTotal), 0, 4);
            packet.Write (BitConverter.GetBytes (m_nS2CSlowCurrent), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nS2CFastCurrent), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nReliableCount), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nCurrentPing), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nAveragePing), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nLowPing), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nHighPing), 0, 2);
            packet.Write (BitConverter.GetBytes (m_bSlowFrame), 0, 2);

            // Return the packet data
            return packet.GetBuffer ();
         }
      }
   }
}
