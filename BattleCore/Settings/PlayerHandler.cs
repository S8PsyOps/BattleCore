//-----------------------------------------------------------------------
//
// NAME:        PlayerHandler.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Handler implementation.
//
// NOTES:       None.
//
// $History: PlayerHandler.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using BattleCore.Events;

// Namespace declaration
namespace BattleCore.Bot
{
   /// <summary>
   /// Player information handler object
   /// </summary>
   internal class PlayerHandler
   {
      /// <summary>
      /// Sorted list of player information 
      /// </summary>
      SortedList m_playerInfo = new SortedList ();

      /// <summary>
      /// Reverse lookup of player identifer
      /// </summary>
      SortedList m_playerLookup = new SortedList ();

      /// <summary>
      /// Property to get the player database
      /// </summary>
      public SortedList PlayerData 
      {
         get { return m_playerInfo; }
      }

      /// <summary>
      /// Get the player data from the internal database
      /// </summary>
      /// <param name="playerId">Player Identifier</param>
      /// <returns></returns>
      public PlayerInfo PlayerInformation (UInt16 playerId)
      {
         if (m_playerInfo.ContainsKey (playerId))
         {
            // Get the player information form the database
            return (PlayerInfo)(m_playerInfo[playerId]);
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// Get the player information from the internal database
      /// </summary>
      /// <param name="playerName">Player Name</param>
      /// <returns></returns>
      public PlayerInfo PlayerInformation (String playerName)
      {
         if (m_playerLookup.ContainsKey (playerName.ToUpper()))
         {
            // Get the player information form the database
            UInt16 playerId = (UInt16)(m_playerLookup[playerName.ToUpper()]);
 
            // Get the player information form the database
            return PlayerInformation (playerId);
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// Handle a player entering event
      /// </summary>
      /// <param name="playerEnter">Player entered event</param>
      public void HandlePlayerEnter (PlayerEnteredEvent playerEnter)
      {
         // Create the new player information object
         PlayerInfo playerInfo = new PlayerInfo ();

         // Set the player information
         playerInfo.PlayerEntered = playerEnter;

         // Add the player information to the database
         m_playerInfo[playerEnter.PlayerId] = playerInfo;

         // Add the player to the reverse lookup database
         m_playerLookup[playerEnter.PlayerName.ToUpper()] = playerEnter.PlayerId;
      }

      /// <summary>
      /// Handle a player leaving event
      /// </summary>
      /// <param name="playerLeft">Player left event</param>
      public void HandlePlayerLeft (PlayerLeftEvent playerLeft)
      {
         if (m_playerInfo.ContainsKey (playerLeft.PlayerId))
         {
            // Remove the player information from the database
            m_playerInfo.Remove (playerLeft.PlayerId);

            if (m_playerLookup.ContainsKey (playerLeft.PlayerName.ToUpper()))
            {
               // Remove the player from the reverse lookup database
               m_playerLookup.Remove (playerLeft.PlayerName.ToUpper());
            }
         }
      }
   }
}
