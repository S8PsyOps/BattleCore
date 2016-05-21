//-----------------------------------------------------------------------
//
// NAME:        BehaviorManager.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Behavior Manager implementation.
//
// NOTES:       None.
//
// $History: BehaviorManager.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Data;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

// namespace declaration
namespace BattleCore.Core
{
   /// <summary>
   /// Behavior Manager
   /// </summary>
   internal class BehaviorManager
   {
      /// <summary>
      /// The <see cref="AppDomain"/> of this BehaviorManager
      /// </summary>
      protected AppDomain behaviorAppDomain = null;
      /// <summary>
      /// The <see cref="AppDomainSetup"/> of this BehaviorManager
      /// </summary>
      protected AppDomainSetup behaviorAppDomainSetup = null;
      /// <summary>
      /// The <see cref="RemoteClassLoader"/> of this BehaviorManager
      /// </summary>
      protected RemoteClassLoader remoteLoader = null;
      /// <summary>
      /// The <see cref="LocalClassLoader"/> of this BehaviorManager
      /// </summary>
      protected LocalClassLoader localLoader = null;

      /// <summary>
      /// The Directory where the plugins are located
      /// </summary>
      internal string m_behaviorDirectory = null;
      /// <summary>
      /// The Path to the file that says which plugins should load at startup
      /// </summary>
      internal string behaviorFile = Path.Combine (Directory.GetCurrentDirectory (), "behaviors.txt");
      /// <summary>
      /// Creates a new behavior manager using "behaviors" as the plugin path
      /// </summary>
      internal BehaviorManager () : this ("behaviors") { }
      /// <summary>
      /// Creates a new BehaviorManager
      /// </summary>
      /// <param name="behaviorDirectory">The path relative to the current directory of the plugin</param>
      internal BehaviorManager (string behaviorDirectory)
      {
         string assemblyLoc = Assembly.GetExecutingAssembly ().Location;
         string currentDirectory = assemblyLoc.Substring (0, assemblyLoc.LastIndexOf (Path.DirectorySeparatorChar) + 1);
         m_behaviorDirectory = Path.Combine (currentDirectory, behaviorDirectory);
         if (!m_behaviorDirectory.EndsWith (Path.DirectorySeparatorChar.ToString ()))
         {
            m_behaviorDirectory = behaviorDirectory + Path.DirectorySeparatorChar;
         }

         if (!Directory.Exists (m_behaviorDirectory))
            Directory.CreateDirectory (m_behaviorDirectory);

         localLoader = new LocalClassLoader (m_behaviorDirectory);
      }
      /// <summary>
      /// Loads all of the Assemblies in the directory
      /// </summary>
      internal void LoadUserAssemblies ()
      {
         DirectoryInfo directory = new DirectoryInfo (m_behaviorDirectory);
         foreach (FileInfo file in directory.GetFiles ("*.dll"))
         {
            try
            {
               localLoader.LoadAssembly (file.FullName);
            }
            catch (PolicyException e)
            {
               throw new PolicyException (String.Format ("Cannot load {0} - code requires privilege to execute", file.Name), e);
            }
         }
      }
      /// <summary>
      /// Gets all of the Attributes of A type
      /// </summary>
      /// <param name="typeName">The FullName of the type whose Attributes you want</param>
      /// <returns>An object Array of all the attributes</returns>
      internal object[] GetAttributes (string typeName)
      {
         return localLoader.GetAttributes (typeName);
      }
      /// <summary>
      /// A string array of all of the Assemblies handled by this PluginManager
      /// </summary>
      internal string[] Assemblies
      {
         get { return localLoader.Assemblies; }
      }
      /// <summary>
      /// A string array of all of the <see cref="BotEventListener"/>s 
      /// handled by this PluginManager
      /// </summary>
      internal string[] Listeners
      {
         get { return localLoader.Listeners; }
      }
      /// <summary>
      /// Checks if <paramref name="typeName"/>is part of this assembly
      /// </summary>
      /// <param name="typeName">The TypeName to look for</param>
      /// <returns>Returns True if <paramref name="typeName"/> belongs to any of the assemblies loaded</returns>
      internal bool ManagesType (string typeName)
      {
         return localLoader.ManagesType (typeName);
      }
      /// <summary>
      /// Creates an Instance of the <paramref name="typeName"/>
      /// </summary>
      /// <param name="typeName">The FullName of the type to be created</param>
      /// <returns>A new instance of the class</returns>
      internal object CreateInstance (string typeName)
      {
         return localLoader.CreateInstance (typeName);
      }

      /// <summary>
      /// Closes the <see cref="LocalClassLoader"/>
      /// </summary>
      internal void Unload ()
      {
         localLoader.Unload ();
      }
   }
}
