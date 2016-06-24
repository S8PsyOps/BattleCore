//-----------------------------------------------------------------------
//
// NAME:        BotInstance.cs
//
// PROJECT:     Battle Core Bot Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Bot Instance implementation.
//
// NOTES:       None.
//
// $History: BotInstance.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using BattleCore.Session;
using BattleCore.Protocol;
using BattleCore.Events;
using BattleCore.Core;
using BattleCore.Settings;

// Namespace declaration
namespace BattleCore.Bot
{
   /// <summary>
   /// Bot Instance object
   /// </summary>
   public partial class BotInstance
   {
      UInt16 m_botIdentifier;    // Player identifier

      // Create the server session
      Session.Session m_session = new Session.Session ();

      // Create the behavior core handler
      Core.Core m_core = new Core.Core ();

      BotSettings m_botSettings = new BotSettings ();

      ManualResetEvent m_closeNotifyEvent = new ManualResetEvent (false);  

      /// <summary>
      /// Player Information Manager
      /// </summary>
      PlayerHandler m_playerHandler = new PlayerHandler ();

      /// <summary>
      ///  // Event queue for core events
      /// </summary>
      ArrayList m_eventQueue = new ArrayList ();

      internal BotEvent CoreEventManager
      {
         set { m_core.CoreEventManager = value; }
         get { return m_core.CoreEventManager; }
      }

      /// <summary>
      /// BotInstance Constructor
      /// </summary>
      public BotInstance (BotSettings settings)
      {
         // Initialize the bot identifier
         m_botIdentifier = 0xFFFF;

         // Add the connection notifiers
         m_session.LoginSuccessHandler += (new LoginNotify (HandleSessionConnected));
         m_session.DisconnectHandler += (new DisconnectNotify (HandleSessionDisconnect));

         // Add the core events manager
         m_core.GameEventManager += (new BotEvent (HandleBehaviorEvents));

         // Set the bot settings
         m_botSettings = settings;
      }

      /// <summary>
      /// Property to retrieve the bot settings
      /// </summary>
      internal BotSettings Settings { get { return m_botSettings; } }

      /// <summary>
      /// Start the bot instance
      /// </summary>
      public void Start ()
      {
         // Create the bot thread information
         m_botThreadInfo = new BotThreadInfo ();

         // Reset the bot identifier
         m_botIdentifier = 0xFFFF;

         // Start the server session
         m_session.Start (m_botSettings.SessionSettings);

         // Start the core
         m_core.Start (m_botSettings.BotDirectory);
      }

      /// <summary>
      /// Close the bot instance
      /// </summary>
      public void Close ()
      {
         // Stop the bot thread
         m_closeNotifyEvent.Set ();
      }

      /// <summary>
      /// Shutdown the bot instance
      /// </summary>
      public void Shutdown ()
      {
         // Stop the bot thread
         m_botThreadInfo.m_pKill.Set ();
         m_botThreadInfo.m_pDead.WaitOne (500, false);

         // Close the sessions
         m_session.Close ();
         m_core.Close ();

      }

      /// <summary>
      /// Called by BattleCore to broadcast an event to all loaded
      /// bot behaviors.
      /// </summary>
      /// <param name="e">Broadcast Event</param>
      internal void SendBotEvent (EventArgs e)
      {
         // Add the event to the event queue
         m_eventQueue.Add (e);
      }

      /// <summary>
      /// Called by BattleCore to send a game event to the server.
      /// </summary>
      /// <param name="e">Game Event</param>
      internal void SendGameEvent (EventArgs e)
      {
         // Add the event to the event queue
         m_core.OnGameEvent (this, e);
      }

      /// <summary>
      /// Called by BattleCore to send a game event to the server.
      /// </summary>
      /// <param name="e">Game Event</param>
      internal void Game(EventArgs e)
      {
          // Add the event to the event queue
          m_core.OnGameEvent(this, e);
      }

      /// <summary>
      /// Handle a moderator list message
      /// </summary>
      /// <param name="message">arena chat message</param>
      /// <returns>Command Handled state</returns>
      private bool HandleModeratorList(String message)
      {
          if (message.StartsWith("-")) return false;

          if (!(message.Contains("- bot -") || message.Contains("- sysop -") || message.Contains("- smod -") || message.Contains("- mod -"))) return false;

          bool bCommandHandled = false;

          bool isAsss = false;

          //if (message.Trim().StartsWith(": ") && message.Contains("  ")) isAsss = true;

          // Check if this is a mod information message
          int nStartPos = message.IndexOf("-");
          int nEndPos = message.LastIndexOf("-");
          string mod;
          string[] data = new string[] { };

          if (nStartPos < nEndPos || isAsss)
          {
              if (isAsss)
              {
                  mod = message.Replace("  ", "@");
                  data = mod.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
              }

              // Extract the moderator information
              String strName = isAsss ? data[1].Trim() : message.Substring(0, nStartPos - 1);
              String strModType = isAsss ? data[3].Trim() : message.Substring(nStartPos + 2, nEndPos - nStartPos - 3).ToLower();

              // Get the player information
              PlayerInfo pInfo = m_playerHandler.PlayerInformation(strName);

              // If the player is in the arena
              if (pInfo != null)
              {
                  // Verify the Moderator Type
                  if (strModType.CompareTo("sysop") == 0)
                  {
                      // Set the sysop attribute for the player name
                      pInfo.ModeratorLevel = ModLevels.Sysop;
                  }
                  // Verify the Moderator Type
                  if (strModType.CompareTo("bot") == 0)
                  {
                      // Set the sysop attribute for the player name
                      pInfo.ModeratorLevel = ModLevels.Sysop;
                  }
                  else if (strModType.CompareTo("smod") == 0)
                  {
                      // Set the SMod attribute for the player name
                      pInfo.ModeratorLevel = ModLevels.SMod;
                  }
                  else if (strModType.CompareTo("mod") == 0)
                  {
                      // Set the Mod attribute for the player name
                      pInfo.ModeratorLevel = ModLevels.Mod;
                  }
                  else if (strModType.CompareTo("custom") == 0)
                  {
                      // Set the Mod attribute for the player name
                      pInfo.ModeratorLevel = ModLevels.Custom;
                  }

                  // Create the new listmod event handler
                  ListModEvent listModEvent = new ListModEvent();

                  // Set the player information
                  listModEvent.Moderator = pInfo;

                  // Send the event to all behaviors
                  SendBotEvent(listModEvent);
              }

              // Set the command handled to true
              bCommandHandled = true;
          }

          // Return the command handled state
          return bCommandHandled;
      }



      #region Bot Instance Thread Implementation
      /// <summary>
      /// Session Thread Control Object
      /// </summary>
      private class BotThreadInfo
      {
         public ManualResetEvent m_pKill = new ManualResetEvent (false);  // Thread kill event
         public ManualResetEvent m_pDead = new ManualResetEvent (true);  // Thread dead event
      };

      /// <summary>
      /// Bot Instance Thread 
      /// </summary>
      private Thread m_botThread;

      /// <summary>
      /// Session thread control 
      /// </summary>
      BotThreadInfo m_botThreadInfo = new BotThreadInfo ();

      /// <summary>
      /// Bot Instance Thread
      /// </summary>
      private void BotInstanceThread ()
      {
         // Create the timestamp values
         TimeSpan currentTime    = TimeSpan.FromMilliseconds(Environment.TickCount);
         TimeSpan listModDelay   = currentTime + TimeSpan.FromSeconds(1);
         //TimeSpan positionDelay = currentTime + TimeSpan.FromMilliseconds(50);

         //TimeSpan getInfoDelay = currentTime + TimeSpan.FromMilliseconds(250);//250 - udp's setting
         //bool gotinfo = false;

         // Reset the thread dead event
         m_botThreadInfo.m_pDead.Reset ();
        
         // Loop until a kill request is received
         while (!m_botThreadInfo.m_pKill.WaitOne (10, false))
         {
            // Get the current timestamp
            currentTime = TimeSpan.FromMilliseconds(Environment.TickCount);

            // Handle the next core event 
            if (m_eventQueue.Count > 0)
            {
               // Get the next event from the queue
               EventArgs coreEvent = (EventArgs)(m_eventQueue[0]);

               if (coreEvent != null)
               {
                  if (coreEvent is ResponseEvent)
                  {
                     ResponseEvent response = (coreEvent as ResponseEvent);

                     // Handle the behavior response event
                     m_core.OnBotEvent (this, response.destination, response.e);
                  }
                  else
                  {
                     // Handle the behavior event
                     m_core.OnBotEvent (this, coreEvent);
                  }
               }
               // Remove the event from the event queue
               m_eventQueue.Remove (coreEvent); 
            }
             /*
            // Check if it is time to send a position packet
            if (currentTime > positionDelay)
            {
                // Get the bot player information
                PlayerInfo pInfo = m_playerHandler.PlayerInformation(m_botIdentifier);

                if (pInfo != null)
                {
                    // Create the player position packet
                    PlayerPositionPacket positionPacket = new PlayerPositionPacket();

                    // Set the player position values
                    positionPacket.Event = pInfo.Position;
                    positionPacket.Event.PlayerName = pInfo.PlayerName;
                    positionPacket.Event.PlayerId = pInfo.PlayerId;
                    positionPacket.Event.ModLevel = ModLevels.Sysop;
                    
                    // Send the player position
                    m_session.TransmitPacket(positionPacket);

                    // Create the listmod chat message packet
                    //ChatPacket chatPacket = new ChatPacket();
                    // Set the listmod message string
                    //chatPacket.Event.Message = "*arena update Player[" + positionPacket.Event.PlayerName + "] Pos[x:" + positionPacket.Event.MapPositionX + "|y:" + positionPacket.Event.MapPositionY + "]";
                    //m_session.TransmitPacket(chatPacket);
                }

                // Reset the position delay value
                positionDelay = currentTime + TimeSpan.FromMilliseconds(10000);
            }
            */
            // Check if it is time to send a listmod request
            if (currentTime > listModDelay)
            {
               // Create the listmod chat message packet
               ChatPacket chatPacket = new ChatPacket ();

               // Set the listmod message string
               //chatPacket.Event.Message = "?listmod";
               // Transmit the listmod command packet 
               //m_session.TransmitPacket(chatPacket);
               chatPacket.Event.Message = "*listmod";
               // Transmit the listmod command packet 
               m_session.TransmitPacket(chatPacket);

               // Reset the listmod delay value
               listModDelay = currentTime + TimeSpan.FromMinutes (5);
            }

            // Handle the 10ms timer tick
            m_core.OnTimerTick ();

            // If a bot close request is issued
            if (m_closeNotifyEvent.WaitOne (0, false))
            {
               // Reset the close notify event
               m_closeNotifyEvent.Reset ();

               m_botThreadInfo.m_pKill.Set ();
               m_session.Close ();
               m_core.Close ();
            }
         }

         // Remove all events from the event queue
         m_eventQueue.Clear (); 

         // Reset the kill event
         m_botThreadInfo.m_pKill.Reset ();

         // Set the core thread dead event
         m_botThreadInfo.m_pDead.Set ();
      }
      #endregion

      /// <summary>
      /// Handler called when a session is disconnected
      /// </summary>
      internal void HandleSessionDisconnect ()
      {
         // Stop the bot thread
         m_botThreadInfo.m_pKill.Set ();
         m_botThreadInfo.m_pDead.WaitOne (100, false);

         // Reset the bot identifier
         m_botIdentifier = 0xFFFF;
      }

      /// <summary>
      /// Handler called when a session is connected
      /// </summary>
      internal void HandleSessionConnected ()
      {
         // Start the bot core thread
         m_botThreadInfo.m_pKill.Reset ();
         m_botThread = new Thread (new ThreadStart (BotInstanceThread));
         m_botThread.Start ();

         // Add all game packet handlers to the session
         m_session.AddGamePacketHandler (0x01, new SessionPacketHandler (HandleIdentity));
         m_session.AddGamePacketHandler (0x02, new SessionPacketHandler (HandleInGameFlag));
         m_session.AddGamePacketHandler (0x03, new SessionPacketHandler (HandlePlayerEntering));
         m_session.AddGamePacketHandler (0x04, new SessionPacketHandler (HandlePlayerLeaving));
         m_session.AddGamePacketHandler (0x05, new SessionPacketHandler (HandleWeaponUpdate));
         m_session.AddGamePacketHandler (0x06, new SessionPacketHandler (HandlePlayerDeath));
         m_session.AddGamePacketHandler (0x07, new SessionPacketHandler (HandleChat));
         m_session.AddGamePacketHandler (0x08, new SessionPacketHandler (HandlePlayerPrize));
         m_session.AddGamePacketHandler (0x09, new SessionPacketHandler (HandleScoreUpdate));
         m_session.AddGamePacketHandler (0x0B, new SessionPacketHandler (HandleSoccerGoal));
         m_session.AddGamePacketHandler (0x0C, new SessionPacketHandler (HandlePlayerVoice));
         m_session.AddGamePacketHandler (0x0D, new SessionPacketHandler (HandlePlayerUpdate));
         m_session.AddGamePacketHandler (0x0E, new SessionPacketHandler (HandleModifyTurret));
         m_session.AddGamePacketHandler (0x0F, new SessionPacketHandler (HandleArenaSettings));
         m_session.AddGamePacketHandler (0x10, new SessionPacketHandler (HandleFileTransfer));
         m_session.AddGamePacketHandler (0x12, new SessionPacketHandler (HandleFlagPosition));
         m_session.AddGamePacketHandler (0x13, new SessionPacketHandler (HandleFlagClaim));
         m_session.AddGamePacketHandler (0x14, new SessionPacketHandler (HandleFlagVictory));
         m_session.AddGamePacketHandler (0x15, new SessionPacketHandler (HandleDeleteTurret));
         m_session.AddGamePacketHandler (0x16, new SessionPacketHandler (HandleFlagDrop));
         m_session.AddGamePacketHandler (0x19, new SessionPacketHandler (HandleFileRequest));
         m_session.AddGamePacketHandler (0x1A, new SessionPacketHandler (HandleScoreReset));
         m_session.AddGamePacketHandler (0x1B, new SessionPacketHandler (HandleShipReset));
         m_session.AddGamePacketHandler (0x1C, new SessionPacketHandler (HandleSpecPlayer));
         m_session.AddGamePacketHandler (0x1D, new SessionPacketHandler (HandlePlayerUpdate));
         m_session.AddGamePacketHandler (0x1E, new SessionPacketHandler (HandleBannerFlag));
         m_session.AddGamePacketHandler (0x1F, new SessionPacketHandler (HandlePlayerBanner));
         m_session.AddGamePacketHandler (0x20, new SessionPacketHandler (HandleSelfPrize));
         m_session.AddGamePacketHandler (0x21, new SessionPacketHandler (HandleBrickDrop));
         m_session.AddGamePacketHandler (0x22, new SessionPacketHandler (HandleTurfFlagStatus));
         m_session.AddGamePacketHandler (0x23, new SessionPacketHandler (HandleFlagReward));
         m_session.AddGamePacketHandler (0x24, new SessionPacketHandler (HandleSpeedStats));
         m_session.AddGamePacketHandler (0x25, new SessionPacketHandler (HandleToggleUFO));
         m_session.AddGamePacketHandler (0x27, new SessionPacketHandler (HandleKeepAlive));
         m_session.AddGamePacketHandler (0x28, new SessionPacketHandler (HandlePlayerPosition));
         m_session.AddGamePacketHandler (0x29, new SessionPacketHandler (HandleMapInfo));
         m_session.AddGamePacketHandler (0x2A, new SessionPacketHandler (HandleMapFile));
         m_session.AddGamePacketHandler (0x2B, new SessionPacketHandler (HandleSetKoTHTimer));
         m_session.AddGamePacketHandler (0x2C, new SessionPacketHandler (HandleKoTHReset));
         m_session.AddGamePacketHandler (0x2E, new SessionPacketHandler (HandleBallPosition));
         m_session.AddGamePacketHandler (0x2F, new SessionPacketHandler (HandleArenaList));
         m_session.AddGamePacketHandler (0x30, new SessionPacketHandler (HandleBannerAds));
         m_session.AddGamePacketHandler (0x32, new SessionPacketHandler (HandleChangePosition));
         m_session.AddGamePacketHandler (0x35, new SessionPacketHandler (HandleObjectToggle));
         m_session.AddGamePacketHandler (0x36, new SessionPacketHandler (HandleReceivedObject));
         m_session.AddGamePacketHandler (0x37, new SessionPacketHandler (HandleDamageToggle));
         m_session.AddGamePacketHandler (0x38, new SessionPacketHandler (HandleWatchDamage));
      }
   }
}
