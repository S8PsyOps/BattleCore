//-----------------------------------------------------------------------
//
// NAME:        BotInstance.Protocol.cs
//
// PROJECT:     Battle Core Bot Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Bot Instance Protocol implementation.
//
// NOTES:       None.
//
// $History: BotInstance.Protocol.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
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
      /// Handle the player identity packet received from the server
      /// </summary>
      /// <param name="buffer">packet data</param>
      private void HandleIdentity (Byte[] buffer)
      {
         PlayerIdentifierPacket packet = new PlayerIdentifierPacket ();

         // Parse the player identifier packet
         packet.Packet = buffer;

         // Get the bot identifier from the packet
         m_botIdentifier = packet.Identifier;
      }

      /// <summary>
      /// Handle the In game flag
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleInGameFlag (Byte[] buffer)
      {

      }

      /// <summary>
      /// Handle a player entering message.  This packet may 
      /// contain multiple player entering messages.
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandlePlayerEntering (Byte[] buffer)
      {
         int nIndex = 0;
         Byte[] playerEnterPacket = new Byte[64];

         // Process all arena entered packets
         while (nIndex < buffer.Length)
         {
            // Get the next player entered packet from the packet
            Array.Copy (buffer, nIndex, playerEnterPacket, 0, 64);

            // Create the player entered message packet
            PlayerEnteredPacket playerEntered = new PlayerEnteredPacket ();

            // Parse the player entered packet
            playerEntered.Packet = playerEnterPacket;

            // Handle the player entered event
            m_playerHandler.HandlePlayerEnter (playerEntered.Event);

            //  Add the event to the core event queue
            m_eventQueue.Add (playerEntered.Event);

            // Increment the player message
            nIndex += 64;
         }
      }

      /// <summary>
      /// Handle a player leaving message
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandlePlayerLeaving (Byte[] buffer)
      {
         PlayerLeftPacket packet = new PlayerLeftPacket ();

         // Parse the player left packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {
            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;
         }

         // Handle the player left event
         m_playerHandler.HandlePlayerLeft (packet.Event);

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a Weapon update message.
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleWeaponUpdate (Byte[] buffer)
      {
         PlayerWeaponPacket packet = new PlayerWeaponPacket ();

         // Parse the player weapon message
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {

            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;

            // Set the position information
            playerInfo.Position = packet.Event;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a player death message.
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandlePlayerDeath (Byte[] buffer)
      {
         PlayerDeathPacket packet = new PlayerDeathPacket ();

         // Parse the player death packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo killedInfo = m_playerHandler.PlayerInformation (packet.Event.KilledId);

         if (killedInfo != null)
         {
            // Increment the loss count
            killedInfo.Losses ++;

            // Set the player name and moderator level in the event
            packet.Event.KilledName = killedInfo.PlayerName;
            packet.Event.KilledModLevel = killedInfo.ModeratorLevel;
         }

         // Get the player information
         PlayerInfo killerInfo = m_playerHandler.PlayerInformation (packet.Event.KillerId);

         if (killerInfo != null)
         {
            // Increment the player win count
            killedInfo.Wins ++;

            // Set the player name and moderator level in the event
            packet.Event.KillerName = killerInfo.PlayerName;
            packet.Event.KillerModLevel = killerInfo.ModeratorLevel;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a player chat message.
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleChat (Byte[] buffer)
      {
         ChatPacket packet = new ChatPacket ();

         try
         {
            // Parse the chat message
            packet.Packet = buffer;

            // Check if the player identifier is set
            if (packet.Event.PlayerId != 0xFFFF)
            {
               // Get the player information
               PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

               if (playerInfo != null)
               {
                  // Set the player name and moderator level in the event
                  packet.Event.PlayerName = playerInfo.PlayerName;
                  packet.Event.ModLevel = playerInfo.ModeratorLevel;
               }
            }

            bool bCommandHandled = false;

            switch (packet.Event.ChatType)
            {
            case ChatTypes.Arena:
               // Look for the moderator list message
               bCommandHandled = HandleModeratorList (packet.Event.Message);
               break;

            case ChatTypes.Public:
            case ChatTypes.Private:
            case ChatTypes.Team:
            case ChatTypes.TeamPrivate:
               // Handle registered chat commands
               bCommandHandled = HandleChatCommands (packet.Event);
               break;

            case ChatTypes.RemotePrivate:

               if (packet.Event.Message.StartsWith ("help: "))
               {
                  // Set the chat type
                  packet.Event.ChatType = ChatTypes.Help;
                  packet.Event.Message = packet.Event.Message.Remove (0, 6);
               }
               else if (packet.Event.Message.StartsWith ("cheater: "))
               {
                  // Set the chat type
                  packet.Event.ChatType = ChatTypes.Cheater;
                  packet.Event.Message = packet.Event.Message.Remove (0, 9);
               }
               else
               {
                  // Split the user name from the message
                  string[] message = packet.Event.Message.Split (new char[] { '>' });

                  // Check that this really is a remote chat message
                  if (message.Length > 1)
                  {
                     // Set the player name and the 
                     packet.Event.PlayerName = message[0].Trim (new char[] { '(', ')' });
                     packet.Event.Message = message[1];
                  }
               }
               break;
            }

            // Check if the command is already handled
            if (bCommandHandled == false)
            {
               // Add the event to the core event queue
               m_eventQueue.Add (packet.Event);
            }
         }
         catch (Exception ex)
         {
            // Write the exception to the console
            Console.WriteLine (ex);
         }
      }

      /// <summary>
      /// Handle a prize collected event
      /// </summary>
      /// <param name="buffer"></param>
      private void HandlePlayerPrize (Byte[] buffer)
      {
         PrizeCollectedPacket packet = new PrizeCollectedPacket ();

         // Parse the prize collected packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {
            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a score update packet from the server
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleScoreUpdate (Byte[] buffer)
      {
         ScoreUpdatePacket packet = new ScoreUpdatePacket ();

         // Parse the score update packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {
            // Update the player information with the new data
            playerInfo.ScoreUpdate = packet.Event;

            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a soccer goal packet from the server
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleSoccerGoal (Byte[] buffer)
      {
         SoccerGoalPacket packet = new SoccerGoalPacket ();

         // Parse the soccer goal packet
         packet.Packet = buffer;

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a player voice pacekt from the server
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandlePlayerVoice (Byte[] buffer)
      {
         PlayerSoundPacket packet = new PlayerSoundPacket ();

         // Parse the player voice packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {
            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle the player update packet received from the server
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandlePlayerUpdate (Byte[] buffer)
      {
         PlayerUpdatePacket packet = new PlayerUpdatePacket ();

         // Parse the player update packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.TeamEvent.PlayerId);

         if (playerInfo != null)
         {
             // Bugfix for shipchange event - PsyOps 12-4-15
             if ((int)packet.ShipEvent.ShipType == 255)
                 packet.ShipEvent.ShipType = playerInfo.Ship;
             else if ((int)packet.ShipEvent.PreviousShipType == 255)
                 packet.ShipEvent.PreviousShipType = playerInfo.Ship;

            // Check if the ship changed
            if (packet.ShipEvent.ShipType != playerInfo.Ship)
            {
               // Set the player name and moderator level in the event
               packet.ShipEvent.PlayerName = playerInfo.PlayerName;
               packet.ShipEvent.ModLevel = playerInfo.ModeratorLevel;
               packet.ShipEvent.PreviousShipType = playerInfo.Ship;

               // Update the ship type in the player information
               playerInfo.Ship = packet.ShipEvent.ShipType;

               // Add the event to the core event queue
               m_eventQueue.Add (packet.ShipEvent);
            }

            // Check if the frequency changed
            if (packet.TeamEvent.Frequency != playerInfo.Frequency)
            {
               // Set the player name and moderator level in the event
               packet.TeamEvent.PlayerName = playerInfo.PlayerName;
               packet.TeamEvent.ModLevel = playerInfo.ModeratorLevel;

               // Update the frequency in the player information
               playerInfo.Frequency = packet.TeamEvent.Frequency;

               // Add the event to the core event queue
               m_eventQueue.Add (packet.TeamEvent);
            }
         }
      }

      /// <summary>
      /// Handle the create turret packet sent by the server
      /// </summary>
      /// <param name="buffer"></param>
      private void HandleModifyTurret (Byte[] buffer) 
      {
          ModifyTurretPacket packet = new ModifyTurretPacket();

          // Parse the turret modification packet
          packet.Packet = buffer;

          // Get the player information
          PlayerInfo turretHostInfo = m_playerHandler.PlayerInformation(packet.Event.TurretHostId);

          if (turretHostInfo != null)
          {
              // Set the player name in the event
              packet.Event.TurretHostName = turretHostInfo.PlayerName;
          }

          // Get the player information
          PlayerInfo turretAttacherInfo = m_playerHandler.PlayerInformation(packet.Event.TurretAttacherId);

          if (turretAttacherInfo != null)
          {
              // Set the player name and moderator level in the event
              packet.Event.TurretAttacherName = turretAttacherInfo.PlayerName;
          }

          // Add the event to the core event queue
          m_eventQueue.Add(packet.Event);
      }

      /// <summary>
      /// Handle the arena settings sent by the server
      /// </summary>
      /// <param name="buffer"></param>
      private void HandleArenaSettings (Byte[] buffer)
      {
         ArenaSettingsPacket packet = new ArenaSettingsPacket ();

         // Parse the arena settings packet
         packet.Packet = buffer;

         // Set the arena settings for the security checksum
         m_session.SecurityChecksum.ArenaSettings = packet.ArenaSettings;
      }

      /// <summary>
      /// Handle a flag position packet received from the server
      /// </summary>
      /// <param name="buffer"></param>
      private void HandleFlagPosition (Byte[] buffer)
      {
         FlagPositionPacket packet = new FlagPositionPacket ();

         // Parse the flag position packet
         packet.Packet = buffer;

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a flag claim packet recieved from the server
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleFlagClaim (Byte[] buffer)
      {
         FlagClaimPacket packet = new FlagClaimPacket ();

         // Parse the flag claim packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {
            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      /// <summary>
      /// Handle a flag victory packet from the server
      /// </summary>
      /// <param name="buffer">packet data</param>
      private void HandleFlagVictory (Byte[] buffer) 
      { 
         Console.WriteLine ("FlagVictory"); 
      }

      /// <summary>
      /// Handle a delete turret packet from the server.
      /// </summary>
      /// <param name="buffer">packet data</param>
      private void HandleDeleteTurret (Byte[] buffer) 
      {
          DestroyTurretPacket packet = new DestroyTurretPacket();

          // Parse the destroy turret packet
          packet.Packet = buffer;

          // Get the player information
          PlayerInfo turretDriver = m_playerHandler.PlayerInformation(packet.Event.TurretHostId);

          if (turretDriver != null)
          {
              // Set the player name in the event
              packet.Event.TurretHostName = turretDriver.PlayerName;
          }

          // Add the event to the core event queue
          m_eventQueue.Add(packet.Event);
      }

      /// <summary>
      /// Handle a player flag drop packet from the server
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleFlagDrop (Byte[] buffer)
      {
         FlagDropPacket packet = new FlagDropPacket ();

         // Parse the flag drop packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {
            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);
      }

      private void HandleFileTransfer (Byte[] buffer)
      {
         LevelFilePacket packet = new LevelFilePacket ();
         packet.Packet = buffer;

      }

      private void HandleFileRequest (Byte[] buffer)
      {

      }

      /// <summary>
      /// Handle a score reset packet received from the server
      /// </summary>
      /// <param name="buffer"></param>
      private void HandleScoreReset (Byte[] buffer)
      {
         ScoreResetPacket packet = new ScoreResetPacket ();

         // Parse the score reset packet
         packet.Packet = buffer;

         if (packet.Event.PlayerId == 0xFFFF)
         {
            // Update every player in the arena
             foreach (PlayerInfo p in m_playerHandler.PlayerData.Values)
            {
               ScoreUpdateEvent scoreEvent = new ScoreUpdateEvent ();

               // Set the event information
               scoreEvent.PlayerId = p.PlayerId;
               scoreEvent.PlayerName = p.PlayerName;
               scoreEvent.ModLevel = p.ModeratorLevel;

               // Add the event to the core event queue
               m_eventQueue.Add (packet.Event);

               // Update the player database
               p.ScoreUpdate = scoreEvent;
            }
         }
         else
         {
            // Get the player information
            PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

            if (playerInfo != null)
            {
               // Set the player name and moderator level in the event
               packet.Event.PlayerName = playerInfo.PlayerName;
               packet.Event.ModLevel = playerInfo.ModeratorLevel;
            }

            // Add the event to the core event queue
            m_eventQueue.Add (packet.Event);
         }
      }

      /// <summary>
      /// Handle a ship reset packet received from the server
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandleShipReset (Byte[] buffer)
      {

      }

      private void HandleSpecPlayer (Byte[] buffer) { Console.WriteLine ("SpecPlayer"); }
      private void HandleSetTeamAndShip (Byte[] buffer) { Console.WriteLine ("SetTeamAndShip"); }
      private void HandleBannerFlag (Byte[] buffer) { Console.WriteLine ("BannerFlag"); }
      private void HandlePlayerBanner (Byte[] buffer)
      { 

      }
      private void HandleSelfPrize (Byte[] buffer) { Console.WriteLine ("SelfPrize"); }
      private void HandleBrickDrop (Byte[] buffer) {   }
      private void HandleTurfFlagStatus (Byte[] buffer) { Console.WriteLine ("TurfFlagStatus"); }
      private void HandleFlagReward (Byte[] buffer) { Console.WriteLine ("FlagReward"); }
      private void HandleSpeedStats (Byte[] buffer) { Console.WriteLine ("SpeedStats"); }
      private void HandleToggleUFO (Byte[] buffer) { Console.WriteLine ("ToggleUFO"); }
      private void HandleKeepAlive (Byte[] buffer) { }

      /// <summary>
      /// Handle a player position packet 
      /// </summary>
      /// <param name="buffer">packet buffer</param>
      private void HandlePlayerPosition (Byte[] buffer)
      {
         PlayerPositionPacket packet = new PlayerPositionPacket ();

         // Parse the player position packet
         packet.Packet = buffer;

         // Get the player information
         PlayerInfo playerInfo = m_playerHandler.PlayerInformation (packet.Event.PlayerId);

         if (playerInfo != null)
         {
            // Set the player name and moderator level in the event
            packet.Event.PlayerName = playerInfo.PlayerName;
            packet.Event.ModLevel = playerInfo.ModeratorLevel;

            // Update the player information with the new data
            playerInfo.Position = packet.Event;
         }

         // Add the event to the core event queue
         m_eventQueue.Add (packet.Event);

          // TODO - echo position packets of players that the bot is turreting
      }

      private void HandleMapInfo (Byte[] buffer)
      {
         LevelInfoPacket packet = new LevelInfoPacket ();

         packet.Packet = buffer;
      }

      private void HandleMapFile (Byte[] buffer)
      {
         LevelFilePacket packet = new LevelFilePacket ();

         // Parse the level file packet
         packet.Packet = buffer;

         // Set the map file data
         m_session.SecurityChecksum.LevelData = packet.FileData;
      }

      private void HandleSetKoTHTimer (Byte[] buffer) { Console.WriteLine ("KOTHTimer"); }
      private void HandleKoTHReset (Byte[] buffer) { Console.WriteLine ("KOTHReset"); }
      //private void HandleArenaList (Byte[] buffer) { Console.WriteLine ("ArenaList"); }
      private void HandleArenaList(Byte[] buffer)
      {
          // Create new packet
          ArenaListPacket packet = new ArenaListPacket();
          // Parse info to event
          packet.Packet = buffer;

          // Add the event to the core event queue
          m_eventQueue.Add(packet.Event);
      }
      private void HandleBallPosition (Byte[] buffer) { }
      private void HandleBannerAds (Byte[] buffer) { Console.WriteLine ("BannerAds"); }
      private void HandleChangePosition (Byte[] buffer) { Console.WriteLine ("ChangePosition"); }
      private void HandleObjectToggle (Byte[] buffer) { Console.WriteLine ("ObjectToggle"); }
      private void HandleReceivedObject (Byte[] buffer) { Console.WriteLine ("ReceivedObject"); }
      private void HandleDamageToggle (Byte[] buffer) { Console.WriteLine ("DamageToggle"); }
      
      /// <summary>
      /// Handle a watch damage packet received from the server
      /// </summary>
      /// <param name="buffer">Packet data</param>
      private void HandleWatchDamage (Byte[] buffer)
      {
         WatchDamagePacket packet = new WatchDamagePacket ();

         // Parse the watch damage packet
         packet.Packet = buffer;

         // Add all damage events to the event queue
         foreach (WatchDamageEvent damageEvent in packet.Events)
         {
            // Get the player information
            PlayerInfo playerInfo = m_playerHandler.PlayerInformation (damageEvent.PlayerId);

            if (playerInfo != null)
            {
               // Set the player name and moderator level in the event
               damageEvent.PlayerName = playerInfo.PlayerName;
               damageEvent.PlayerModLevel = playerInfo.ModeratorLevel;
            }

            // Get the player information
            PlayerInfo attackerInfo = m_playerHandler.PlayerInformation (damageEvent.AttackerId);

            if (attackerInfo != null)
            {
               // Set the player name and moderator level in the event
               damageEvent.AttackerName = attackerInfo.PlayerName;
               damageEvent.AttackerModLevel = attackerInfo.ModeratorLevel;
            }

            // Add the event to the core event queue
            m_eventQueue.Add (damageEvent);
         }
      }
   }
}
