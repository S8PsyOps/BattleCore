using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

// Needed core files
using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;

namespace Devastation
{
    // Add the attribute and the base class
    [Behavior("Help", "true", "0.01", "PsyOps", "Module to assist with Help commands.")]
    public class Help : BotEventListener
    {
        public Help()
        {
            RegisterCommand("!refreshhelp", Refresh);
            RegisterCommand(".refreshhelp", Refresh);
            RegisterTimedEvent("HelpStart", 1000, StartBot);
        }

        private List<HelpMessage> m_MasterHelpList = new List<HelpMessage>();

        class HelpMessage
        {
            private string m_Command;
            public string Command { get { return m_Command; } }

            private string[] m_Description;
            public string[] Description { get { return m_Description; } }

            public HelpMessage(string command, string[] description)
            {
                this.m_Command = command;
                this.m_Description = description;
            }
        }

        // Custom chat module
        private ShortChat msg = new ShortChat();

        private string m_BotName = null;
        bool Initialized = false;

        public void StartBot()
        {
            RemoveTimedEvent("HelpStart");
            if (Initialized) return;
            Initialized = true;
            Game(new BotInfoRequest());
        }
        public void GetBotInfo(object sender, BotInfoRequest b)
        {
            if (m_BotName != null) return;
            m_BotName = b.BotName;
            LoadCFG();
        }
        public void Refresh(ChatEvent c)
        {
            if (c.ModLevel >= ModLevels.Sysop) LoadCFG();
        }
        public void MonitorPlayerChat(object sender, ChatEvent c)
        {
            if (c.Message.ToLower().StartsWith("!help") || c.Message.ToLower().StartsWith(".help") || c.Message.ToLower().StartsWith("!halp") || c.Message.ToLower().StartsWith(".halp")) DoHelpCommand(c);
        }
        public void DoHelpCommand(ChatEvent c)
        {
            string cmd = c.Message.Trim().ToLower().Remove(0, 1);
            if (cmd == "help" || cmd == "halp")
            {
                PrintHelpMessage(c.PlayerName, "overview");
                return;
            }
            else if (c.Message.Contains(" ") || c.Message.Length > 5)
            {
                string command = c.Message.Remove(0, 5).Trim().ToLower();
                PrintHelpMessage(c.PlayerName, command);
                return;
            }

            // Code shouldnt get here.
            Game(msg.arena("I wear a pink tutu with orange socks."));
        }

        public void PrintHelpMessage(string player, string command)
        {
            for (int i = 0; i < m_MasterHelpList.Count; i += 1)
            {
                if (m_MasterHelpList[i].Command == command)
                {
                    for (int j = 0; j < m_MasterHelpList[i].Description.Length; j += 1)
                    {
                        Game(msg.pm(player, m_MasterHelpList[i].Description[j]));
                    }
                    return;
                }
            }
            Game(msg.pm(player, "Im sorry I do not understand the command [" + command + "]."));
            return;
        }
        public void LoadCFG()
        {
            m_MasterHelpList = new List<HelpMessage>();
            string BotPath = Directory.GetCurrentDirectory();

            // Check to see if user setup folder properly
            if (Directory.Exists(BotPath + "/" + m_BotName + "/help"))
            {
                // grab all help file names
                string[] filePaths = Directory.GetFiles(BotPath + "/" + m_BotName + "/help", "*.help");

                // Go through each file
                for (int i = 0; i < filePaths.Length; i += 1)
                {
                    string command = System.IO.Path.GetFileNameWithoutExtension(filePaths[i]);

                    try
                    {
                        //Opening appropriate file
                        StreamReader SR = File.OpenText(filePaths[i]);
                        //used to read each line
                        string S;

                        // Temp list to store printout
                        List<string> tmp = new List<string>();

                        // Extract and save all info from the cfg file
                        while ((S = SR.ReadLine()) != null)
                            tmp.Add(S.Trim());

                        // Only add command if the file contained info
                        if (tmp.Count > 0)
                            m_MasterHelpList.Add(new HelpMessage(command.ToLower(), tmp.ToArray()));

                        // Closing file
                        SR.Close();
                    }
                    catch (Exception ex)
                    { Game(msg.arena("Help File [" + command + "] Load Error:" + ex)); }
                }

                Game(msg.arena("Help Files Loaded. Command total:[" + m_MasterHelpList.Count + "]"));
            }
            else
                Game(msg.arena("Help directory not found."));
        }
        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

