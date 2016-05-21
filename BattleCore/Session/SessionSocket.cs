//-----------------------------------------------------------------------
//
// NAME:        SessionSocket.cs
//
// PROJECT:     Battle Core Session Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Session Socket implementation.
//
// NOTES:       None.
//
// $History: SessionStatistics.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.IO;
using BattleCore.Protocol;

// Namespace declaration
namespace BattleCore.Session
{
   /// <summary>
   /// Packet direction enumeration
   /// </summary>
   internal enum PacketDirection { Transmit, Receive };

   /// <summary>
   /// Packet logger delegate called when a packet is transmitted and received.
   /// </summary>
   /// <param name="direction">packet direction enumeration</param>
   /// <param name="packet">packet buffer</param>
   internal delegate void PacketLogger  (PacketDirection direction, Byte[] packet);

   /// <summary>
   /// Session Socket definition
   /// </summary>
   internal class SessionSocket
   {
      // Constant size definitions
      private const int LargeChunkSize = 496;
      private const int SmallChunkSize = 500;

      // Buffer to handle small chunked data
      private MemoryStream m_chunkBuffer = new MemoryStream ();

      // Buffer to handle large buffer data
      private MemoryStream m_largeBuffer = new MemoryStream(); 

      // Client Socket Connection
      private UdpClient m_pSocket = new UdpClient();

      // Packet Encryption Handler
      private PacketEncryption m_packetEncryption = new PacketEncryption ();

      // Reliable Message handler
      private ReliableMessageHandler m_reliableMessageHandler = new ReliableMessageHandler ();

      private ArrayList m_pTransmitQueue = new ArrayList ();
      private Queue m_pReceiveQueue = new Queue ();

      // Packet logger delegate
      private PacketLogger m_packetLogger;

      // Statistic data
      private UInt32 m_nPacketsSent;             // Number of packets sent
      private UInt32 m_nPacketsReceived;         // Number of packets received
      private UInt32 m_nReliablePacketsReceived; // Reliable packets received

      public UInt32 PacketsSent { get { return m_nPacketsSent; } }
      public UInt32 PacketsReceived { get { return m_nPacketsReceived; } }
      public UInt32 ReliablePacketsReceived { get { return m_nReliablePacketsReceived; } }
      public UInt32 ClientEncryptKey { get { return m_packetEncryption.ClientKey; } }
      public UInt32 RetransmitTime { set { m_reliableMessageHandler.RetransmitTime = value; } }

      #region Socket Thread information
      private class SocketThreadInfo
      {
         public ManualResetEvent m_pKill = new ManualResetEvent(false);  // Thread kill event
         public ManualResetEvent m_pDead = new ManualResetEvent (true);  // Thread dead event
      };

      Thread m_pTxThread;  // Transmit Thread
      Thread m_pRxThread;  // Receive Thread

      SocketThreadInfo m_transmitThreadInfo = new SocketThreadInfo();
      SocketThreadInfo m_receiveThreadInfo = new SocketThreadInfo();
      #endregion

      /// <summary>
      /// SessionSocket Constructor
      /// </summary>
      public SessionSocket ()
      {
         // Set the delegate event handlers
         m_packetLogger = new PacketLogger (LogPacket);
      }

      /// <summary>
      /// Property to add a packet logger
      /// </summary>
      public PacketLogger PacketLog { set { m_packetLogger += value; } }

      /// <summary>
      /// Open a connection with the server and start the 
      /// transmit and receive threads.
      /// </summary>
      /// <param name="strServerName">Server Name</param>
      /// <param name="nServerPort">Remote Server port</param>
      /// <returns>True if successful</returns>
      public Boolean Connect (String strServerName, int nServerPort)
      {
         Boolean bConnected = true;

         // Create a new Udp Client
         m_pSocket = new UdpClient ();

         try
         {
            // Create a connection with the server
            m_pSocket.Connect (strServerName, nServerPort);

            // Reset the thread events
            m_transmitThreadInfo.m_pKill.Reset ();
            m_receiveThreadInfo.m_pKill.Reset ();
            m_transmitThreadInfo.m_pDead.Reset ();
            m_receiveThreadInfo.m_pDead.Reset ();

            // Reset the socket statistics  
            m_nPacketsSent     = 0;     
            m_nPacketsReceived = 0; 

            // Create the socket transmit and receive threads
            m_pTxThread = new Thread (new ThreadStart (TxThread));
            m_pRxThread = new Thread (new ThreadStart (RxThread));

            // Start the transmit and receive threads
            m_pTxThread.Start ();
            m_pRxThread.Start ();
         }
         catch (Exception e)
         {
            // Write the exception to the console
            Console.WriteLine ("Exception : {0}", e);

            // Set the transmit thread shutdown events
            m_transmitThreadInfo.m_pKill.Set ();

            // Set the receive thread shutdown events
            m_receiveThreadInfo.m_pKill.Set ();

            // Set the return value to false
            bConnected = false;
         }

         return bConnected;
      }

      /// <summary>
      /// Close the connection with the server
      /// </summary>
      public void Close ()
      {
         // Wait for the transmit thread to exit
         m_transmitThreadInfo.m_pKill.Set ();
         m_transmitThreadInfo.m_pDead.WaitOne (500, false);

         // Close the connection with the server
         m_pSocket.Close ();
       
         // Wait for the receive thread to exit
         m_receiveThreadInfo.m_pKill.Set ();
         m_receiveThreadInfo.m_pDead.WaitOne (500, false);
      }

      /// <summary>
      /// Tests if the socket has an active connection to the server
      /// </summary>
      /// <returns>Active state</returns>
      public Boolean Active 
      {
         get
         {
            // Return true if both threads are active
            return ((!m_transmitThreadInfo.m_pDead.WaitOne (0, false))
                 && (!m_receiveThreadInfo.m_pDead.WaitOne (0, false)));
         }
      }

      /// <summary>
      /// Transmit a packet to the server
      /// </summary>
      /// <param name="pData">packet data</param>
      public void TransmitPacket (Byte[] pData)
      {
         try
         {
            // Check if the message should be broken up
            if (pData.Length > (LargeChunkSize + 12))
            {
               Int32 nPosition = 0;  // Current buffer position

               // Check if this is a large message
               if (pData.Length > 1000)
               {
                  // Create the large chunk packet
                  LargeChunkPacket pLargeChunkPacket = new LargeChunkPacket ();

                  // Set the total length of the packet
                  pLargeChunkPacket.TotalLength = (UInt32)pData.Length;

                  while (nPosition < pData.Length)
                  {
                     // Get the remaining length
                     Int32 nLength = Math.Min ((pData.Length - nPosition), LargeChunkSize);

                     // Create the message buffer
                     Byte[] pcMessage = new Byte[nLength];

                     // Copy the data into the new message buffer
                     Array.Copy (pData, nPosition, pcMessage, 0, nLength);

                     // Copy the message data into the Large Chunk packet
                     pLargeChunkPacket.Message = pcMessage;

                     // Get the large chunk packet
                     Byte[] pLargeChunk = pLargeChunkPacket.Packet;

                     // Write the data to the packet loggers
                     m_packetLogger (PacketDirection.Transmit, pLargeChunk);

                     // Add the message to the transmit queue
                     m_pTransmitQueue.Add (pLargeChunk);

                     // Increment the current position
                     nPosition += pcMessage.Length;
                  }
               }
               else
               {
                  // Create the chunked data packets 
                  ChunkBodyPacket pChunkBodyPacket = new ChunkBodyPacket ();
                  ChunkTailPacket pChunkTailPacket = new ChunkTailPacket ();

                  while (nPosition < pData.Length)
                  {
                     // Get the remaining length
                     Int32 nLength = Math.Min ((pData.Length - nPosition), SmallChunkSize);

                     // Create the message buffer
                     Byte[] pcMessage = new Byte[nLength];

                     // Copy the data into the new message buffer
                     Array.Copy (pData, nPosition, pcMessage, 0, nLength);

                     // Increment the current position
                     nPosition += nLength;

                     if (pcMessage.Length == SmallChunkSize)
                     {
                        // Copy the message data into the Chunk Body packet
                        pChunkBodyPacket.Message = pcMessage;

                        Byte[] pChunkBody = pChunkBodyPacket.Packet;

                        // Write the data to the packet loggers
                        m_packetLogger (PacketDirection.Transmit, pChunkBody);

                        // Add the message to the transmit queue
                        m_pTransmitQueue.Add (pChunkBody);
                     }
                     else
                     {
                        // Copy the message data into the Chunk Tail packet
                        pChunkTailPacket.Message = pcMessage;

                        // Ge the chunk tail packet
                        Byte[] pChunkTail = pChunkTailPacket.Packet;

                        // Write the data to the packet loggers
                        m_packetLogger (PacketDirection.Transmit, pChunkTail);

                        // Add the message to the transmit queue
                        m_pTransmitQueue.Add (pChunkTail);
                     }
                  }
               }
            }
            else
            {
               // Write the data to the packet loggers
               m_packetLogger (PacketDirection.Transmit, pData);

               // Add the data to the end of the queue
               m_pTransmitQueue.Add (pData);
            }
         }
         catch (Exception e)
         {
            Console.WriteLine (e);
         }
      }

      /// <summary>
      /// Transmit a reliable packet to the server
      /// </summary>
      /// <param name="pData">Packet data</param>
      public void TransmitReliablePacket (Byte[] pData)
      {
         // Make the packet reliable
         pData = m_reliableMessageHandler.GetReliablePacket (pData);

         // Transmit the packet
         TransmitPacket (pData);
      }

      /// <summary>
      /// Receive a packet from the server
      /// </summary>
      /// <returns>Received packet data</returns>
      public Byte[] ReceivePacket ()
      {
         ReliablePacket   pReliablePacket   = new ReliablePacket ();
         AcknowlegePacket pAckPacket        = new AcknowlegePacket ();
         Byte[]           pData             = new Byte[0];
         ClusteredPacket  pClusteredPacket  = new ClusteredPacket (); 
         ChunkBodyPacket  pChunkBodyPacket  = new ChunkBodyPacket ();
         ChunkTailPacket  pChunkTailPacket  = new ChunkTailPacket ();
         LargeChunkPacket pLargeChunkPacket = new LargeChunkPacket ();

         // Check if there are any elements on the queue
         if (m_pReceiveQueue.Count > 0)
         {
            // Get the next element from the receive queue
            pData = (Byte[])(m_pReceiveQueue.Dequeue());

            if (pData != null)
            {
               // Write the data to the packet loggers
               m_packetLogger (PacketDirection.Receive, pData);

               // Check if the message is reliable
               if (pData[0] == 0x00)
               {
                  switch (pData[1])
                  {
                  case 0x03:
                     // Set the packet information
                     pReliablePacket.Packet = pData;

                     // Extract the message 
                     pData = pReliablePacket.Message;

                     // Increment the reliable receive counter
                     m_nReliablePacketsReceived++;

                     // Set the acknowlege transaction identifier and transmit
                     pAckPacket.TransactionId = pReliablePacket.TransactionId;
                     TransmitPacket (pAckPacket.Packet);
                     break;

                  case 0x04:
                     // Set the acknowlegement packet data
                     pAckPacket.Packet = pData;

                     // Handle the Acknowlege packet
                     m_reliableMessageHandler.HandleAcknowlegement (pAckPacket.TransactionId);
                     break;
                  }

                  // Check if the packet is a chunk body packet
                  if ((pData[0] == 0x00) && (pData[1] == 0x08))
                  {
                     // Set the chunk data packet
                     pChunkBodyPacket.Packet = pData;

                     // Get the message packet from the buffer
                     pData = pChunkBodyPacket.Message;

                     // Append the data to the buffer
                     m_chunkBuffer.Write (pData, 0, pData.Length);

                     // Clear the returned data
                     pData = new Byte[0];
                  }
                  // Check if the packet is a chunk tail packet
                  else if ((pData[0] == 0x00) && (pData[1] == 0x09))
                  {
                     // Set the chunk data packet
                     pChunkTailPacket.Packet = pData;

                     // Get the message packet from the buffer
                     pData = pChunkTailPacket.Message;

                     // Append the data to the chunk buffer
                     m_chunkBuffer.Write (pData, 0, pData.Length);

                     // return the full data packet
                     pData = m_chunkBuffer.ToArray ();

                     // Clear the chunk buffer
                     m_chunkBuffer.Position = 0;
                  }
                  // Check if the packet is a large chunk packet
                  else if ((pData[0] == 0x00) && (pData[1] == 0x0A))
                  {
                     // Set the large chunk packet
                     pLargeChunkPacket.Packet = pData;

                     // Get the message packet from the buffer
                     pData = pLargeChunkPacket.Message;

                     // Append the message to the large buffer
                     m_largeBuffer.Write (pData, 0, pData.Length);

                     // Check if the packet is complete
                     if (m_largeBuffer.Length == pLargeChunkPacket.TotalLength)
                     {
                        // return the full data packet
                        pData = m_largeBuffer.ToArray ();

                        // Clear the chunk buffer
                        m_largeBuffer.Position = 0;
                     }
                     else
                     {
                        // Clear the returned data
                        pData = new Byte[0];
                     }
                  }
                  // Check if the packet is a clustered packet
                  else if ((pData[0] == 0x00) && (pData[1] == 0x0E))
                  {
                     // Set the clustered packet data
                     pClusteredPacket.Packet = pData;

                     // Get the enumerator for the packet list
                     IEnumerator pEnum = pClusteredPacket.Packets.GetEnumerator ();

                     while (pEnum.MoveNext ())
                     {
                        // Add the next element to the receive queue
                        m_pReceiveQueue.Enqueue ((Byte[])pEnum.Current);
                     }

                     // Clear the returned data
                     pData = new Byte[0];
                  }
               }
            }
         }
         
         return pData;

      }

      /// <summary>
      /// Packet logger delegate called when a packet is transmitted and received.
      /// </summary>
      /// <param name="direction">packet direction enumeration</param>
      /// <param name="packet">packet buffer</param>
      private void LogPacket (PacketDirection direction, Byte[] packet)
      {

      }

      /// <summary>
      /// Socket transmit thread
      /// </summary>
      public void TxThread ()
      {
         // Execute until a kill event occurs
         while (!m_transmitThreadInfo.m_pKill.WaitOne (5, false))
         {
            try
            {
               // Check the reliable message queue for retransmits
               Byte[] pReliableData = m_reliableMessageHandler.GetRetransmitPacket ();

               // If a packet is found
               if (pReliableData.Length > 0)
               {
                  // Add the message to the transmit queue
                  TransmitPacket (pReliableData);
               }

               if (m_pTransmitQueue.Count != 0)
               {
                  // Check if there is more than one packet 
                  if (m_pTransmitQueue.Count > 1)
                  {
                     // Create a clustered packet
                     ClusteredPacket packetCluster = new ClusteredPacket ();

                     while (m_pTransmitQueue.Count > 0)
                     {
                        // Get the next packet 
                        Byte[] nextPacket = (Byte[])(m_pTransmitQueue[0]);

                        if (nextPacket != null)
                        {
                           // If the packet is too big to cluster
                           if (nextPacket.Length > 256)
                           {
                              // Encrypt and transmit the packet
                              EncryptAndTransmitPacket (nextPacket);
                           }
                           else
                           {
                              // Add the next packet to the cluster
                              packetCluster.AddPacket (nextPacket);

                              if (packetCluster.ClusterLength > 256)
                              {
                                 // Encrypt and transmit the packet
                                 EncryptAndTransmitPacket (packetCluster.Packet);
                              }
                           }
                        }
                        // Remove the item from the queue
                        m_pTransmitQueue.RemoveAt (0);
                     }

                     if (packetCluster.ClusterLength > 0)
                     {
                        // Encrypt and transmit the packet
                        EncryptAndTransmitPacket (packetCluster.Packet);
                     }
                  }
                  else
                  {
                     // Encrypt and transmit the packet
                     EncryptAndTransmitPacket ((Byte[])(m_pTransmitQueue[0]));

                     // Remove the item from the queue
                     m_pTransmitQueue.RemoveAt (0);
                  }
               }
            }
            catch (Exception e)
            {
               Console.WriteLine (e);
               m_receiveThreadInfo.m_pKill.Set ();
               m_transmitThreadInfo.m_pKill.Set();
            }
         }

         try
         {
            // Send any outstanding messages
            while (m_pTransmitQueue.Count > 0)
            {
               // Get the next packet to transmit
               Byte[] pData = (Byte[])(m_pTransmitQueue[0]);

               // Encrypt the next packet
               pData = m_packetEncryption.Encrypt (pData);

               // Send the packet to the server
               m_pSocket.Send (pData, pData.Length);

               // Remove the item from the queue
               m_pTransmitQueue.RemoveAt (0);
            }
         }
         catch (Exception e)
         {
            Console.WriteLine (e);
         }

         // Reset the kill event
         m_transmitThreadInfo.m_pKill.Reset ();

         // Set the thread dead event
         m_transmitThreadInfo.m_pDead.Set ();
      }

      /// <summary>
      /// Encrypt and transmit a packet to the server
      /// </summary>
      /// <param name="packet"></param>
      void EncryptAndTransmitPacket (Byte[] packet)
      {
         if (packet != null)
         {
            // Encrypt the packet
            packet = m_packetEncryption.Encrypt (packet);

            // Increment the packets sent count
            m_nPacketsSent++;

            // Send the packet to the server
            m_pSocket.Send (packet, packet.Length);
         }
      }
      /// <summary>
      /// Socket receive thread
      /// </summary>
      public void RxThread ()
      {
         // Execute until a kill event occurs
         while (!m_receiveThreadInfo.m_pKill.WaitOne (1, false))
         {
            try
            {
               // Create an IpEndPoint to capture the identity of the sending host.
               IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

               // Create the receive buffer
               Byte[] pData = m_pSocket.Receive (ref sender);

               // Wait for the next packet to arrive
               if (pData.Length > 0)
               {
                  // Decrypt the data
                  pData = m_packetEncryption.Decrypt (pData);

                  // Increment the packets received count
                  m_nPacketsReceived ++;

                  // Add the packet to the receive queue
                  m_pReceiveQueue.Enqueue (pData);
               }
            } 
            catch (Exception e)
            {
               Console.WriteLine (e);
               m_receiveThreadInfo.m_pKill.Set();
               m_transmitThreadInfo.m_pKill.Set();
            }
         }

         // Reset the kill event
         m_receiveThreadInfo.m_pKill.Reset();

         // Set the thread dead event
         m_receiveThreadInfo.m_pDead.Set();
      }
   }
}
