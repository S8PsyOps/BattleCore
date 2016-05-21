/***********************************************************************
*
* NAME:        SqlCommandEvent.cs
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Sql Command Event implementation.
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
   /// SqlQueryEvent object.  This event is used to execute a command on 
   /// the configured database.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle the command data.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnSqlEvent (object sender, SqlCommandEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <para>
   /// To execute a command on the configured database, add the
   /// SQL command to the command field before sending the event</para>
   /// <code lang="C#" escaped="true">
   /// SqlCommandEvent e = new SqlCommandEvent ();
   /// e.Command.TransactionId = 1234;
   /// e.Command.CommandText ("UPDATE blah blah");
   /// SendGameEvent (e);
   /// </code>
   /// </remarks>
   public class SqlCommandEvent : EventArgs
   {
      // Sql query command
      private SqlCommand m_sqlCommand = new SqlCommand();
      private int m_recordsAffected = 0;
      private int m_transactionId = 0;

      /// <summary>
      /// Set/Get the command transaction identifer 
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
      /// Returns the number of rows affected from the command
      /// </summary>
      public int RecordsAffected
      {
         get { return m_recordsAffected; }
         set { m_recordsAffected = value; }
      }
   }
}
