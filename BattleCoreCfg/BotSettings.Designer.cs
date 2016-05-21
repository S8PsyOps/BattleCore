namespace BattleCoreCfg
{
   partial class BotSettings
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
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.label5 = new System.Windows.Forms.Label();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.chkLoad = new System.Windows.Forms.CheckBox();
         this.txtStaffPassword = new System.Windows.Forms.TextBox();
         this.txtPassword = new System.Windows.Forms.TextBox();
         this.txtUserName = new System.Windows.Forms.TextBox();
         this.btnCancel = new System.Windows.Forms.Button();
         this.btnOK = new System.Windows.Forms.Button();
         this.txtServerAddress = new System.Windows.Forms.TextBox();
         this.txtServerPort = new System.Windows.Forms.TextBox();
         this.txtInitialArena = new System.Windows.Forms.TextBox();
         this.label6 = new System.Windows.Forms.Label();
         this.label7 = new System.Windows.Forms.Label();
         this.txtSqlConnection = new System.Windows.Forms.TextBox();
         this.groupBox1.SuspendLayout();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(6, 25);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(60, 13);
         this.label1.TabIndex = 0;
         this.label1.Text = "User Name";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(6, 51);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(53, 13);
         this.label2.TabIndex = 1;
         this.label2.Text = "Password";
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(6, 77);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(78, 13);
         this.label3.TabIndex = 2;
         this.label3.Text = "Staff Password";
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(18, 152);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(110, 13);
         this.label4.TabIndex = 3;
         this.label4.Text = "Game Server Address";
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(189, 152);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(26, 13);
         this.label5.TabIndex = 4;
         this.label5.Text = "Port";
         // 
         // groupBox1
         // 
         this.groupBox1.Controls.Add(this.chkLoad);
         this.groupBox1.Controls.Add(this.txtStaffPassword);
         this.groupBox1.Controls.Add(this.txtPassword);
         this.groupBox1.Controls.Add(this.txtUserName);
         this.groupBox1.Controls.Add(this.label1);
         this.groupBox1.Controls.Add(this.label2);
         this.groupBox1.Controls.Add(this.label3);
         this.groupBox1.Location = new System.Drawing.Point(12, 12);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(272, 127);
         this.groupBox1.TabIndex = 5;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "Login";
         // 
         // chkLoad
         // 
         this.chkLoad.AutoSize = true;
         this.chkLoad.Location = new System.Drawing.Point(164, 99);
         this.chkLoad.Name = "chkLoad";
         this.chkLoad.Size = new System.Drawing.Size(99, 17);
         this.chkLoad.TabIndex = 6;
         this.chkLoad.Text = "Load at Startup";
         this.chkLoad.UseVisualStyleBackColor = true;
         // 
         // txtStaffPassword
         // 
         this.txtStaffPassword.Location = new System.Drawing.Point(90, 70);
         this.txtStaffPassword.Name = "txtStaffPassword";
         this.txtStaffPassword.PasswordChar = '*';
         this.txtStaffPassword.Size = new System.Drawing.Size(173, 20);
         this.txtStaffPassword.TabIndex = 5;
         // 
         // txtPassword
         // 
         this.txtPassword.Location = new System.Drawing.Point(90, 44);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '*';
         this.txtPassword.Size = new System.Drawing.Size(173, 20);
         this.txtPassword.TabIndex = 4;
         // 
         // txtUserName
         // 
         this.txtUserName.Location = new System.Drawing.Point(90, 18);
         this.txtUserName.Name = "txtUserName";
         this.txtUserName.Size = new System.Drawing.Size(173, 20);
         this.txtUserName.TabIndex = 3;
         // 
         // btnCancel
         // 
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(209, 292);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(75, 23);
         this.btnCancel.TabIndex = 6;
         this.btnCancel.Text = "Cancel";
         this.btnCancel.UseVisualStyleBackColor = true;
         // 
         // btnOK
         // 
         this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.btnOK.Location = new System.Drawing.Point(128, 292);
         this.btnOK.Name = "btnOK";
         this.btnOK.Size = new System.Drawing.Size(75, 23);
         this.btnOK.TabIndex = 7;
         this.btnOK.Text = "OK";
         this.btnOK.UseVisualStyleBackColor = true;
         // 
         // txtServerAddress
         // 
         this.txtServerAddress.Location = new System.Drawing.Point(21, 168);
         this.txtServerAddress.Name = "txtServerAddress";
         this.txtServerAddress.Size = new System.Drawing.Size(165, 20);
         this.txtServerAddress.TabIndex = 8;
         // 
         // txtServerPort
         // 
         this.txtServerPort.Location = new System.Drawing.Point(192, 168);
         this.txtServerPort.Name = "txtServerPort";
         this.txtServerPort.Size = new System.Drawing.Size(83, 20);
         this.txtServerPort.TabIndex = 9;
         // 
         // txtInitialArena
         // 
         this.txtInitialArena.Location = new System.Drawing.Point(86, 204);
         this.txtInitialArena.Name = "txtInitialArena";
         this.txtInitialArena.Size = new System.Drawing.Size(189, 20);
         this.txtInitialArena.TabIndex = 11;
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(18, 211);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(62, 13);
         this.label6.TabIndex = 10;
         this.label6.Text = "Initial Arena";
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(18, 244);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(101, 13);
         this.label7.TabIndex = 8;
         this.label7.Text = "SQL Connect String";
         // 
         // txtSqlConnection
         // 
         this.txtSqlConnection.Location = new System.Drawing.Point(21, 266);
         this.txtSqlConnection.Name = "txtSqlConnection";
         this.txtSqlConnection.Size = new System.Drawing.Size(254, 20);
         this.txtSqlConnection.TabIndex = 12;
         // 
         // BotSettings
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(296, 328);
         this.Controls.Add(this.txtSqlConnection);
         this.Controls.Add(this.label7);
         this.Controls.Add(this.txtInitialArena);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.txtServerPort);
         this.Controls.Add(this.txtServerAddress);
         this.Controls.Add(this.btnOK);
         this.Controls.Add(this.btnCancel);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.label4);
         this.Name = "BotSettings";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "BotSettings";
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Button btnCancel;
      private System.Windows.Forms.Button btnOK;
      private System.Windows.Forms.Label label6;
      internal System.Windows.Forms.TextBox txtStaffPassword;
      internal System.Windows.Forms.TextBox txtPassword;
      internal System.Windows.Forms.TextBox txtUserName;
      internal System.Windows.Forms.TextBox txtServerAddress;
      internal System.Windows.Forms.TextBox txtServerPort;
      internal System.Windows.Forms.TextBox txtInitialArena;
      public System.Windows.Forms.CheckBox chkLoad;
      private System.Windows.Forms.Label label7;
      internal System.Windows.Forms.TextBox txtSqlConnection;
   }
}