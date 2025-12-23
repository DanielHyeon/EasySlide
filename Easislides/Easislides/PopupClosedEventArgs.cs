using System;
using System.Windows.Forms;

namespace Easislides
{
	public class PopupClosedEventArgs : EventArgs
	{
		private Form popup = null;

		public Form Popup => popup;

		public PopupClosedEventArgs(Form popup)
		{
			this.popup = popup;
		}
	}
}
