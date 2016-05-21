//-----------------------------------------------------------------------
//
// NAME:        BotInstance.Events.cs
//
// PROJECT:     Battle Core Bot Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Bot Instance Events implementation.
//
// NOTES:       None.
//
// $History: BotInstance.Events.cs $
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
      /// Response event 
      /// </summary>
      class ResponseEvent : EventArgs
      {
         public object destination; // Destination address
         public EventArgs e;        // Event
      }

      /// <summary>
      /// Core event handler
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void HandleBehaviorEvents (Object sender, EventArgs e)
      {
         try
         {
            // If the event is a chat event
            if (e is ChatEvent)
            {
               // Cast the event to a chat event
               ChatEvent chatEvent = (e as ChatEvent);

               // Copy the chat event
               ChatEvent behaviorChat = new ChatEvent ();
               behaviorChat.Message = chatEvent.Message;
               behaviorChat.PlayerName = chatEvent.PlayerName;
               behaviorChat.PlayerId = chatEvent.PlayerId;
               behaviorChat.ChatType = chatEvent.ChatType;
               behaviorChat.SoundCode = chatEvent.SoundCode;
               behaviorChat.ModLevel = chatEvent.ModLevel;

               // Process the chat message type
               switch (behaviorChat.ChatType)
               {
               case ChatTypes.Arena:
                  // insert the *arena prefix
                  behaviorChat.ChatType = ChatTypes.Public;
                  behaviorChat.Message = behaviorChat.Message.Insert (0, "*arena ");
                  break;

               case ChatTypes.Zone:
                  // insert the *zone prefix
                  behaviorChat.ChatType = ChatTypes.Public;
                  behaviorChat.Message = behaviorChat.Message.Insert (0, "*zone ");
                  break;

               // If the chat type is remote private
               case ChatTypes.RemotePrivate:
                  string strPrefix = ":" + behaviorChat.PlayerName + ":";
                  behaviorChat.Message = behaviorChat.Message.Insert (0, strPrefix);
                  behaviorChat.PlayerId = 0xFFFF;
                  break;

               case ChatTypes.TeamPrivate:
               // If the chat type is private
               case ChatTypes.Private:
                  // If the player name is set
                  if ((behaviorChat.PlayerName != null) && (behaviorChat.PlayerName.Length > 0))
                  {
                     // Get the player information from the player database
                     PlayerInfo p = m_playerHandler.PlayerInformation (behaviorChat.PlayerName);

                     if (p != null)
                     {
                        // Set the player identifier
                        behaviorChat.PlayerId = p.PlayerId;
                     }
                  }
                  break;
               }

               // Createm and send the chat packet
               ChatPacket packet = new ChatPacket ();
               packet.Event = behaviorChat;
               m_session.TransmitPacket (packet);
            }
            // If the event is a player spectate request
            else if (e is SpectatePlayerEvent)
            {
               SpectatePlayerEvent specPlayer = (e as SpectatePlayerEvent);

               // If the player name is set
               if ((specPlayer.PlayerId == 0xFFFF) && (specPlayer.PlayerName.Length > 0))
               {
                  PlayerInfo p = m_playerHandler.PlayerInformation (specPlayer.PlayerName);

                  // Set the player identifier
                  specPlayer.PlayerId = p.PlayerId;
               }

               // Create and send the spectate player packet
               SpectatePlayerPacket packet = new SpectatePlayerPacket ();
               packet.Event = specPlayer;
               m_session.TransmitPacket (packet);
            }
            // If the event is a player information request
            else if (e is PlayerInfoEvent)
            {
               PlayerInfoEvent players = (e as PlayerInfoEvent);

               // If there are players specified
               if (players.Players.Count > 0)
               {
                  // Add the player information to the list
                  foreach (string s in players.Players)
                  {
                     PlayerInfo p = m_playerHandler.PlayerInformation (s);

                     if (p != null)
                     {
                        // Add the player information to the list
                        players.PlayerList.Add (p);
                     }
                  }
               }
               else
               {
                  // Add all player information to the list
                  foreach (PlayerInfo p in m_playerHandler.PlayerData.Values)
                  {
                     // Add the player information
                     players.PlayerList.Add (p);
                  }
               }

               // Create the player information response
               ResponseEvent response = new ResponseEvent ();
               response.destination = sender;
               response.e = e;

               // add the response to the event queue
               m_eventQueue.Add (response);
            }
            // If the event is a bot information request
            else if (e is BotInfoRequest)
            {
                // Create the player information response
                ResponseEvent response = new ResponseEvent();
                response.destination = sender;

                BotInfoRequest b = new BotInfoRequest();
                b.BotName = m_playerHandler.PlayerInformation(m_botIdentifier).PlayerName;
                b.MapFile = m_session.SecurityChecksum.MapFile;
                b.MapData = m_session.SecurityChecksum.PsyLevelData;

                response.e = b;

                // add the response to the event queue
                m_eventQueue.Add(response);
            }
            // If the event is a bot information request
            else if (e is BotInfoEvent)
            {
                BotInfoEvent botInfo = (e as BotInfoEvent);

                // Get the bot information from the player data
                botInfo.BotInfo = m_playerHandler.PlayerInformation(m_botIdentifier);

                // Create the bot information response
                ResponseEvent response = new ResponseEvent();
                response.destination = sender;
                response.e = e;

                // add the response to the event queue
                m_eventQueue.Add(response);
            }
            // If the event is a player position event
            else if (e is SqlQueryEvent)
            {
                // Execute the SQL query
                ExecuteSqlQuery(sender, e as SqlQueryEvent);
            }
            else if (e is SqlCommandEvent)
            {
                // Execute the SQL command
                ExecuteSqlCommand(sender, e as SqlCommandEvent);
            }
            // If the event is a player position event
            else if (e is PlayerPositionEvent)
            {
                PlayerPositionEvent positionEvent = (e as PlayerPositionEvent);

                // Set the new player position
                m_playerHandler.PlayerInformation(m_botIdentifier).Position = positionEvent;

                PlayerPositionPacket packet = new PlayerPositionPacket();
                packet.Event = positionEvent;
                m_session.TransmitPacket(packet);
            }
            else if (e is PlayerDeathEvent)
            {
                // Create the player death packet
                PlayerDeathPacket packet = new PlayerDeathPacket();
                PlayerDeathEvent deathEvent = (e as PlayerDeathEvent);

                if (deathEvent.KillerName.Length != 0)
                {
                    // Get the player information from the player database
                    PlayerInfo p = m_playerHandler.PlayerInformation(deathEvent.KillerName);

                    if (p != null)
                    {
                        deathEvent.KillerId = p.PlayerId;
                    }
                }
                // Send the player death packet
                packet.Event = deathEvent;
                m_session.TransmitPacket(packet);
            }
            else if (e is CreateTurretEvent)
            {
                // Create the turret packet
                CreateTurretPacket packet = new CreateTurretPacket();
                CreateTurretEvent turretEvent = (e as CreateTurretEvent);

                if (turretEvent.TurretHostName.Length != 0)
                {
                    // Get the player information from the player database
                    PlayerInfo p = m_playerHandler.PlayerInformation(turretEvent.TurretHostName);

                    if (p != null)
                    {
                        turretEvent.TurretHostId = p.PlayerId;
                    }
                }

                // Send the turret creation request packet
                packet.Event = turretEvent;
                m_session.TransmitPacket(packet);
            }
            else if (e is DestroyTurretEvent)
            {
                // Create the turret packet
                DestroyTurretPacket packet = new DestroyTurretPacket();
                DestroyTurretEvent turretEvent = (e as DestroyTurretEvent);

                // Don't need to check for playername or id...packet contents are hardcoded
                // It doesn't seem to work otherwise >_>

                // Send the turret creation request packet
                packet.Event = turretEvent;
                m_session.TransmitPacket(packet);
            }
            else if (e is LVZToggleEvent)
            {
                // Create the turret packet
                LVZTogglePacket packet = new LVZTogglePacket();
                LVZToggleEvent lvz_event = (e as LVZToggleEvent);

                // Don't need to check for playername or id...packet contents are hardcoded
                // It doesn't seem to work otherwise >_>

                // Send the turret creation request packet
                packet.Event = lvz_event;
                m_session.TransmitPacket(packet);
            }
            else if (e is ArenaListEvent)
            {
                ChatPacket cP = new ChatPacket();
                ChatEvent c = new ChatEvent();
                c.Message = "?arena";
                c.ChatType = ChatTypes.Public;
                cP.Event = c;
                m_session.TransmitPacket(cP);
            }
         }
         catch (Exception ex)
         {
            // Write the line to the console
            Console.WriteLine (ex.Message);
         }
      }
   }
}
