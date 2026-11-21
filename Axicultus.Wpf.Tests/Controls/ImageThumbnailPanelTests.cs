using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using Axicultus.Wpf.Controls;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Controls;

public class ImageThumbnailPanelTests
{
    [Fact]
    public void LongGalleryRealizesTheViewportAndCanScrollToTheLastImage()
    {
        StaHelper.RunOnSta(() =>
        {
            var list = new ListBox
            {
                ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(ImageThumbnailPanel))),
                ItemsSource = Enumerable.Range(0, 300).ToArray(),
            };
            ScrollViewer.SetCanContentScroll(list, true);
            ScrollViewer.SetHorizontalScrollBarVisibility(list, ScrollBarVisibility.Disabled);
            VirtualizingPanel.SetVirtualizationMode(list, VirtualizationMode.Recycling);

            using var source = new HwndSource(new HwndSourceParameters("Gallery layout")
            {
                Width = 460,
                Height = 320,
                WindowStyle = unchecked((int)0x80000000), // Hidden popup supplies a presentation source for layout.
            });
            source.RootVisual = list;
            list.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            var viewport = new Size(460, 320);
            list.Measure(viewport);
            list.Arrange(new Rect(viewport));
            list.UpdateLayout();
            list.ItemContainerGenerator.ContainerFromIndex(0).Should().NotBeNull();
            list.ItemContainerGenerator.ContainerFromIndex(150).Should().BeNull();

            list.SelectedIndex = 299;
            list.ScrollIntoView(list.SelectedItem);
            list.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            list.UpdateLayout();
            list.ItemContainerGenerator.ContainerFromIndex(299).Should().NotBeNull();
            list.ItemContainerGenerator.ContainerFromIndex(150).Should().BeNull();
        });
    }
}
