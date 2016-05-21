//-----------------------------------------------------------------------
//
// NAME:        WeaponInfo.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Weapon Information implementation.
//
// NOTES:       None.
//
// $History: WeaponInfo.cs $
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
   /// Weapon Information Object
   /// </summary>
   public class WeaponInfo
   {
      /// <summary>
      /// Weapon Information Bitfield
      /// </summary>
      private UInt16 m_WeaponInfo;

      /// <summary>
      /// Property to set/get the weapon bitfield value
      /// </summary>
      public UInt16 Value
      {
         set { m_WeaponInfo = value; }
         get { return m_WeaponInfo; }
      }

      /// <summary>
      /// Weapon Type Property
      /// </summary>
      public WeaponTypes Type
      {
         set 
         {
            // Clear the field 
            m_WeaponInfo &= 0xFFE0;

            if ((UInt16)value > 10)
            {
               // Set the shift bit and insert the weapon
               m_WeaponInfo |= 0x8000;
               m_WeaponInfo |= (UInt16)((UInt16)(value - 10) & 0x001F); 
            }
            else
            {
               // Clear the shift bit and insert the weapon
               m_WeaponInfo &= 0x7FFF;
               m_WeaponInfo |= (UInt16)((UInt16)value & 0x001F); 
            }
         }

         get 
         {
            UInt16 nModifier = 0;
            if ((m_WeaponInfo & 0x8000) != 0) { nModifier = 10; }
            return (WeaponTypes)((m_WeaponInfo & 0x001F) + nModifier); 
         }
      }

      /// <summary>
      /// Weapon Level Property
      /// </summary>
      public Byte Level
      {
         set
         {
            // Clear the field and set the new bit data
            m_WeaponInfo &= 0xFF9F;
            m_WeaponInfo |= (UInt16)(((UInt16)value << 5) & 0x0060);
         }

         get { return (Byte)((m_WeaponInfo & 0x0060) >> 5); }
      }

      /// <summary>
      /// Shrapnel Level Property
      /// </summary>
      public Byte ShrapnelLevel 
      { 
         set 
         {
            // Clear the field and set the new bit data
            m_WeaponInfo &= 0xFCFF;
            m_WeaponInfo |= (UInt16)(((UInt16)value & 0x03) << 8);
         }

         get { return (Byte)((m_WeaponInfo & 0x0300) >> 8); }
      }

      /// <summary>
      /// Shrapnel Count Property
      /// </summary>
      public Byte ShrapnelCount 
      { 
         set
         {
            m_WeaponInfo &= 0x83FF;
            m_WeaponInfo |= (UInt16)(((UInt16)value & 0x1F) << 10);
         }

         get { return (Byte)((m_WeaponInfo & 0x7C00) >> 10); } 
      }

      /// <summary>
      /// Bouncing Shrapnel Property
      /// </summary>
      public Boolean BouncingShrapnel
      {
         set { if (value) { m_WeaponInfo |= 0x0080; } else { m_WeaponInfo &= 0xFF7F; } }
         get { return (m_WeaponInfo & 0x0080) != 0; }
      }
   }
}
