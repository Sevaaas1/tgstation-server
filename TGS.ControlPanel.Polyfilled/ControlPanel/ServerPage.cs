using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tgstation.Server.Api.Models;

namespace TGS.ControlPanel
{
	partial class ControlPanel
	{
		bool updatingFields = false;

		/// <summary>
		/// <see cref="GitHubClient"/> used for checking the merged state of <see cref="PullRequest"/>s
		/// </summary>
		GitHubClient ghclient;

		void InitServerPage()
		{
			projectNameText.LostFocus += ProjectNameText_LostFocus;
			projectNameText.KeyDown += ProjectNameText_KeyDown;
			ServerStartBGW.RunWorkerCompleted += ServerStartBGW_RunWorkerCompleted;
			ghclient = new GitHubClient(new ProductHeaderValue(Assembly.GetExecutingAssembly().GetName().Name));
			var config = Properties.Settings.Default;
			if (!String.IsNullOrWhiteSpace(config.GitHubAPIKey))
				ghclient.Credentials = new Credentials(Helpers.DecryptData(config.GitHubAPIKey, config.GitHubAPIKeyEntropy));
		}

		private void ServerStartBGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result != null)
				MessageBox.Show((string)e.Result);
			LoadServerPage();
		}

		private void ProjectNameText_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				UpdateProjectName();
		}

		private void CompileCancelButton_Click(object sender, EventArgs e)
		{
			MessageBox.Show("NOT SUPPORTED!");
		}

		void LoadServerPage()
		{
			var RepoExists = true;
			compileButton.Visible = RepoExists;
			AutoUpdateCheckbox.Visible = RepoExists;
			initializeButton.Visible = RepoExists;
			AutostartCheckbox.Visible = RepoExists;
			WebclientCheckBox.Visible = RepoExists;
			PortSelector.Visible = RepoExists;
			projectNameText.Visible = RepoExists;
			CompilerStatusLabel.Visible = RepoExists;
			CompileCancelButton.Visible = RepoExists;
			CompilerLabel.Visible = RepoExists;
			ProjectPathLabel.Visible = RepoExists;
			ServerGStopButton.Visible = RepoExists;
			ServerStartButton.Visible = RepoExists;
			ServerGRestartButton.Visible = RepoExists;
			ServerRestartButton.Visible = RepoExists;
			PortLabel.Visible = RepoExists;
			ServerStopButton.Visible = RepoExists;
			TestMergeManagerButton.Visible = RepoExists;
			UpdateServerButton.Visible = RepoExists;
			RemoveAllTestMergesButton.Visible = RepoExists;
			WorldAnnounceField.Visible = RepoExists;
			WorldAnnounceButton.Visible = RepoExists;
			WorldAnnounceLabel.Visible = RepoExists;
			SyncCommitsCheckBox.Visible = RepoExists;

			if (updatingFields)
				return;

			var DM = client.DreamMaker.Read(default).GetAwaiter().GetResult();
			var DD = client.DreamDaemon.Read(default).GetAwaiter().GetResult();
			var Config = client.Configuration;
			var Repo = client.Repository;

			try
			{
				updatingFields = true;

				ServerPathLabel.Text = "Server Path: " + Instance.Path;

				SecuritySelector.SelectedIndex = (int)(DD.CurrentSecurity ?? DD.SecurityLevel.Value);

				if (!RepoExists)
					return;

				var interval = Instance.AutoUpdateInterval.Value;
				var interval_not_zero = interval != 0;
				AutoUpdateCheckbox.Checked = interval_not_zero;
				AutoUpdateInterval.Visible = interval_not_zero;
				AutoUpdateMLabel.Visible = interval_not_zero;
				if (interval_not_zero)
					AutoUpdateInterval.Value = interval;

				var DaeStat = DD.Status.Value;
				var Online = DaeStat == Tgstation.Server.Api.Models.WatchdogStatus.Online;
				ServerStartButton.Enabled = !Online;
				ServerGStopButton.Enabled = Online;
				ServerGRestartButton.Enabled = Online;
				ServerStopButton.Enabled = Online;
				ServerRestartButton.Enabled = Online;

				switch (DaeStat)
				{
					case Tgstation.Server.Api.Models.WatchdogStatus.Restoring:
						ServerStatusLabel.Text = "REBOOTING";
						break;
					case Tgstation.Server.Api.Models.WatchdogStatus.Offline:
						ServerStatusLabel.Text = "OFFLINE";
						break;
					case Tgstation.Server.Api.Models.WatchdogStatus.Online:
						ServerStatusLabel.Text = "ONLINE";
						break;
					case Tgstation.Server.Api.Models.WatchdogStatus.DelayedRestart:
						ServerStatusLabel.Text = "REBOOTING (DELAYED)";
						break;
				}

				ServerGStopButton.Checked = DD.SoftShutdown.Value;

				AutostartCheckbox.Checked = DD.AutoStart.Value;
				WebclientCheckBox.Checked = DD.AllowWebClient.Value;
				if (!PortSelector.Focused)
					PortSelector.Value = DD.Port.Value;
				if (!projectNameText.Focused)
					projectNameText.Text = DM.ProjectName;

				UpdateServerButton.Enabled = false;
				TestMergeManagerButton.Enabled = false;
				RemoveAllTestMergesButton.Enabled = false;
				CompilerStatusLabel.Text = "Unknown! (NOT SUPPORTED)";
				initializeButton.Enabled = true;
				compileButton.Enabled = true;
				CompileCancelButton.Enabled = true;
			}
			finally
			{
				updatingFields = false;
			}
		}

		private void ProjectNameText_LostFocus(object sender, EventArgs e)
		{
			UpdateProjectName();
		}

		void UpdateProjectName()
		{
			if (!updatingFields)
				client.DreamMaker.Update(new Tgstation.Server.Api.Models.DreamMaker { ProjectName = projectNameText.Text }, default).GetAwaiter().GetResult();
		}

		private void PortSelector_ValueChanged(object sender, EventArgs e)
		{
			if (!updatingFields)
				client.DreamDaemon.Update(new Tgstation.Server.Api.Models.DreamDaemon { Port = (ushort)PortSelector.Value }, default).GetAwaiter().GetResult();
		}

		private void ServerPageRefreshButton_Click(object sender, EventArgs e)
		{
			LoadServerPage();
		}

		private void InitializeButton_Click(object sender, EventArgs e)
		{
			MessageBox.Show("NOT SUPPORTED!");
		}
		private void CompileButton_Click(object sender, EventArgs e)
		{
			var error = WrapServerOp(() => client.DreamMaker.Compile(default)).GetAwaiter().GetResult();
			MessageBox.Show(error ?? "Deployment started!");
		}

		private void AutostartCheckbox_CheckedChanged(object sender, System.EventArgs e)
		{
			client.DreamDaemon.Update(new Tgstation.Server.Api.Models.DreamDaemon { AutoStart = AutostartCheckbox.Checked }, default).GetAwaiter().GetResult();
		}
		private void ServerStartButton_Click(object sender, System.EventArgs e)
		{
			if (!ServerStartBGW.IsBusy)
				ServerStartBGW.RunWorkerAsync();
		}

		private void ServerStartBGW_DoWork(object sender, DoWorkEventArgs e)
		{
			client.DreamDaemon.Start(default).GetAwaiter().GetResult();
		}

		private void ServerStopButton_Click(object sender, EventArgs e)
		{
			var DialogResult = MessageBox.Show("This will immediately shut down the server. Continue?", "Confim", MessageBoxButtons.YesNo);
			if (DialogResult == DialogResult.No)
				return;
			var res = WrapServerOp(() => client.DreamDaemon.Shutdown(default)).GetAwaiter().GetResult();
			if (res != null)
				MessageBox.Show(res);
		}

		private void ServerRestartButton_Click(object sender, EventArgs e)
		{
			var DialogResult = MessageBox.Show("This will immediately restart the server. Continue?", "Confim", MessageBoxButtons.YesNo);
			if (DialogResult == DialogResult.No)
				return;
			var res = WrapServerOp(() => client.DreamDaemon.Restart(default)).GetAwaiter().GetResult();
			if (res != null)
				MessageBox.Show(res);
		}

		private void ServerGStopButton_Checked(object sender, EventArgs e)
		{
			if (updatingFields)
				return;
			var DialogResult = MessageBox.Show("This will shut down the server when the current round ends. Continue?", "Confim", MessageBoxButtons.YesNo);
			if (DialogResult == DialogResult.No)
				return;
			client.DreamDaemon.Update(new Tgstation.Server.Api.Models.DreamDaemon { SoftShutdown = true }, default).GetAwaiter().GetResult();
			LoadServerPage();
		}

		private void ServerGRestartButton_Click(object sender, EventArgs e)
		{
			var DialogResult = MessageBox.Show("This will restart the server when the current round ends. Continue?", "Confim", MessageBoxButtons.YesNo);
			if (DialogResult == DialogResult.No)
				return;
			client.DreamDaemon.Update(new Tgstation.Server.Api.Models.DreamDaemon { SoftRestart = true }, default).GetAwaiter().GetResult();
			LoadServerPage();
		}

		/// <summary>
		/// Launches the <see cref="TestMergeManager"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		void TestMergeManagerButton_Click(object sender, System.EventArgs e)
		{
			using (var TMM = new TestMergeManager(client, ghclient))
				TMM.ShowDialog();
			LoadServerPage();
		}

		/// <summary>
		/// Calls <see cref="UpdateServer"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		void UpdateServerButton_Click(object sender, EventArgs e)
		{
			UpdateServer();
		}

		/// <summary>
		/// Calls <see cref="ITGRepository.Update(bool)"/> with a <see langword="true"/> parameter, re-merging any current <see cref="PullRequest"/>s at their current commit, calls <see cref="ITGRepository.GenerateChangelog(out string)"/> and <see cref="ITGRepository.SynchronizePush"/>, and starts the <see cref="ITGCompiler.Compile(bool)"/> prompting the user with any errors that may occur. Merged <see cref="PullRequest"/>s are not remerged
		/// </summary>
		async void UpdateServer()
		{
			try
			{
				UseWaitCursor = true;
				Enabled = false;
				try
				{
					string res = null;
					var repo = client.Repository;
					Tgstation.Server.Api.Models.Repository repoInfo = null;
					res = await WrapServerOp(async () => repoInfo = await repo.Read(default));

					if (res != null)
					{
						MessageBox.Show(res);
						return;
					}

					var pulls = repoInfo.RevisionInformation.ActiveTestMerges.ToList();

					List<Task<PullRequest>> pullsRequests = null;
					if (Program.GetRepositoryRemote(repo, out string remoteOwner, out string remoteName))
					{
						//find out which of the PRs have been merged
						pullsRequests = new List<Task<PullRequest>>();
						foreach (var I in pulls)
							pullsRequests.Add(ghclient.PullRequest.Get(remoteOwner, remoteName, I.Number));
					}

					res = await WrapServerOp(() => repo.Update(new Tgstation.Server.Api.Models.Repository { UpdateFromOrigin = true }, default));


					if (pullsRequests != null)
						Task.WaitAll(pullsRequests.ToArray());

					foreach (var I in pullsRequests)
						if (I.Result.Merged)
							pulls.RemoveAll(x => x.Number == I.Result.Number);

					res = await WrapServerOp(() => repo.Update(new Tgstation.Server.Api.Models.Repository { UpdateFromOrigin = true, NewTestMerges = pulls.Select(x => new Tgstation.Server.Api.Models.TestMergeParameters { TargetCommitSha = x.TargetCommitSha, Number = x.Number }).ToList() }, default));
					if (res != null)
					{
						MessageBox.Show(res, "Error updating repository");
						return;
					}
					res = await WrapServerOp(() => client.DreamMaker.Compile(default));

					if (res != null)
						MessageBox.Show(res, "Error starting compile!");
				}
				finally
				{
					UseWaitCursor = false;
					Enabled = true;
				}
			}
			catch (ForbiddenException)
			{
				if (ghclient.Credentials.AuthenticationType == AuthenticationType.Anonymous)
				{
					if (Program.RateLimitPrompt(ghclient))
						UpdateServer();
					return;
				}
				else
					throw;
			}
			LoadServerPage();
		}

		/// <summary>
		/// Calls <see cref="ITGRepository.Reset(bool)"/> with a <see langword="true"/> parameter, calls <see cref="ITGRepository.GenerateChangelog(out string)"/>, and starts the <see cref="ITGCompiler.Compile(bool)"/> prompting the user with any errors that may occur.
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void RemoveAllTestMergesButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to remove all test merges?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			try
			{
				UseWaitCursor = true;
				Enabled = false;
				try
				{
					var repo = client.Repository;

					var res = await WrapServerOp(() => repo.Update(new Tgstation.Server.Api.Models.Repository { Reference = "master" }, default));

					if (res != null)
					{
						MessageBox.Show(res, "Error resetting repository");
						return;
					}

					res = await WrapServerOp(() => client.DreamMaker.Compile(default));
					if (res != null)
						MessageBox.Show(res, "Error starting compile!");
				}
				finally
				{
					UseWaitCursor = false;
					Enabled = true;
				}
			}
			catch (ForbiddenException)
			{
				if (ghclient.Credentials.AuthenticationType == AuthenticationType.Anonymous)
				{
					if (Program.RateLimitPrompt(ghclient))
						UpdateServer();
					return;
				}
				else
					throw;
			}
			LoadServerPage();
		}

		private void SecuritySelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!updatingFields)
			{
				client.DreamDaemon.Update(new DreamDaemon { SecurityLevel = (DreamDaemonSecurity)SecuritySelector.SelectedIndex }, default).GetAwaiter().GetResult();
				MessageBox.Show("Security change will be applied after next server reboot.");
			}
		}

		private void WorldAnnounceButton_Click(object sender, EventArgs e)
		{
			MessageBox.Show("NOT SUPPORTED!");
		}

		private void WebclientCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!updatingFields)
				client.DreamDaemon.Update(new DreamDaemon { AllowWebClient = WebclientCheckBox.Checked}, default).GetAwaiter().GetResult();
		}

		private void AutoUpdateInterval_ValueChanged(object sender, EventArgs e)
		{
			if (updatingFields)
				return;
			MessageBox.Show("NOT SUPPORTED!");
		}

		private void AutoUpdateCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (updatingFields)
				return;
			MessageBox.Show("NOT SUPPORTED!");
		}

		private void createMinidump_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("This will create a full memory minidump of the current DD process and save it in Diagnostics/Minidumps. Continue?", "Minidump", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;

			client.DreamDaemon.CreateDump(default).GetAwaiter().GetResult();
		}
	}
}
