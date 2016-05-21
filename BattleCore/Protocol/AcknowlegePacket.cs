//-----------------------------------------------------------------------
//
// NAME:        AcknowlegePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Acknowlege Packet implementation.
//
// NOTES:       None.
//
// $History: AcknowlegePacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// Acknowlege Packet object
   /// </summary>
   internal class AcknowlegePacket
   {
      private UInt32 m_nTransactionId; // Transaction identifier

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return true; } }

      /// <summary>
      /// Transaction identifier property
      /// </summary>
      public UInt32 TransactionId
      {
         set { m_nTransactionId = value; }
         get { return m_nTransactionId; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set 
         {
            // Extract the Transaction identifier
            m_nTransactionId = BitConverter.ToUInt32 (value, 2);
         }
         get
         {
            MemoryStream packet = new MemoryStream (6);

            // Create the packet memory stream
            packet.WriteByte (0x00);
            packet.WriteByte (0x04);
            packet.Write (BitConverter.GetBytes (m_nTransactionId), 0, 4);

            // Return the packet data
            return packet.ToArray ();
         }
      }

   }
}
