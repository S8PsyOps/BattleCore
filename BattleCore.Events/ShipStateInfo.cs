//-----------------------------------------------------------------------
//
// NAME:        ShipStateInfo.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Ship State Information implementation.
//
// NOTES:       None.
//
// $History: ShipStateInfo.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using BattleCore.Events;

// Namespace declaration
namespace BattleCore.Events
{
   /// <summary>
   /// Ship State Information Object
   /// </summary>
   public class ShipStateInfo
   {
      /// <summary>
      /// Ship State Information Bitfield
      /// </summary>
      private Byte m_ShipStateInfo;

      /// <summary>
      /// Property to set/get the ShipState bitfield value
      /// </summary>
      public Byte Value
      {
         set { m_ShipStateInfo = value; }
         get { return m_ShipStateInfo; }
      }

      /// <summary>
      /// Stealth Active Property
      /// </summary>
      public Boolean StealthActive 
      {
         set { if (value) { m_ShipStateInfo |= 0x01; } else { m_ShipStateInfo &= 0xFE; } }
         get { return (m_ShipStateInfo & 0x01) != 0; }
      }

      /// <summary>
      /// Cloak Active Property
      /// </summary>
      public Boolean CloakActive  
      { 
         set { if (value) { m_ShipStateInfo |= 0x02; } else { m_ShipStateInfo &= 0xFD; } }
         get { return (m_ShipStateInfo & 0x02) != 0; }
      }

      /// <summary>
      /// XRadar Active Property
      /// </summary>
      public Boolean XRadarActive   
      { 
         set { if (value) { m_ShipStateInfo |= 0x04; } else { m_ShipStateInfo &= 0xFB; } }
         get { return (m_ShipStateInfo & 0x04) != 0; }
      }

      /// <summary>
      /// Antiwarp Active Property
      /// </summary>
      public Boolean AntiwarpActive   
      {
         set { if (value) { m_ShipStateInfo |= 0x08; } else { m_ShipStateInfo &= 0xF7; } }
         get { return (m_ShipStateInfo & 0x08) != 0; }
      }

      /// <summary>
      /// Ship is flashing
      /// </summary>
      public Boolean IsFlashing   
      {
         set { if (value) { m_ShipStateInfo |= 0x10; } else { m_ShipStateInfo &= 0xEF; } }
         get { return (m_ShipStateInfo & 0x10) != 0; }
      }

      /// <summary>
      /// Ship is in a safety zone
      /// </summary>
      public Boolean IsSafe  
      { 
         set { if (value) { m_ShipStateInfo |= 0x20; } else { m_ShipStateInfo &= 0xDF; } }
         get { return (m_ShipStateInfo & 0x20) != 0; }
      }

      /// <summary>
      /// Ship is a UFO
      /// </summary>
      public Boolean IsUFO   
      { 
         set { if (value) { m_ShipStateInfo |= 0x40; } else { m_ShipStateInfo &= 0xBF; } }
         get { return (m_ShipStateInfo & 0x40) != 0; }
      }
   }
}
