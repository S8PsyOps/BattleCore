using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BattleCore;
using BattleCore.Settings;

namespace BattleCoreCfg
{
   public partial class ConfigureBots : Form
   {
      private CoreSettings m_coreSettings;

      public ConfigureBots ()
      {
         InitializeComponent ();
      }

      private void UpdateListView ()
      {
         // Clear the list view
         botView.Items.Clear ();

         if (m_coreSettings.ConfiguredBots.Count != 0)
         {
            foreach (BattleCore.Settings.BotSettings bot in m_coreSettings.ConfiguredBots)
            {
               ListViewItem item = botView.Items.Add (bot.SessionSettings.UserName);
               item.SubItems.Add (bot.SessionSettings.ServerAddress);
               item.SubItems.Add (bot.SessionSettings.ServerPort.ToString());
               item.SubItems.Add (bot.SessionSettings.InitialArena);
               item.SubItems.Add (bot.AutoLoad.ToString());
            }

            // Refresh the bot view
            botView.Refresh ();
         }
      }

      private void botView_KeyPress (object sender, KeyPressEventArgs e)
      {
         if (e.KeyChar == (Char)Keys.Delete)
         {
         }
      }

      private void botView_MouseDoubleClick (object sender, MouseEventArgs e)
      {
         BotSettings settings = new BotSettings ();

         if (settings.ShowDialog () == DialogResult.OK)
         {
            // Add the new bot to the configuration
            BattleCore.Settings.BotSettings bot = new BattleCore.Settings.BotSettings ();
            bot.BotDirectory = settings.txtUserName.Text;
            bot.AutoLoad = settings.chkLoad.Checked;
            bot.SqlConnectString = settings.txtSqlConnection.Text;
            bot.SessionSettings.UserName = settings.txtUserName.Text;
            bot.SessionSettings.Password = settings.txtPassword.Text;
            bot.SessionSettings.StaffPassword = settings.txtStaffPassword.Text;
            bot.SessionSettings.ServerAddress = settings.txtServerAddress.Text;
            bot.SessionSettings.ServerPort = Convert.ToUInt16 (settings.txtServerPort.Text);
            bot.SessionSettings.InitialArena = settings.txtInitialArena.Text;

            m_coreSettings.ConfiguredBots.Add (bot);

            // Refresh the list control
            UpdateListView ();
         }
      }

      private void ConfigureBots_Load (object sender, EventArgs e)
      {
         // Create the password dialog
         ConfigPassword passwordDlg = new ConfigPassword ();

         passwordDlg.ShowDialog ();

         m_coreSettings = new CoreSettings (passwordDlg.txtPassword.Text);

         if (m_coreSettings.ConfiguredBots.Count == 0)
         {
            BotSettings settings = new BotSettings ();

            // Force the initial bot to autoload
            settings.chkLoad.Checked = true;
            settings.chkLoad.Enabled = false;

            // Display the bot settings dilaog
            if (settings.ShowDialog () == DialogResult.OK)
            {
               // Add the new bot to the configuration
               BattleCore.Settings.BotSettings bot = new BattleCore.Settings.BotSettings ();
               bot.BotDirectory = settings.txtUserName.Text;
               bot.AutoLoad = settings.chkLoad.Checked;
               bot.SqlConnectString = settings.txtSqlConnection.Text;
               bot.SessionSettings.UserName = settings.txtUserName.Text;
               bot.SessionSettings.Password = settings.txtPassword.Text;
               bot.SessionSettings.StaffPassword = settings.txtStaffPassword.Text;
               bot.SessionSettings.ServerAddress = settings.txtServerAddress.Text;
               bot.SessionSettings.ServerPort = Convert.ToUInt16 (settings.txtServerPort.Text);
               bot.SessionSettings.InitialArena = settings.txtInitialArena.Text;

               m_coreSettings.ConfiguredBots.Add (bot);

               // Refresh the list control
               UpdateListView ();
            }
            else
            {
               Application.Exit ();
            }
         }
         else
         {
            // Update the list view
            UpdateListView ();
         }
      }

      private void btnSave_Click (object sender, EventArgs e)
      {
         // Create the password dialog
         ConfigPassword passwordDlg = new ConfigPassword ();

         // Display the password dialog
         if (passwordDlg.ShowDialog () == DialogResult.OK)
         {
            // Write the settings to the file
            m_coreSettings.WriteSettings (passwordDlg.txtPassword.Text);
         }
      }

      private void btnClose_Click (object sender, EventArgs e)
      {
         // Exit the application
         Application.Exit ();
      }

      private void botView_KeyDown (object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Delete)
         {
            if (botView.SelectedItems.Count > 0)
            {
               if (MessageBox.Show ("Delete Selected Bots?",
                  "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
               {
                  // Remove all selected items from the view
                  foreach (ListViewItem item in botView.SelectedItems)
                  {
                     botView.Items.Remove (item);
                  }
               }
            }
         }
      }

      private void btnAdd_Click (object sender, EventArgs e)
      {
         BotSettings settings = new BotSettings ();

         if (settings.ShowDialog () == DialogResult.OK)
         {
            // Add the new bot to the configuration
            BattleCore.Settings.BotSettings bot = new BattleCore.Settings.BotSettings ();
            bot.BotDirectory = settings.txtUserName.Text;
            bot.AutoLoad = settings.chkLoad.Checked;
            bot.SqlConnectString = settings.txtSqlConnection.Text;
            bot.SessionSettings.UserName = settings.txtUserName.Text;
            bot.SessionSettings.Password = settings.txtPassword.Text;
            bot.SessionSettings.StaffPassword = settings.txtStaffPassword.Text;
            bot.SessionSettings.ServerAddress = settings.txtServerAddress.Text;
            bot.SessionSettings.ServerPort = Convert.ToUInt16 (settings.txtServerPort.Text);
            bot.SessionSettings.InitialArena = settings.txtInitialArena.Text;

            m_coreSettings.ConfiguredBots.Add (bot);

            // Refresh the list control
            UpdateListView ();
         }
      }

      private void btnRemove_Click (object sender, EventArgs e)
      {
         MessageBox.Show ("Not supported yet.");
      }
   }
}