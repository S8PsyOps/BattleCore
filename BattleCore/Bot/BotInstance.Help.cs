using System;
using System.Collections.Generic;
using System.Text;

using BattleCore.Session;
using BattleCore.Protocol;
using BattleCore.Events;
using BattleCore.Core;
using BattleCore.Settings;

namespace BattleCore.Bot
{
   /// <summary>
   /// Bot Instance Help command handlers
   /// </summary>
   partial class BotInstance
   {
       /// <summary>
       /// Handle the !help request 
       /// </summary>
       /// <param name="chatEvent"></param>
       private void HandleBotHelpCommand(ChatEvent chatEvent)
       {
           // Create the chat response message
           ChatPacket response = new ChatPacket();

           // Set the player Identifier top reply to
           response.Event.PlayerId = chatEvent.PlayerId;
           response.Event.ChatType = ChatTypes.Private;

           // Show the header block
           response.Event.Message = "< ===================== >";
           m_session.TransmitPacket(response);
           response.Event.Message = "<    BattleCore Help    >";
           m_session.TransmitPacket(response);
           response.Event.Message = "< ===================== >";
           m_session.TransmitPacket(response);

           if (chatEvent.ModLevel >= ModLevels.SMod)
           {
               response.Event.Message = "< !blist              | Lists loaded and available behaviors";
               m_session.TransmitPacket(response);
               response.Event.Message = "< !load               | Load a behavior ";
               m_session.TransmitPacket(response);
               response.Event.Message = "< !unload             | Unload a behavior";
               m_session.TransmitPacket(response);
               response.Event.Message = "< !listbots           | list available bots";
               m_session.TransmitPacket(response);
               response.Event.Message = "< !spawn Bot:Arena    | Spawn a new bot in arena";
               m_session.TransmitPacket(response);
               response.Event.Message = "< !kill               | kills the bot (must be PM)";
               m_session.TransmitPacket(response);
           }

           // Show All Available Help Commands             
           response.Event.Message = "< !info               | Display core information";
           m_session.TransmitPacket(response);

           response.Event.Message = "< ===================== >";
           m_session.TransmitPacket(response);
       }
      /// <summary>
      /// Handle the !help request 
      /// </summary>
      /// <param name="chatEvent"></param>
       private void HandleHelpCommand(ChatEvent chatEvent)
       {
           // Create the chat response message
           ChatPacket response = new ChatPacket();

           // Set the player Identifier top reply to
           response.Event.PlayerId = chatEvent.PlayerId;
           response.Event.ChatType = ChatTypes.Private;
           Queue<string> helpmsgs = new Queue<string>();


           // Show the header block
           //response.Event.Message = "< ===================== >";
           //m_session.TransmitPacket(response);
           //response.Event.Message = "<  Registered Commands  >";
           //m_session.TransmitPacket(response);
           //response.Event.Message = "< ===================== >";
           //m_session.TransmitPacket(response);


           // Show All Available Help Commands             
           foreach (Object behavior in m_core.LoadedBehaviors)
           {
               // Get the attributes for the listener
               object[] os = m_core.GetAttributes(behavior.GetType().ToString());

               foreach (object obj in os)
               {
                   // If the object is a Behavior attribute
                   if (obj is CommandHelpAttribute)
                   {
                       CommandHelpAttribute ca = (obj as CommandHelpAttribute);

                       // Check the command access level
                       if (ca.AccessLevel <= chatEvent.ModLevel)
                       {
                           //response.Event.Message = "< " + ca.Command.PadRight(20, ' ') + "| " + ca.Description;
                           helpmsgs.Enqueue("< " + ca.Command.PadRight(20, ' ') + "| " + ca.Description);
                           //m_session.TransmitPacket(response);
                       }
                   }
               }
           }

           if (helpmsgs.Count > 0)
           {
               response.Event.Message = "< ===================== >";
               m_session.TransmitPacket(response);
               response.Event.Message = "<  Registered Commands  >";
               m_session.TransmitPacket(response);
               response.Event.Message = "< ===================== >";
               m_session.TransmitPacket(response);

               for (int i = 0; i < helpmsgs.Count; i++)
               {
                   response.Event.Message = helpmsgs.Dequeue();
                   m_session.TransmitPacket(response);
               }

               response.Event.Message = "< ===================== >";
               m_session.TransmitPacket(response);
           }
       }
       /// <summary>
      /// Handle the !Unf request 
      /// </summary>
      /// <param name="chatEvent"></param>
      private void HandleUnfCommand(ChatEvent chatEvent)
      {
          // Create the chat response message
          ChatPacket response = new ChatPacket();

          // Set the player Identifier top reply to
          response.Event.PlayerId = chatEvent.PlayerId;
          response.Event.ChatType = ChatTypes.Private;
          response.Event.SoundCode = SoundCodes.Ohhhhh;

          if (chatEvent.PlayerName == "PsyOps")
          {
              response.Event.Message = "Unf always and forever!";
          }
          else
          {
              response.Event.Message = "Im sorry silly lil person, I belong to PsyOps!";
          }

          m_session.TransmitPacket(response);
      }
   }
}
