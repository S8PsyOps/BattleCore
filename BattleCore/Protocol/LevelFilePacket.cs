//-----------------------------------------------------------------------
//
// NAME:        LevelFilePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Level File Packet implementation.
//
// NOTES:       None.
//
// $History: LevelFilePacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// LevelFilePacket object.  This object is used to handle a level
   /// file packet.
   /// </summary>
   internal class LevelFilePacket : IPacket
   {
      /// <summary>
      /// Level file name
      /// </summary>
      String m_strFileName;

      /// <summary>
      /// Level File data
      /// </summary>
      private MemoryStream m_fileData = new MemoryStream ();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// File name property
      /// </summary>
      public String FileName
      {
         get { return m_strFileName; }
      }

      /// <summary>
      /// File Data Property
      /// </summary>
      public Byte[] FileData
      {
         get { return m_fileData.ToArray (); }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Extract the file name
            m_strFileName = (new ASCIIEncoding ()).GetString (value, 1, 16);
            m_strFileName = m_strFileName.Substring (0, m_strFileName.LastIndexOf ('.') + 3);

            // Create a memory stream to hold the compressed file
            MemoryStream memStream = new MemoryStream ();
            memStream.Write (value, 17, value.Length - 17);

            // Reset the stream position
            memStream.Position = 0;

            try
            {
               // Create a zip stream to decompress the file data
               GZipStream zipStream = new GZipStream (memStream, CompressionMode.Decompress);

               // Retrieve the size of the file from the compressed archive's footer
               byte[] bufferWrite = new byte[4];

               memStream.Position = (int)memStream.Length - 4;

               // Write the first 4 bytes of data from the compressed file into the buffer

               memStream.Read (bufferWrite, 0, 4);

               // Set the position back at the start
               memStream.Position = 0;

               int bufferLength = 500;// BitConverter.ToInt32 (bufferWrite, 0);

               byte[] buffer = new byte[bufferLength + 100];

               int readOffset = 0;
               int totalBytes = 0;

               // Loop through the compressed stream and put it into the buffer
               while (true)
               {
                  int bytesRead = zipStream.Read (buffer, readOffset, 100);

                  // If we reached the end of the data
                  if (bytesRead == 0)
                     break;

                  readOffset += bytesRead;
                  totalBytes += bytesRead;
               }

               m_fileData = new MemoryStream ();
               m_fileData.Write (buffer, 0, totalBytes);


               // Close the streams
               memStream.Close ();
               zipStream.Close ();

            }
            catch (Exception e)
            {
               // Write the exception to the console
               Console.WriteLine (e.Message);
            }
         }

         get
         {
            // Return the packet data
            return null;
         }
      }
   }
}
