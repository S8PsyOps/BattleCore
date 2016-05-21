//----------------------------------------------------------------------
//
// NAME:        BehaviorAttribute.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Behavior Attribute implementation.
//
// NOTES:       None.
//
// HISTORY:
// DATE      AUTHOR     CHANGES
// --------  ---------  -------------------------------------------------
// 12-29-06  udp        Initial Creation
//
//-----------------------------------------------------------------------

// Namespace usage
using System;

// Namespace declaration
namespace BattleCore
{
   /// <summary>
   /// Attribute required for every BattleCore behavior
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: The attribute must be defined for every core behavior.</para>
   /// <code lang="C#" escaped="true">
   /// [Behavior ("MyBehavior", "true", "1.0", "udp", "A simple C# behavior")]
	/// public class CoreTest : BotEventListener
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// /** @attribute Behavior ("MyBehavior", "true", "1.0", "udp", "A simple Java behavior") */
   /// public class JavaTest extends BotEventListener
   /// {
   ///    ...
   /// }
   /// </code>
   /// </remarks>
   [System.AttributeUsage (AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
   [Serializable()]
   public sealed class BehaviorAttribute : Attribute
   {
      private string m_strTitle;        // Behavior title
      private string m_strAutoload;     // Automatic load of the behavior
      private string m_strVersion;      // Version of the behavior
      private string m_strDevelopers;   // Developers of the behavior
      private string m_strDescription;  // Short description of the behavior

      /// <summary>Creates a new BattleCore Behavior Attribute</summary>
      /// <param name ="title">The title of the behavior</param>
      /// <param name ="autoload">automatic load at startup (true/false)</param>
      /// <param name ="version">The version of the behavior</param>
      /// <param name ="developers">The developers of the behavior</param>
      /// <param name ="description">A short description of the behavior</param>
      public BehaviorAttribute (string title, string autoload, string version,
                                         string developers, string description)
      {
         // Set the member data
         this.m_strTitle       = title;
         this.m_strAutoload    = autoload.ToLower();
         this.m_strVersion     = version;
         this.m_strDevelopers  = developers;
         this.m_strDescription = description;
      }

      ///<summary>Title of the behavior</summary>
      public string Title
      {
         get { return m_strTitle; }
      }

      ///<summary>Automatic load of the behavior</summary>
      public bool Autoload
      {
          get { return (m_strAutoload == "true"); }
      }

      ///<summary>Version of the behavior</summary>
      public string Version
      {
         get { return m_strVersion; }
      }

      ///<summary>Developers of the behavior</summary>
      public string Developers
      {
         get { return m_strDevelopers; }
      }

      ///<summary>Description of the behavior</summary>
      public string Description
      {
         get { return m_strDescription; }
      }
   }
}
