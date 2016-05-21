//-----------------------------------------------------------------------
//
// NAME:        ReliableMessageHandler.cs
//
// PROJECT:     Battle Core Session Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Reliable Message Handler implementation.
//
// NOTES:       None.
//
// $History: ReliableMessageHandler.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using BattleCore.Protocol;
using System.Threading;

// Namespace declaration
namespace BattleCore.Session
{
   /// <summary>
   /// Releable message handler object.  This object is used to process
   /// pending reliable messages waiting for an acknowlegement.
   /// </summary>
   class ReliableMessageHandler
   {
      // private member data
      private UInt32 m_nTransactionId  = 0;   // Current Transaction id
      private UInt32 m_nRetransmitTime = 750; // Time to retransmit a message

      // Pending Reliable message list
      private SortedList m_reliableList = new SortedList ();

      // Create the reliable list mutex
      private Mutex m_mutex = new Mutex ();

      /// <summary>
      /// Property to set the Retransmit Timeout value
      /// </summary>
      public UInt32 RetransmitTime
      {
         set { m_nRetransmitTime = value; }
      }

      /// <summary>
      /// Add a reliable wrapper to a packet
      /// </summary>
      /// <param name="packet">Message Packet</param>
      /// <returns>New message packet</returns>
      public Byte[] GetReliablePacket (Byte[] packet)
      {
         // Create a new reliable packet
         ReliablePacket reliablePacket = new ReliablePacket ();

         // Set the packet data
         reliablePacket.TransactionId = m_nTransactionId;
         reliablePacket.Message       = packet;
         reliablePacket.TimeStamp     = TimeSpan.FromMilliseconds(Environment.TickCount);

         // Increment the transaction identifier
         m_nTransactionId ++;

         // Lock the mutex
         if (m_mutex.WaitOne (500, false))
         {
            // Add the reliable packet to the list to be acknowleged
            m_reliableList.Add (reliablePacket.TransactionId, reliablePacket);

            // release the mutex
            m_mutex.ReleaseMutex ();
         }
         // return the packet data
         return reliablePacket.Packet;
      }

      /// <summary>
      /// Handle a received reliable acknowlegement
      /// </summary>
      /// <param name="nTransactionId">Transaction Identifier</param>
      /// <returns></returns>
      public void HandleAcknowlegement (UInt32 nTransactionId)
      {
         // Lock the mutex
         if (m_mutex.WaitOne (500, false))
         {
            // Remove the element with the matching transaction identifier
            m_reliableList.Remove (nTransactionId);

            // release the mutex
            m_mutex.ReleaseMutex ();
         }
      }

      /// <summary>
      /// Get a packet that is ready to retransmit.
      /// </summary>
      /// <returns>Packet Data</returns>
      public Byte[] GetRetransmitPacket ()
      {
         Byte[] pPacket = new Byte[0];

         // Lock the mutex
         if (m_mutex.WaitOne (500, false))
         {
            IDictionaryEnumerator listEnum = m_reliableList.GetEnumerator ();
            ReliablePacket pReliable;

            // Get the current timestamp
            TimeSpan currentTime = TimeSpan.FromMilliseconds (Environment.TickCount);
            TimeSpan retransmitTime = TimeSpan.FromMilliseconds (m_nRetransmitTime);

            // Find a message that needs to be sent
            while (listEnum.MoveNext () && (pPacket.Length == 0))
            {
               // Get the next reliable packet
               pReliable = (ReliablePacket)(listEnum.Value);

               // Check if the retransmit timer expired
               if (currentTime > (pReliable.TimeStamp + retransmitTime))
               {
                  // Set the message timestamp
                  pReliable.TimeStamp = currentTime;

                  // Get the packet to retransmit
                  pPacket = pReliable.Packet;
               }
            }

            // release the mutex
            m_mutex.ReleaseMutex ();
         }

         // Return the packet
         return pPacket;
      }
   }
}
