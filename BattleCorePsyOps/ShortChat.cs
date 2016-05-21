using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Add the following namespaces
using BattleCore;
using BattleCore.Events;

namespace BattleCorePsyOps
{
    public class ShortChat
    {
        private bool m_Debug = false;

        /// <summary>
        /// Used for debug mode. Assign the chat using .debug and it will send it to your first chat assigned
        /// uasge msg.debug();
        /// message will only go out if you have it set to true
        /// </summary>
        public bool DebugMode
        {
            get { return m_Debug; }
            set { m_Debug = value; }
        }
        /// <summary>
        /// <para>Use this if you want to only send message when debug is set to true.</para>
        /// <para>Make sure to:</para>
        /// <para>1: have your bot set ?chat [Your Debug Chat], any chat,any chat   (debug chat must be your first registered chat)</para>
        /// <para>2: set debug to true if you want to use it</para>
        /// <para>3: use this method to send it</para>
        /// <para>Usage: ShortChat msg = new ShortChat()</para>
        /// <para>Game(msg.debug("Message to send"));</para>
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public ChatEvent debugChan(string Message)
        {
            if (!m_Debug) return null;
            Message = "[DEBUG] " + Message;  
            return chan(1, Message);
        }
        public ChatEvent debugArena(string Message)
        {
            if (!m_Debug) return null;
            Message = "[DEBUG] " + Message;
            return arena(Message);
        }
        public ChatEvent debugArena(string Message, SoundCodes Sound)
        {
            if (!m_Debug) return null;
            Message = "[DEBUG] " + Message;
            return arena(Message, Sound);
        }

        /// <summary>
        /// Private Message: [Example] /Hello
        /// </summary>
        /// <param name="PlayerName">Player's Name</param>
        /// <param name="Message">Message to send</param>
        /// <returns>Return ChatEvent</returns>
        public ChatEvent pm(string PlayerName, string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.PlayerName = PlayerName;
            ce.Message = Message;
            ce.ChatType = ChatTypes.Private;
            return ce;
        }
        /// <summary>
        /// Private Message(with attached sound code): [Example] /Hello %2
        /// </summary>
        /// <param name="PlayerName">Player's Name</param>
        /// <param name="Message">Message to send</param>
        /// <param name="SoundCode">Sound code you want to use</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent pm(string PlayerName, string Message, SoundCodes SoundCode)
        {
            ChatEvent ce = new ChatEvent();
            ce.PlayerName = PlayerName;
            ce.Message = Message;
            ce.SoundCode = SoundCode;
            ce.ChatType = ChatTypes.Private;
            return ce;
        }
        /// <summary>
        /// Remote Private Message:  [Example] :PsyOps: Hello
        /// </summary>
        /// <param name="PlayerName">Player's Name</param>
        /// <param name="Message">Message to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent remote_pm(string PlayerName, string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.PlayerName = PlayerName;
            ce.Message = Message;
            ce.ChatType = ChatTypes.RemotePrivate;
            return ce;
        }

        /// <summary>
        /// Team Message (to freq): [Example] //Hello
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent team(string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.ChatType = ChatTypes.Team;
            return ce;
        }
        /// <summary>
        /// Team Message(with attached sound code): [Example] //Hello %2
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <param name="SoundCode">Sound code to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent team(string Message, SoundCodes SoundCode)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.SoundCode = SoundCode;
            ce.ChatType = ChatTypes.Team;
            return ce;
        }
        /// <summary>
        /// Sends message to a players team (frequency): [Example] "Hello
        /// </summary>
        /// <param name="PlayerName">Player's Name</param>
        /// <param name="Message">Message to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent teamPM(string PlayerName, string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.PlayerName = PlayerName;
            ce.ChatType = ChatTypes.TeamPrivate;
            return ce;
        }
        /// <summary>
        /// Sends message to a player's team (frequency)(with attached sound code): [Example] "Hello %2
        /// </summary>
        /// <param name="PlayerName">Player's Name</param>
        /// <param name="Message">Message to send</param>
        /// <param name="SoundCode">Sound code</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent teamPM(string PlayerName, string Message, SoundCodes SoundCode)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.PlayerName = PlayerName;
            ce.SoundCode = SoundCode;
            ce.ChatType = ChatTypes.TeamPrivate;
            return ce;
        }

        /// <summary>
        /// ?chat Message: [Example] ;1;hello
        /// </summary>
        /// <param name="num">Number of the ?chat you wish to message to</param>
        /// <param name="Message">Message to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent chan(byte num, string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = ";" + num + ";" + Message;
            ce.ChatType = ChatTypes.Channel;
            return ce;
        }

        /// <summary>
        /// Arena Message: [Example] *arena Hello
        /// </summary> 
        /// <param name="Message">Message to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent arena(string Message)
        {
            return arena(Message,SoundCodes.None);
        }
        /// <summary>
        /// Arena Message(with attached sound code): [Example] *arena Hello %2
        /// </summary>
        /// <param name="Message">Message to send</param> 
        /// <param name="SoundCode">Sound code</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent arena(string Message, SoundCodes SoundCode)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.SoundCode = SoundCode;
            ce.ChatType = ChatTypes.Arena;
            return ce;
        }

        /// <summary>
        /// Macro Message [Example] Hello 
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent macro(string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.ChatType = ChatTypes.Macro;
            return ce;
        }

        /// <summary>
        /// Macro Message [Example] Hello 
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <param name="Sound">Sound to use</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent macro(string Message,SoundCodes Sound)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.ChatType = ChatTypes.Macro;
            ce.SoundCode = Sound;
            return ce;
        }

        /// <summary>
        /// Public Message [Example] Hello 
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent pub(string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.ChatType = ChatTypes.Public;
            return ce;
        }
        /// <summary>
        /// Public Message (with attached sound code): [Example] Hello %2
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <param name="SoundCode">Sound code</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent pub(string Message, SoundCodes SoundCode)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.SoundCode = SoundCode;
            ce.ChatType = ChatTypes.Public;
            return ce;
        }

        /// <summary>
        /// Zone Message
        /// </summary>
        /// <param name="Message">Message to send</param> 
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent zone(string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.ChatType = ChatTypes.Zone;
            return ce;
        }
        /// <summary>
        /// Zone Message(with attached sound code): [Example] *zone Hello %2
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <param name="SoundCode">Sound code</param>
        /// <returns>Returns Configured ChatEvent</returns>
        public ChatEvent zone(string Message, SoundCodes SoundCode)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = Message;
            ce.SoundCode = SoundCode;
            ce.ChatType = ChatTypes.Zone;
            return ce;
        }

        /// <summary>
        /// Warning Message
        /// </summary>
        /// <param name="PlayerName">Player to be warned</param>
        /// <param name="Message">Message to send</param>
        /// <returns>Return Configured ChatEvent</returns>
        public ChatEvent warn(string PlayerName, string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = "*warn " + Message;
            ce.PlayerName = PlayerName;
            ce.ChatType = ChatTypes.Private;
            return ce;
        }
        /// <summary>
        /// Shortened *warpto command (not by much)
        /// </summary>
        /// <param name="PlayerName">Player to warp</param>
        /// <param name="xTiles">X coords in tiles NOT PIXELS</param>
        /// <param name="yTiles">Y coords in tiles NOT PIXELS</param>
        /// <returns>Returns configured ChatEvent</returns>
        public ChatEvent warp(string PlayerName, int xTiles, int yTiles)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = "*warpto " + xTiles + " " + yTiles;
            ce.PlayerName = PlayerName;
            ce.ChatType = ChatTypes.Private;
            return ce;
        }
        /// <summary>
        /// Use this to format a channel message properly
        /// </summary>
        /// <param name="chatevent">Incoming ChatEvent</param>
        public void FormatMessage(ChatEvent chatevent)
        {
            if (chatevent.ChatType == ChatTypes.Channel)
            {
                chatevent.PlayerName = chatevent.Message.Substring(2, chatevent.Message.IndexOf("> ") - 2);
                chatevent.Message = (chatevent.Message.Remove(0, chatevent.Message.IndexOf("> ") + 1)).Trim();
            }
        }

        /// <summary>
        /// <para>ASSS ONLY COMMAND</para>
        /// <para>Sends a message using red mod chat.</para>
        /// </summary>
        /// <param name="Message">Message to send.</param>
        /// <returns></returns>
        public ChatEvent mod(string Message)
        {
            ChatEvent ce = new ChatEvent();
            ce.Message = "\\" + Message;
            ce.ChatType = ChatTypes.Public;
            return ce;
        }
    }
}