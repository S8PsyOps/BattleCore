//-----------------------------------------------------------------------
//
// NAME:        Core.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Core implementation.
//
// NOTES:       None.
//
// $History: Core.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Remoting;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using BattleCore.Events;

// namespace declaration
namespace BattleCore.Core
{
   /// <summary>
   /// The Delegate to use if you want to throw an Event to the game
   /// or to the core.  The core events are distributed to all loaded bots.
   /// </summary>
   /// <param name="sender">The Object sending the event</param>
   /// <param name="e">A Wrapper for the arguements for this Event</param>
   public delegate void BotEvent (Object sender, EventArgs e);

   /// <summary>
   /// This is the core of the Plug-in engine. It provides support for dynamically loading and unloading
   /// plugins and protocols.  All public event sare heard by the core and 
   /// plugins can add listeners to them with only a reference to GPCore.
   /// </summary>
   internal class Core
   {
      #region Members
      private BehaviorManager m_coreBehaviorManager;
      /// <summary>
      /// Gets the core behavior manager.
      /// </summary>
      public BehaviorManager CoreBehaviorManager
      {
         get { return m_coreBehaviorManager; }
      }
      private BehaviorManager m_botBehaviorManager;
      /// <summary>
      /// Gets the bot behavior manager.
      /// </summary>
      public BehaviorManager BotBehaviorManager
      {
         get { return m_botBehaviorManager; }
      }
      private string m_botDirectory;
      /// <summary>
      /// Get the bot directory path
      /// </summary>
      public string BotDirectory
      {
         get { return m_botDirectory; }
      }
      private List<BotEventListener> m_listeners;
      /// <summary>
      /// Gets a list of loaded behaviors.
      /// </summary>
      public List<BotEventListener> LoadedBehaviors
      {
         get { return m_listeners; }
      }
      /// <summary>
      /// Gets a list of available behaviors.
      /// </summary>
      public string[] AvailableBehaviors
      {
         get 
         {
            int coreLength = m_coreBehaviorManager.Listeners.Length;
            int botLength = m_botBehaviorManager.Listeners.Length;
            string[] behaviors = new string[coreLength + botLength];

            // Copy the listeners into a combined array
            Array.Copy (m_coreBehaviorManager.Listeners, behaviors, coreLength);
            Array.Copy (m_botBehaviorManager.Listeners, 0, behaviors, coreLength, botLength);

            return behaviors;
         }
      }
      private BotEvent m_behaviorEventHandler;
      /// <summary>
      /// Returns the behavior event manager.
      /// </summary>
      public BotEvent GameEventManager
      {
         set { m_behaviorEventHandler = value; }
         get { return m_behaviorEventHandler; }
      }
      private BotEvent m_coreEventHandler;
      /// <summary>
      /// Returns the core event manager.
      /// </summary>
      public BotEvent CoreEventManager
      {
         set { m_coreEventHandler = value; }
         get { return m_coreEventHandler; }
      }
      #endregion

      /// <summary>
      /// Starts the bot core behavior manager
      /// </summary>
      /// <param name="botDirectory">Path to the bot behaviors</param>
      public void Start (String botDirectory)
      {
         // Set the bot directory
         m_botDirectory = botDirectory;

         // Create the behavior manager
         m_coreBehaviorManager = new BehaviorManager ("behaviors");
         m_botBehaviorManager = new BehaviorManager (botDirectory);

         // Create the listeners list
         m_listeners = new List<BotEventListener> ();

         // Load all assemblies in the assembly directories
         m_coreBehaviorManager.LoadUserAssemblies ();
         m_botBehaviorManager.LoadUserAssemblies ();

         // Load the autorun behaviors
         LoadAutorunBehaviors (ref m_coreBehaviorManager);
         LoadAutorunBehaviors (ref m_botBehaviorManager);

         // Load the behaviors from the file
         string[] behaviors = LoadBehaviorFile ();

         // Load all behaviors listed in the behavior file
         foreach (string behavior in behaviors)
         {
            // Create an instace of the object
            CreateInstance (behavior);
         }
      }

      internal void OnCoreEvent (object sender, EventArgs e)
      {
         // Send the event to the core event handler
         if (m_coreEventHandler != null)
         {
            m_coreEventHandler (sender, e);
         }
      }

      /// <summary>
      /// Event received by the game that is distributed 
      /// to all loaded behaviors.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      internal void OnBotEvent (object sender, EventArgs e)
      {
         foreach (BotEventListener cel in m_listeners)
         {
            cel.onEvent (sender, e);
         }
      }

      /// <summary>
      /// Handle the 10ms timer tick.
      /// </summary>
      internal void OnTimerTick ()
      {
         foreach (BotEventListener cel in m_listeners)
         {
            cel.onTimerTick ();
         }
      }

      /// <summary>
      /// Event received by the game that is distributed 
      /// to all loaded behaviors.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="receiver"></param>
      /// <param name="e"></param>
      internal void OnBotEvent (object sender, object receiver, EventArgs e)
      {
       //  if (receiver is BotEventListener)
         {
            ((BotEventListener)receiver).onEvent (sender, e);
         }
      }

      /// <summary>
      /// Event received by a behavior and will be sent to the
      /// game.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      internal void OnGameEvent (object sender, EventArgs e)
      {
         // Send the event to the behavior event handler
         if (m_behaviorEventHandler != null)
         {
            m_behaviorEventHandler (sender, e);
         }
      }

      /// <summary>
      /// Get the attributes list for the type name
      /// </summary>
      /// <param name="TypeName"></param>
      /// <returns></returns>
      internal object[] GetAttributes (string TypeName)
      {
         object[] attributes = null;

         if (Array.IndexOf<string> (m_coreBehaviorManager.Listeners, TypeName) >= 0)
         {
            attributes = m_coreBehaviorManager.GetAttributes (TypeName);
         }
         else if (Array.IndexOf<string> (m_botBehaviorManager.Listeners, TypeName) >= 0)
         {
            attributes = m_botBehaviorManager.GetAttributes (TypeName);
         }

         return attributes;
      }

      private void LoadAutorunBehaviors (ref BehaviorManager manager)
      {
         foreach (string listener in manager.Listeners)
         {
            // Get the attributes for the listener
            object[] os = manager.GetAttributes (listener);

            foreach (object obj in os)
            {
               // If the object is a Behavior attribute
               if (obj is BehaviorAttribute)
               {
                  if (((BehaviorAttribute)obj).Autoload)
                  {
                     // Create an instace of the object
                     CreateInstance (listener);
                  }
               }
            }
         }
      }
      
      public string[] LoadBehaviorFile ()
      {
         // Create the behavior list
         List<string> behaviorList = new List<string>();

         // Get the file path
         string behaviorFile = Path.Combine (m_botDirectory, "Behaviors.txt");

         // Check if the file exists
         if (File.Exists (behaviorFile))
         {
            // Create the file stream reader
            StreamReader sr = new StreamReader (behaviorFile);

            while (!sr.EndOfStream)
            {
               // Add the next element in the list
               behaviorList.Add (sr.ReadLine());
            }

            sr.Close();
         }

         // Return the string array
         return behaviorList.ToArray ();
      }

      public void SaveBehaviorFile (string[] behaviors)
      {
         // Create the behavior list
         List<string> behaviorList = new List<string> ();

         // Add the current behaviors to the list
         behaviorList.AddRange (behaviors);

         // Get the file path
         string behaviorFile= Path.Combine (m_botDirectory, "Behaviors.txt");

         // Create the file stream writer
         StreamWriter sw = new StreamWriter (behaviorFile);

         // Write the loaded behaviors to the file
         foreach (string s in behaviorList)
         {
            sw.WriteLine (s);
         }

         sw.Close ();
      }


      #region Create and Destroy Instances
      /// <summary>
      /// Creates an instance of the given type and adds it to the 
      /// Listeners list if it is a <see cref="BotEventListener"/>
      /// </summary>
      /// <param name="TypeName">The full name of the type to create an instance of.</param>
      /// <returns>A reference to the instance.</returns>
      public object CreateInstance (string TypeName)
      {
         object behavior = null;

         if (Array.IndexOf<string> (m_coreBehaviorManager.Listeners, TypeName) >= 0)
         {
            behavior = m_coreBehaviorManager.CreateInstance (TypeName);
         }
         else if (Array.IndexOf<string> (m_botBehaviorManager.Listeners, TypeName) >= 0)
         {
            behavior = m_botBehaviorManager.CreateInstance (TypeName);
         }

         if (behavior != null)
         {
            // Add the event handlers to the behavior
            AddEventHandlers (behavior);

            // Check if the behavior listens for events
            if (behavior is BotEventListener)
            {
               // Add the behavior to the listeners list
               m_listeners.Add (behavior as BotEventListener);
            }
         }

         return behavior;
      }

      private void AddEventHandlers (object o)
      {
         foreach (EventInfo ev in o.GetType ().GetEvents ())
         {
            if (ev.EventHandlerType.FullName.CompareTo ("BattleCore.NotifyCore") == 0)
            {
               ev.AddEventHandler (o, Delegate.CreateDelegate (ev.EventHandlerType, this, "OnCoreEvent"));
            }

            if (ev.EventHandlerType.FullName.CompareTo ("BattleCore.NotifyBot") == 0)
            {
               ev.AddEventHandler (o, Delegate.CreateDelegate (ev.EventHandlerType, this, "OnBotEvent"));
            }

            if (ev.EventHandlerType.FullName.CompareTo ("BattleCore.NotifyGame") == 0)
            {
               ev.AddEventHandler (o, Delegate.CreateDelegate (ev.EventHandlerType, this, "OnGameEvent"));
            }

         }
      }

      /// <summary>
      /// Calls the destructor for the given behavior and removes it from the Listeners list.
      /// </summary>
      /// <param name="listener">The FullName of the Type of listener you wish to destroy</param>
      public int DestroyInstance (string listener)
      {
         // find the listener in the listener array
         int nIndex = 0;

         while ((nIndex < m_listeners.Count)
            && (m_listeners[nIndex].GetType ().FullName != listener))
         {
            nIndex++;
         }

         if (nIndex < m_listeners.Count)
         {
            m_listeners[nIndex].Dispose ();
            m_listeners[nIndex] = null;
            m_listeners.RemoveAt (nIndex);
         }
         else
         {
            nIndex = -1;
         }

         return nIndex;
      }

      /// <summary>
      /// Calls the destructor for the given plugin and removes it from the Listeners list.
      /// </summary>
      /// <param name="listener">The Type of Listener you wish to destroy</param>
      public void DestroyInstance (Type listener) { DestroyInstance (listener.FullName); }
      /// <summary>
      /// Calls the destructor for the given plugin and removes it from the Listeners list.
      /// </summary>
      public void DestroyInstance (BotEventListener listener) { DestroyInstance (listener.GetType ().FullName); }
      #endregion

      /// <summary>
      /// Closes the core.
      /// </summary>
      public void Close ()
      {
         foreach (BotEventListener cel in m_listeners)
         {
            try
            {
               cel.Dispose ();
            }
            catch { }
         }

         m_coreBehaviorManager.Unload ();
         m_botBehaviorManager.Unload ();
      }
   }
}