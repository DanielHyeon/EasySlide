using System;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

	public class PopupCancelEventArgs(Form popup, Point location) : EventArgs
	{
		private bool cancel = false;

		private Point location = location;

		private Form popup = popup;

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
}
