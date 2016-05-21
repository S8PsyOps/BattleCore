//-----------------------------------------------------------------------
//
// NAME:        PasswordPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Password Packet implementation.
//
// NOTES:       None.
//
// $History: PasswordPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Globalization;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// PasswordPacket object.  This object is used to send a login
   /// packet to the server.
   /// </summary>
   internal class PasswordPacket : IPacket
   {
      Byte m_bNewUser;     // New user flag
      String m_strUserName; // User name
      String m_strPassword; // User Password
      Byte m_bContinuum;   // Continuum password mode

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// User Name Property
      /// </summary>
      public String UserName { set { m_strUserName = value; } }

      /// <summary>
      /// Password Property
      /// </summary>
      public String Password { set { m_strPassword = value; } }

      /// <summary>
      /// New User Property
      /// </summary>
      public Boolean NewUser { set { m_bNewUser = (Byte)((value == true) ? 0x01 : 0x00); } }

      /// <summary>
      /// Use Continuum password Property
      /// </summary>
      public Boolean Continuum { set { m_bContinuum = (Byte)((value == true) ? 0x01 : 0x00); } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set { }
         get
         {
            // Create the disconnect packet
            MemoryStream packet = new MemoryStream ((m_bContinuum == 0x01) ? 165 : 101);

            TimeSpan timeBias = TimeZone.CurrentTimeZone.GetUtcOffset (DateTime.Now);

            // Create the registry key
            RegistryKey rootKey = Registry.LocalMachine.OpenSubKey ("SOFTWARE", true);
            RegistryKey key = rootKey.CreateSubKey ("BattleCore");

            // Get the permission and machine identifiers
            String strPermissionId = (String)key.GetValue ("PermissionId");
            UInt32 nPermissionId   = 0;

            if (strPermissionId == null)
            {
               // Set the permission identifier
               nPermissionId = (UInt32)(Environment.TickCount ^ 0xAAAAAAAA) * 0x5f346d + 0x5abcdef;
               key.SetValue ("PermissionId", nPermissionId.ToString("X08"));    
            }
            else
            {
               nPermissionId = UInt32.Parse (strPermissionId, NumberStyles.HexNumber);
            }

            UInt32 nMachineId = nPermissionId;
            if (nMachineId > 0x7fffffff) nMachineId += 0x7fffffff;

            Int16 timezone = Math.Abs((Int16)(timeBias.TotalMinutes));

            Byte[] userName = new Byte[32];
            Byte[] password = new Byte[32];

            (new ASCIIEncoding()).GetBytes(m_strUserName, 0, m_strUserName.Length, userName, 0);
            (new ASCIIEncoding()).GetBytes(m_strPassword, 0, m_strPassword.Length, password, 0);

            // Compose the password packet
            packet.WriteByte (0x09);
            packet.WriteByte (m_bNewUser);
            packet.Write (userName, 0, 32);
            packet.Write (password, 0, 32);
            packet.Write (BitConverter.GetBytes(nMachineId), 0, 4);
            packet.WriteByte (0x04);
            packet.Write (BitConverter.GetBytes(timezone), 0, 2);
            packet.Write (BitConverter.GetBytes((UInt16)0x6F9D), 0, 2);
            packet.Write (BitConverter.GetBytes ((UInt16)((m_bContinuum == 0x01) ? 38 : 134)), 0, 2);
            packet.Write (BitConverter.GetBytes((UInt32)444), 0, 4);
            packet.Write (BitConverter.GetBytes((UInt32)555), 0, 4);
            packet.Write (BitConverter.GetBytes(nPermissionId), 0, 4);

            // Return the packet data
            return packet.GetBuffer ();
         }
      }
   }
}
