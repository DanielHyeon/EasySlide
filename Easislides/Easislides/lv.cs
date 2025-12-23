using System.Windows.Forms;

namespace Easislides
{
	internal class lv
	{
		public static void Sort(ref ListView InListView, int SortColumn)
		{
			Sort(ref InListView, ref SortColumn, SortColumn, FlipSort: false);
		}

		public static void Sort(ref ListView InListView, ref int CurSortColumn, int NewSortColumn, bool FlipSort)
		{
			if (InListView.Columns.Count == 0)
			{
				return;
			}
			if ((NewSortColumn < 0) | (NewSortColumn > InListView.Columns.Count))
			{
				NewSortColumn = 0;
			}
			if (NewSortColumn != CurSortColumn)
			{
				CurSortColumn = NewSortColumn;
				InListView.Sorting = SortOrder.Ascending;
			}
			else if (FlipSort)
			{
				if (InListView.Sorting == SortOrder.Ascending)
				{
					InListView.Sorting = SortOrder.Descending;
				}
				else
				{
					InListView.Sorting = SortOrder.Ascending;
				}
			}
			InListView.ListViewItemSorter = new ListViewItemComparer(NewSortColumn, InListView.Sorting);
		}
	}
}
