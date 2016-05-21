
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCore.Settings
{
   /// <summary>
   /// Session Settings object used to configure session parameters.
   /// </summary>
   [Serializable()]
   public class SessionSettings
   {
      #region Members
      private String m_strUserName = "";
      /// <summary>
      /// Gets the User Name.
      /// </summary>
      public String UserName
      {
         get { return m_strUserName; }
         set { m_strUserName = value; }
      }
      private String m_strPassword = "";
      /// <summary>
      /// Gets the Password.
      /// </summary>
      public String Password
      {
         get { return m_strPassword; }
         set { m_strPassword = value; }
      }
      private String m_strStaffPassword = "";
      /// <summary>
      /// Gets the Staff Password.
      /// </summary>
      public String StaffPassword
      {
         get { return m_strStaffPassword; }
         set { m_strStaffPassword = value; }
      }
      private String m_strServerAddress = "";
      /// <summary>
      /// Gets the Server Address.
      /// </summary>
      public String ServerAddress
      {
         get { return m_strServerAddress; }
         set { m_strServerAddress = value; }
      }
      private String m_strInitialArena = "";
      /// <summary>
      ///Initial Arena to enter.
      /// </summary>
      public String InitialArena
      {
         get { return m_strInitialArena; }
         set { m_strInitialArena = value; }
      }
      private String m_strPacketLogFile = "";
      /// <summary>
      /// Gets the Packet log file.
      /// </summary>
      public String PacketLogFile
      {
         get { return m_strPacketLogFile; }
      }
      private UInt16 m_nServerPort = 5000;
      /// <summary>
      /// Gets the Server Port.
      /// </summary>
      public UInt16 ServerPort
      {
         get { return m_nServerPort; }
         set { m_nServerPort = value; }
      }
      private UInt32 m_nActivityTimeout = 10;
      /// <summary>
      /// Gets the Activity timeout for disconnect
      /// </summary>
      public UInt32 ActivityTimeout
      {
         get { return m_nActivityTimeout; }
      }
      private UInt16 m_nServerSyncTime = 5;
      /// <summary>
      /// Gets the Time between server time syncs
      /// </summary>
      public UInt16 ServerSyncTime
      {
         get { return m_nServerSyncTime; }
      }
      private UInt32 m_nRetransmitTime = 1250;
      /// <summary>
      /// Gets the Retransmit time in milliseconds
      /// </summary>
      public UInt32 RetransmitTime
      {
         get { return m_nRetransmitTime; }
      }
      private Boolean m_bForceContinuum = false;
      /// <summary>
      /// Gets the Force continuum flag.
      /// </summary>
      public Boolean ForceContinuum
      {
         get { return m_bForceContinuum; }
      }
      private String m_strRealName = "Battle Core";
      /// <summary>
      /// Gets the Real Name.
      /// </summary>
      public String RealName
      {
         get { return m_strRealName; }
      }
      private String m_strEmailAddress = "alphacore@sscxalpha.com";
      /// <summary>
      /// Gets the Email Address.
      /// </summary>
      public String EmailAddress
      {
         get { return m_strEmailAddress; }
      }
      private String m_strCity = "Battle Zone";
      /// <summary>
      /// Gets the City.
      /// </summary>
      public String City
      {
         get { return m_strCity; }
      }
      private String m_strState = "SSCX";
      /// <summary>
      /// Gets the State.
      /// </summary>
      public String State
      {
         get { return m_strState; }
      }
      private String m_strRegisteredName = "Battle Core";
      /// <summary>
      /// Gets the Registered Name.
      /// </summary>
      public String RegisteredName
      {
         get { return m_strRegisteredName; }
      }
      private String m_strOrganization = "SSCX Battle West SVS";
      /// <summary>
      /// Gets the Organization.
      /// </summary>
      public String Organization
      {
         get { return m_strOrganization; }
      }
      #endregion
   }
}
