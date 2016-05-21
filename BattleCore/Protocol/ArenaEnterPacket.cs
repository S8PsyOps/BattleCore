//-----------------------------------------------------------------------
//
// NAME:        ArenaEnterPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Arena Enter Packet implementation.
//
// NOTES:       None.
//
// $History: ArenaEnterPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using BattleCore.Events;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// ArenaEnterPacket object.  This object is used to enter an arena
   /// after the login is completed.
   /// </summary>
   internal class ArenaEnterPacket : IPacket
   {
      // Private member data
      private String    m_strArenaName;
      private UInt16    m_nArenaIdentifier;
      private Byte      m_nBlockObscenity;
      private Byte      m_nHearVoices;
      private ShipTypes m_shipType;
      private UInt16    m_nResolutionX;
      private UInt16    m_nResolutionY;

      /// <summary>
      /// Arena Enter packet constructor
      /// </summary>
      public ArenaEnterPacket ()
      {
         m_nArenaIdentifier = 0xFFFF;
         m_shipType = ShipTypes.Spectator;
         m_nResolutionX = 1280;
         m_nResolutionY = 1024;
         m_nHearVoices = 1;
         m_nBlockObscenity = 0;
         m_strArenaName = "";
      }

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// Block Obscenity Property
      /// </summary>
      public Byte BlockObscenity
      {
         set { m_nBlockObscenity = value; }
      }

      /// <summary>
      /// Hear Voices Property
      /// </summary>
      public Byte HearVoices
      {
         set { m_nHearVoices = value; }
      }

      /// <summary>
      /// Resolution X Property
      /// </summary>
      public UInt16 ResolutionX
      {
         set { m_nResolutionX = value; }
      }

      /// <summary>
      /// Resolution Y Property
      /// </summary>
      public UInt16 ResolutionY
      {
         set { m_nResolutionY = value; }
      }

      /// <summary>
      /// Arena Identifier Property
      /// </summary>
      public UInt16 ArenaIdentifier
      {
         set { m_nArenaIdentifier = value; }
      }

      /// <summary>
      /// Ship Type Property
      /// </summary>
      public ShipTypes ShipType
      {
         set { m_shipType = value; }
      }

      /// <summary>
      /// Arena Name Property
      /// </summary>
      public String ArenaName
      {
         set
         {
            // Set the arena name
            m_strArenaName = value;

            // Check if the arena name is set
            if (value.Length != 0)
            {
               // Set the private arena identifier
               m_nArenaIdentifier = 0xFFFD;
            }
         }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (26);

            // Write the packet data to the memory stream
            packet.WriteByte (0x01);
            packet.WriteByte ((Byte)m_shipType);
            packet.WriteByte (m_nBlockObscenity);
            packet.WriteByte (m_nHearVoices);
            packet.Write (BitConverter.GetBytes (m_nResolutionX), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nResolutionY), 0, 2);
            packet.Write (BitConverter.GetBytes (m_nArenaIdentifier), 0, 2);
            packet.Write ((new ASCIIEncoding ()).GetBytes (m_strArenaName), 0, m_strArenaName.Length);

            // Convert the packet to a byte array
            return packet.GetBuffer ();
         }
      }
   }
}
