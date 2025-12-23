using System;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class PopupCancelEventArgs : EventArgs
	{
		private bool cancel = false;

		private Point location;

		private Form popup = null;

		public Form Popup => popup;

		public Point CursorLocation => location;

		public bool Cancel
		{
			get
			{
				return cancel;
			}
			set
			{
				cancel = value;
			}
		}

		public PopupCancelEventArgs(Form popup, Point location)
		{
			this.popup = popup;
			this.location = location;
			cancel = false;
		}
	}
}
