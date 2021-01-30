using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using Tgstation.Server.Api.Models;
using System.Collections.Generic;

namespace TGS.ControlPanel
{
	partial class ControlPanel
	{
		enum RepoAction {
			Clone,
			Checkout,
			Update,
			Merge,
			Reset,
			Test,
			Wait,
			GenCL,
		}

		RepoAction action;
		string CloneRepoURL;
		string CheckoutBranch;
		int TestPR;

		string repoError;

		private void InitRepoPage()
		{
			RepoBGW.ProgressChanged += RepoBGW_ProgressChanged;
			RepoBGW.RunWorkerCompleted += RepoBGW_RunWorkerCompleted;
			RepoBGW.DoWork += RepoBGW_DoWork;
			BackupTagsList.MouseDoubleClick += BackupTagsList_MouseDoubleClick;
		}

		private void BackupTagsList_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			int index = BackupTagsList.IndexFromPoint(e.Location);
			if (index != ListBox.NoMatches)
			{
				var indexText = (string)BackupTagsList.Items[index];
				if (indexText == "None" || indexText == "Unknown")
					return;
				var tagname = indexText.Split(':')[0];
				var spaceSplits = indexText.Split(' ');
				var sha = spaceSplits[spaceSplits.Length - 1];

				if (MessageBox.Show(String.Format("Checkout tag {0} ({1})?", tagname, sha), "Restore Backup", MessageBoxButtons.YesNo) != DialogResult.Yes)
					return;

				CheckoutBranch = tagname;
				DoAsyncOp(RepoAction.Checkout, "Checking out " + tagname + "...");
			}
		}

		private void RepoBGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			RepoProgressBar.Value = 100;
			RepoProgressBar.Style = ProgressBarStyle.Blocks;
			RepoPanel.UseWaitCursor = false;
			PopulateRepoFields();
		}

		private void RepoRefreshButton_Click(object sender, EventArgs e)
		{
			PopulateRepoFields();
		}

		private void PopulateRepoFields()
		{
			if (repoError != null)
				MessageBox.Show("An error occured: " + repoError);

			if (RepoBusyCheck())
				return;

			var Repo = client.Repository.Read(default).GetAwaiter().GetResult();

			RepoProgressBar.Style = ProgressBarStyle.Marquee;
			RepoProgressBar.Visible = false;
			RemoteNameTitle.Visible = true;
			RepoRemoteTextBox.Visible = true;
			BranchNameTitle.Visible = true;
			RepoBranchTextBox.Visible = true;
			RepoRefreshButton.Visible = true;
			SyncCommitsCheckBox.Visible = true;
			SyncCommitsCheckBox.Checked = Repo.PushTestMergeCommits.Value;

			if (Repo.Origin != null)
			{
				//repo unavailable
				RepoRemoteTextBox.Text = "git://github.com/tgstation/tgstation.git";
				RepoBranchTextBox.Text = "master";
				RepoProgressBarLabel.Text = "Unable to locate repository";
				CloneRepositoryButton.Visible = true;
			}
			else
			{
				RepoProgressBarLabel.Visible = false;

				CurrentRevisionLabel.Visible = true;
				CurrentRevisionTitle.Visible = true;
				IdentityLabel.Visible = true;
				MergePRButton.Visible = true;
				TestMergeListLabel.Visible = true;
				TestMergeListTitle.Visible = true;
				UpdateRepoButton.Visible = true;
				BackupTagsList.Visible = true;
				HardReset.Visible = true;
				RepoApplyButton.Visible = true;
				TestmergeSelector.Visible = true;
				RepoGenChangelogButton.Visible = true;
				RecloneButton.Visible = true;
				ResetRemote.Visible = true;
				TGSJsonUpdate.Visible = true;

				CurrentRevisionLabel.Text = Repo.RevisionInformation.CommitSha;
				RepoRemoteTextBox.Text = Repo.Origin.ToString();
				RepoBranchTextBox.Text = Repo.Reference;

				BackupTagsList.Items.Add("NOT SUPPORTED");

				checkBox1.Checked = Repo.AutoUpdatesKeepTestMerges.Value;
				var PRs = Repo.RevisionInformation.ActiveTestMerges;
				TestMergeListLabel.Items.Clear();
				if (PRs != null)
					if (PRs.Count == 0)
						TestMergeListLabel.Items.Add("None");
					else
						foreach (var I in PRs)
							TestMergeListLabel.Items.Add(String.Format("#{0}: {2} by {3} at commit {1}\r\n", I.Number, I.TargetCommitSha, I.TitleAtMerge, I.Author));
				else
					TestMergeListLabel.Items.Add("Unknown");
			}
		}

		bool RepoBusyCheck()
		{
			return false;
		}
		private void RepoBGW_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			var val = e.ProgressPercentage;
			if (val < 0)
			{
				RepoProgressBar.Style = ProgressBarStyle.Marquee;
				return;
			}
			RepoProgressBar.Style = ProgressBarStyle.Blocks;
			RepoProgressBar.Value = val;
		}

		private void RepoBGW_DoWork(object sender, DoWorkEventArgs e)
		{
			//Only for clones
			var Repo = client.Repository;

			switch (action) {
				case RepoAction.Clone:
					repoError = WrapServerOp(() => Repo.Clone(new Tgstation.Server.Api.Models.Repository { Origin = new Uri(CloneRepoURL), Reference = CheckoutBranch }, default)).GetAwaiter().GetResult();
					break;
				case RepoAction.Checkout:
					repoError = WrapServerOp(() => Repo.Update(new Tgstation.Server.Api.Models.Repository { Reference = CheckoutBranch }, default)).GetAwaiter().GetResult();
					break;
				case RepoAction.Merge:
					repoError = WrapServerOp(() => Repo.Update(new Tgstation.Server.Api.Models.Repository { UpdateFromOrigin = true }, default)).GetAwaiter().GetResult();
					break;
				case RepoAction.Reset:
					repoError = WrapServerOp(() => Repo.Update(new Tgstation.Server.Api.Models.Repository { UpdateFromOrigin = false, Reference = CheckoutBranch }, default)).GetAwaiter().GetResult();
					break;
				case RepoAction.Test:
					repoError = WrapServerOp(() => Repo.Update(new Tgstation.Server.Api.Models.Repository { NewTestMerges = new List<TestMergeParameters> { new TestMergeParameters { Number = TestPR } } }, default)).GetAwaiter().GetResult();
					break;
				case RepoAction.Update:
					repoError = WrapServerOp(() => Repo.Update(new Tgstation.Server.Api.Models.Repository { UpdateFromOrigin = true, Reference = CheckoutBranch }, default)).GetAwaiter().GetResult();
					break;
				case RepoAction.Wait:
					break;
				case RepoAction.GenCL:
					repoError = "NOT SUPPORTED";
					break;
				default:
					//reeee
					return;
			}
		}

		private void CloneRepositoryButton_Click(object sender, EventArgs e)
		{
			CloneRepo();
		}

		void CloneRepo()
		{
			CloneRepoURL = RepoRemoteTextBox.Text;
			CheckoutBranch = RepoBranchTextBox.Text;

			DoAsyncOp(RepoAction.Clone, String.Format("Cloning {0} branch of {1}...", CheckoutBranch, CloneRepoURL));
		}
		private void RecloneButton_Click(object sender, EventArgs e)
		{
			var DialogResult = MessageBox.Show("This will re-clone the repository, backup, and reset the Static configuration folders. Continue?", "Confim", MessageBoxButtons.YesNo);
			if (DialogResult == DialogResult.No)
				return;
			CloneRepo();
		}
		void DoAsyncOp(RepoAction ra, string message)
		{
			if (RepoBGW.IsBusy || (ra != RepoAction.Wait && RepoBusyCheck()))
				return;

			SyncCommitsCheckBox.Visible = false;
			CurrentRevisionLabel.Visible = false;
			CurrentRevisionTitle.Visible = false;
			TestMergeListLabel.Visible = false;
			TestMergeListTitle.Visible = false;
			RepoApplyButton.Visible = false;
			UpdateRepoButton.Visible = false;
			MergePRButton.Visible = false;
			CloneRepositoryButton.Visible = false;
			RemoteNameTitle.Visible = false;
			RepoRemoteTextBox.Visible = false;
			BranchNameTitle.Visible = false;
			RepoBranchTextBox.Visible = false;
			RepoProgressBar.Visible = true;
			HardReset.Visible = false;
			IdentityLabel.Visible = false;
			TestmergeSelector.Visible = false;
			RepoGenChangelogButton.Visible = false;
			RecloneButton.Visible = false;
			ResetRemote.Visible = false;
			BackupTagsList.Visible = false;
			RepoRefreshButton.Visible = false;
			TGSJsonUpdate.Visible = false;

			RepoPanel.UseWaitCursor = true;

			RepoProgressBar.Value = 0;
			RepoProgressBar.Style = ProgressBarStyle.Marquee;

			RepoProgressBarLabel.Text = message;
			RepoProgressBarLabel.Visible = true;

			action = ra;
			repoError = null;

			RepoBGW.RunWorkerAsync();
		}
		private void RepoApplyButton_Click(object sender, EventArgs e)
		{
			var Repo = client.Repository;

			if (RepoBusyCheck())
				return;

			Repository current = null;
			var error = WrapServerOp(async () => current = await Repo.Update(new Repository { AutoUpdatesSynchronize = SyncCommitsCheckBox.Checked }, default)).GetAwaiter().GetResult();
			if (error != null)
			{
				MessageBox.Show("Error: " + error);
				return;
			}

			CheckoutBranch = RepoBranchTextBox.Text;
			if (current.Reference != CheckoutBranch)
				DoAsyncOp(RepoAction.Checkout, String.Format("Checking out {0}...", CheckoutBranch));
		}

		private void UpdateRepoButton_Click(object sender, EventArgs e)
		{
			DoAsyncOp(RepoAction.Merge, "Merging origin branch...");
		}

		private void HardReset_Click(object sender, EventArgs e)
		{
			DoAsyncOp(RepoAction.Reset, "Resetting to origin branch...");
		}
		
		private void ResetRemote_Click(object sender, EventArgs e)
		{
			DoAsyncOp(RepoAction.Update, "Updating and resetting to remote branch...");
		}

		private void TestMergeButton_Click(object sender, EventArgs e)
		{
			if (TestmergeSelector.Value == 0)
			{
				MessageBox.Show("Invalid PR number!");
				return;
			}
			TestPR = (int)TestmergeSelector.Value;
			DoAsyncOp(RepoAction.Test, String.Format("Merging latest commit of PR #{0}...", TestPR));
		}

		private void RepoGenChangelogButton_Click(object sender, System.EventArgs e)
		{
			DoAsyncOp(RepoAction.GenCL, "Generating changelog...");
		}

		private void TGSJsonUpdate_Click(object sender, EventArgs e)
		{
			MessageBox.Show("NOT SUPPORTED!");
		}

		private async void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			var result = await WrapServerOp(() => client.Repository.Update(new Repository { AutoUpdatesKeepTestMerges = checkBox1.Checked }, default));
			if (result != null)
				MessageBox.Show(result);
		}
	}
}
