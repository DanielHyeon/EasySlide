using System;
using System.Windows.Forms;

namespace Axicultus;

	public class PopupClosedEventArgs(Form popup) : EventArgs
	{
		private Form popup = popup;

		public Form Popup => popup;
}
