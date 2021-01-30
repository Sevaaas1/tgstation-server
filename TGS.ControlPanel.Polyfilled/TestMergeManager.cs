using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tgstation.Server.Api.Models;
using Tgstation.Server.Client.Components;

namespace TGS.ControlPanel
{
	sealed partial class TestMergeManager : ServerOpForm
	{
		/// <summary>
		/// Error message format used when <see cref="ITGRepository.MergedPullRequests(out string)"/> fails
		/// </summary>
		const string MergedPullsError = "Error retrieving currently merged pull requests: {0}";

		/// <summary>
		/// The <see cref="IInstance"/>to handle pull requests for
		/// </summary>
		readonly IInstanceClient currentInterface;

		/// <summary>
		/// The <see cref="GitHubClient"/> to use to read PR lists
		/// </summary>
		readonly GitHubClient client;

		/// <summary>
		/// The owner of the target <see cref="Repository"/>
		/// </summary>
		string repoOwner;
		/// <summary>
		/// The name of the target <see cref="Repository"/>
		/// </summary>
		string repoName;

		/// <summary>
		/// Construct a <see cref="TestMergeManager"/>
		/// </summary>
		/// <param name="interfaceToUse">The <see cref="IInstance"/> to manage pull requests for</param>
		/// <param name="clientToUse">The <see cref="GitHubClient"/> to use for getting pull request information</param>
		public TestMergeManager(IInstanceClient interfaceToUse, GitHubClient clientToUse)
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
			UpdateToRemoteRadioButton.Checked = true;
			currentInterface = interfaceToUse;
			client = clientToUse;
			Load += PullRequestManager_Load;
			PullRequestListBox.ItemCheck += PullRequestListBox_ItemCheck;
		}

		/// <summary>
		/// Called when an item in <see cref="PullRequestListBox"/> is checked or unchecked. Unchecks opposing PRs
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="ItemCheckEventArgs"/></param>
		void PullRequestListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (e.NewValue != CheckState.Checked)
				return;
			//let's uncheck the opposing PR# if this is one of the outdated/updated testmerge
			var item = (string)PullRequestListBox.Items[e.Index];
			var prefix = item.Split(' ')[0];

			for (var I = 0; I < PullRequestListBox.Items.Count; ++I)
			{
				var S = (string)PullRequestListBox.Items[I];
				if (S.Split(' ')[0] == prefix && S != item)
				{
					PullRequestListBox.SetItemChecked(I, false);
					break;
				}
			}
		}

		/// <summary>
		/// Populate <see cref="PullRequestListBox"/> with <see cref="PullRequestInfo"/> from github and check off ones that are currently merged. Prompts the user to login to GitHub if they hit the rate limit
		/// </summary>
		async void LoadPullRequests()
		{
			try
			{
				Enabled = false;
				UseWaitCursor = true;
				try
				{
					PullRequestListBox.Items.Clear();

					var repo = currentInterface.Repository;
					string error = null;
					List<TestMerge> pulls = null;
					
					//get started on this while we're processing here
					var pullsRequest = Task.Run(() => pulls = (repo.Read(default).GetAwaiter().GetResult()).RevisionInformation.ActiveTestMerges.ToList());

					//Search for open PRs
					Enabled = false;
					UseWaitCursor = true;
					SearchIssuesResult result;
					try
					{
						result = await client.Search.SearchIssues(new SearchIssuesRequest
						{
							Repos = new RepositoryCollection { { repoOwner, repoName } },
							State = ItemState.Open,
							Type = IssueTypeQualifier.PullRequest
						});
					}
					finally
					{
						Enabled = true;
						UseWaitCursor = false;
					}

					//now we need to know what's merged
					await pullsRequest;
					if (pulls == null)
						MessageBox.Show(String.Format(MergedPullsError, error));

					//insert the open pull requests, checking already merged once
					foreach (var I in result.Items)
						if (!pulls.Any(x => x.Number == I.Number))
							InsertPullRequest(I, false, CheckState.Unchecked);

					//insert remaining merged pulls
					foreach (var I in pulls)
					{
						var pr = await client.PullRequest.Get(repoOwner, repoName, I.Number);
						var outdated = pr.Head.Sha != I.TargetCommitSha;
						var mergedOrOutdated = pr.Merged || outdated;
						InsertPullRequest(await client.Issue.Get(repoOwner, repoName, I.Number), true, mergedOrOutdated ? CheckState.Indeterminate : CheckState.Checked, String.Format("{1}{0}", mergedOrOutdated ? String.Format(" - {0}", I.TargetCommitSha) : String.Empty, pr.Merged ? " - MERGED ON REMOTE: " : (outdated ? " - OUTDATED: " : String.Empty)));
					}
				}
				finally
				{
					Enabled = true;
					UseWaitCursor = false;
				}
			}
			catch (ForbiddenException)
			{
				if (client.Credentials.AuthenticationType == AuthenticationType.Anonymous)  //assume request limit hit
				{
					if(Program.RateLimitPrompt(client))
						LoadPullRequests();
				}
				else
					throw;
			}
		}

		/// <summary>
		/// Format an entry for <paramref name="issue"/> and insert it into <see cref="PullRequestListBox"/>
		/// </summary>
		/// <param name="issue">The <see cref="Issue"/> to format, must contain a <see cref="PullRequest"/></param>
		/// <param name="prioritize">If this or <paramref name="checkState"/> is mpt <see cref="CheckState.Unchecked"/>, <paramref name="issue"/> will be inserted at the top of <see cref="PullRequestListBox"/> as opposed to the bottom</param>
		/// <param name="checkState">The <see cref="CheckState"/> of the item</param>
		/// <param name="append">An optional <see cref="string"/> to append to the entry before it's inserted</param>
		void InsertPullRequest(Issue issue, bool prioritize, CheckState checkState, string append = null)
		{
			bool needsTesting = issue.Labels.Any(x => x.Name.ToLower().Contains("test"));
			prioritize |= needsTesting;
			var itemString = String.Format("#{0} - {1}{2}{3}", issue.Number, issue.Title, needsTesting ? " - TESTING REQUESTED" : String.Empty, append ?? String.Empty);
			InsertItem(itemString, prioritize, checkState);
		}

		/// <summary>
		/// Insert an <paramref name="itemString"/> into <see cref="PullRequestListBox"/>
		/// </summary>
		/// <param name="itemString">The <see cref="string"/> to insert</param>
		/// <param name="prioritize">If this or <paramref name="checkState"/> is not <see cref="CheckState.Unchecked"/>, <paramref name="itemString"/> will be inserted at the top of <see cref="PullRequestListBox"/> as opposed to the bottom</param>
		/// <param name="checkState">The <see cref="CheckState"/> of the item</param>
		void InsertItem(string itemString, bool prioritize, CheckState checkState)
		{
			prioritize = prioritize || checkState != CheckState.Unchecked;
			if (prioritize)
			{
				PullRequestListBox.Items.Insert(0, itemString);
				PullRequestListBox.SetItemCheckState(0, checkState);
			}
			else
				PullRequestListBox.Items.Add(itemString, checkState);
		}

		/// <summary>
		/// Called when the <see cref="TestMergeManager"/> is loaded. Sets <see cref="repoOwner"/> and <see cref="repoName"/>
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		void PullRequestManager_Load(object sender, EventArgs e)
		{
			Enabled = false;
			var repo = currentInterface.Repository;
			if(!Program.GetRepositoryRemote(repo, out repoOwner, out repoName))
			{
				Close();
				return;
			}
			LoadPullRequests();
		}

		/// <summary>
		/// Called when the <see cref="ApplyButton"/> is clicked. Calls <see cref="ITGRepository.Update(bool)"/> if necessary, merge pull requests, and call <see cref="ITGCompiler.Compile(bool)"/>. Closes the <see cref="TestMergeManager"/> if appropriate
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void ApplyButton_Click(object sender, EventArgs e)
		{
			Enabled = false;
			UseWaitCursor = true;
			ApplyingPullRequestsLabel.Visible = true;
			ApplyingPullRequestsProgressBar.Visible = true;
			PullRequestListBox.Visible = false;
			try
			{
				//so first collect a list of pulls that are checked
				var pulls = new List<TestMergeParameters>();
				foreach (int I in PullRequestListBox.CheckedIndices)
				{
					var S = (string)PullRequestListBox.Items[I];
					string mergedSha = null;
					var splits = S.Split(' ');
					var mergedOnRemote = S.Contains(" - MERGED ON REMOTE: ");
					if (mergedOnRemote && UpdateToRemoteRadioButton.Checked)
						continue;
					if ((S.Contains(" - OUTDATED: ") || mergedOnRemote) && PullRequestListBox.GetItemCheckState(I) == CheckState.Indeterminate)
						mergedSha = splits[splits.Length - 1];
					var key = Convert.ToInt32((splits[0].Substring(1)));
					try
					{
						pulls.Add(new TestMerge { Number = key, TargetCommitSha = mergedSha });
					}
					catch
					{
						MessageBox.Show(String.Format("Checked both keep and update option for #{0}", key), "Error");
						return;
					}
				}

				var repo = currentInterface.Repository;

				string error = null;
				//Do standard repo updates
				if (UpdateToRemoteRadioButton.Checked)
					error = await WrapServerOp(() => repo.Update(new Tgstation.Server.Api.Models.Repository { UpdateFromOrigin = true, NewTestMerges = pulls }, default));
				else if (UpdateToOriginRadioButton.Checked)
					error = await WrapServerOp(() => repo.Update(new Tgstation.Server.Api.Models.Repository { Reference = "master", NewTestMerges = pulls }, default));

				if (error != null)
				{
					MessageBox.Show(String.Format("Error updating repository: {0}", error));
					return;
				}

				//Wait ten seconds then start the compile
				await Task.Delay(TimeSpan.FromSeconds(5));
				error = await WrapServerOp(() => currentInterface.DreamMaker.Compile(default));

				if (error != null)
					MessageBox.Show(String.Format("Error sychronizing repo: {0}", error));

				MessageBox.Show("Test merges updated and compilation started!");
			}
			finally
			{
				Enabled = true;
				UseWaitCursor = false;
				ApplyingPullRequestsLabel.Visible = false;
				ApplyingPullRequestsProgressBar.Visible = false;
				PullRequestListBox.Visible = true;
			}
		}

		/// <summary>
		/// Called when the <see cref="RefreshButton"/> is clicked
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		void RefreshButton_Click(object sender, EventArgs e)
		{
			LoadPullRequests();
		}

		/// <summary>
		/// Called when the <see cref="AddPRButton"/> is clicked. Adds the PR with the number in <see cref="AddPRNumericUpDown"/> to <see cref="PullRequestListBox"/> or shows an error prompt if it doesn't/already exists
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The <see cref="EventArgs"/></param>
		async void AddPRButton_Click(object sender, EventArgs e)
		{

			Enabled = false;
			UseWaitCursor = true;
			try
			{
				IList<TestMerge> pulls = null;
				string error = null;
				var mergedPullsTask = WrapServerOp(async () => pulls = (await currentInterface.Repository.Read(default)).RevisionInformation.ActiveTestMerges?.ToList());

				int PRNumber;
				try
				{
					PRNumber = Convert.ToInt32(AddPRNumericUpDown.Value);
				}
				catch
				{
					MessageBox.Show("Invalid PR number!");
					return;
				}
				var found = false;
				var asString = PRNumber.ToString();
				foreach (var I in PullRequestListBox.Items)
					if (((string)I).Split(' ')[0].Substring(1) == asString)
					{
						found = true;
						break;
					}
				if (found)
				{
					MessageBox.Show("That PR is already in the list!");
					return;
				}

				await mergedPullsTask;
				if (pulls == null)
					MessageBox.Show(String.Format(MergedPullsError, error));
				//get the PR in question
				var PR = await client.Issue.Get(repoOwner, repoName, PRNumber);
				if(PR == null ||PR.PullRequest == null)
				{
					MessageBox.Show("That doesn't seem to be a valid PR!");
					return;
				}
				InsertPullRequest(PR, true, CheckState.Unchecked);
			}
			finally
			{
				UseWaitCursor = false;
				Enabled = true;
			}
		}
	}
}
