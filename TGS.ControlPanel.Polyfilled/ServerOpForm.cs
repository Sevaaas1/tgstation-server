using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tgstation.Server.Client;

namespace TGS.ControlPanel
{
	/// <summary>
	/// Used to provide an ATP function for calls into an <see cref="TGS.Interface.IClient"/>
	/// </summary>
#if !DEBUG
	abstract
#endif
	class ServerOpForm : Form
	{
		/// <summary>
		/// Used to wrap <see cref="TGS.Interface.IClient"/> calls in a non-blocking fashion while disabling the <see cref="Form"/> and enabling the wait cursor
		/// </summary>
		/// <param name="action">The <see cref="TGS.Interface.IClient"/> operation to wrap</param>
		/// <returns>A <see cref="Task"/> wrapping <paramref name="action"/></returns>
		protected async Task<string> WrapServerOp(Func<Task> action)
		{
			Enabled = false;
			UseWaitCursor = true;
			try
			{
				await action();
				return null;
			}
			catch(ApiException ex)
			{
				return $"{ex.ErrorCode}: {ex.Message}";
			}
			catch(Exception ex)
			{
				return ex.ToString();
			}
			finally
			{
				Enabled = true;
				UseWaitCursor = false;
			}
		}
	}
}
