//-----------------------------------------------------------------------
//
// NAME:        ArenaSettingsPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Arena Settings Packet implementation.
//
// NOTES:       None.
//
// $History: ArenaSettingsPacket.cs $
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
   /// ArenaSettints packet handler
   /// </summary>
   public class ArenaSettingsPacket : IPacket
   {
      Byte[] m_settings = new Byte[1428];

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Property to get the arena settings
      /// </summary>
      public Byte[] ArenaSettings
      {
         get { return m_settings; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            if (value.Length == 1428)
            {
               // Extract the arena settings
               Array.Copy (value, 0, m_settings, 0, 1428);
            }
         }
         get
         {
            // return the packet data buffer
            return null;
         }
      }
   }
}
