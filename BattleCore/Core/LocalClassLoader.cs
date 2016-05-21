//-----------------------------------------------------------------------
//
// NAME:        LocalClassLoader.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Local Class Loader implementation.
//
// NOTES:       None.
//
// $History: LocalClassLoader.cs $
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
using System.Runtime.Remoting.Proxies;

// namespace declaration
namespace BattleCore.Core
{
   /// <summary>
   /// A class that acts as a midway between the BehaviorManager and the 
   /// RemoteClassLoader
   /// </summary>
   public class LocalClassLoader : MarshalByRefObject
   {
      //AppDomain m_appDomain;
      RemoteClassLoader m_remoteLoader;
      string m_behaviorDirectory;

      /// <summary>
      /// Creates a new LocalClassLoader
      /// </summary>
      /// <param name="behaviorDirectory">The directory where the behaviors are located</param>
      public LocalClassLoader (String behaviorDirectory)
      {
         m_behaviorDirectory = behaviorDirectory;
         m_remoteLoader = new RemoteClassLoader ();
      }

      /// <summary>
      /// Loads the assembly into the <see cref="RemoteClassLoader"/>
      /// </summary>
      /// <param name="filename">The Full filename of the Assembly</param>
      public void LoadAssembly (String filename)
      {
         try
         {
            m_remoteLoader.LoadAssembly (filename);
         }
         catch (BadImageFormatException) { }
         catch (FileNotFoundException) { }
         catch (FileLoadException) { }
      }
      /// <summary>
      /// Destroys the <see cref="RemoteClassLoader"/>
      /// </summary>
      public void Unload ()
      {
         m_remoteLoader = null;
      }

      /// <summary>
      /// A string array of all of the Assemblies handled by this ClassLoader
      /// </summary>
      public string[] Assemblies
      {
         get
         {
            return m_remoteLoader.GetAssemblies ();
         }
      }

      /// <summary>
      /// Gets all of the Attributes of A type
      /// </summary>
      /// <param name="typeName">The FullName of the type whose Attributes you want</param>
      /// <returns>An object Array of all the attributes</returns>
      public object[] GetAttributes (string typeName)
      {
         return m_remoteLoader.GetAttributes (typeName);
      }

      /// <summary>
      /// A string array of all of the <see cref="BotEventListener"/>s 
      /// handled by this ClassLoader
      /// </summary>
      public string[] Listeners
      {
         get
         {
            return m_remoteLoader.GetListeners ();
         }
      }

      /// <summary>
      /// Checks if <paramref name="typeName"/>is part of this assembly
      /// </summary>
      /// <param name="typeName">The TypeName to look for</param>
      /// <returns>Returns True if <paramref name="typeName"/> belongs to any of the assemblies loaded</returns>
      public bool ManagesType (string typeName)
      {
         return m_remoteLoader.ManagesType (typeName);
      }

      /// <summary>
      /// Creates an Instance of the <paramref name="typeName"/>
      /// </summary>
      /// <param name="typeName">The FullName of the type to be created</param>
      /// <returns>A new instance of the class</returns>
      public object CreateInstance (string typeName)
      {
         string file;

         file = m_remoteLoader.GetOwningAssembly (typeName);
         file = Path.Combine (m_behaviorDirectory, file);

         ObjectHandle oh = Activator.CreateInstanceFrom (file, typeName);
         return oh.Unwrap ();
      }
   }
}
