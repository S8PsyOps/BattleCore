//-----------------------------------------------------------------------
//
// NAME:        SetPositionPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Set Position Packet implementation.
//
// NOTES:       None.
//
// $History: SetPositionPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BattleCore.Events;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// SetPositionPacket Object.  This packet is received when the 
   /// bot position is changed by the server.
   /// </summary>
   internal class SetPositionPacket : IPacket
   {
      /// <summary>Player Identifier</summary>
      private UInt16 m_nPositionX;     // X map position
      private UInt16 m_nPositionY;     // Y map position

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      ///<summary>Map Postion X Property</summary>
      public UInt16 MapPositionX
      {
         set { m_nPositionX = value; }
         get { return m_nPositionX; }
      }

      ///<summary>Map Postion Y Property</summary>
      public UInt16 MapPositionY
      {
         set { m_nPositionY = value; }
         get { return m_nPositionY; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Set the event data from the packet
            m_nPositionX = BitConverter.ToUInt16 (value, 1);
            m_nPositionY = BitConverter.ToUInt16 (value, 3);
         }

         // Return an empty array
         get { return new Byte[1]; }
      }
   }
}
