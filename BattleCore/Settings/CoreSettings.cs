//-----------------------------------------------------------------------
//
// NAME:        CoreSettings.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Core Settings implementation.
//
// NOTES:       None.
//
// $History: CoreSettings.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Security.Cryptography;

// Namespace declaration
namespace BattleCore.Settings
{
   /// <summary>
   /// Core settings object.  This object contains the configuration
   /// for all enablaed bots.
   /// </summary>
   public class CoreSettings
   {
      String m_configFile;

      List<BotSettings> m_botSettings = new List<BotSettings> ();

      /// <summary>
      /// Core settings constructor
      /// </summary>
      public CoreSettings (string strPassword)
      {
         string assemblyLoc = Assembly.GetExecutingAssembly ().Location;
         string currentDirectory = assemblyLoc.Substring (0, assemblyLoc.LastIndexOf (Path.DirectorySeparatorChar) + 1);

         m_configFile = Path.Combine (currentDirectory, "BattleCore.dat");

         // Read the configuration
         Read (strPassword);
      }
      /// <summary>
      /// Gets the list of bot configurations
      /// </summary>
      public List<BotSettings> ConfiguredBots
      {
         get { return m_botSettings; }
      }

      /// <summary>
      /// Read the settings from the configuration file
      /// </summary>
      /// <param name="strPassword"></param>
      private void Read (string strPassword)
      {
         if (strPassword.Length > 8)
         {
            strPassword = strPassword.Remove (8);
         }
         else if (strPassword.Length < 8)
         {
            strPassword = strPassword.PadRight (8, '+');
         }

         try
         {
            // Check if the configuration file exists
            if (File.Exists (m_configFile))
            {
               // Open the configuration file 
               FileStream configStream = new FileStream (m_configFile, FileMode.Open);

               // Create the service provider
               DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider ();

               // Create the encryption keys
               cryptic.Key = ASCIIEncoding.ASCII.GetBytes (strPassword);
               cryptic.IV = ASCIIEncoding.ASCII.GetBytes (strPassword);

               // Create the Decryption stream provider
               CryptoStream crStream = new CryptoStream (configStream,
                                                         cryptic.CreateDecryptor (),
                                                         CryptoStreamMode.Read);

               // Create the binary formatter for the settings
               BinaryFormatter formatter = new BinaryFormatter ();

               // Serialize the decrypted stream into the core settings
               m_botSettings = (List<BotSettings>)formatter.Deserialize (crStream);

               // Close the file stream
               crStream.Close ();
            }
         }
         catch (Exception e)
         {
            Console.WriteLine (e.Message);
         }
      }

      /// <summary>
      /// Write the Settings to the settings file
      /// </summary>
      /// <param name="strPassword">configuration file password</param>
      public void WriteSettings (string strPassword)
      {
         if (strPassword.Length > 8)
         {
            strPassword = strPassword.Remove (8);
         }
         else if (strPassword.Length < 8)
         {
            strPassword = strPassword.PadRight (8, '+');
         }

         // Create the file stream
         FileStream configStream = new FileStream (m_configFile, FileMode.Create);

         // Create the service provider
         DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider ();

         // Create the encryption keys
         cryptic.Key = ASCIIEncoding.ASCII.GetBytes (strPassword);
         cryptic.IV = ASCIIEncoding.ASCII.GetBytes (strPassword);

         // Create the Decryption stream provider
         CryptoStream crStream = new CryptoStream (configStream,
                                                   cryptic.CreateEncryptor (),
                                                   CryptoStreamMode.Write);

         // Create the binary formatter for the settings
         BinaryFormatter formatter = new BinaryFormatter ();

         // Serialize the settings to the crypto stream
         formatter.Serialize (crStream, m_botSettings);

         // Close the file stream
         crStream.Close ();

         // Create the password file
         WritePasswordFile (strPassword);
      }

      /// <summary>
      /// Write the password to the security file
      /// </summary>
      /// <param name="strPassword"></param>
      void WritePasswordFile (string strPassword)
      {
         string assemblyLoc = Assembly.GetExecutingAssembly ().Location;
         string currentDirectory = assemblyLoc.Substring (0, assemblyLoc.LastIndexOf (Path.DirectorySeparatorChar) + 1);

         string securityFile = Path.Combine (currentDirectory, "security.dat");

         // Create the file stream
         FileStream securityStream = new FileStream (securityFile, FileMode.Create);

         // Create the service provider
         DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider ();

         // Create the encryption keys
         cryptic.Key = ASCIIEncoding.ASCII.GetBytes ("ALTT1029");
         cryptic.IV = ASCIIEncoding.ASCII.GetBytes ("ALTT1029");

         // Create the Decryption stream provider
         CryptoStream crStream = new CryptoStream (securityStream,
                                                   cryptic.CreateEncryptor (),
                                                   CryptoStreamMode.Write);

         // Create the binary formatter for the settings
         BinaryFormatter formatter = new BinaryFormatter ();

         // Serialize the settings to the crypto stream
         formatter.Serialize (crStream, strPassword);

         // Close the file stream
         crStream.Close ();
      }
   }
}
