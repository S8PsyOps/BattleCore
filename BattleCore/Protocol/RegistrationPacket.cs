//-----------------------------------------------------------------------
//
// NAME:        RegistrationPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Registration Packet implementation.
//
// NOTES:       None.
//
// $History: RegistrationPacket.cs $
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
   /// RegistrationPacket Object.  This packet is sent to register 
   /// a new player name with the server.
   /// </summary>
   internal class RegistrationPacket : IPacket
   {
      private String m_strRealName;
      private String m_strEmailAddress;
      private String m_strCity;
      private String m_strState;
      private String m_strRegisteredName;
      private String m_strOrganization;

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      ///<summary>Real Name Property</summary>
      public String RealName { set { m_strRealName = value; } }

      ///<summary>Email Address Property</summary>
      public String EmailAddress { set { m_strEmailAddress = value; } }

      ///<summary>City Name Property</summary>
      public String City { set { m_strCity = value; } }

      ///<summary>State Property</summary>
      public String State { set { m_strState = value; } }

      ///<summary>Registered Name Property</summary>
      public String RegisteredName { set { m_strRegisteredName = value; } }

      ///<summary>Organization Property</summary>
      public String Organization { set { m_strOrganization = value; } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }
         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream (766);

            Byte[] m_pRealName = new Byte[32];
            Byte[] m_pEmailAddress = new Byte[64];
            Byte[] m_pCity = new Byte[32];
            Byte[] m_pState = new Byte[24];
            Byte[] m_pRegisteredName = new Byte[40];
            Byte[] m_pOrganization = new Byte[40];

            (new ASCIIEncoding ()).GetBytes (m_strRealName, 0, m_strRealName.Length, m_pRealName, 0);
            (new ASCIIEncoding ()).GetBytes (m_strEmailAddress, 0, m_strEmailAddress.Length, m_pEmailAddress, 0);
            (new ASCIIEncoding ()).GetBytes (m_strCity, 0, m_strCity.Length, m_pCity, 0);
            (new ASCIIEncoding ()).GetBytes (m_strState, 0, m_strState.Length, m_pState, 0);
            (new ASCIIEncoding ()).GetBytes (m_strRegisteredName, 0, m_strRegisteredName.Length, m_pRegisteredName, 0);
            (new ASCIIEncoding ()).GetBytes (m_strOrganization, 0, m_strOrganization.Length, m_pOrganization, 0);

            // Write the packet data to the memory stream
            packet.WriteByte (0x17);
            packet.Write (m_pRealName, 0, m_pRealName.Length);
            packet.Write (m_pEmailAddress, 0, m_pEmailAddress.Length);
            packet.Write (m_pCity, 0, m_pCity.Length);
            packet.Write (m_pState, 0, m_pState.Length);
            packet.WriteByte (0x4D);
            packet.WriteByte (0x20);
            packet.WriteByte (0x01);
            packet.WriteByte (0x01);
            packet.WriteByte (0x00);
            packet.Write (BitConverter.GetBytes ((UInt32)586), 0, 4);
            packet.Write (BitConverter.GetBytes ((UInt16)0xC000), 0, 2);
            packet.Write (BitConverter.GetBytes ((UInt16)2036), 0, 2);
            packet.Write (m_pRegisteredName, 0, m_pRegisteredName.Length);
            packet.Write (m_pOrganization, 0, m_pOrganization.Length);

            // Convert the packet to a byte array
            return packet.GetBuffer ();
         }
      }
   }
}
