//-----------------------------------------------------------------------
//
// NAME:        BotSpawnEvent.cs
//
// PROJECT:     BattleCore Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Bot Spawn Event implementation.
//
// NOTES:       None.
//
// $History: BotSpawnEvent.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;

// Namespace definition
namespace BattleCore.Events
{
   /// <summary>
   /// Bot Spawn event for the !spawn commands
   /// handles the following parameters:
   ///   !spawn BotName:Arena
   ///   !spawn BotName:Arena:Password:StaffPassword
   /// </summary>
   internal class BotSpawnEvent : EventArgs
   {
      // Parameter array positions
      private const UInt32 SPAWN_NAME = 0;
      private const UInt32 SPAWN_ARENA = 1;
      private const UInt32 SPAWN_PASSWORD = 2;
      private const UInt32 SPAWN_STAFFPASS = 3;

      private UInt16 m_nPlayerId = 0xFFFF;
      /// <summary>
      /// Player Identifier Property
      /// </summary>
      public UInt16 PlayerId 
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; } 
      }

      private ModLevels m_modLevel = ModLevels.None;
      /// <summary>
      /// Player ModLevel Property
      /// </summary>
      public ModLevels ModLevel
      {
         set { m_modLevel = value; }
         get { return m_modLevel; }
      }

      private string m_strBotName = "";
      /// <summary>
      /// Bot Name Property
      /// </summary>
      public string BotName { get { return m_strBotName; } }

      private string m_strArena = "";
      /// <summary>
      /// Arena Name Property
      /// </summary>
      public string Arena { get { return m_strArena; } }

       /// <summary>
      /// Property to set the spawn parameters
      ///   BotName:Arena
      /// </summary>
      public string Parameters
      {
         set
         {
            // Parse the string parameters
            string[] strParameters = value.Split (new char[]{':'});

            // Check if the user name is included
            if (strParameters.Length > SPAWN_NAME)
            {
               // Set the bot name
               m_strBotName = strParameters[SPAWN_NAME];
            }

            // Check if the arena is included
            if (strParameters.Length > SPAWN_ARENA)
            {
               // Set the user name
               m_strArena = strParameters[SPAWN_ARENA];
            }
         }
      }
   }
}
