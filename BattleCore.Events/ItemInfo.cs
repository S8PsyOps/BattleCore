//-----------------------------------------------------------------------
//
// NAME:        ItemInfo.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Items Information implementation.
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
   /// Items Information object
   /// </summary>
   public class ItemInfo
   {
      /// <summary>
      /// Items Information Bitfield
      /// </summary>
      private UInt32 m_ItemInfo;

      /// <summary>
      /// Property to set/get the items bitfield value
      /// </summary>
      public UInt32 Value
      {
         set { m_ItemInfo = value; }
         get { return m_ItemInfo; }
      }

      /// <summary>
      /// Shields Property
      /// </summary>
      public Boolean HasShields 
      {
         set { if (value) { m_ItemInfo |= 0x00000001; } else { m_ItemInfo &= 0xFFFFFFFE; } }
         get { return ((m_ItemInfo & 0x00000001) != 0); }
      }

      /// <summary>
      /// Super Property
      /// </summary>
      public Boolean HasSuper 
      { 
         set { if (value) { m_ItemInfo |= 0x00000002; } else { m_ItemInfo &= 0xFFFFFFFD; } }
         get { return ((m_ItemInfo & 0x00000002) != 0); }
      }

      /// <summary>
      /// Burst Count Property
      /// </summary>
      public Byte BurstCount
      {
         set 
         {
            // Cap the count at 15
            if ((value & 0xF0) != 0) { value = 0x0F; }

            // Clear the field and set the new bit data
            m_ItemInfo &= 0xFFFFFFC3;
            m_ItemInfo |= (UInt32)(value & 0x0F) << 2;
         }

         get { return (Byte)((m_ItemInfo & 0x0000003C) >> 2); }
      }

      /// <summary>
      /// Repel Count Property
      /// </summary>
      public Byte RepelCount
      {
         set
         {
            // Cap the count at 15
            if ((value & 0xF0) != 0) { value = 0x0F; }

            // Clear the field and set the new bit data
            m_ItemInfo &= 0xFFFFFC3F;
            m_ItemInfo |= (UInt32)(value & 0x0F) << 6;
         }

         get { return (Byte)((m_ItemInfo & 0x000003C0) >> 6); }
      }

      /// <summary>
      /// Thor Count Property
      /// </summary>
      public Byte ThorCount
      {
         set
         {
            // Cap the count at 15
            if ((value & 0xF0) != 0) { value = 0x0F; }

            // Clear the field and set the new bit data
            m_ItemInfo &= 0xFFFFC3FF;
            m_ItemInfo |= (UInt32)(value & 0x0F) << 10;
         }

         get { return (Byte)((m_ItemInfo & 0x00003C00) >> 10); }
      }

      /// <summary>
      /// Brick Count Property
      /// </summary>
      public Byte BrickCount
      {
         set
         {
            // Cap the count at 15
            if ((value & 0xF0) != 0) { value = 0x0F; }

            // Clear the field and set the new bit data
            m_ItemInfo &= 0xFFFC3FFF;
            m_ItemInfo |= (UInt32)(value & 0x0F) << 14;
         }

         get { return (Byte)((m_ItemInfo & 0x0003C000) >> 14); }
      }

      /// <summary>
      /// Decoy Count Property
      /// </summary>
      public Byte DecoyCount
      {
         set
         {
            // Cap the count at 15
            if ((value & 0xF0) != 0) { value = 0x0F; }

            // Clear the field and set the new bit data
            m_ItemInfo &= 0xFFC3FFFF;
            m_ItemInfo |= (UInt32)(value & 0x0F) << 18;
         }

         get { return (Byte)((m_ItemInfo & 0x003C0000) >> 18); }
      }

      /// <summary>
      /// Rocket Count Property
      /// </summary>
      public Byte RocketCount
      {
         set
         {
            // Cap the count at 15
            if ((value & 0xF0) != 0) { value = 0x0F; }

            // Clear the field and set the new bit data
            m_ItemInfo &= 0xFC3FFFFF;
            m_ItemInfo |= (UInt32)(value & 0x0F) << 22;
         }

         get { return (Byte)((m_ItemInfo & 0x03C00000) >> 22); }
      }

      /// <summary>
      /// Portal Count Property
      /// </summary>
      public Byte PortalCount
      {
         set
         {
            // Cap the count at 15
            if ((value & 0xF0) != 0) { value = 0x0F; }

            // Clear the field and set the new bit data
            m_ItemInfo &= 0xC3FFFFFF;
            m_ItemInfo |= (UInt32)(value & 0x0F) << 26;
         }

         get { return (Byte)((m_ItemInfo & 0x3C000000) >> 26); }
      }
   }
}
