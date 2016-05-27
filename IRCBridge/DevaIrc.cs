using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
// Needed for timers
using System.Timers;
// Bot Core stuffs
using BattleCore;
using BattleCore.Events;
// My chat module
using BattleCorePsyOps;
using Meebey.SmartIrc4net;

namespace IRCBridge
{
    // Add the attribute and the base class
    [Behavior("DevaIrc", "false", "0.01", "PsyOps", "Bridge between Deva Zone and IRC.")]
    public class DevaIrc: BotEventListener
    {
        public DevaIrc()
        {
            this.msg = new ShortChat();
            this.m_IRCTimer = new System.Timers.Timer();
            this.m_IRCTimer.Elapsed += new ElapsedEventHandler(myIRCTimer);

            RegisterCommand("!connect", ConnectIRC);
            RegisterCommand("!disco", DisconnectIRC);    
        }


        private ShortChat msg;

        // Main IRC Class
        public static IrcClient irc = new IrcClient();
        // Login info
        string MyNick = "DevaBot";
        string MyPassword = "";
        string MyChannel = "#devastation";
        string MyDescription = "Devastation Bridge Bot";
        string MyServer = "irc.mibbit.net";

        System.Timers.Timer m_IRCTimer;
        
        
        int MyPort = 6667;
        double IRCreconnectDelay = 5;//sec
        int IRCSendDelay = 200;//ms
        string[] IRCCommand = new string[6];
        string[] IRCCommandDesc = new string[6];
        byte irc_seq = 0;

        public void myIRCTimer(object source, ElapsedEventArgs e)
        {
            try
            {
                irc.ListenOnce();
            }
            catch (Exception x)
            {
                //Game(msg.arena("Error (irc.ListenOnce()) : " + x.Message));
            }
        }

        public void ConnectIRC(ChatEvent c)
        {
            //Setup
            irc.Encoding = System.Text.Encoding.UTF8;
            irc.SendDelay = IRCSendDelay * 3;
            irc.AutoRetry = true;
            irc.ActiveChannelSyncing = true;

            //Event Handlers
            irc.OnJoin += new JoinEventHandler(irc_OnJoin);
            irc.OnError += new Meebey.SmartIrc4net.ErrorEventHandler(irc_OnError);
            irc.OnConnected += new EventHandler(irc_OnConnected);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);
            irc.OnDisconnecting += new EventHandler(OnDisco);
            irc.OnQuit += new QuitEventHandler(OnQuit);
            irc.OnKick += new KickEventHandler(irc_OnKick);
            irc.OnBan += new BanEventHandler(irc_OnKick);
            irc.OnUnban += new UnbanEventHandler(irc_OnKick);

            try
            {
                //Connect, log in
                irc.Connect(MyServer, MyPort);
                irc.Login(MyNick, MyDescription);
                Game(msg.arena("IRC Connected."));
                IRCJoinChan();

                m_IRCTimer.Interval = 250;
                m_IRCTimer.Start();
            }
            catch (Exception e)
            {
                Game(msg.arena("IRC Login: " + "Could not connect, exception:" + Environment.NewLine
                    + e.Message + Environment.NewLine
                    + e.ToString()));
            }
        }

        public void MonitorChatEvents(object sender, ChatEvent e)
        {
            if (e.ChatType == ChatTypes.Public && AllowedMessage(e.Message))
                irc.SendMessage(SendType.Message, MyChannel, (e.PlayerName + "> " + e.Message));
        }

        public void DisconnectIRC(ChatEvent e)
        {
            this.m_IRCTimer.Stop();
            irc.Disconnect(); 
        }

        public void IRCJoinChan()
        {
            irc.RfcJoin(MyChannel);
            irc.SendMessage(SendType.Message, "nickserv", "identify " + MyPassword);
            Game(msg.arena("IRC Joined channel[ " + MyChannel + " ]."));
        }

        // Connection confirmation
        public void irc_OnConnected(object sender, EventArgs e)
        {
        }

        // ERROR message
        public void irc_OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e)
        {
        }

        // irc join event
        public void irc_OnJoin(object sender, IrcEventArgs e)
        {
        }

        public void irc_OnKick(object sender, IrcEventArgs e)
        {
        }

        public void OnQuit(object sender, IrcEventArgs e)
        {
        }

        public void OnDisco(object sender, EventArgs e)
        {
            irc.RfcPing(MyServer);
            //IRCreconnect = true;
            Game(msg.arena("Connection lost, attempting to reconnect in (" + IRCreconnectDelay + ") seconds"));
        }

        // this method will get all IRC messages
        public void OnRawMessage(object sender, IrcEventArgs e)
        {
            if (e.Data.Message == null)
                return;

            string sentmsg = StripFormat(e.Data.Message);

            if (e.Data.Type == ReceiveType.ChannelMessage && AllowedMessage(sentmsg))
            {
                Game(msg.arena("[\u0392] " + e.Data.Nick + "> " + StripFormat(e.Data.Message)));
            }
        }

        public void ProcessIncomingIRCMsg(IrcEventArgs e)
        {
        }

        private void ProcessIncomingSSMsg(ChatEvent c)
        {
        }

        public void ProcessIncomingIRCCommand(IrcEventArgs e)
        {
        }

        // Making sure we dont relay commands: IRC => SS or vice versa
        bool AllowedMessage(string m)
        {
            if (m.StartsWith("*") || m.StartsWith("?") || m.StartsWith("!") || m.StartsWith("/") || m.StartsWith(":") || m.StartsWith("."))
                return false;
            return true;
        }
        // Stripping any color code or fomat code - bold, underline, etc..
        string StripFormat(string OriginalMessage)
        {
            if (OriginalMessage == null)
                return null;

            string ircmsg = new Regex(@"\x03(\d{1,2}(,\d{1,2})?)?").Replace(OriginalMessage, "").Trim();
            ircmsg = ircmsg.Trim();
            return ircmsg;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
