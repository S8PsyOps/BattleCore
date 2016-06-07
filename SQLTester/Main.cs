using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleCore;
using BattleCore.Events;
using BattleCorePsyOps;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace SQLTester
{
    // Add the attribute and the base class
    [Behavior("SQLTest", "true", "0.01", "PsyOps", "Testing SQL tasks.")]
    public class Main: BotEventListener
    {
        public Main()
        {
            // Store needed info
            server = "107.180.55.20";
            database = "";
            uid = "DevaBot";
            password = "SSCJDevastation8675309";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);

            RegisterCommand("!sqlconnect", doConnect);
            RegisterCommand("!sqldisconnect", doDisconnect);
            RegisterCommand("!sqlcheck", DBConnectionStatus);
        }

        ShortChat msg = new ShortChat();
        private MySqlConnection connection;
        // connections strings
        private string server, database, uid, password;

        public void doConnect(ChatEvent e)
        {
            if (e.ModLevel < ModLevels.Sysop) return;
            try
            {
                connection.Open();
                Game(msg.arena( "Connected Successfully."));
                return;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Game(msg.arena( "Cannot connect to server.  Contact administrator"));
                        return;
                    case 1045:
                        Game(msg.arena("Invalid username/password, please try again"));
                        return;
                    default:
                        Game(msg.arena("Error: Num[" + ex.Number + "]"));
                        return;
                }
            }
        }

        public void doDisconnect(ChatEvent e)
        {
            if (e.ModLevel < ModLevels.Sysop) return;
            try
            {
                connection.Close();
                //q.Enqueue(msg.chan(1, "Connection Closed."));
                Game(msg.arena("Connection closed."));
            }
            catch (MySqlException ex)
            {
                Game(msg.arena("Error closing connection: " + ex.Message));
            }
        }

        public void DBConnectionStatus(ChatEvent e)
        {
            if (e.ModLevel < ModLevels.Sysop) return;
            if (connection.State == System.Data.ConnectionState.Open)
            {
                Game(msg.arena("Connection is open."));
                return;
            }

            Game(msg.arena("Connection is not open at this time."));
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
