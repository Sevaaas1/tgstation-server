using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TGS.ControlPanel
{
	partial class ControlPanel
	{
		string lastReadError = null;
		void InitBYONDPage()
		{
			UpdateBYONDButtons();
		}
		private async void UpdateButton_Click(object sender, EventArgs e)
		{
			UpdateBYONDButtons();

			var error = await WrapServerOp(() => client.Byond.SetActiveVersion(new Tgstation.Server.Api.Models.Byond { Version = new Version((int)MajorVersionNumeric.Value, (int)MinorVersionNumeric.Value) }, null, default));
			if (error != null)
				MessageBox.Show(error);
		}
		
		private void BYONDRefreshButton_Click(object sender, EventArgs e)
		{
			UpdateBYONDButtons();
		}

		void UpdateBYONDButtons()
		{
			var BYOND = client.Byond.ActiveVersion(default).GetAwaiter().GetResult();
			var CV = BYOND.Version;

			VersionLabel.Text = CV.ToString();

			StagedVersionTitle.Visible = false;
		}
	}
}
