/***********************************************************************
*
* NAME:        SqlQueryEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Sql Query Event implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE      AUTHOR     CHANGES
* --------  ---------  -------------------------------------------------
* 07-13-07  udp        Initial Creation
*
************************************************************************/

// Namespace usage
using System;
using System.Data;
using System.Data.SqlClient;

// Namespace 
namespace BattleCore.Events
{
   /// <summary>
   /// SqlQueryEvent object.  This event is used to execute a query on 
   /// the configured database.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle the query data.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnSqlEvent (object sender, SqlQueryEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <para>
   /// To execute a query on the configured database, add the
   /// SQL select command to the command field before sending the event</para>
   /// <code lang="C#" escaped="true">
   /// SqlQueryEvent e = new SqlQueryEvent ();
   /// e.Command.CommandText ("SELECT * from player_table");
   /// SendGameEvent (e);
   /// </code>
   /// </remarks>
   public class SqlQueryEvent : EventArgs
   {
      // Sql query command
      private SqlCommand m_sqlCommand = new SqlCommand ();
      private DataTable  m_queryData = new DataTable ();
      private int m_recordsAffected = 0;
      private int m_transactionId = 0;

      /// <summary>
      /// Set/Get the query transaction identifer 
      /// </summary>
      public int TransactionId
      {
         get { return m_transactionId; }
         set { m_transactionId = value; }
      }

      /// <summary>
      /// SQL command property used to set the command.  
      /// </summary>
      public SqlCommand Command
      {
         set { m_sqlCommand = value; }
         get { return m_sqlCommand; }
      }

      /// <summary>
      /// Returns a data table generated from a SELECT query
      /// </summary>
      public DataTable QueryData
      {
         get { return m_queryData; }
         set { m_queryData = value; }
      }

      /// <summary>
      /// Returns the number of records affected from the command
      /// </summary>
      public int RecordsAffected
      {
         get { return m_recordsAffected; }
         set { m_recordsAffected = value; }
      }
   }
}
