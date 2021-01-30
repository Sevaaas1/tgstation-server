using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Tgstation.Server.Api.Models;
using Tgstation.Server.Client;

namespace TGS.ControlPanel
{
	partial class ControlPanel
	{
		IDictionary<int, string> IndexesToPaths = new Dictionary<int, string>();
		ConfigurationFile originalCurrentFileContent;
		IList<string> EnumeratedPaths = new List<string>() { "" };
		bool enumerating = false;
		/// <summary>
		/// Whether or not the static page has been initialized yet
		/// </summary>
		bool initializedStaticPage = false;

		enum EnumResult
		{
			Enumerated,
			Unauthorized,
			NotEnumerated,
			Error,
		}

		void InitStaticPage()
		{
			initializedStaticPage = true;
		}

		void BuildFileList()
		{
			enumerating = true;
			IndexesToPaths.Clear();
			StaticFileListBox.Items.Clear();
			IndexesToPaths.Add(StaticFileListBox.Items.Add("/"), "/");
			if (EnumeratePath("/", 1) == EnumResult.Unauthorized)
			{
				StaticFileListBox.Items[0] += " (UNAUTHORIZED)";
				IndexesToPaths[0] = null;
			}
			StaticFileListBox.SelectedIndex = 0;
			enumerating = false;
		}

		EnumResult EnumeratePath(string path, int level)
		{
			if (!EnumeratedPaths.Contains(path))
				return EnumResult.NotEnumerated;
			IReadOnlyList<ConfigurationFile> Enum;
			try
			{
				Enum = client.Configuration.List(null, path, default).GetAwaiter().GetResult();
			}
			catch (Tgstation.Server.Client.UnauthorizedException)
			{
				return EnumResult.Unauthorized;
			}
			catch(Tgstation.Server.Client.ApiException ex)
			{
				MessageBox.Show(String.Format("Could not enumerate static path \"{0}\" error: {1}", path, $"{ex.ErrorCode}: {ex.Message}"));
				return EnumResult.Error;
			}

			foreach(var I in Enum)
			{
				IndexesToPaths.Add(StaticFileListBox.Items.Add(DSNTimes(level) + I.Path), I.Path);
			}
			return EnumResult.Enumerated;
		}

		string DSNTimes(int n)
		{
			var res = "";
			for (var I = 0; I < n; ++I)
				res += "  ";
			return res;
		}
		private void StaticFilesRefreshButton_Click(object sender, EventArgs e)
		{
			BuildFileList();
			UpdateEditText();
		}

		private void StaticFileUploadButton_Click(object sender, EventArgs e)
		{
			if (StaticFileEditTextbox.Text != "Directory")
			{
				MessageBox.Show("Please select a directory to upload the file to.");
				return;
			}
			var ofd = new OpenFileDialog()
			{
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultExt = ".txt",
				Multiselect = false,
				Title = "Static File Upload",
				ValidateNames = true,
				Filter = "All files (*.*)|*.*",
				AddExtension = false,
				SupportMultiDottedExtensions = true,
			};
			if (ofd.ShowDialog() != DialogResult.OK)
				return;

			var fileToUpload = ofd.FileName;

			var FileName = Path.Combine(IndexesToPaths[StaticFileListBox.SelectedIndex], Path.GetFileName(fileToUpload));

			string fileContents = null;
			string error = null;
			try
			{
				fileContents = File.ReadAllText(fileToUpload);
			}
			catch (Exception ex)
			{
				error = ex.ToString();
			}
			if (error == null)
				try
				{
					client.Configuration.Write(new ConfigurationFile { Path = FileName }, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContents)), default).GetAwaiter().GetResult();
				}
				catch (ApiException ex)
				{
					error = $"{ex.ErrorCode}: {ex.Message}";
				}
			if (error != null)
				MessageBox.Show("An error occurred: " + error);
			BuildFileList();
		}

		private void StaticFileDownloadButton_Click(object sender, EventArgs e)
		{
			if (StaticFileEditTextbox.ReadOnly)
			{
				MessageBox.Show("Cannot download this file!");
				return;
			}
			var remotePath = IndexesToPaths[StaticFileListBox.SelectedIndex];
			if (remotePath == null)
				return;
			string error;
			Stream stream;
			var ofd = new SaveFileDialog()
			{
				CheckFileExists = false,
				CheckPathExists = true,
				DefaultExt = ".txt",
				Title = "Static File Download",
				ValidateNames = true,
				Filter = "All files (*.*)|*.*",
				AddExtension = false,
				CreatePrompt = false,
				OverwritePrompt = true,
				SupportMultiDottedExtensions = true,
			};
			if (ofd.ShowDialog() != DialogResult.OK)
				return;

			try
			{
				var tuple = client.Configuration.Read(new ConfigurationFile { Path = remotePath }, default).GetAwaiter().GetResult();
				stream = tuple.Item2;
				error = null;
			}
			catch (ApiException ex)
			{
				error = $"{ex.ErrorCode}: {ex.Message}";
				stream = null;
			}

			if (stream != null)
			{
				try
				{
					using (var fs = new FileStream(ofd.FileName, FileMode.Create))
						stream.CopyTo(fs);
					return;
				}
				catch (Exception ex)
				{
					error = ex.ToString();
				}
			}
			MessageBox.Show("An error occurred: " + error);
		}

		private void StaticFileDeleteButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to delete " + ((string)StaticFileListBox.SelectedItem).Trim() + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;

			try
			{
				client.Configuration.Write(new ConfigurationFile { Path = IndexesToPaths[StaticFileListBox.SelectedIndex] }, null, default).GetAwaiter().GetResult();
			}
			catch (ApiException ex)
			{
				MessageBox.Show($"{ex.ErrorCode}: {ex.Message}");
			}

			BuildFileList();
		}

		private void StaticFileCreateButton_Click(object sender, EventArgs e)
		{
			if(StaticFileEditTextbox.Text != "Directory")
			{
				MessageBox.Show("Please select a directory to create the file in.");
				return;
			}
			var FileName = Program.TextPrompt("Static File/Directory Creation", "Enter the name of the file/directory:");
			if (FileName == null)
				return;

			var resu = MessageBox.Show("Is this the name of a directory?", "Directory", MessageBoxButtons.YesNoCancel);
			if (resu == DialogResult.Cancel)
				return;
			var FullFileName = Path.Combine(IndexesToPaths[StaticFileListBox.SelectedIndex], FileName);
			if (resu == DialogResult.Yes)
				FullFileName = Path.Combine(FullFileName, "__TGS3_CP_DIRECTORY_CREATOR__");
			try
			{
				var cf = new ConfigurationFile { Path = FullFileName };
				if (resu == DialogResult.Yes)
					client.Configuration.CreateDirectory(cf, default).GetAwaiter().GetResult();
				else
					client.Configuration.Write(cf, new MemoryStream(), default).GetAwaiter().GetResult();
			}
			catch (ApiException ex)
			{
				MessageBox.Show($"{ex.ErrorCode}: {ex.Message}");
			}

			BuildFileList();
		}

		private void StaticFileSaveButton_Click(object sender, EventArgs e)
		{
			var index = StaticFileListBox.SelectedIndex;
			ConfigurationFile res;
			string error;
			bool unauthorized = false;
			try
			{
				res = client.Configuration.Write(new ConfigurationFile { Path = IndexesToPaths[index], LastReadHash = originalCurrentFileContent?.LastReadHash }, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(StaticFileEditTextbox.Text)), default).GetAwaiter().GetResult();
					originalCurrentFileContent = res;
				unauthorized = false;
				error = null;
			}
			catch (ApiException ex)
			{
				unauthorized = false;
				MessageBox.Show("Error: " + ex.ToString());
				error = "Failed to write file, most likely due to it being too large. The transfer limit is much higher on non-remote connections. " + ex.ToString();
				unauthorized = ex is UnauthorizedException;
			}

			if (error != null)
			{
				MessageBox.Show("Error: " + error);
			}

			var title = (string)StaticFileListBox.Items[index];
			if (unauthorized && !title.Contains(" (UNAUTHORIZED)"))
				StaticFileListBox.Items[index] = title + " (UNAUTHORIZED)";
			UpdateEditText();
		}

		private void StaticFileListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateEditText();
		}

		void UpdateEditText()
		{
			if (enumerating)
				return;
			var newIndex = StaticFileListBox.SelectedIndex;
			if (newIndex == -1)
				return;
			var path = IndexesToPaths[newIndex];
			var title = (string)StaticFileListBox.Items[newIndex];
			var authed_title = title.Replace(" (UNAUTHORIZED)", "").Replace(" (...)", "");
			if (authed_title[authed_title.Length - 1] == '/')
			{
				StaticFileEditTextbox.ReadOnly = true;
				StaticFileEditTextbox.Text = "Directory";
				if (path != null && path != "/")
				{
					EnumeratedPaths.Add(path);
					BuildFileList();
					enumerating = true;
					StaticFileListBox.SelectedIndex = newIndex;
					enumerating = false;
					return;
				}
			}
			else
			{
				ConfigurationFile entry;
				string error;
				string text;
				bool unauthorized;
				try
				{
					var tuple = client.Configuration.Read(new ConfigurationFile { Path = path }, default).GetAwaiter().GetResult();
					entry = tuple.Item1;

					using (var ms = new MemoryStream())
					{
						tuple.Item2.CopyTo(ms);
						text = System.Text.Encoding.UTF8.GetString(ms.ToArray());
					}

					tuple.Item2.Dispose();



					error = null;
					unauthorized = false;
				}
				catch(Exception e)
				{
					entry = null;
					unauthorized = e is UnauthorizedException;
					error = "Failed to read file, most likely due to it being too large. The transfer limit is much higher on non-remote connections. " + e.ToString();
					text = null;
				}

				if (entry == null)
				{
					StaticFileEditTextbox.ReadOnly = true;
					StaticFileEditTextbox.Text = "ERROR: " + error;
					if (unauthorized && !title.Contains(" (UNAUTHORIZED)"))
						StaticFileListBox.Items[newIndex] = title + " (UNAUTHORIZED)";
				}
				else
				{
					StaticFileEditTextbox.ReadOnly = false;
					originalCurrentFileContent = entry;
					StaticFileEditTextbox.Text = text;
				}
			}
		}

		private void RecreateStaticButton_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Deprectated functionality!");
		}
	}
}
