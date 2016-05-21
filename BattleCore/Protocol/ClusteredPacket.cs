//-----------------------------------------------------------------------
//
// NAME:        ClusteredPacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Clustered Packet implementation.
//
// NOTES:       None.
//
// $History: ClusteredPacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using BattleCore.Events;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// ClusteredPacket object.  This object is used to send the last 
   /// chunk of a message.
   /// </summary>
   internal class ClusteredPacket : IPacket
   {
      /// <summary>
      /// Memory stream containing the packet data
      /// </summary>
      private MemoryStream m_Cluster = new MemoryStream ();
      private ArrayList m_packets = new ArrayList ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// Method to add a packet to the clustered stream
      /// </summary>
      /// <param name="packet"></param>
      public void AddPacket (Byte[] packet)
      {
         // Add the packet to the array list
         m_packets.Add (packet);

         // Write the packet to the clustered data
         m_Cluster.WriteByte ((Byte)packet.Length);
         m_Cluster.Write (packet, 0, packet.Length);
      }

      /// <summary>
      /// Property to get hte current lenth of the cluster
      /// </summary>
      public UInt16 ClusterLength
      {
         get { return (UInt16)m_Cluster.Length; }
      }

      /// <summary>
      /// Packetes Property
      /// </summary>
      public ArrayList Packets 
      {
         get { return m_packets; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            UInt16 nBufferIndex  = 2;
            Byte   nPacketLength = 0;

            while (nBufferIndex < value.Length)
            {
               // Get the length of the next packet
               nPacketLength = value[nBufferIndex ++];

               // Create a new byte array to hold the message
               Byte[] nextPacket = new Byte[nPacketLength];

               // copy the data into the new packet
               Array.Copy (value, nBufferIndex, nextPacket, 0, nPacketLength);

               // add the packet to the packet list
               m_packets.Add (nextPacket);

               // increment the message index
               nBufferIndex += nPacketLength;
            }
         }

         get
         {
            // Create the packet memory stream
            MemoryStream packet = new MemoryStream ();

            // Write the packet data
            packet.WriteByte (0x00);
            packet.WriteByte (0x0E);
            packet.Write (m_Cluster.ToArray (), 0, (int)m_Cluster.Length);

            // Reinitialize the cluster
            m_Cluster = new MemoryStream ();

            // Return the packet data
            return packet.ToArray ();
         }
      }
   }
}
