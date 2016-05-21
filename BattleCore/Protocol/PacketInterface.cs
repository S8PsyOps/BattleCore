//-----------------------------------------------------------------------
//
// NAME:        PacketInterface.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Packet Interface implementation.
//
// NOTES:       None.
//
// $History: PacketInterface.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// IPacket object.  This object is used as a base interface 
   /// for all packets sent to and from the server.
   /// </summary>
   internal interface IPacket
   {
      Byte[] Packet { set; get; }
      Boolean Reliable { get; }
   }
}
