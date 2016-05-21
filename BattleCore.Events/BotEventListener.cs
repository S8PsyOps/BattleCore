/***********************************************************************
*
* NAME:        BotEventListener.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Bot Event Listener implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE      AUTHOR     CHANGES
* --------  ---------  -------------------------------------------------
* 12-29-06  udp        Initial Creation
*
************************************************************************/

// Namespace usage
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using BattleCore.Events;

// BattleCore namespace
namespace BattleCore
{
   /// <summary>Delegate to use to send an event to the game server</summary>
   /// <param name="sender">The Object sending the event</param>
   /// <param name="e">A Wrapper for the arguments for this Event</param>
   public delegate void NotifyGame (object sender, EventArgs e);

   /// <summary>Delegate to use to throw an Event to all loaded behaviors</summary>
   /// <param name="sender">The Object sending the event</param>
   /// <param name="e">A Wrapper for the arguments for this Event</param>
   public delegate void NotifyCore (object sender, EventArgs e);

   /// <summary>The Delegate to use to throw an Event to the Bot</summary>
   /// <param name="sender">The Object sending the event</param>
   /// <param name="e">A Wrapper for the arguements for this Event</param>
   public delegate void NotifyBot (object sender, EventArgs e);

   /// <summary>
   /// Delegate to handle a registered behavior command
   /// </summary>
   /// <param name="e">Chat Event</param>
   public delegate void BehaviorCommand (ChatEvent e);

   /// <summary>
   /// Delegate to handle a timed event
   /// </summary>
   public delegate void TimedEventHandler ();

   /// <summary>
   /// Delegate to handle when a player enters a registered region
   /// </summary>
   /// <param name="e">Player Position Event</param>
   public delegate void RegionHandler (PlayerPositionEvent e);

   /// <summary>
   /// Base class required for all behaviors.
   /// </summary>
   public abstract class BotEventListener : IDisposable
   {
      // Create the registered behavior commands list
      SortedList<String, BehaviorCommand> m_commands = new SortedList<string, BehaviorCommand> ();

      // Create a list for the expired events
      List<string> m_expiredEvents = new List<string> ();

      /// <summary>
      /// Register a behavior command with the event listener
      /// </summary>
      /// <param name="command">command string</param>
      /// <param name="handler">Event handler method</param>
      public void RegisterCommand (string command, BehaviorCommand handler)
      {
         try
         {
            // Add the command to the handler list
            m_commands.Add (command, handler);
         }
         catch (Exception e)
         {
            Console.WriteLine (e);
         }
      }

      /// <summary>
      /// Register a timed event to be called every X milliseconds
      /// </summary>
      /// <param name="name">Name of the event handler</param>
      /// <param name="time">Periodic time in milliseconds</param>
      /// <param name="handler">Timed Event Handler</param>
      public void RegisterTimedEvent (string name, int time, TimedEventHandler handler)
      {
         // Create the timed event
         TimedEvent t = new TimedEvent ();
         t.mSecs = time;
         t.handler = handler;
         t.timeout = TimeSpan.FromMilliseconds (Environment.TickCount + time);

         // Add the timed event handler
         m_timedEvents.Add (name, t);
      }

      /// <summary>
      /// Register a timed event to be called every X milliseconds
      /// </summary>
      /// <param name="name">Name of the event handler</param>
      /// <param name="time">Periodic time in milliseconds</param>
      /// <param name="count">number of times to execute the timer</param>
      /// <param name="handler">Timed Event Handler</param>
      public void RegisterTimedEvent (string name, int time, int count, TimedEventHandler handler)
      {
         // Create the timed event
         TimedEvent t = new TimedEvent ();
         t.mSecs = time;
         t.nCount= count;
         t.handler = handler;
         t.timeout = TimeSpan.FromMilliseconds (Environment.TickCount + time);

         // Add the timed event handler
         m_timedEvents.Add (name, t);
      }


      /// <summary>
      /// Removed a timed event handler
      /// </summary>
      /// <param name="name">Name of the event handler</param>
      public void RemoveTimedEvent (string name)
      {
         // Check if the handler is registered
         if (m_timedEvents.ContainsKey (name))
         {
            // Remove the event handler
            m_expiredEvents.Add (name);
         }
      }

      /// <summary>
      /// Register a region handler to be called when a player enters 
      /// the region
      /// </summary>
      /// <param name="name">Name to identify the region</param>
      /// <param name="x">Upper left corner Map X position</param>
      /// <param name="y">Upper left corner Map Y position</param>
      /// <param name="width">Region width</param>
      /// <param name="height">Region height</param>
      /// <param name="handler">Handler to be called when a player enters</param>
      public void RegisterRegion (string name, int x, int y, int width, int height, RegionHandler handler)
      {
         // Create the timed event
         RegionInfo regionInfo = new RegionInfo ();
         regionInfo.region = new Rectangle(x, y, width, height);
         regionInfo.handler = handler;

         // Add the region handler
         m_regionList.Add (name, regionInfo);
      }

      /// <summary>
      /// Removed a region handler
      /// </summary>
      /// <param name="name">Name of the region handler</param>
      public void RemoveRegion (string name)
      {
         // Check if the handler is registered
         if (m_regionList.ContainsKey (name))
         {
            // Remove the event handler
            m_regionList.Remove (name);
         }
      }

      ///<summary>
      /// Send an event to the game server.  This is the most
      /// commonly used method to send events.
      /// </summary>
      /// <param name="e">A Wrapper for the arguments for the Event</param>
      public void SendGameEvent (EventArgs e)
      {
         if (onGameNotifyEvent != null)
         {
            // Call the event handler
            onGameNotifyEvent (this, e);
         }
      }

      ///<summary>
      /// Send an event to the game server.  This is the most
      /// commonly used method to send events.
      /// </summary>
      /// <param name="e">A Wrapper for the arguments for the Event</param>
      public void Game(EventArgs e)
      {
          if (onGameNotifyEvent != null)
          {
              // Call the event handler
              onGameNotifyEvent(this, e);
          }
      }

      ///<summary>Send an event to all bots running in the core</summary>
      /// <param name="e">A Wrapper for the arguments for the Event</param>
      public void SendCoreEvent (EventArgs e)
      {
         if (onCoreNotifyEvent != null)
         {
            // Call the event handler
            onCoreNotifyEvent (this, e);
         }
      }

      ///<summary>Send an event within the bot to all loaded behaviors</summary>
      /// <param name="e">A Wrapper for the arguments for the Event</param>
      public void SendBotEvent (EventArgs e)
      {
         if (onBotNotifyEvent != null)
         {
            // Call the event handler
            onBotNotifyEvent (this, e);
         }
      }

      /// Bot Listener event handlers
      public event NotifyGame onGameNotifyEvent;

      /// Core Listener event handlers
      public event NotifyCore onCoreNotifyEvent;

      /// Bot Listener event handlers
      public event NotifyBot onBotNotifyEvent;

      /// <summary>
      /// Acts as a midway between BotCore and this BotEventListener.
      /// </summary>
      /// <param name="sender">The object that sent the Event</param>
      /// <param name="e">the EventArgs</param>
      public void onEvent (object sender, EventArgs e)
      {
         if (sender != null && sender.GetType () == this.GetType ()) return;

         // Check if the event is a player position
         if (e is PlayerPositionEvent)
         {
            // Check if the player is in a defined region
            HandlePlayerPosition (e as PlayerPositionEvent);
         }

         // Check if the event is a chat event
         if (e is ChatEvent) 
         {
            // Check for chat commands
            HandleChatCommands (e as ChatEvent);
         }

         foreach (MethodInfo mi in this.GetType ().GetMethods ())
         {
            ParameterInfo[] pis = mi.GetParameters ();

            if ((pis.Length >= 2) && (pis[1].ParameterType == e.GetType ()))
            {
               try
               {
//                   if (!(e is PlayerPositionEvent))
                       mi.Invoke (this, new object[] { sender, Convert.ChangeType (e, e.GetType ()) });
               }
               catch (Exception ex)
               {
                  ChatEvent msg = new ChatEvent ();
                  msg.ChatType = ChatTypes.Arena;
                  msg.SoundCode = SoundCodes.Fart2;
                  msg.Message = this.GetType ().ToString () + ": " + e.GetType ().ToString() + " Handler: " + ex.Message;
                  SendGameEvent (msg);
               }
            }
         }
      }

      /// <summary>
      /// Timed Event Handler
      /// </summary>
      class TimedEvent
      {
         public int mSecs = 0;
         public int nCount = 0;
         public TimeSpan timeout;
         public TimedEventHandler handler;
      }

      // List of timed event handlers
      SortedList<string, TimedEvent> m_timedEvents = new SortedList<string, TimedEvent> ();

      /// <summary>
      /// Called to handle the 10ms timer tick
      /// </summary>
      public void onTimerTick ()
      {
         // Get the current time and create a timespan object
         TimeSpan currentTime = TimeSpan.FromMilliseconds (Environment.TickCount);

         // Handle the registered time events
         foreach (string timerKey in m_timedEvents.Keys)
         {
            TimedEvent t = m_timedEvents[timerKey];

            // Check if it is time to call the handler
            if (currentTime > t.timeout)
            {
               try
               {
                  // Call the handler
                  t.handler ();

                  // Check if there is a count associated with the timer
                  if ((t.nCount > 0) && ((--t.nCount) == 0))
                  {
                     // Add the timed event to the expired list
                     m_expiredEvents.Add (timerKey);
                  }
               }
               catch (Exception ex)
               {
                  ChatEvent msg = new ChatEvent ();
                  msg.ChatType = ChatTypes.Arena;
                  msg.SoundCode = SoundCodes.Fart2;
                  msg.Message = this.GetType ().ToString () + " Time Handler: " + ex.Message;
                  SendGameEvent (msg);
               }


               // Adjust the timeout
               t.timeout = currentTime + TimeSpan.FromMilliseconds (t.mSecs);
            }
         }

         if (m_expiredEvents.Count > 0)
         {
            // Remove the expitred events from the list
            foreach (string timeEvent in m_expiredEvents)
            {
               // Remove the expired event
               m_timedEvents.Remove (timeEvent);
            }

            // Clear the expired events list
            m_expiredEvents.Clear ();
         }
      }

      /// <summary>
      /// Handle a chat command by processing the registered
      /// chat commands
      /// </summary>
      /// <param name="e">Chat Event</param>
      private void HandleChatCommands (ChatEvent e)
      {
         try
         {
            // Get the message from the chat event
            string strMessage = e.Message;

            int nIndex = strMessage.IndexOf (' ');

            // Strip any parameters from the message
            if (nIndex > 0) strMessage = strMessage.Remove (nIndex);

            // Check if the message is a command
            if (m_commands.ContainsKey (strMessage))
            {
               try
               {
                  // Call the behavior command handler
                  m_commands[strMessage] (e);
               }
               catch (Exception ex)
               {
                  ChatEvent msg = new ChatEvent ();
                  msg.ChatType = ChatTypes.Arena;
                  msg.SoundCode = SoundCodes.Fart2;
                  msg.Message = this.GetType ().ToString () + " " + strMessage + " Handler: " + ex.Message;
                  SendGameEvent (msg);
               }
            }
         }
         catch (Exception ex)
         {
            // Write the exception to the console
            Console.WriteLine (ex);
         }
      }

      #region Region Handling

      /// <summary>
      /// Region definition
      /// </summary>
      class RegionInfo
      {
         public Rectangle region;
         public RegionHandler handler;
      }

      // List of timed event handlers
      SortedList<string, RegionInfo> m_regionList = new SortedList<string, RegionInfo> ();

      /// <summary>
      /// Handle the player positon and check if the player is 
      /// within a registered region.
      /// </summary>
      /// <param name="e"></param>
      private void HandlePlayerPosition (PlayerPositionEvent e)
      {
         // Check all defined regions
         foreach (RegionInfo r in m_regionList.Values)
         {
            // Check if the player is within the region
            if (r.region.Contains (e.MapPositionX, e.MapPositionY))
            {
               try
               {
                  // Call the region handler
                  r.handler (e);
               }
               catch (Exception ex)
               {
                  ChatEvent msg = new ChatEvent ();
                  msg.ChatType = ChatTypes.Arena;
                  msg.SoundCode = SoundCodes.Fart2;
                  msg.Message = this.GetType ().ToString () + " Region handler: " + ex.Message;
                  SendGameEvent (msg);
               }
            }
         }
      }
      #endregion

      #region IDisposable Members
      /// <summary>
      /// Disposes the resources of the object
      /// </summary>
      public abstract void Dispose ();

      #endregion
   }
}
