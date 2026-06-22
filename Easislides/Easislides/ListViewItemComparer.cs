using System;
using System.Collections;
using System.Windows.Forms;

namespace Easislides
{
	internal class ListViewItemComparer : IComparer
	{
		private int col;

		private SortOrder order;

		public ListViewItemComparer()
		{
			col = 0;
			order = SortOrder.Ascending;
		}

		public ListViewItemComparer(int column, SortOrder order)
		{
			col = column;
			this.order = order;
		}

		public int Compare(object x, object y)
		{
			int num;
			try
			{
				DateTime t = DateTime.Parse(((ListViewItem)x).SubItems[col].Text);
				DateTime t2 = DateTime.Parse(((ListViewItem)y).SubItems[col].Text);
				num = DateTime.Compare(t, t2);
			}
			catch
			{
				num = string.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
			}
			if (order == SortOrder.Descending)
			{
				num *= -1;
			}
			return num;
		}
	}
}
