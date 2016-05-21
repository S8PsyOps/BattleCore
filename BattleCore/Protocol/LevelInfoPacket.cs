//-----------------------------------------------------------------------
//
// NAME:        LevelInfoPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Level Info Packet implementation.
//
// NOTES:       None.
//
// $History: LevelInfoPacket.cs $
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
   /// Level file information packet
   /// </summary>
   public class LevelInfoPacket : IPacket
   {
      // Private member data
      private String m_strFileName;
      private UInt32 m_fileChecksum;

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// File Name Property
      /// </summary>
      public String FileName
      {
         get { return m_strFileName; }
      }

      /// <summary>
      /// File Checksum Property
      /// </summary>
      public UInt32 FileChecksum
      {
         get { return m_fileChecksum; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            m_strFileName = (new ASCIIEncoding ()).GetString (value, 1, 16);
            m_strFileName = m_strFileName.Substring (0, m_strFileName.LastIndexOf ('.') + 4);
            m_fileChecksum = BitConverter.ToUInt32(value, 17);
         }
         get
         {
            // return the packet data buffer
            return null;
         }
      }
   }
}
