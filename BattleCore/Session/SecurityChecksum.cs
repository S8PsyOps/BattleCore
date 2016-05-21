//-----------------------------------------------------------------------
//
// NAME:        SecurityChecksum.cs
//
// PROJECT:     Battle Core Session Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Security Checksum implementation.
//
// NOTES:       Some of this was ripped from Merv and Hybrid, 
//              so thanks to Catid and Cerium.
//
// $History: SecurityChecksum.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// namespace declaration
namespace BattleCore.Session
{
   /// <summary>
   /// Security checksum object
   /// </summary>
   public class SecurityChecksum
   {
      // Constant modifier definitions
      private const UInt32 G4_MODIFIER = 0x77073096;
      private const UInt32 G16_MODIFIER = 0x076dc419;
      private const UInt32 G64_MODIFIER = 0x1db71064;
      private const UInt32 G256_MODIFIER = 0x76dc4190;

      /// <summary>
      /// Default constructor
      /// </summary>
      public SecurityChecksum ()
      {
         m_bLevelData = null;
         m_pLevelData = null;
         m_ArenaSettings = null;
      }

      // The current arena's .lvl data
      private Byte[] m_bLevelData;
      /// <summary>
      /// Property the set the Arena level data
      /// </summary>
      public Byte[] LevelData
      {
         set { m_pLevelData = value; }
         get { return m_pLevelData; }
      }
      private String m_MapFile;
      public String MapFile
      {
          get { return m_MapFile; }
          set { m_MapFile = value; }
      }
      private Byte[] m_pLevelData;
      public Byte[] PsyLevelData
      {
          set { m_pLevelData = value; }
          get { return m_pLevelData; }
      }

      // Current arena's configuration
      private UInt32[] m_ArenaSettings;
      /// <summary>
      /// Propertry to set the arena settings
      /// </summary>
      public Byte[] ArenaSettings
      {
         set
         {
            if ((value != null) && (value.Length == 1428))
            {
               UInt32 nIndex = 0;

               m_ArenaSettings = new UInt32[357];

               for (int intCounter = 0; intCounter < value.Length; intCounter += 4)
               {
                  // Add the next value to the settings
                  m_ArenaSettings[nIndex++] = BitConverter.ToUInt32 (value, intCounter);
               }
            }
            else
            {
               m_ArenaSettings = null;
            }
         }
      }

      private void generate4 (ref UInt32[] buf, UInt32 offset, UInt32 key)
      {
         buf[offset] = key;
         buf[offset + 1] = key ^ G4_MODIFIER;
         buf[offset + 2] = (key ^= (G4_MODIFIER << 1));
         buf[offset + 3] = key ^ G4_MODIFIER;
      }

      private void generate16 (ref UInt32[] buf, UInt32 offset, UInt32 key)
      {
         generate4 (ref buf, offset, key);
         generate4 (ref buf, offset + 4, key ^ G16_MODIFIER);
         generate4 (ref buf, offset + 8, key ^= (G16_MODIFIER << 1));
         generate4 (ref buf, offset + 12, key ^ G16_MODIFIER);
      }

      private void generate64 (ref UInt32[] buf, UInt32 offset, UInt32 key)
      {
         generate16 (ref buf, offset, key);
         generate16 (ref buf, offset + 16, key ^ G64_MODIFIER);
         generate16 (ref buf, offset + 32, key ^= (G64_MODIFIER << 1));
         generate16 (ref buf, offset + 48, key ^ G64_MODIFIER);
      }

      /// <summary>
      /// Generate the checksum dictionary
      /// </summary>
      /// <param name="key">checksum key</param>
      /// <returns></returns>
      internal UInt32[] generateDictionary (UInt32 key)
      {
         UInt32[] buf = new UInt32[256];

         generate64 (ref buf, 0, key);
         generate64 (ref buf, 64, key ^ G256_MODIFIER);
         generate64 (ref buf, 128, key ^= (G256_MODIFIER << 1));
         generate64 (ref buf, 192, key ^ G256_MODIFIER);

         return buf;
      }

      internal UInt32 generateFileChecksum (UInt32[] intDictionary, Byte[] bFileData)
      {
         if ((intDictionary != null) && (intDictionary.Length == 256) && (bFileData != null))
         {
            UInt32 index = 0;
            UInt32 key = index - 1;

            for (UInt32 i = 0; i < bFileData.Length; i++)
            {
               index = intDictionary[(key & 0xFF) ^ (bFileData[i] & 0xFF)];
               key = (key >> 8 & 0xFFFFFF) ^ index;
            }

            return ~key;
         }
         else
         {
            return 0;
         }
      }

      void Reset ()
      {
         m_bLevelData = null;
         m_ArenaSettings = null;
      }

      ///////////////////////////////////////
      // Generate a checksum on the map file
      internal UInt32 generateLevelChecksum (UInt32 key)
      {
         if (m_bLevelData != null)
         {
            try
            {
               Int32 savekey = (Int32)key;

               for (int y = savekey % 32; y < 1024; y += 32)
               {
                  for (int x = savekey % 31; x < 1024; x += 31)
                  {
                     Byte tile = m_bLevelData[(1024 * y) + x];
                     if ((tile >= 1 && tile <= 160) || tile == 171)
                        key += (UInt32)(savekey ^ tile);
                  }
               }
            }
            catch (Exception e)
            {
               Console.WriteLine (e.Message);
            }
         }

         return (UInt32)key;
      }

      internal UInt32 generateParameterChecksum (UInt32 key)
      {
         if (m_ArenaSettings != null)
         {
            UInt32 c = 0;

            for (UInt32 i = 0; i < 357; i++) c += m_ArenaSettings[i] ^ key;
            return c;
         }
         else
         {
            return 0;
         }
      }

      internal UInt32 generateEXEChecksum (UInt32 key)
      {
         UInt32 part = 0;
         UInt32 csum = 0;

         part = 0xc98ed41f;
         part += 0x3e1bc | key;
         part ^= 0x42435942 ^ key;
         part += 0x1d895300 | key;
         part ^= 0x6b5c4032 ^ key;
         part += 0x467e44 | key;
         part ^= 0x516c7eda ^ key;
         part += 0x8b0c708b | key;
         part ^= 0x6b3e3429 ^ key;
         part += 0x560674c9 | key;
         part ^= 0xf4e6b721 ^ key;
         part += 0xe90cc483 | key;
         part ^= 0x80ece15a ^ key;
         part += 0x728bce33 | key;
         part ^= 0x1fc5d1e6 ^ key;
         part += 0x8b0c518b | key;
         part ^= 0x24f1a96e ^ key;
         part += 0x30ae0c1 | key;
         part ^= 0x8858741b ^ key;
         csum += part;

         part = 0x9c15857d;
         part += 0x424448b | key;
         part ^= 0xcd0455ee ^ key;
         part += 0x727 | key;
         part ^= 0x8d7f29cd ^ key;
         csum += part;

         part = 0x824b9278;
         part += 0x6590 | key;
         part ^= 0x8e16169a ^ key;
         part += 0x8b524914 | key;
         part ^= 0x82dce03a ^ key;
         part += 0xfa83d733 | key;
         part ^= 0xb0955349 ^ key;
         part += 0xe8000003 | key;
         part ^= 0x7cfe3604 ^ key;
         csum += part;

         part = 0xe3f8d2af;
         part += 0x2de85024 | key;
         part ^= 0xbed0296b ^ key;
         part += 0x587501f8 | key;
         part ^= 0xada70f65 ^ key;
         csum += part;

         part = 0xcb54d8a0;
         part += 0xf000001 | key;
         part ^= 0x330f19ff ^ key;
         part += 0x909090c3 | key;
         part ^= 0xd20f9f9f ^ key;
         part += 0x53004add | key;
         part ^= 0x5d81256b ^ key;
         part += 0x8b004b65 | key;
         part ^= 0xa5312749 ^ key;
         part += 0xb8004b67 | key;
         part ^= 0x8adf8fb1 ^ key;
         part += 0x8901e283 | key;
         part ^= 0x8ec94507 ^ key;
         part += 0x89d23300 | key;
         part ^= 0x1ff8e1dc ^ key;
         part += 0x108a004a | key;
         part ^= 0xc73d6304 ^ key;
         part += 0x43d2d3 | key;
         part ^= 0x6f78e4ff ^ key;
         csum += part;

         part = 0x45c23f9;
         part += 0x47d86097 | key;
         part ^= 0x7cb588bd ^ key;
         part += 0x9286 | key;
         part ^= 0x21d700f8 ^ key;
         part += 0xdf8e0fd9 | key;
         part ^= 0x42796c9e ^ key;
         part += 0x8b000003 | key;
         part ^= 0x3ad32a21 ^ key;
         csum += part;

         part = 0xb229a3d0;
         part += 0x47d708 | key;
         part ^= 0x10b0a91 ^ key;
         csum += part;

         part = 0x466e55a7;
         part += 0xc7880d8b | key;
         part ^= 0x44ce7067 ^ key;
         part += 0xe4 | key;
         part ^= 0x923a6d44 ^ key;
         part += 0x640047d6 | key;
         part ^= 0xa62d606c ^ key;
         part += 0x2bd1f7ae | key;
         part ^= 0x2f5621fb ^ key;
         part += 0x8b0f74ff | key;
         part ^= 0x2928b332;
         csum += part;

         part = 0x62cf369a;
         csum += part;

         return csum;
      }
   }
}

