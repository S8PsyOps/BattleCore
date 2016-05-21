//-----------------------------------------------------------------------
//
// NAME:        BattleCore.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Battle Core implementation.
//
// NOTES:       None.
//
// $History: BattleCore.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ServiceProcess;
using BattleCore.Core;
using BattleCore.Events;
using BattleCore.Protocol;
using BattleCore.Bot;
using BattleCore.Settings;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// Namespace declaration
namespace BattleCore
{
   /// <summary>
   /// Battle Core object.  This object is the root of the 
   /// core, and created instances of all configured bots.
   /// </summary>
   public class BattleCore
   {
      // Create the tray icon 
      private NotifyIcon m_trayIcon = new NotifyIcon ();

      /// <summary>
      /// List of all bot instances managed by the core
      /// </summary>
      SortedList<string, BotInstance> m_activeBots = new SortedList<string,BotInstance>();

      /// <summary>
      /// List of all bot instances managed by the core
      /// </summary>
      SortedList<string, BotInstance> m_availableBots = new SortedList<string,BotInstance>();

      /// <summary>
      /// BattleCore constructor
      /// </summary>
      public BattleCore ()
      {
         // Initialize the tray icon
         m_trayIcon.BalloonTipText = "BattleCore Management Service";
       //  m_trayIcon.Icon = new Icon("alphacore.ico");
      }

      /// <summary>
      /// OnStart: Put startup code here
      ///  - Start threads, get inital data, etc.
      /// </summary>
      /// <param name="args"></param>
      public void OnStart (string[] args)
      {
         string assemblyLoc = Assembly.GetExecutingAssembly ().Location;
         string currentDirectory = assemblyLoc.Substring (0, assemblyLoc.LastIndexOf (Path.DirectorySeparatorChar) + 1);
         string securityFile = Path.Combine (currentDirectory, "security.dat");

         // Check if the security file exists
         if (File.Exists (securityFile))
         {
            try
            {
               // Open the configuration file 
               FileStream securityStream = new FileStream (securityFile, FileMode.Open);

               // Create the service provider
               DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider ();

               // Create the encryption keys
               cryptic.Key = ASCIIEncoding.ASCII.GetBytes ("ALTT1029");
               cryptic.IV = ASCIIEncoding.ASCII.GetBytes ("ALTT1029");

               // Create the Decryption stream provider
               CryptoStream crStream = new CryptoStream (securityStream,
                                                         cryptic.CreateDecryptor (),
                                                         CryptoStreamMode.Read);

               // Create the binary formatter for the settings
               BinaryFormatter formatter = new BinaryFormatter ();

               // Serialize the decrypted stream into the core settings
               String strPassword = (String)formatter.Deserialize (crStream);

               // Close the file stream
               crStream.Close ();

               // Create the core settings object
               CoreSettings coreSettings = new CoreSettings (strPassword);

               // Get a List of each bot configuration
               foreach (BotSettings botSettings in coreSettings.ConfiguredBots)
               {
                  // Create the new bot instance
                  BotInstance bot = new BotInstance (botSettings);

                  // Add the core event manager
                  bot.CoreEventManager += (new BotEvent (HandleCoreEvents));

                  // if the bot is to be loaded automatically
                  if (botSettings.AutoLoad)
                  {
                     // Add a bot to the bot list
                     m_activeBots.Add (bot.Settings.SessionSettings.UserName.ToUpper (), bot);

                     // Start the bot
                     bot.Start ();
                  }
                  else
                  {
                     // Add a bot to the bot list
                     m_availableBots.Add (bot.Settings.SessionSettings.UserName.ToUpper (), bot);
                  }
               }

               // Show the tray icon
               m_trayIcon.Visible = true;
            }
            catch (Exception)
            {

            }
         }
      }

      /// <summary>
      /// OnStop: Put your stop code here
      /// - Stop threads, set final data, etc.
      /// </summary>
      public void OnStop ()
      {
         // Shutdown all active bots
         foreach (BotInstance bot in m_activeBots.Values)
         {
            bot.Close ();
         }

         // Clear the bot lists
         m_activeBots.Clear ();
         m_availableBots.Clear ();

         // Show the tray icon
         m_trayIcon.Visible = false;
      }

      /// <summary>
      /// OnPause: Put your pause code here
      /// - Pause working threads, etc.
      /// </summary>
      public void OnPause ()
      {

      }

      /// <summary>
      /// OnContinue: Put your continue code here
      /// - Un-pause working threads, etc.
      /// </summary>
      public void OnContinue ()
      {

      }

      /// <summary>
      /// OnShutdown(): Called when the System is shutting down
      /// - Put code here when you need special handling
      ///   of code that deals with a system shutdown, such
      ///   as saving special data before shutdown.
      /// </summary>
      public void OnShutdown ()
      {
         // Shutdown all the bots
         foreach (BotInstance bot in m_activeBots.Values)
         {
            bot.Close ();
         }

         // Clear the active bots list
         m_activeBots.Clear ();

         // Clear the available bots list
         m_availableBots.Clear ();

         // Show the tray icon
         m_trayIcon.Visible = false;
      }

      /// <summary>
      /// OnCustomCommand(): If you need to send a command to your
      ///   service without the need for Remoting or Sockets, use
      ///   this method to do custom methods.
      /// </summary>
      /// <param name="command">Arbitrary Integer between 128 and 256</param>
      public void OnCustomCommand (int command)
      {
         //  A custom command can be sent to a service by using this method:
         //#  int command = 128; //Some Arbitrary number between 128 & 256
         //#  ServiceController sc = new ServiceController("NameOfService");
         //#  sc.ExecuteCommand(command);

      }

      /// <summary>
      /// OnPowerEvent(): Useful for detecting power status changes,
      ///   such as going into Suspend mode or Low Battery for laptops.
      /// </summary>
      /// <param name="powerStatus">The Power Broadcase Status (BatteryLow, Suspend, etc.)</param>
      public bool OnPowerEvent (PowerBroadcastStatus powerStatus)
      {
         return false;
      }

      /// <summary>
      /// OnSessionChange(): To handle a change event from a Terminal Server session.
      ///   Useful if you need to determine when a user logs in remotely or logs off,
      ///   or when someone logs into the console.
      /// </summary>
      /// <param name="changeDescription"></param>
      public void OnSessionChange (SessionChangeDescription changeDescription)
      {

      }

      /// <summary>
      /// Handle a broadcast event sent to all loaded bots within the core.
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="e">Event</param>
      internal void HandleCoreEvents (Object sender, EventArgs e)
      {
         // if the event is a bot spawn event
         if (e is BotSpawnEvent)
         {
            // Handle the spawn event
            HandleBotSpawnEvent (sender, e as BotSpawnEvent);
         }
         // if the event is a bot spawn event
         else if (e is BotListEvent)
         {
            // Handle the spawn event
            HandleBotListEvent (sender, e as BotListEvent);
         }
         // if the event is a bot spawn event
         else if (e is BotKillEvent)
         {
            // Handle the spawn event
            HandleBotKillEvent (sender, e as BotKillEvent);
         }
         // if the event is a bot spawn event
         else if (e is BotGetInfoEvent)
         {
             // Handle the spawn event
             HandleGetBotInfoEvent(sender, e as BotGetInfoEvent);
         }
         else
         {
            // Send the event to each loaded bot
            foreach (BotInstance bot in m_activeBots.Values)
            {
               // Send the event
               bot.SendBotEvent (e);
            }
         }
      }
      /// <summary>
      /// Handle a bot list command to list all bots
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="listEvent"></param>
      internal void HandleBotListEvent (object sender, BotListEvent listEvent)
      {
         // Send the response message to the bot
         BotInstance sourceBot = (sender as BotInstance);

         // Create the chat response packet
         ChatEvent response = new ChatEvent ();

         // Set the player Identifier to reply to
         response.PlayerId = listEvent.PlayerId;
         response.ChatType = ChatTypes.Private;

         response.Message = "< ================================================= >";
         sourceBot.SendGameEvent (response);
         response.Message = "<             BattleCore Configured Bots             >";
         sourceBot.SendGameEvent (response);
         response.Message = "<                                                   >";
         sourceBot.SendGameEvent (response);
         response.Message = "<  Name                   Arena           Status    >";
         sourceBot.SendGameEvent (response);
         response.Message = "< ================================================= >";
         sourceBot.SendGameEvent (response);

         foreach (BotInstance bot in m_activeBots.Values)
         {
            // Only list the bots configured for the same server
            if (bot.Settings.SessionSettings.ServerAddress == sourceBot.Settings.SessionSettings.ServerAddress)
            {
               // List the bot status as active
               response.Message = "< " + bot.Settings.SessionSettings.UserName.PadRight (24)
                                       + bot.Settings.SessionSettings.InitialArena.PadRight (16)
                                       + "ACTIVE    >";
               sourceBot.SendGameEvent (response);
            }
         }

         foreach (BotInstance bot in m_availableBots.Values)
         {
            // Only list the bots configured for the same server
            if (bot.Settings.SessionSettings.ServerAddress == sourceBot.Settings.SessionSettings.ServerAddress)
            {
               // List the bot as inactive
               response.Message = "< " + bot.Settings.SessionSettings.UserName.PadRight (24)
                                       + bot.Settings.SessionSettings.InitialArena.PadRight (16)
                                       + "          >";
               sourceBot.SendGameEvent (response);
            }
         }

         // Send the response message to the bot
         response.Message = "< ================================================= >";
         sourceBot.SendGameEvent (response);
      }

       /// <summary>
       /// Handle BotGetInfoEvents and sends back info to user - used for modules
       /// Going to try and keep this internal only.
       /// </summary>
       /// <param name="sender">Sending object</param>
       /// <param name="infoEvent"></param>
      internal void HandleGetBotInfoEvent(object sender, BotGetInfoEvent infoEvent)
      {
          // Send the response message to the bot
          BotInstance sourceBot = (sender as BotInstance);

          // Create the chat response packet
          ChatEvent response = new ChatEvent();

          // Get the settings for the source bot
          BotSettings sourceSettings = sourceBot.Settings;

          // Set the player Identifier to reply to
          response.PlayerName = sourceSettings.SessionSettings.UserName;
          response.ChatType = ChatTypes.Private;


          response.Message = "@BotInfo@:" + sourceSettings.SessionSettings.UserName + ":" + sourceSettings.SessionSettings.InitialArena;

          // Send the response message to the bot
          sourceBot.SendGameEvent(response);
      }

      /// <summary>
      /// Handle a bot spawn command to create a new bot instance
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="spawnEvent"></param>
      internal void HandleBotSpawnEvent (object sender, BotSpawnEvent spawnEvent)
      {
         // Send the response message to the bot
         BotInstance sourceBot = (sender as BotInstance);

         // Create the chat response packet
         ChatEvent response = new ChatEvent ();

         // Set the player Identifier to reply to
         response.PlayerId = spawnEvent.PlayerId;
         response.ChatType = ChatTypes.Private;

         // Check if the spawn bot name is valid
         if (spawnEvent.BotName.Length > 0)
         {
            // Find the bot in the available bot list
            int nIndex = m_availableBots.IndexOfKey (spawnEvent.BotName.ToUpper());

            if (nIndex >= 0)
            {
               // Get the bot instance from the available bots
               BotInstance bot = m_availableBots[spawnEvent.BotName.ToUpper()];

               if (spawnEvent.Arena.Length > 0)
               {
                  // Set the new arena to enter
                  bot.Settings.SessionSettings.InitialArena = spawnEvent.Arena;
               }

               // Remove the bot from the available list
               m_availableBots.Remove (spawnEvent.BotName.ToUpper());

               // Add the bot to the active bot list
               m_activeBots.Add (spawnEvent.BotName.ToUpper(), bot);

               // Create the response message
               response.Message = "Spawning bot: " + spawnEvent.BotName;

               // Start the bot
               bot.Start ();
            }
            else if (m_activeBots.IndexOfKey (spawnEvent.BotName.ToUpper()) != -1)
            {
               response.Message = "Error: " + spawnEvent.BotName + "is already active!";
            }
            else
            {
               response.Message = "Error: " + spawnEvent.BotName + "is not a valid bot name!";
            }
            /*
            else if (spawnEvent.ModLevel == ModLevels.Sysop)
            {

               // Create the core settings object
               CoreSettings coreSettings = new CoreSettings ();

               // Create the bot settings object
               BotSettings spawnSettings = new BotSettings ();

               // Get the settings for the source bot
               BotSettings sourceSettings = sourceBot.Settings;

               // Set the bot settings
               spawnSettings.BotDirectory = spawnEvent.BotName;
               spawnSettings.AutoLoad = false;

               // Set the session connection settings
               spawnSettings.SessionSettings.UserName = spawnEvent.BotName;
               spawnSettings.SessionSettings.Password = sourceSettings.SessionSettings.Password;
               spawnSettings.SessionSettings.StaffPassword = sourceSettings.SessionSettings.StaffPassword;
               spawnSettings.SessionSettings.InitialArena = spawnEvent.Arena;

               // Set the server address and port of the sending bot
               spawnSettings.SessionSettings.ServerAddress = sourceSettings.SessionSettings.ServerAddress;
               spawnSettings.SessionSettings.ServerPort = sourceSettings.SessionSettings.ServerPort;

               // Add the new bot to the core settings
               coreSettings.ConfiguredBots.Add (spawnSettings);
               coreSettings.WriteSettings ();

               // Create the new bot instance
               BotInstance bot = new BotInstance (spawnSettings);

               // Add the core event manager
               bot.CoreEventManager += (new BotEvent (HandleCoreEvents));

               // Start the bot
               bot.Start ();

               // Add a bot to the bot list
               m_activeBots.Add (spawnEvent.BotName.ToUpper(), bot);

               // Create the response message
               response.Message = "Bot does not exist. Creating a new bot: " + spawnEvent.BotName;
            }
            */
         }
         else
         {
            response.Message = "!spawn usage:  !spawn bot   !spawn bot:arena";
         }

         // Send the response message to the bot
         sourceBot.SendGameEvent (response);
      }

      /// <summary>
      /// Handle a bot kill command to kill the bots
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="listEvent"></param>
      internal void HandleBotKillEvent (object sender, BotKillEvent listEvent)
      {
         // Send the response message to the bot
         BotInstance sourceBot = (sender as BotInstance);

         // Make sure the bot is not a public arena bot
         if ((sourceBot.Settings.SessionSettings.InitialArena.Length > 0)||(listEvent.Message.Equals("!forcekill")))
         {
            // close the bot
            sourceBot.Close ();

            // Create a fresh new bot instance
            BotInstance newBot = new BotInstance (sourceBot.Settings);
            newBot.CoreEventManager += (new BotEvent (HandleCoreEvents));

            // Add the bot to the available list
            m_availableBots.Add (newBot.Settings.SessionSettings.UserName.ToUpper (), newBot);

            // Remove the bot from the active list
            m_activeBots.Remove (sourceBot.Settings.SessionSettings.UserName.ToUpper ());
         }
         else
         {
            // Create the chat response packet
            ChatEvent response = new ChatEvent ();

            // Set the player Identifier to reply to
            response.PlayerId = listEvent.PlayerId;
            response.ChatType = ChatTypes.Private;

            response.Message = "ERROR: unable to !kill bots in public arena!";
            sourceBot.SendGameEvent (response);
         }
      }
   }
}
