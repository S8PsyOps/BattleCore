//-----------------------------------------------------------------------
//
// NAME:        PasswordResponsePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Password Response Packet implementation.
//
// NOTES:       None.
//
// $History: PasswordResponsePacket.cs $
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
   internal enum LoginResponseError
   {
      Success,            //  - Move along.
      UnknownUser,        //  - Unknown player, continue as new user?
      InvalidPassword,    //  - Invalid password for specified user.  The name you have chosen is probably in use by another player, try picking a different name.
      FullArena,          //  - This arena is currently full, try again later.
      LockedOut,          //  - You have been locked out of SubSpace, for more information inquire on Web BBS.
      NoPermission,       //  - You do not have permission to play in this arena, see Web Site for more information.
      SpectateOnly,       //  - You only have permission to spectate in this arena.
      TooManyPoints,      //  - You have too many points to play in this arena, please choose another arena.
      SlowConnection,     //  - Your connection appears to be too slow to play in this arena.
      NoPermission2,      //  - You do not have permission to play in this arena, see Web Site for more information.
      NoNewConnections,   //  - The server is currently not accepting new connections.
      InvalidName,        //  - Invalid user name entered, please pick a different name.
      ObsceneName,        //  - Possibly offensive user name entered, please pick a different name.
      BillerDown,         //  - NOTICE: Server difficulties; this zone is currently not keeping track of scores.  Your original score will be available later.  However, you are free to play in the zone until we resolve this problem.
      WrongPassword,      //  - The server is currently busy processing other login requests, please try again in a few moments.
      ExperiencedOnly,    //  - This zone is restricted to experienced players only (ie. certain number of game-hours logged).
      UsingDemoVersion,   //  - You are currently using the demo version.  Your name and score will not be kept track of.
      TooManyDemos,       //  - This arena is currently has(sic) the maximum Demo players allowed, try again later.
      ClosedToDemos,      //  - This arena is closed to Demo players.
      NeedModerator = 255 //  - Moderator access required for this zone (MGB addition)
   };

   /// <summary>
   /// PasswordResponsePacket object.  This object is used to handle password
   /// responses from the server.
   /// </summary>
   internal class PasswordResponsePacket : IPacket
   {
      private LoginResponseError m_responseError;  // Response error code
      private UInt32 m_nExecutableChecksum;
      private UInt32 m_nCodeChecksum;
      private UInt32 m_nNewsChecksum;
      private UInt32 m_nServerVersion;
      private Byte m_nRequestRegistration;
      private Byte m_nIsVIP;

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Login Response Error Property
      /// </summary>
      public LoginResponseError Error { get { return m_responseError; } }

      /// <summary>
      /// Executable Checksum Property
      /// </summary>
      public UInt32 ExecutableChecksum { get { return m_nExecutableChecksum; } }

      /// <summary>
      /// Code Checksum Property
      /// </summary>
      public UInt32 CodeChecksum { get { return m_nCodeChecksum; } }

      /// <summary>
      /// News Checksum Property
      /// </summary>
      public UInt32 NewsChecksum { get { return m_nNewsChecksum; } }

      /// <summary>
      /// Server Version Property
      /// </summary>
      public UInt32 ServerVersion { get { return m_nServerVersion; } }

      /// <summary>
      /// Request Registration Property
      /// </summary>
      public Boolean RequestRegistration { get { return (m_nRequestRegistration == 0x01); } }

      /// <summary>
      /// Is VIP Property
      /// </summary>
      public Boolean IsVIP { get { return (m_nIsVIP == 0x01); } }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Create a memory stream to hold the packet
            m_responseError = (LoginResponseError)value[1];
            m_nServerVersion = BitConverter.ToUInt32 (value, 2);
            m_nIsVIP = value[6];
            m_nExecutableChecksum = BitConverter.ToUInt32 (value, 10);
            m_nRequestRegistration = value[19];
            m_nCodeChecksum = BitConverter.ToUInt32 (value, 20);
            m_nNewsChecksum = BitConverter.ToUInt32 (value, 24);
         }

         get
         {
            // Return the packet data
            return new Byte[1];
         }
      }

      /// <summary>
      /// Property to get the response string
      /// </summary>
      public String ResponseString 
      {
         get
         {
            String response = "Unknown";

            switch (m_responseError)
            {
            case LoginResponseError.Success:
               response = "Login Successful";
               break;

            case LoginResponseError.UnknownUser:
               response = "Unknown player, continue as new user?";
               break;

            case LoginResponseError.InvalidPassword:
               response = "Invalid password for specified user.";
               break;

            case LoginResponseError.FullArena:
               response = "This arena is currently full, try again later.";
               break;

            case LoginResponseError.LockedOut:
               response = "You have been locked out of SubSpace.";
               break;

            case LoginResponseError.NoPermission:
            case LoginResponseError.NoPermission2:
               response = "You do not have permission to play in this arena.";
               break;

            case LoginResponseError.SpectateOnly:
               response = "You only have permission to spectate in this arena.";
               break;

            case LoginResponseError.TooManyPoints:
               response = "You have too many points to play in this arena.";
               break;

            case LoginResponseError.SlowConnection:
               response = "Your connection appears to be too slow to play in this arena.";
               break;

            case LoginResponseError.NoNewConnections:
               response = "The server is currently not accepting new connections.";
               break;

            case LoginResponseError.InvalidName:
               response = "Invalid user name entered, please pick a different name.";
               break;

            case LoginResponseError.ObsceneName:
               response = "Possibly offensive user name entered, please pick a different name.";
               break;

            case LoginResponseError.BillerDown:
               response = "NOTICE: Server difficulties; this zone is currently not keeping track of scores.";
               break;

            case LoginResponseError.WrongPassword:
               response = "You have tried to login using the wrong password too many times.  Try again in a few hours.";
               break;

            case LoginResponseError.ExperiencedOnly:
               response = "This zone is restricted to experienced players only.";
               break;

            case LoginResponseError.UsingDemoVersion:
               response = "You are currently using the demo version.";
               break;

            case LoginResponseError.TooManyDemos:
               response = "This arena is currently has the maximum Demo players allowed.";
               break;

            case LoginResponseError.ClosedToDemos:
               response = "This arena is closed to Demo players.";
               break;

            case LoginResponseError.NeedModerator:
               response = "Moderator access required for this zone.";
               break;
            }

            return response;
         }
      }
   }
}
