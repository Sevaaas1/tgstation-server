namespace TGS.ControlPanel
{
	partial class Login
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
			this.CurrentRevisionTitle = new System.Windows.Forms.Label();
			this.UsernameTextBox = new System.Windows.Forms.TextBox();
			this.PasswordTextBox = new System.Windows.Forms.TextBox();
			this.RemoteLoginButton = new System.Windows.Forms.Button();
			this.AddressLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.PortSelector = new System.Windows.Forms.NumericUpDown();
			this.SavePasswordCheckBox = new System.Windows.Forms.CheckBox();
			this.IPComboBox = new System.Windows.Forms.ComboBox();
			this.DeleteLoginButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.PortSelector)).BeginInit();
			this.SuspendLayout();
			// 
			// CurrentRevisionTitle
			// 
			this.CurrentRevisionTitle.AutoSize = true;
			this.CurrentRevisionTitle.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.CurrentRevisionTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
			this.CurrentRevisionTitle.Location = new System.Drawing.Point(135, 80);
			this.CurrentRevisionTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.CurrentRevisionTitle.Name = "CurrentRevisionTitle";
			this.CurrentRevisionTitle.Size = new System.Drawing.Size(128, 18);
			this.CurrentRevisionTitle.TabIndex = 14;
			this.CurrentRevisionTitle.Text = "Remote Login:";
			this.CurrentRevisionTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// UsernameTextBox
			// 
			this.UsernameTextBox.Location = new System.Drawing.Point(150, 152);
			this.UsernameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.UsernameTextBox.Name = "UsernameTextBox";
			this.UsernameTextBox.Size = new System.Drawing.Size(271, 23);
			this.UsernameTextBox.TabIndex = 16;
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Location = new System.Drawing.Point(150, 189);
			this.PasswordTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = new System.Drawing.Size(271, 23);
			this.PasswordTextBox.TabIndex = 17;
			this.PasswordTextBox.UseSystemPasswordChar = true;
			// 
			// RemoteLoginButton
			// 
			this.RemoteLoginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoteLoginButton.Location = new System.Drawing.Point(119, 231);
			this.RemoteLoginButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.RemoteLoginButton.Name = "RemoteLoginButton";
			this.RemoteLoginButton.Size = new System.Drawing.Size(183, 29);
			this.RemoteLoginButton.TabIndex = 18;
			this.RemoteLoginButton.Text = "Connect to Remote Service";
			this.RemoteLoginButton.UseVisualStyleBackColor = true;
			this.RemoteLoginButton.Click += new System.EventHandler(this.RemoteLoginButton_Click);
			// 
			// AddressLabel
			// 
			this.AddressLabel.AutoSize = true;
			this.AddressLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.AddressLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
			this.AddressLabel.Location = new System.Drawing.Point(10, 114);
			this.AddressLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.AddressLabel.Name = "AddressLabel";
			this.AddressLabel.Size = new System.Drawing.Size(121, 14);
			this.AddressLabel.TabIndex = 19;
			this.AddressLabel.Text = "Scheme+Address:";
			this.AddressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
			this.label2.Location = new System.Drawing.Point(10, 152);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(97, 18);
			this.label2.TabIndex = 20;
			this.label2.Text = "Username:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
			this.label3.Location = new System.Drawing.Point(10, 189);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(92, 18);
			this.label3.TabIndex = 21;
			this.label3.Text = "Password:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
			this.label4.Location = new System.Drawing.Point(-22, 46);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(468, 18);
			this.label4.TabIndex = 22;
			this.label4.Text = "______________________________________________";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// PortSelector
			// 
			this.PortSelector.Location = new System.Drawing.Point(329, 114);
			this.PortSelector.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.PortSelector.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
			this.PortSelector.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.PortSelector.Name = "PortSelector";
			this.PortSelector.Size = new System.Drawing.Size(93, 23);
			this.PortSelector.TabIndex = 16;
			this.PortSelector.Value = new decimal(new int[] {
            38607,
            0,
            0,
            0});
			// 
			// SavePasswordCheckBox
			// 
			this.SavePasswordCheckBox.AutoSize = true;
			this.SavePasswordCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
			this.SavePasswordCheckBox.Location = new System.Drawing.Point(309, 237);
			this.SavePasswordCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.SavePasswordCheckBox.Name = "SavePasswordCheckBox";
			this.SavePasswordCheckBox.Size = new System.Drawing.Size(103, 19);
			this.SavePasswordCheckBox.TabIndex = 23;
			this.SavePasswordCheckBox.Text = "Save Password";
			this.SavePasswordCheckBox.UseVisualStyleBackColor = true;
			this.SavePasswordCheckBox.CheckedChanged += new System.EventHandler(this.SavePasswordCheckBox_CheckedChanged);
			// 
			// IPComboBox
			// 
			this.IPComboBox.FormattingEnabled = true;
			this.IPComboBox.Location = new System.Drawing.Point(152, 113);
			this.IPComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.IPComboBox.Name = "IPComboBox";
			this.IPComboBox.Size = new System.Drawing.Size(170, 23);
			this.IPComboBox.TabIndex = 24;
			// 
			// DeleteLoginButton
			// 
			this.DeleteLoginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteLoginButton.Location = new System.Drawing.Point(122, 113);
			this.DeleteLoginButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.DeleteLoginButton.Name = "DeleteLoginButton";
			this.DeleteLoginButton.Size = new System.Drawing.Size(22, 22);
			this.DeleteLoginButton.TabIndex = 25;
			this.DeleteLoginButton.Text = "x";
			this.DeleteLoginButton.UseVisualStyleBackColor = true;
			this.DeleteLoginButton.Click += new System.EventHandler(this.DeleteLoginButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.ForeColor = System.Drawing.SystemColors.Control;
			this.label1.Location = new System.Drawing.Point(13, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(409, 32);
			this.label1.TabIndex = 26;
			this.label1.Text = "TGS4 GHETTO V3 CONTROL PANEL";
			// 
			// Login
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(40)))), ((int)(((byte)(34)))));
			this.ClientSize = new System.Drawing.Size(436, 273);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.DeleteLoginButton);
			this.Controls.Add(this.IPComboBox);
			this.Controls.Add(this.SavePasswordCheckBox);
			this.Controls.Add(this.PortSelector);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.AddressLabel);
			this.Controls.Add(this.RemoteLoginButton);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.UsernameTextBox);
			this.Controls.Add(this.CurrentRevisionTitle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.Name = "Login";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Login";
			((System.ComponentModel.ISupportInitialize)(this.PortSelector)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label CurrentRevisionTitle;
		private System.Windows.Forms.TextBox UsernameTextBox;
		private System.Windows.Forms.TextBox PasswordTextBox;
		private System.Windows.Forms.Button RemoteLoginButton;
		private System.Windows.Forms.Label AddressLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown PortSelector;
		private System.Windows.Forms.CheckBox SavePasswordCheckBox;
		private System.Windows.Forms.ComboBox IPComboBox;
		private System.Windows.Forms.Button DeleteLoginButton;
		private System.Windows.Forms.Label label1;
	}
}