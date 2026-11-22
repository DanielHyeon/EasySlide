using Xunit;

namespace Axicultus.Tests;

public class WorshipListRemovalTests
{
    // outputItemNo is OutputItem.CurItemNo (1-based, 0 = not a worship-list row); removed rows are 0-based.
    [Theory]
    [InlineData(2, new[] { 1 }, true)]      // the single removed row is the output row
    [InlineData(4, new[] { 3, 1 }, true)]   // the output row is removed together with a lower row
    [InlineData(3, new[] { 3, 1 }, false)]  // the output row stays
    [InlineData(0, new int[0], false)]      // nothing selected, output item from outside the list
    [InlineData(0, new[] { 0 }, false)]     // a row removed while the output item is not from the list
    public void OnlyARemovedOutputRowSendsThePreviewToOutput(int outputItemNo, int[] removedRows, bool expected)
    {
        Assert.Equal(expected, FrmMain.RemovedRowsIncludeOutput(outputItemNo, removedRows));
    }

    // "Previous" from the first slide (or from the gap item) opens the item before it at FrmMain.LastSlide: the
    // loaded item's own last slide, as FrmLaunchShow does, not the slide count of the item that was left.
    [Theory]
    [InlineData(FrmMain.LastSlide, 6, 6)]   // the previous item has more slides than the item left behind
    [InlineData(FrmMain.LastSlide, 1, 1)]
    [InlineData(FrmMain.LastSlide, 0, 1)]   // an item without slides: never left at LastSlide
    [InlineData(3, 6, 3)]
    [InlineData(9, 6, 6)]
    [InlineData(0, 6, 1)]
    [InlineData(2, 0, 2)]                   // the slide count is not known yet: an ordinary slide number is kept
    public void AnItemOpensOnASlideItHas(int startingSlide, int totalSlides, int expected)
    {
        Assert.Equal(expected, FrmMain.SlideToOpen(startingSlide, totalSlides));
        Assert.Equal(30000, FrmMain.LastSlide);
    }

    // Clear moves the session's file to Trash and leaves the (empty) session selected. A list restored under that
    // name must be loaded: FrmMain saves after every change, so the next change would overwrite the restored file.
    [Theory]
    [InlineData(false, true, true)]    // cleared session, its list restored in the list manager
    [InlineData(true, true, false)]    // the session's file was there all along: the list on screen is that file
    [InlineData(false, false, false)]  // still cleared
    [InlineData(true, false, false)]   // the session's list was deleted in the list manager
    public void AListRestoredUnderTheNameOfTheClearedSessionIsLoaded(bool existedBefore, bool existsNow, bool expected)
    {
        Assert.Equal(expected, FrmMain.SessionListWasRestored(existedBefore, existsNow));
    }
}
