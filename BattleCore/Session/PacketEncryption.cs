//-----------------------------------------------------------------------
//
// NAME:        PacketEncryption.cs
//
// PROJECT:     Battle Core Session Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Packet Encryption implementation.
//
// NOTES:       None.
//
// $History: PacketEncryption.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

// Namespace declaration
namespace BattleCore.Session
{
   /// <summary>
   /// PacketEncryption object used to encrypt and decrypt 
   /// protocol packets.
   /// </summary>
   internal class PacketEncryption
   {
      // Private member data
      UInt32 m_nSessionKey;
      UInt32 m_nClientKey;

      // Create the key buffer memory stream
      Byte[] m_keyBuffer;

      /// <summary>
      /// Property to get the client encryption key
      /// </summary>
      public UInt32 ClientKey
      {
         get
         {
           
	         if (m_nClientKey == 0)
            {
               m_nClientKey = (UInt32)(-(new Random()).Next() * Int32.MaxValue);
               if (m_nClientKey <= 0x7fffffff) m_nClientKey = ~m_nClientKey + 1;
            }

            return m_nClientKey;
         }
      }

      /// <summary>
      /// Property to set the session key
      /// </summary>
      private UInt32 SessionKey
      {
         set
         {
            m_nClientKey = value; // TEMP

            // If the key is valid
            if ((value == m_nClientKey) || (value == ((~m_nClientKey) + 1)))
            {
               // Set the session key
               m_nSessionKey = value;

               MemoryStream keyStream = new MemoryStream (520);

               UInt64 keySeed = m_nSessionKey;

               // Fill the encryption buffer
               for (int nIndex = 0; nIndex < 260; nIndex++)
               {
                  UInt64 keyLast = keySeed;

                  keySeed = (((keyLast * 0x834E0B5F) >> 48) & 0xFFFFFFFF);
                  keySeed = ((keySeed + (keySeed >> 31)) & 0xFFFFFFFF);
                  keySeed = ((((keyLast % 0x1F31D) * 16807) - (keySeed * 2836) + 123) & 0xFFFFFFFF);
                  if (keySeed > 0x7FFFFFFF) { keySeed = ((keySeed + 0x7FFFFFFF) & 0xFFFFFFFF); }

                  // Add the next key to the key buffer
                  keyStream.Write (BitConverter.GetBytes (keySeed), 0, 2);
               }

               // Get the key buffer
               m_keyBuffer = keyStream.GetBuffer ();
            }
            else
            {
               // Invalid key provided:  Keystream not initialized!
               m_nSessionKey = 0;
            }
         }
      }

      /// <summary>
      /// Encrypts a packet
      /// </summary>
      /// <param name="packet">packet buffer to encrypt</param>
      /// <returns>Encrypted packet buffer</returns>
      public Byte[] Encrypt (Byte[] packet)
      {
         // Create the memory stream to hold the encrypted data
         MemoryStream pBuffer = new MemoryStream ();

         // Get the length of the buffer
         int nLength = packet.Length + (4 - (packet.Length % 4));

         // Only encrypt if the keystream
         // has been initialized
         if (m_nSessionKey != 0)
         {
            UInt32 dwKey = m_nSessionKey;

            // Get the index to start at
            int nStartIndex = (packet[0] != 0x00) ? 1 : 2;

            // Create an array to hold the packet data
            Byte[] packetData = new Byte[nLength + nStartIndex];
            Array.Copy (packet, packetData, packet.Length);

            // Copy the packet header to the memory stream
            pBuffer.Write (packet, 0, nStartIndex);

            // Encryption loop
            for (int nIndex = nStartIndex; nIndex < nLength; nIndex += 4)
            {
               // Get the next encryption key
               dwKey ^= BitConverter.ToUInt32 (m_keyBuffer, nIndex - nStartIndex);

               // Encrypt the data with the encryption key
               dwKey ^= BitConverter.ToUInt32 (packetData, nIndex);

               // Put the encrypted data back into the buffer
               pBuffer.Write (BitConverter.GetBytes (dwKey), 0, 4);
            }

            // Reset the buffer position
            pBuffer.Position = 0;

            // Read the packet back from the memory stream
            pBuffer.Read (packet, 0, packet.Length);
         }
         
         // Return the encrypted packet 
         return packet;
      }

      /// <summary>
      /// Decrypts a packet
      /// </summary>
      /// <param name="packet">packet buffer to decrypt</param>
      /// <returns>Decrypted packet buffer </returns>
      public Byte[] Decrypt (Byte[] packet)
      {
         // Create the memory stream to hold the decrypted data
         MemoryStream pBuffer = new MemoryStream ();

         UInt32 nDecryptKey      = m_nSessionKey;
         UInt32 nDecryptedBytes  = 0;

         // Get the length of the buffer
         int nLength = packet.Length + (4 - (packet.Length % 4));

         if (m_nSessionKey != 0)
         {
            // Get the index to start at
            int nStartIndex = (packet[0] != 0x00) ? 1 : 2;

            // Create an array to hold the packet data
            Byte[] packetData = new Byte[nLength + nStartIndex];
            Array.Copy (packet, packetData, packet.Length);

            // Write the header bytes to the buffer
            pBuffer.Write (packet, 0, nStartIndex);

            // Decryption loop
            for (int nIndex = nStartIndex; nIndex < nLength; nIndex += 4)
            {
               // Get the next decryption key
               nDecryptKey ^= BitConverter.ToUInt32 (m_keyBuffer, nIndex - nStartIndex);

               // Decrypt the next data cluster
               nDecryptedBytes = BitConverter.ToUInt32 (packetData, nIndex) ^ nDecryptKey;

               // Get the next decryption key
               nDecryptKey = BitConverter.ToUInt32 (packetData, nIndex);

               // Add the decrypted data back into the buffer
               pBuffer.Write(BitConverter.GetBytes(nDecryptedBytes), 0, 4);
            }

            // Reset the buffer position
            pBuffer.Position = 0;

            // Read the packet back from the memory stream
            pBuffer.Read (packet, 0, packet.Length);
         }
         else if (BitConverter.ToUInt16 (packet, 0) == 0x0200)
         {
            // Initialize the encryption with the session key
            SessionKey = BitConverter.ToUInt32 (packet, 2);
         }

         // Return the decrypted packet
         return packet;
      }
   }
}
