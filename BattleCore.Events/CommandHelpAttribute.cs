//----------------------------------------------------------------------
//
// NAME:        CommandHelpAttribute.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Command Help Attribute implementation.
//
// NOTES:       None.
//
// HISTORY:
// DATE      AUTHOR     CHANGES
// --------  ---------  -------------------------------------------------
// 03-18-06  udp        Initial Creation
//
//-----------------------------------------------------------------------

// Namespace usage
using System;

// Namespace declaration
namespace BattleCore
{
   /// <summary>
   /// Attribute used to add command descriptions to the core help.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: The attribute must be defined for every core behavior.</para>
   /// <code lang="C#" escaped="true">
   /// [CommandHelp ("!mycommand", "This is a description for my command", ModLevels.None)]
   /// public class CoreTest : BotEventListener
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <code lang="Java" escaped="true">
   /// /** @attribute CommandHelp ("!mycommand", "This is a description for my command", ModLevels.None) */
   /// public class JavaTest extends BotEventListener
   /// {
   ///    ...
   /// }
   /// </code>
   /// </remarks>
   [System.AttributeUsage (AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
   [Serializable ()]
   public sealed class CommandHelpAttribute : Attribute
   {
      private string    m_strCommand;      // Behavior Command
      private string    m_strDescription;  // Command Description
      private ModLevels m_accessLevel;  // Required access level

       /// <summary>Creates a new BattleCore Behavior Attribute</summary>
      /// <param name ="command">Command name</param>
      /// <param name ="description">A short description of the command</param>
      /// <param name ="accessLevel">Command Moderator Access Level</param>
      public CommandHelpAttribute (string command, string description, ModLevels accessLevel)
      {
         // Set the member data
         this.m_strCommand = command;
         this.m_strDescription = description;
         this.m_accessLevel = accessLevel;
      }

      ///<summary>Behavior Command</summary>
      public string Command
      {
         get { return m_strCommand; }
      }

      ///<summary>Description of the command</summary>
      public string Description
      {
         get { return m_strDescription; }
      }

      ///<summary>Command Access Level</summary>
      public ModLevels AccessLevel
      {
         get { return m_accessLevel; }
      }
   }
}
