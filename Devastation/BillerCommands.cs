using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore.Events;

namespace Devastation
{
    public class BillerCommands
    {
        // Message list to send
        private Queue<ChatEvent> m_MessagesToSend = new Queue<ChatEvent>();

        // Time vars to keep track of how often to send messages
        private DateTime m_LastMessageTimeStamp = DateTime.Now;
        private double m_DelayForMessages = 250; // in ms

        private DateTime m_InitializeTS = DateTime.Now;
        private double m_InitializeTime = 1500;
        private bool m_Initialized = false;

        // Add a message to list
        public void SendMessage(ChatEvent c)
        {
            if (c != null) m_MessagesToSend.Enqueue(c);
        }

        // Chat message packet to send
        public ChatEvent MessageToSend
        {
            get
            {
                if (!m_Initialized)
                {
                    if ((DateTime.Now - m_InitializeTS).TotalMilliseconds > m_InitializeTime)
                        m_Initialized = true;
                    return null;
                }
                // If there are no messages to send or it is too early - dont update
                if (!(m_MessagesToSend.Count > 0) || (DateTime.Now - m_LastMessageTimeStamp).TotalMilliseconds < m_DelayForMessages) return null;
                // update timestamp and send message
                m_LastMessageTimeStamp = DateTime.Now;
                return m_MessagesToSend.Dequeue();
            }
        }
    }
}
