using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;

namespace BattleCorePsyOps
{
    public class MyGame
    {
        public MyGame()
        {
            m_Events = new Queue<EventArgs>();
            m_SafeEvents = new Queue<EventArgs>();
            m_SafeEventsDelay = 250;
            m_SafeDelayTimeStamp = DateTime.Now;
        }

        private Queue<EventArgs> m_Events;
        private Queue<EventArgs> m_SafeEvents;
        private double m_SafeEventsDelay;
        private DateTime m_SafeDelayTimeStamp;

        /// <summary>
        /// Set the delay in which biller messages are sent
        /// </summary>
        public double BillerMessageDelay
        {
            get { return m_SafeEventsDelay; }
            set { m_SafeEventsDelay = value; }
        }

        /// <summary>
        /// Attach this to your main timer in your main class.
        /// </summary>
        public Queue<EventArgs> EmptyQueue
        {
            get
            {
                // if there is a safe event in q and the delay has passed, add it to outgoing events
                if (m_SafeEvents.Count > 0 && (DateTime.Now - m_SafeDelayTimeStamp).TotalMilliseconds >= m_SafeEventsDelay)
                {
                    m_SafeDelayTimeStamp = DateTime.Now;
                    m_Events.Enqueue(m_SafeEvents.Dequeue());
                }

                return m_Events;
            }
        }

        /// <summary>
        /// Send an event like using Game()
        /// </summary>
        /// <param name="e"></param>
        public void Send(EventArgs e)
        {
            if (e != null) m_Events.Enqueue(e);
        }

        /// <summary>
        /// Send a biller event making sure to have a delay so you dont get kicked for flooding.
        /// </summary>
        /// <param name="e"></param>
        public void SafeSend(EventArgs e)
        {
            if (e != null) m_SafeEvents.Enqueue(e);
        }
    }
}
