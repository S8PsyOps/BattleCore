//-----------------------------------------------------------------------
//
// NAME:        SessionStatistics.cs
//
// PROJECT:     Battle Core Session Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Session Statistics implementation.
//
// NOTES:       None.
//
// $History: SessionStatistics.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;

// Namespace declaration
namespace BattleCore.Session
{
   /// <summary>
   /// SessionStatistics object to handle communication statistics.
   /// </summary>
   internal class SessionStatistics
   {
      private UInt16 m_nReliableReceiveCount; // Number of reliable packets received
      private UInt32 m_nWeaponCount;          // Number of weapons packets received
      private UInt16 m_nAveragePing;          // Average ping time
      private UInt16 m_nCurrentPing;          // Current Ping time
      private UInt16 m_nLowPing;              // Lowest ping time
      private UInt16 m_nHighPing;             // Highest ping time
      private UInt16 m_nS2CSlowCurrent;       // Current server to client slow packets
      private UInt16 m_nS2CFastCurrent;       // Current server to client fast packets
      private UInt32 m_nS2CSlowTotal;         // Total server to client slow packets
      private UInt32 m_nS2CFastTotal;         // Total server to client fast packets

      /// <summary>
      /// Increment the Number of reliable packets received
      /// </summary>
      public void IncrementReliableReceiveCount () { m_nReliableReceiveCount++; }

      /// <summary>
      /// Increment the number of weapon packets sent
      /// </summary>
      public void IncrementWeaponCount () { m_nWeaponCount++; }

      /// <summary>
      /// Increment the Number of S2C slow packets sent
      /// </summary>
      public void IncrementS2CSlow () { m_nS2CSlowCurrent++; m_nS2CSlowTotal++; }

      /// <summary>
      /// Increment the Number of S2C slow packets sent
      /// </summary>
      public void IncrementS2CFast () { m_nS2CFastCurrent++; m_nS2CFastTotal++; }

      /// <summary>
      /// Reliable Receive Count Property
      /// </summary>
      public UInt32 WeaponCount { get { return m_nWeaponCount; } }

      /// <summary>
      /// Weapon Receive Count Property
      /// </summary>
      public UInt16 ReliableReceiveCount 
      { 
         get 
         {
            UInt16 count = m_nReliableReceiveCount;

            // Reset the reliable receive count
            m_nReliableReceiveCount = 0;

            return count; 
         }
      }

      /// <summary>
      /// Current Ping Property
      /// </summary>
      public UInt16 CurrentPing
      {
         set
         {
            // Calculate an average ping value (not exact, but good enough)
            m_nAveragePing = (UInt16)((m_nAveragePing + value) / 2);

            // Set the high and low ping values
            if (value > m_nHighPing) m_nHighPing = value;
            if ((value < m_nLowPing) || (m_nLowPing == 0)) m_nLowPing = value;

            // Set the ping value
            m_nCurrentPing = value;
         }
         get
         {
            return m_nCurrentPing;
         }
      }

      /// <summary>
      /// Low Ping Property
      /// </summary>
      public UInt16 LowPing { get { return m_nLowPing; } }

      /// <summary>
      /// High Ping Property
      /// </summary>
      public UInt16 HighPing { get { return m_nHighPing; } }

      /// <summary>
      /// Average Ping Property
      /// </summary>
      public UInt16 AveragePing { get { return m_nAveragePing; } }

      /// <summary>
      /// S2C Slow Current Property
      /// </summary>
      public UInt16 S2CSlowCurrent { get { return m_nS2CSlowCurrent; } }

      /// <summary>
      /// S2C Fast Current Property
      /// </summary>
      public UInt16 S2CFastCurrent { get { return m_nS2CFastCurrent; } }

      /// <summary>
      /// S2C Slow Total Property
      /// </summary>
      public UInt32 S2CSlowTotal { get { return m_nS2CSlowTotal; } }

      /// <summary>
      /// S2C Fast Current Property
      /// </summary>
      public UInt32 S2CFastTotal { get { return m_nS2CFastTotal; } }

      /// <summary>
      /// Reset the session statistics
      /// </summary>
      public void Reset ()
      {
         m_nReliableReceiveCount = 0; // Number of reliable packets received
         m_nWeaponCount = 0;          // Number of weapons packets received
         m_nAveragePing = 0;          // Average ping time
         m_nCurrentPing = 0;          // Current Ping time
         m_nLowPing = 0;              // Lowest ping time
         m_nHighPing = 0;             // Highest ping time
         m_nS2CSlowCurrent = 0;       // Current server to client slow packets
         m_nS2CFastCurrent = 0;       // Current server to client fast packets
         m_nS2CSlowTotal = 0;         // Total server to client slow packets
         m_nS2CFastTotal = 0;         // Total server to client fast packets
      }
   }
}
