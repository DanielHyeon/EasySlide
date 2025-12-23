using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Easislides
{
	public class PopupWindowHelper : NativeWindow
	{
		private const int WM_ACTIVATE = 6;

		private const int WM_ACTIVATEAPP = 28;

		private const int WM_NCACTIVATE = 134;

		private const int KEYEVENTF_KEYUP = 2;

		private EventHandler popClosedHandler = null;

		private PopupWindowHelperMessageFilter filter = null;

		private Form popup = null;

		private Form owner = null;

		private bool popupShowing = false;

		private bool skipClose = false;

		public event PopupClosedEventHandler PopupClosed;

		public event PopupCancelEventHandler PopupCancel;

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern int SendMessage(IntPtr handle, int msg, int wParam, IntPtr lParam);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern int PostMessage(IntPtr handle, int msg, int wParam, IntPtr lParam);

		[DllImport("user32")]
		private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

		public void ShowPopup(Form owner, Form popup, Point location)
		{
			this.owner = owner;
			this.popup = popup;
			Application.AddMessageFilter(filter);
			popup.StartPosition = FormStartPosition.Manual;
			popup.Location = location;
			owner.AddOwnedForm(popup);
			popClosedHandler = popup_Closed;
			popup.Closed += popClosedHandler;
			popupShowing = true;
			popup.Show();
			popup.Activate();
			keybd_event(9, 0, 0, 0);
			keybd_event(9, 0, 2, 0);
			keybd_event(16, 0, 0, 0);
			keybd_event(9, 0, 0, 0);
			keybd_event(9, 0, 2, 0);
			keybd_event(16, 0, 2, 0);
			filter.Popup = popup;
		}

		private void popup_Closed(object sender, EventArgs e)
		{
			ClosePopup();
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (!popupShowing)
			{
				return;
			}
			if (m.Msg == 134)
			{
				if ((int)m.WParam == 0)
				{
					SendMessage(base.Handle, 134, 1, IntPtr.Zero);
				}
			}
			else if (m.Msg == 28 && (int)m.WParam == 0)
			{
				ClosePopup();
				PostMessage(base.Handle, 134, 0, IntPtr.Zero);
			}
		}

		public void ClosePopup()
		{
			if (popupShowing)
			{
				if (!skipClose)
				{
					OnPopupClosed(new PopupClosedEventArgs(popup));
				}
				skipClose = false;
				owner.RemoveOwnedForm(popup);
				popupShowing = false;
				popup.Closed -= popClosedHandler;
				popClosedHandler = null;
				popup.Close();
				Application.RemoveMessageFilter(filter);
				owner.Activate();
				popup = null;
				owner = null;
			}
		}

		protected virtual void OnPopupClosed(PopupClosedEventArgs e)
		{
			if (this.PopupClosed != null)
			{
				this.PopupClosed(this, e);
			}
		}

		private void popup_Cancel(object sender, PopupCancelEventArgs e)
		{
			OnPopupCancel(e);
		}

		protected virtual void OnPopupCancel(PopupCancelEventArgs e)
		{
			if (this.PopupCancel != null)
			{
				this.PopupCancel(this, e);
				if (!e.Cancel)
				{
					skipClose = true;
				}
			}
		}

		public PopupWindowHelper()
		{
			filter = new PopupWindowHelperMessageFilter(this);
			filter.PopupCancel += popup_Cancel;
		}
	}
}
