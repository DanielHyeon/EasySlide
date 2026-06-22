using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class PopupWindowHelperMessageFilter : IMessageFilter
	{
		private const int WM_LBUTTONDOWN = 513;

		private const int WM_RBUTTONDOWN = 516;

		private const int WM_MBUTTONDOWN = 519;

		private const int WM_NCLBUTTONDOWN = 161;

		private const int WM_NCRBUTTONDOWN = 164;

		private const int WM_NCMBUTTONDOWN = 167;

		private Form popup = null;

		private PopupWindowHelper owner = null;

		public Form Popup
		{
			get
			{
				return popup;
			}
			set
			{
				popup = value;
			}
		}

		public event PopupCancelEventHandler PopupCancel;

		public PopupWindowHelperMessageFilter(PopupWindowHelper owner)
		{
			this.owner = owner;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (popup != null)
			{
				switch (m.Msg)
				{
				case 161:
				case 164:
				case 167:
				case 513:
				case 516:
				case 519:
					OnMouseDown();
					break;
				}
			}
			return false;
		}

		private void OnMouseDown()
		{
			Point position = Cursor.Position;
			if (!popup.Bounds.Contains(position))
			{
				OnCancelPopup(new PopupCancelEventArgs(popup, position));
			}
		}

		protected virtual void OnCancelPopup(PopupCancelEventArgs e)
		{
			if (this.PopupCancel != null)
			{
				this.PopupCancel(this, e);
			}
			if (!e.Cancel)
			{
				owner.ClosePopup();
				popup = null;
			}
		}
	}
}
