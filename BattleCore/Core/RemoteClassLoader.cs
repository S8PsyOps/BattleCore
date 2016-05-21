//-----------------------------------------------------------------------
//
// NAME:        RemoteClassLoader.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Remote Class Loader implementation.
//
// NOTES:       None.
//
// $History: RemoteClassLoader.cs $
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

// namespace declaration
namespace BattleCore.Core
{
   /// <summary>
   /// A Class that loads other classes and is meant to run in
   /// a seperate AppDomain than the rest of the Application
   /// </summary>
   public class RemoteClassLoader : MarshalByRefObject
   {
      /// <summary>
      /// The list of <see cref= "BotEventListener"/>s
      /// </summary>
      protected ArrayList listenerList = new ArrayList ();
      /// <summary>
      /// The List of Assemblies handled by this Classloader
      /// </summary>
      protected ArrayList assemblyList = new ArrayList ();
      /// <summary>
      /// Creates a new RemoteClassLoader
      /// </summary>
      public RemoteClassLoader () { }
      /// <summary>
      /// Loads an Assembly and all of its types into the
      /// AppDomain
      /// </summary>
      /// <param name="fullname">The full path of the Assembly</param>
      public void LoadAssembly (string fullname)
      {
         // Get the directory name
         string path = Path.GetDirectoryName (fullname);

         // Get the filename of the assembly
         string filename = Path.GetFileNameWithoutExtension (fullname);

         try
         {
            // Load the assembly
            Assembly assembly = Assembly.LoadFrom (fullname);

            // Add the assembly to the assembly list
            assemblyList.Add (assembly);

            // Extract the listeners from the assembly list
            foreach (Type loadedType in assembly.GetTypes ())
            {
               // Check if the assembly is a Bot behavior 
               if (Attribute.IsDefined (loadedType, typeof (BehaviorAttribute)))
               {
                  // Check if the behavior listens for events
                  if (loadedType.BaseType == typeof (BotEventListener))
                  {
                     // Add the behavior to the listener list
                     listenerList.Add (loadedType);
                  }
               }
            }
         }
         catch (Exception e)
         {
            Console.WriteLine (e);
         }
      }
      /// <summary>
      /// Gets all of the Attributes of A type
      /// </summary>
      /// <param name="typeName">The FullName of the type whose Attributes you want</param>
      /// <returns>An object Array of all the attributes</returns>
      public object[] GetAttributes (string typeName)
      {
         MemberInfo inf = GetTypeByName (typeName);
         return inf.GetCustomAttributes (true);
      }
      /// <summary>
      /// Gets the <see cref=" BotEventListener"/>s
      /// </summary>
      /// <returns>a string array of all of the Types handled by this ClassLoader</returns>
      public string[] GetListeners ()
      {
         ArrayList classList = new ArrayList ();
         foreach (Type pluginType in listenerList)
         {
            classList.Add (pluginType.FullName);
         }
         return (string[])classList.ToArray (typeof (string));
      }
      /// <summary>
      /// Gets all the Assemblies
      /// </summary>
      /// <returns>a string array of all of the Assemblies handled by this Classloader</returns>
      public string[] GetAssemblies ()
      {
         ArrayList assemblyNameList = new ArrayList ();
         foreach (Assembly userAssembly in assemblyList)
         {
            assemblyNameList.Add (userAssembly.FullName);
         }
         return (string[])assemblyNameList.ToArray (typeof (string));
      }
      /// <summary>
      /// Gets the name of the owning assembly
      /// </summary>
      /// <param name="typeName">the type whose Assembly you want</param>
      /// <returns>the filename of the assembly that owns <paramref name="typeName"/></returns>
      /// <exception cref="InvalidOperationException">Thrown when <paramref name="typeName"/>is not handled by this Assembly</exception>
      public string GetOwningAssembly (string typeName)
      {
         Assembly owningAssembly = null;

         foreach (Assembly assembly in assemblyList)
         {
            if (assembly.GetType (typeName) != null)
            {
               owningAssembly = assembly;
               return assembly.ManifestModule.ToString ();
            }
         }
         throw new InvalidOperationException ("Could not find owning assembly for type " + typeName);
      }
      /// <summary>
      /// Checks if <paramref name="typeName"/>is part of this assembly
      /// </summary>
      /// <param name="typeName">The TypeName to look for</param>
      /// <returns>Returns True if <paramref name="typeName"/> belongs to any of the assemblies loaded</returns>
      public bool ManagesType (string typeName)
      {
         return (GetTypeByName (typeName) != null);
      }

      internal Type GetTypeByName (string typeName)
      {
         foreach (Type pluginType in listenerList)
         {
            if (pluginType.FullName == typeName)
            {
               return pluginType;
            }
         }
         return null;
      }
   }
}
