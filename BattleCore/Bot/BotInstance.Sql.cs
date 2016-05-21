//-----------------------------------------------------------------------
//
// NAME:        BotInstance.Sql.cs
//
// PROJECT:     Battle Core Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: SQL Handler implementation.
//
// NOTES:       Thanks to Arry for the startup code.
//
// $History: BotInstance.Sql.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections;
using BattleCore.Events;

// Namespace declaration
namespace BattleCore.Bot
{
   // Partial class definition for BotInstance
   partial class BotInstance
   {
      /// <summary>
      /// Execute a SQL query on the configured database
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="sqlEvent"></param>
      private void ExecuteSqlQuery (object sender, SqlQueryEvent sqlEvent)
      {
         // SQL connection string set from the configuration
         SqlConnection connection = new SqlConnection(m_botSettings.SqlConnectString);

         try
         {
            // Create the response to send back to the requester
            ResponseEvent response = new ResponseEvent();

            // Set the command connection
            sqlEvent.Command.Connection = connection;

            // open the connection
            connection.Open();

            // Execute the select command
            SqlDataReader dataReader = sqlEvent.Command.ExecuteReader(CommandBehavior.CloseConnection);

            // Set the number of records affected
            sqlEvent.RecordsAffected = dataReader.RecordsAffected;

            // Set the sql event data
            sqlEvent.QueryData.Load(dataReader);

            // Close the data reader
            dataReader.Close();

            // Set the response data
            response.e = sqlEvent;
            response.destination = sender;
            m_eventQueue.Add(response);
         }
         catch (SqlException ex)
         {
            // Send an arena message with the error
            ChatEvent errorMsg = new ChatEvent();
            errorMsg.ChatType = ChatTypes.Arena;
            errorMsg.SoundCode = SoundCodes.Fart;
            errorMsg.Message = ex.Message;

            // Set the response data
            SendGameEvent (errorMsg);
         }
      }

      /// <summary>
      /// Execute a SQL command on the configured database
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="sqlEvent"></param>
      private void ExecuteSqlCommand (object sender, SqlCommandEvent sqlEvent)
      {
         // SQL connection string set from the configuration
         using (SqlConnection connection = new SqlConnection(m_botSettings.SqlConnectString))
         {
            try
            {
               // Create the response to send back to the requester
               ResponseEvent response = new ResponseEvent();

               // Set the command connection
               sqlEvent.Command.Connection = connection;

               // open the connection
               connection.Open();

               // Set the number of records affected
               sqlEvent.RecordsAffected = sqlEvent.Command.ExecuteNonQuery();

               // Close the connection
               connection.Close ();

               // Set the response data
               response.e = sqlEvent;
               response.destination = sender;
               m_eventQueue.Add(response);
            }
            catch (Exception ex)
            {
               // Send an arena message with the error
               ChatEvent errorMsg = new ChatEvent();
               errorMsg.ChatType = ChatTypes.Private;
               errorMsg.PlayerName = "PsyOps";
               errorMsg.SoundCode = SoundCodes.Fart;
               errorMsg.Message = ex.Message;

               // Set the response data
               m_eventQueue.Add(errorMsg);
            }
         }
      }
   }
}
