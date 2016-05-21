
using System;
using System.Collections.Generic;
using System.Text;

// namespace declaration
namespace BattleCore.Settings
{
   /// <summary>
   /// BotSettings object.  
   /// </summary>
   [Serializable()]
   public class BotSettings
   {
      private string m_botDirectory = "";
      /// <summary>
      /// Get the bot root directory
      /// </summary>
      public string BotDirectory
      {
         get { return m_botDirectory; }
         set { m_botDirectory = value; }
      }
      private bool m_bAutoLoad = false;
      /// <summary>
      /// Bot automatic load property
      /// </summary>
      public bool AutoLoad
      {
         get { return m_bAutoLoad; }
         set { m_bAutoLoad = value; }
      }

      private string m_sqlConnectString = "";
      /// <summary>
      /// Get the SQL connection string
      /// </summary>
      public string SqlConnectString
      {
         get { return m_sqlConnectString; }
         set { m_sqlConnectString = value; }
      }

      private SessionSettings m_sessionSettings = new SessionSettings ();
      /// <summary>
      /// Property to get the session settings
      /// </summary>
      public SessionSettings SessionSettings
      {
         set { m_sessionSettings = value; }
         get { return m_sessionSettings; }
      }
   }
}
