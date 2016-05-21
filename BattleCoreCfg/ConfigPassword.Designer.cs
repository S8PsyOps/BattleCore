namespace BattleCoreCfg
{
   partial class ConfigPassword
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
         this.txtPassword = new System.Windows.Forms.TextBox ();
         this.btnOK = new System.Windows.Forms.Button ();
         this.label1 = new System.Windows.Forms.Label ();
         this.SuspendLayout ();
         // 
         // txtPassword
         // 
         this.txtPassword.Location = new System.Drawing.Point (102, 24);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.Size = new System.Drawing.Size (157, 20);
         this.txtPassword.TabIndex = 0;
         // 
         // btnOK
         // 
         this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.btnOK.Location = new System.Drawing.Point (184, 64);
         this.btnOK.Name = "btnOK";
         this.btnOK.Size = new System.Drawing.Size (75, 23);
         this.btnOK.TabIndex = 1;
         this.btnOK.Text = "Submit";
         this.btnOK.UseVisualStyleBackColor = true;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point (12, 27);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size (84, 13);
         this.label1.TabIndex = 2;
         this.label1.Text = "Enter Password:";
         // 
         // ConfigPassword
         // 
         this.AcceptButton = this.btnOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnOK;
         this.ClientSize = new System.Drawing.Size (275, 99);
         this.ControlBox = false;
         this.Controls.Add (this.label1);
         this.Controls.Add (this.btnOK);
         this.Controls.Add (this.txtPassword);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "ConfigPassword";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "Configuration Password";
         this.TopMost = true;
         this.ResumeLayout (false);
         this.PerformLayout ();

      }

      #endregion

      private System.Windows.Forms.Button btnOK;
      private System.Windows.Forms.Label label1;
      public System.Windows.Forms.TextBox txtPassword;
   }
}