//-----------------------------------------------------------------------
//
// NAME:        BotInstance.Commands.cs
//
// PROJECT:     Battle Core Bot Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Bot Instance Commands implementation.
//
// NOTES:       None.
//
// $History: BotInstance.Commands.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using BattleCore.Session;
using BattleCore.Protocol;
using BattleCore.Events;
using BattleCore.Core;
using BattleCore.Settings;

// Namespace declaration
namespace BattleCore.Bot
{
   // Partial class definition for BotInstance
   partial class BotInstance
   {
      /// <summary>
      /// Handle a chat command packet sent to the bot
      /// </summary>
      /// <param name="chatEvent">Chat event</param>
      /// <returns>Command Handled state</returns>
      private bool HandleChatCommands (ChatEvent chatEvent)
      {
         bool bCommandHandled = false;

         // If the chat event is a bot command
         if (chatEvent.Message.StartsWith ("!"))
         {
            // Log the command in the command log
            LogBotCommand (chatEvent);
         }

         // Bot Help Command
         if (chatEvent.Message.StartsWith ("!core help"))
         {
            // Handle the help command
            HandleBotHelpCommand (chatEvent);
         }

         // List available bots
         if (chatEvent.Message.StartsWith("!help"))
         {
             // Handle the help command
             HandleHelpCommand(chatEvent);
         }

         // Display bot information
         if (chatEvent.Message.StartsWith ("!info"))
         {
            // Handle the info command
            HandleInfoCommand (chatEvent);
         }

         // If a private command is received and the user is a SMod+
         if (chatEvent.ModLevel >= ModLevels.SMod)
         {
            // command to list available behaviors
            if (chatEvent.Message.StartsWith ("!blist"))
            {
               // List all loaded and available behaviors
               ListBehaviors (chatEvent);

               // Set the command handled
               bCommandHandled = true;
            }

            // command to load a behavior
            else if (chatEvent.Message.StartsWith ("!load "))
            {
               // Load the requested behavior
               LoadBehavior (chatEvent);

               // Set the command handled
               bCommandHandled = true;
            }

            // command to unload a behavior
            else if (chatEvent.Message.StartsWith ("!unload "))
            {
               // Unload the requested behavior
               UnloadBehavior (chatEvent);
            }

            // Spawn a new bot
            else if (chatEvent.Message.StartsWith ("!spawn "))
            {
               // Handle the spawn command
               HandleSpawnCommand (chatEvent);

               // Set the command handled
               bCommandHandled = true;
            }

            // Spawn a new bot
            else if (chatEvent.Message.CompareTo ("!kill") == 0)
            {
               // Handle the kill command
               HandleKillCommand (chatEvent);
            }
            // Spawn a new bot
            else if (chatEvent.Message.CompareTo("!forcekill") == 0)
            {
                // Handle the kill command
                HandleKillCommand(chatEvent);
            }

            // List available and loaded bots
            else if (chatEvent.Message.StartsWith ("!listbots"))
            {
               // Handle the list bots command
               HandleListBotsCommand (chatEvent);

               // Set the command handled
               bCommandHandled = true;
            }
         }
         // List available and loaded bots
         if (chatEvent.Message.StartsWith("!unf"))
         {
             // Handle the unf command
             HandleUnfCommand(chatEvent);
         }

         // Return the command handled state
         return bCommandHandled;
      }

      /// <summary>
      /// Handle the list information command.  This command lists
      /// information about the core and all loaded behaviors.
      /// </summary>
      /// <param name="chatEvent"></param>
      private void HandleInfoCommand (ChatEvent chatEvent)
      {
         // Create the chat response message
         ChatPacket response = new ChatPacket ();

         // Set the player Identifier to reply to
         response.Event.PlayerId = chatEvent.PlayerId;
         response.Event.ChatType = ChatTypes.Private;

         Version coreVersion = Assembly.GetExecutingAssembly ().GetName ().Version;
         
         string strVersion = coreVersion.ToString (3);

         // Set up the header block
         response.Event.Message = "< =================================================================== >";
         m_session.TransmitPacket (response);
         response.Event.Message = "< BattleCore version " + strVersion.PadRight(32) + "         by udp  >";
         m_session.TransmitPacket (response);
         response.Event.Message = "< =================================================================== >";
         m_session.TransmitPacket (response);
         response.Event.Message = "< Loaded Behavior         Author      Description                     >";
         m_session.TransmitPacket (response);
         response.Event.Message = "< =================================================================== >";
         m_session.TransmitPacket (response);
          
         // Send information about each loaded behavior
         foreach (Object behavior in m_core.LoadedBehaviors)
         {
            try
            {
               // Get the attributes for the listener
               object[] os = m_core.GetAttributes (behavior.GetType ().ToString ());

               // Look for the behavior attribute
               foreach (object obj in os)
               {
                  // If the object is a Behavior attribute
                  if (obj is BehaviorAttribute)
                  {
                     // Build the behavior information from the attribute
                     BehaviorAttribute ba = (obj as BehaviorAttribute);
                     String strTitle = ba.Title + " " + ba.Version;
                     response.Event.Message = "< " + strTitle.PadRight (24, ' ') + ba.Developers.PadRight (12, ' ') + ba.Description.PadRight (32, ' ') + ">";
                     m_session.TransmitPacket (response);
                  }
               }
            }
            catch (Exception e)
            {
               // Send the exception message
               response.Event.Message = e.Message;
               m_session.TransmitPacket (response);
            }
         }

         // Close the information block
         response.Event.Message = "< =================================================================== >";
         m_session.TransmitPacket (response);
      }

      /// <summary>
      /// Handle the list all behaviors command.  This method lists
      /// all loaded and available behaviors.
      /// </summary>
      /// <param name="chatEvent">listbehaviors chat event</param>
      private void ListBehaviors (ChatEvent chatEvent)
      {
         // Create the chat response message
         ChatPacket response = new ChatPacket ();

         // Set the response message information
         response.Event.PlayerId = chatEvent.PlayerId;
         response.Event.ChatType = ChatTypes.Private;

         try
         {
            response.Event.Message = "< ======================= >";
            m_session.TransmitPacket (response);
            response.Event.Message = "<    Loaded Behaviors     >";
            m_session.TransmitPacket (response);
            response.Event.Message = "< ======================= >";
            m_session.TransmitPacket (response);

            if (m_core.LoadedBehaviors.Count > 0)
            {
               foreach (Object behavior in m_core.LoadedBehaviors)
               {
                  response.Event.Message = "<   " + behavior.GetType ().ToString ();

                  // Send the response message
                  m_session.TransmitPacket (response);
               }
            }
            else
            {
               response.Event.Message = "< NO BEHAVIORS LOADED ";
               m_session.TransmitPacket (response);
            }

            response.Event.Message = "< ======================= >";
            m_session.TransmitPacket (response);
            response.Event.Message = "<   Available Behaviors   >";
            m_session.TransmitPacket (response);
            response.Event.Message = "< ======================= >";
            m_session.TransmitPacket (response);

            if (m_core.AvailableBehaviors.Length > 0)
            {
               foreach (string behavior in m_core.AvailableBehaviors)
               {
                  response.Event.Message = "< " + behavior;

                  // Send the response message
                  m_session.TransmitPacket (response);
               }
            }
            else
            {
               response.Event.Message = "< NO AVAILABLE BEHAVIORS";
               m_session.TransmitPacket (response);
            }

            response.Event.Message = "< ======================= >";
            m_session.TransmitPacket (response);
         }
         catch (Exception e)
         {
            // Send the exception message
            response.Event.Message = e.Message;
            m_session.TransmitPacket (response);
         }
      }

      /// <summary>
      /// Handle the load behavior command.  This method finds the
      /// requested behavior in the available behaviors and adds
      /// it to the current event listeners.
      /// </summary>
      /// <param name="chatEvent">loadbehavior chat event</param>
      private void LoadBehavior (ChatEvent chatEvent)
      {
         // Create the chat response message
         ChatPacket response = new ChatPacket ();

         // Set the response message information
         response.Event.PlayerId = chatEvent.PlayerId;
         response.Event.ChatType = ChatTypes.Private;

         try
         {
            // Get the name of the bot to load
            String behavior = chatEvent.Message.Substring (6, chatEvent.Message.Length - 6);

            // Create a new instance of the behavior
            if (m_core.CreateInstance (behavior) != null)
            {

               // Read the current behaviors from the file
               List<string> behaviors = new List<string> ();
               behaviors.AddRange (m_core.LoadBehaviorFile ());

               // Add the new behavior to the list
               behaviors.Add (behavior);

               // Save the loaded behaviors to a file
               m_core.SaveBehaviorFile (behaviors.ToArray ());

               // Send the success response message
               response.Event.Message = behavior + " Loaded successfully!";
               m_session.TransmitPacket (response);
            }
            else
            {
               // Send the Load failure response message
               response.Event.Message = "Failed to load: " + behavior + " is not a valid behavior!";
               m_session.TransmitPacket (response);
            }
         }
         catch (Exception e)
         {
            // Send the exception message
            response.Event.Message = e.Message;
            m_session.TransmitPacket (response);
         }
      }

      /// <summary>
      /// Handle the unload behavior command.  This method finds the
      /// requested behavior in the loaded behaviors and removes
      /// it from the current event listeners.
      /// </summary>
      /// <param name="chatEvent">loadbehavior chat event</param>
      private void UnloadBehavior (ChatEvent chatEvent)
      {
         // Create the chat response message
         ChatPacket response = new ChatPacket ();

         // Set the response message information
         response.Event.PlayerId = chatEvent.PlayerId;
         response.Event.ChatType = ChatTypes.Private;

         try
         {
            // Get the name of the bot to load
            String behavior = chatEvent.Message.Substring (8, chatEvent.Message.Length - 8);

            // Destroy the instance of the behavior
            if (m_core.DestroyInstance (behavior) >= 0)
            {
               // Read the current behaviors from the file
               List<string> behaviors = new List<string> ();
               behaviors.AddRange (m_core.LoadBehaviorFile ());

               // Remove the behavior from the list
               behaviors.Remove (behavior);

               // Save the loaded behaviors to a file
               m_core.SaveBehaviorFile (behaviors.ToArray ());

               // Send the unload success message
               response.Event.Message = behavior + " successfully unloaded!";
               m_session.TransmitPacket (response);
            }
            else
            {
               // Send the failure to unload message
               response.Event.Message = "Failed to unload: " + behavior + " is not a loaded behavior!";
               m_session.TransmitPacket (response);
            }
         }
         catch (Exception e)
         {
            // Send the exception message
            response.Event.Message = e.Message;
            m_session.TransmitPacket (response);
         }
      }

       /// <summary>
       /// Handles BotGetInfoEvent - used to send back bot info to modules
       /// </summary>
       /// <param name="chatEvent"></param>
      /*private void HandleBotInfoCommand(ChatEvent chatEvent)
      {
          BotGetInfoEvent infoEvent = new BotGetInfoEvent ();
          try
          {
              infoEvent.PlayerId = chatEvent.PlayerId;
              infoEvent.ModLevel = chatEvent.ModLevel;
              // Send the botinfo event to the core handler
              m_core.OnCoreEvent(this, infoEvent);
          }
          catch (Exception e)
          {
              // Create the chat response message
              ChatPacket response = new ChatPacket();

              // Set the response message information
              response.Event.PlayerId = chatEvent.PlayerId;
              response.Event.ChatType = ChatTypes.Private;

              // Set the response message
              response.Event.Message = e.Message;

              // Send the exception message to the player
              m_session.TransmitPacket(response);
          }
      }*/

      /// <summary>
      /// Handle the list information command.  This command lists
      /// information about the core and all loaded behaviors.
      /// </summary>
      /// <param name="chatEvent"></param>
      private void HandleSpawnCommand (ChatEvent chatEvent)
      {
         // Create the bot spawn event
         BotSpawnEvent spawnEvent = new BotSpawnEvent ();

         try
         {
            // parse the spawn parameters from the message
            spawnEvent.Parameters = chatEvent.Message.Remove (0, 7);
            spawnEvent.PlayerId = chatEvent.PlayerId;
            spawnEvent.ModLevel = chatEvent.ModLevel;

            // Send the spawn event to the core handler
            m_core.OnCoreEvent (this, spawnEvent);
         }
         catch (Exception e)
         {
            // Create the chat response message
            ChatPacket response = new ChatPacket ();

            // Set the response message information
            response.Event.PlayerId = chatEvent.PlayerId;
            response.Event.ChatType = ChatTypes.Private;

            // Set the response message
            response.Event.Message = e.Message;

            // Send the exception message to the player
            m_session.TransmitPacket (response);
         }
      }

      /// <summary>
      /// Handle the list bots command.  This command lists
      /// information about the available and loaded bots.
      /// </summary>
      /// <param name="chatEvent"></param>
      private void HandleListBotsCommand (ChatEvent chatEvent)
      {
         // Create the bot spawn event
         BotListEvent listEvent = new BotListEvent ();

         try
         {
            // parse the spawn parameters from the message
            listEvent.PlayerId = chatEvent.PlayerId;

            // Send the spawn event to the core handler
            m_core.OnCoreEvent (this, listEvent);
         }
         catch (Exception e)
         {
            // Create the chat response message
            ChatPacket response = new ChatPacket ();

            // Set the response message information
            response.Event.PlayerId = chatEvent.PlayerId;
            response.Event.ChatType = ChatTypes.Private;

            // Set the response message
            response.Event.Message = e.Message;

            // Send the exception message to the player
            m_session.TransmitPacket (response);
         }
      }
      /// <summary>
      /// Handle the kill bot command.  
      /// </summary>
      /// <param name="chatEvent"></param>
      private void HandleKillCommand (ChatEvent chatEvent)
      {
         // The kill command can only be sent privately
         if (chatEvent.ChatType == ChatTypes.Private)
         {
            // Create the bot spawn event
            BotKillEvent killEvent = new BotKillEvent ();

            try
            {
               // parse the spawn parameters from the message
               killEvent.PlayerId = chatEvent.PlayerId;
               killEvent.Message = chatEvent.Message;

               // Send the spawn event to the core handler
               m_core.OnCoreEvent (this, killEvent);
            }
            catch (Exception e)
            {
               // Create the chat response message
               ChatPacket response = new ChatPacket ();

               // Set the response message information
               response.Event.PlayerId = chatEvent.PlayerId;
               response.Event.ChatType = ChatTypes.Private;

               // Set the response message
               response.Event.Message = e.Message;

               // Send the exception message to the player
               m_session.TransmitPacket (response);
            }
         }
      }

      /// <summary>
      /// Log a command to the command logfile
      /// </summary>
      /// <param name="command"></param>
      void LogBotCommand (ChatEvent command)
      {
         // Get the log file path
         string logFile = Path.Combine (m_core.BotDirectory, "commands.log");

         // Create the file stream writer
         StreamWriter sw = new StreamWriter (logFile, true);

         // Create the time stamp
         string strTime = DateTime.Now.ToString ("G") + " ";
         
         // Write the command to the log file
         sw.WriteLine (strTime + "(" + command.ChatType.ToString() + ") "+ command.PlayerName + "> " + command.Message);

         // Close the stream writer
         sw.Close ();
      }
   }
}
