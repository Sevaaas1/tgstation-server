﻿using System;
using System.Drawing;
using System.Windows.Forms;
using TGServiceInterface;
using TGServiceInterface.Components;

namespace TGControlPanel
{
	/// <summary>
	/// The main <see cref="ControlPanel"/> form
	/// </summary>
	partial class ControlPanel : Form
	{
		/// <summary>
		/// The <see cref="TGServiceInterface.Interface"/> instance for this <see cref="ControlPanel"/>
		/// </summary>
		readonly Interface Interface;

		/// <summary>
		/// Constructs a <see cref="ControlPanel"/>
		/// </summary>
		/// <param name="I">The <see cref="TGServiceInterface.Interface"/> for the <see cref="ControlPanel"/></param>
		public ControlPanel(Interface I)
		{
			Interface = I;
			InitializeComponent();
			if (Interface.IsRemoteConnection)
			{
				var splits = Interface.GetComponent<ITGSService>().Version().Split(' ');
				Text = String.Format("TGS {0}: {1}:{2}", splits[splits.Length - 1], Interface.HTTPSURL, Interface.HTTPSPort);
			}
			if (Interface.VersionMismatch(out string error) && MessageBox.Show(error, "Warning", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
			{
				Close();
				return;
			}
			Panels.SelectedIndexChanged += Panels_SelectedIndexChanged;
			Panels.SelectedIndex += Math.Min(Properties.Settings.Default.LastPageIndex, Panels.TabCount - 1);
			InitRepoPage();
			InitBYONDPage();
			InitServerPage();
			LoadChatPage();
			InitStaticPage();
		}

		private void Main_Resize(object sender, EventArgs e)
		{
			Panels.Location = new Point(10, 10);
			Panels.Width = ClientSize.Width - 20;
			Panels.Height = ClientSize.Height - 20;
		}

		private void Panels_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (Panels.SelectedIndex)
			{
				case 0: //repo
					PopulateRepoFields();
					break;
				case 1: //byond
					UpdateBYONDButtons();
					break;
				case 2: //scp
					LoadServerPage();
					break;
				case 3: //chat
					LoadChatPage();
					break;
			}
			Properties.Settings.Default.LastPageIndex = Panels.SelectedIndex;
		}

		bool CheckAdminWithWarning()
		{
			if (!Interface.AuthenticateAdmin())
			{
				MessageBox.Show("Only system administrators may use this command!");
				return false;
			}
			return true;
		}
	}
}