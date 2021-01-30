using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tgstation.Server.Api.Models;
using Tgstation.Server.Api.Rights;
using Tgstation.Server.Client;
using Tgstation.Server.Client.Components;

namespace TGS.ControlPanel
{
	/// <summary>
	/// Form used for managing <see cref="IInstance"/>s
	/// </summary>
	sealed partial class InstanceSelector : CountedForm
	{
		/// <summary>
		/// The <see cref="IServer"/> we build instance connections from
		/// </summary>
		readonly IServerClient server;
		/// <summary>
		/// Used for modifying <see cref="EnabledCheckBox"/> without invoking its side effects
		/// </summary>
		bool UpdatingEnabledCheckbox = false;

		/// <summary>
		/// Construct an <see cref="InstanceSelector"/>
		/// </summary>
		/// <param name="_server">The value of <see cref="server"/></param>
		public InstanceSelector(IServerClient _server)
		{
			InitializeComponent();
			InstanceListBox.DisplayMember = nameof(Instance.Name);
			InstanceListBox.MouseDoubleClick += InstanceListBox_MouseDoubleClick;
			server = _server;
			RefreshInstances();
		}

		/// <summary>
		/// Connects to a <see cref="IInstance"/> if it is double clicked in <see cref="InstanceListBox"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="MouseEventArgs"/></param>
		void InstanceListBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			TryConnectToIndexInstance(InstanceListBox.IndexFromPoint(e.Location));
		}

		/// <summary>
		/// Returns the <see cref="InstanceMetadata"/> associated with <see cref="InstanceListBox"/>'s current selected index
		/// </summary>
		/// <returns>The <see cref="InstanceMetadata"/> associated with <see cref="InstanceListBox"/>'s current selected index if it exists, <see langword="null"/> otherwise</returns>
		Instance GetSelectedInstanceMetadata()
		{
			var index = (Instance)InstanceListBox.SelectedItem;
			return index;
		}

		/// <summary>
		/// Loads the <see cref="InstanceListBox"/> using <see cref="Interface.Components.ITGLanding.ListInstances"/>
		/// </summary>
		void RefreshInstances()
		{
			InstanceListBox.Items.Clear();
			var instances = server.Instances.List(null, default).GetAwaiter().GetResult();
			foreach(var I in instances)
				InstanceListBox.Items.Add(I);
			var managerRights = server.Users.Read(default).GetAwaiter().GetResult().PermissionSet.InstanceManagerRights.Value;
			CreateInstanceButton.Enabled = managerRights.HasFlag(InstanceManagerRights.Create);
			ImportInstanceButton.Enabled = managerRights.HasFlag(InstanceManagerRights.Create);
			RenameInstanceButton.Enabled = managerRights.HasFlag(InstanceManagerRights.Rename);
			DetachInstanceButton.Enabled = managerRights.HasFlag(InstanceManagerRights.Delete);
			EnabledCheckBox.Enabled = managerRights.HasFlag(InstanceManagerRights.SetOnline);
			if(InstanceListBox.Items.Count > 0)
				InstanceListBox.SelectedIndex = 0;
		}

		/// <summary>
		/// Tries to start a <see cref="ControlPanel"/> for a given <see cref="InstanceListBox"/> <paramref name="index"/>
		/// </summary>
		/// <param name="index">The <see cref="ListBox.SelectedIndex"/> of <see cref="InstanceListBox"/> to connect to</param>
		void TryConnectToIndexInstance(int index)
		{
			if (index == ListBox.NoMatches)
				return;
			var instance = (Instance)InstanceListBox.Items[index];
			if (ControlPanel.InstancesInUse.TryGetValue(instance.Name, out ControlPanel activeCP))
			{
				activeCP.BringToFront();
				return;
			}
			
			new ControlPanel(server, instance).Show();
		}

		/// <summary>
		/// Prompts the user for parameters to <see cref="Interface.Components.ITGInstanceManager.DetachInstance(string)"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void DetachInstanceButton_Click(object sender, EventArgs e)
		{
			var imd = GetSelectedInstanceMetadata();
			if (imd == null)
				return;
			if (MessageBox.Show(String.Format("This will dissociate the server instance at \"{0}\"! Are you sure?", imd.Path), "Instance Detach", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			string res = null;
			await WrapServerOp(() => server.Instances.Detach(imd, default));
			if (res != null)
				MessageBox.Show(res);
			RefreshInstances();
		}

		/// <summary>
		/// Calls <see cref="RefreshInstances"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		void RefreshButton_Click(object sender, EventArgs e)
		{
			RefreshInstances();
		}

		/// <summary>
		/// Prompts the user for parameters to <see cref="Interface.Components.ITGInstanceManager.RenameInstance(string, string)"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void RenameInstanceButton_Click(object sender, EventArgs e)
		{
			var imd = GetSelectedInstanceMetadata();
			if (imd == null)
				return;
			var new_name = Program.TextPrompt("Instance Rename", "Enter a new name for the instance:");
			if (new_name == null)
				return;
			if (imd.Online.Value && MessageBox.Show(String.Format("This will temporarily offline the server instance! Are you sure?", imd.Path), "Instance Restart", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			var updated = new Instance
			{
				Id = imd.Id,
				Name = new_name
			};
			var res = await WrapServerOp(() => server.Instances.Update(updated, default));
			if (res != null)
				MessageBox.Show(res);
			RefreshInstances();
		}

		/// <summary>
		/// Prompts the user for parameters to <see cref="Interface.Components.ITGInstanceManager.ImportInstance(string)"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void ImportInstanceButton_Click(object sender, EventArgs e)
		{
			var instance_path = Program.TextPrompt("Instance Import", "Enter the full path to the instance:");
			if (instance_path == null)
				return;
			var res = await WrapServerOp(() => server.Instances.CreateOrAttach(new Instance { Path = instance_path }, default));
			if (res != null)
				MessageBox.Show(res);
			RefreshInstances();
		}

		/// <summary>
		/// Prompts the user for parameters to <see cref="Interface.Components.ITGInstanceManager.CreateInstance(string, string)"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void CreateInstanceButton_Click(object sender, EventArgs e)
		{
			var instance_name = Program.TextPrompt("Instance Creation", "Enter the name of the instance:");
			if (instance_name == null)
				return;
			var instance_path = Program.TextPrompt("Instance Creation", "Enter the full path to the instance:");
			if (instance_path == null)
				return;
			var res = await WrapServerOp(() => server.Instances.CreateOrAttach(new Instance { Name = instance_name, Path = instance_path }, default));
			if (res != null)
				MessageBox.Show(res);
			RefreshInstances();
		}

		/// <summary>
		/// Attempts to connect the user to an <see cref="IInstance"/> based on the <see cref="ListBox.SelectedIndex"/> of <see cref="InstanceListBox"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		void ConnectButton_Click(object sender, EventArgs e)
		{
			TryConnectToIndexInstance(InstanceListBox.SelectedIndex);
		}

		/// <summary>
		/// Prompts the user if they want to call <see cref="Interface.Components.ITGInstanceManager.SetInstanceEnabled(string, bool)"/> to either online or offline an <see cref="IInstance"/> based on its current state
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void EnabledCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (UpdatingEnabledCheckbox)
				return;
			var enabling = EnabledCheckBox.Checked;
			try
			{
				if (MessageBox.Show(String.Format("Are you sure you want to {0} this instance?", enabling ? "online" : "offline"), "Instance Status Change", MessageBoxButtons.YesNo) != DialogResult.Yes)
					return;
				var res = await WrapServerOp(() => server.Instances.Update(new Instance { Id = GetSelectedInstanceMetadata().Id, Online = enabling }, default));
				if (res != null)
					MessageBox.Show(res);
			}
			finally
			{
				RefreshInstances();
			}
		}

		/// <summary>
		/// Update <see cref="EnabledCheckBox"/> based on the selected <see cref="IInstance"/>'s <see cref="InstanceMetadata.Enabled"/> property
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		void InstanceListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (InstanceListBox.SelectedIndex != ListBox.NoMatches)
			{
				UpdatingEnabledCheckbox = true;
				EnabledCheckBox.Checked = GetSelectedInstanceMetadata().Online.Value;
				UpdatingEnabledCheckbox = false;
			}
		}
	}
}
