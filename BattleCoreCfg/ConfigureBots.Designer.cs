namespace BattleCoreCfg
{
   partial class ConfigureBots
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose (bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose ();
         }
         base.Dispose (disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent ()
      {
         System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem ("Bot1");
         this.botView = new System.Windows.Forms.ListView ();
         this.botName = new System.Windows.Forms.ColumnHeader ();
         this.serverAddress = new System.Windows.Forms.ColumnHeader ();
         this.serverPort = new System.Windows.Forms.ColumnHeader ();
         this.startArena = new System.Windows.Forms.ColumnHeader ();
         this.loadBot = new System.Windows.Forms.ColumnHeader ();
         this.btnClose = new System.Windows.Forms.Button ();
         this.btnSave = new System.Windows.Forms.Button ();
         this.btnAdd = new System.Windows.Forms.Button ();
         this.btnRemove = new System.Windows.Forms.Button ();
         this.SuspendLayout ();
         // 
         // botView
         // 
         this.botView.Activation = System.Windows.Forms.ItemActivation.OneClick;
         this.botView.Columns.AddRange (new System.Windows.Forms.ColumnHeader[] {
            this.botName,
            this.serverAddress,
            this.serverPort,
            this.startArena,
            this.loadBot});
         this.botView.FullRowSelect = true;
         this.botView.GridLines = true;
         this.botView.Items.AddRange (new System.Windows.Forms.ListViewItem[] {
            listViewItem3});
         this.botView.Location = new System.Drawing.Point (12, 12);
         this.botView.Name = "botView";
         this.botView.Size = new System.Drawing.Size (453, 193);
         this.botView.TabIndex = 0;
         this.botView.UseCompatibleStateImageBehavior = false;
         this.botView.View = System.Windows.Forms.View.Details;
         this.botView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler (this.botView_MouseDoubleClick);
         this.botView.KeyDown += new System.Windows.Forms.KeyEventHandler (this.botView_KeyDown);
         this.botView.KeyPress += new System.Windows.Forms.KeyPressEventHandler (this.botView_KeyPress);
         // 
         // botName
         // 
         this.botName.Text = "Name";
         this.botName.Width = 120;
         // 
         // serverAddress
         // 
         this.serverAddress.Text = "Server Address";
         this.serverAddress.Width = 110;
         // 
         // serverPort
         // 
         this.serverPort.Text = "Server Port";
         this.serverPort.Width = 77;
         // 
         // startArena
         // 
         this.startArena.Text = "Initial Arena";
         this.startArena.Width = 89;
         // 
         // loadBot
         // 
         this.loadBot.Text = "Active";
         this.loadBot.Width = 53;
         // 
         // btnClose
         // 
         this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.btnClose.Location = new System.Drawing.Point (391, 221);
         this.btnClose.Name = "btnClose";
         this.btnClose.Size = new System.Drawing.Size (74, 23);
         this.btnClose.TabIndex = 1;
         this.btnClose.Text = "Close";
         this.btnClose.UseVisualStyleBackColor = true;
         this.btnClose.Click += new System.EventHandler (this.btnClose_Click);
         // 
         // btnSave
         // 
         this.btnSave.Location = new System.Drawing.Point (310, 221);
         this.btnSave.Name = "btnSave";
         this.btnSave.Size = new System.Drawing.Size (74, 23);
         this.btnSave.TabIndex = 2;
         this.btnSave.Text = "Save";
         this.btnSave.UseVisualStyleBackColor = true;
         this.btnSave.Click += new System.EventHandler (this.btnSave_Click);
         // 
         // btnAdd
         // 
         this.btnAdd.Location = new System.Drawing.Point (146, 221);
         this.btnAdd.Name = "btnAdd";
         this.btnAdd.Size = new System.Drawing.Size (75, 23);
         this.btnAdd.TabIndex = 3;
         this.btnAdd.Text = "Add";
         this.btnAdd.UseVisualStyleBackColor = true;
         this.btnAdd.Click += new System.EventHandler (this.btnAdd_Click);
         // 
         // btnRemove
         // 
         this.btnRemove.Location = new System.Drawing.Point (229, 221);
         this.btnRemove.Name = "btnRemove";
         this.btnRemove.Size = new System.Drawing.Size (75, 23);
         this.btnRemove.TabIndex = 4;
         this.btnRemove.Text = "Remove";
         this.btnRemove.UseVisualStyleBackColor = true;
         this.btnRemove.Click += new System.EventHandler (this.btnRemove_Click);
         // 
         // ConfigureBots
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnClose;
         this.ClientSize = new System.Drawing.Size (477, 259);
         this.Controls.Add (this.btnRemove);
         this.Controls.Add (this.btnAdd);
         this.Controls.Add (this.btnSave);
         this.Controls.Add (this.btnClose);
         this.Controls.Add (this.botView);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "ConfigureBots";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "BattleCore Bot Configuration";
         this.KeyPress += new System.Windows.Forms.KeyPressEventHandler (this.botView_KeyPress);
         this.Load += new System.EventHandler (this.ConfigureBots_Load);
         this.ResumeLayout (false);

      }

      #endregion

      private System.Windows.Forms.ListView botView;
      private System.Windows.Forms.ColumnHeader botName;
      private System.Windows.Forms.Button btnClose;
      private System.Windows.Forms.Button btnSave;
      private System.Windows.Forms.ColumnHeader serverAddress;
      private System.Windows.Forms.ColumnHeader serverPort;
      private System.Windows.Forms.ColumnHeader startArena;
      private System.Windows.Forms.ColumnHeader loadBot;
      private System.Windows.Forms.Button btnAdd;
      private System.Windows.Forms.Button btnRemove;
   }
}

