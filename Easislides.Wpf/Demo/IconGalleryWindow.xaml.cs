using System.Collections.Generic;
using System.Windows;
using Easislides.Wpf.Theme;
using Wpf.Ui.Controls;

namespace Easislides.Wpf.Demo;

/// <summary>
/// EsIcons 시각 갤러리 — docs/ui/icon-migration-map.md 시각 검증.
/// 모든 도메인 아이콘 키를 카테고리별로 표시.
/// </summary>
public partial class IconGalleryWindow : Window
{
    public IconGalleryWindow()
    {
        InitializeComponent();

        LiveIcons.ItemsSource = new[]
        {
            Entry("EsIcons.LiveBlack", EsIcons.LiveBlack),
            Entry("EsIcons.LiveHide", EsIcons.LiveHide),
            Entry("EsIcons.LiveHideText", EsIcons.LiveHideText),
            Entry("EsIcons.LiveToggle", EsIcons.LiveToggle),
            Entry("EsIcons.LiveSendToOutput", EsIcons.LiveSendToOutput),
            Entry("EsIcons.LiveMoveToOutput", EsIcons.LiveMoveToOutput),
            Entry("EsIcons.LiveCamera", EsIcons.LiveCamera),
            Entry("EsIcons.LiveCamcorder", EsIcons.LiveCamcorder),
            Entry("EsIcons.LiveSend", EsIcons.LiveSend),
            Entry("EsIcons.LiveNewScreen", EsIcons.LiveNewScreen),
            Entry("EsIcons.LiveHideDisplay", EsIcons.LiveHideDisplay),
            Entry("EsIcons.StatusCheck", EsIcons.StatusCheck),
        };

        ContentIcons.ItemsSource = new[]
        {
            Entry("EsIcons.Bible", EsIcons.Bible),
            Entry("EsIcons.MediaPlay", EsIcons.MediaPlay),
            Entry("EsIcons.Notebook", EsIcons.Notebook),
            Entry("EsIcons.DocumentWord", EsIcons.DocumentWord),
            Entry("EsIcons.PowerPointSlide", EsIcons.PowerPointSlide),
            Entry("EsIcons.DocumentHtml", EsIcons.DocumentHtml),
        };

        FileIcons.ItemsSource = new[]
        {
            Entry("EsIcons.Folder", EsIcons.Folder),
            Entry("EsIcons.FolderOpen", EsIcons.FolderOpen),
            Entry("EsIcons.ActionAdd", EsIcons.ActionAdd),
            Entry("EsIcons.ActionDelete", EsIcons.ActionDelete),
            Entry("EsIcons.ActionDeleteList", EsIcons.ActionDeleteList),
            Entry("EsIcons.ActionClear", EsIcons.ActionClear),
            Entry("EsIcons.ActionEdit", EsIcons.ActionEdit),
            Entry("EsIcons.ActionNewItem", EsIcons.ActionNewItem),
            Entry("EsIcons.ActionCopy", EsIcons.ActionCopy),
            Entry("EsIcons.ActionMove", EsIcons.ActionMove),
            Entry("EsIcons.ActionMoveUp", EsIcons.ActionMoveUp),
            Entry("EsIcons.ActionMoveDown", EsIcons.ActionMoveDown),
            Entry("EsIcons.ActionRefresh", EsIcons.ActionRefresh),
            Entry("EsIcons.ActionFind", EsIcons.ActionFind),
            Entry("EsIcons.ActionSave", EsIcons.ActionSave),
        };

        SettingsIcons.ItemsSource = new[]
        {
            Entry("EsIcons.Settings", EsIcons.Settings),
            Entry("EsIcons.Shortcuts", EsIcons.Shortcuts),
            Entry("EsIcons.Help", EsIcons.Help),
            Entry("EsIcons.Question", EsIcons.Question),
            Entry("EsIcons.Info", EsIcons.Info),
            Entry("EsIcons.Alert", EsIcons.Alert),
            Entry("EsIcons.Template", EsIcons.Template),
            Entry("EsIcons.Contents", EsIcons.Contents),
            Entry("EsIcons.WorshipList", EsIcons.WorshipList),
            Entry("EsIcons.SessionNote", EsIcons.SessionNote),
            Entry("EsIcons.PptList", EsIcons.PptList),
            Entry("EsIcons.PptPreview", EsIcons.PptPreview),
            Entry("EsIcons.MediaFile", EsIcons.MediaFile),
            Entry("EsIcons.NoRotate", EsIcons.NoRotate),
        };

        OtherIcons.ItemsSource = new[]
        {
            Entry("EsIcons.MonitorSingle", EsIcons.MonitorSingle),
            Entry("EsIcons.MonitorDual", EsIcons.MonitorDual),
            Entry("EsIcons.PlaceholderNoImage", EsIcons.PlaceholderNoImage),
            Entry("EsIcons.BibleGo", EsIcons.BibleGo),
            Entry("EsIcons.ExportRtf", EsIcons.ExportRtf),
        };
    }

    private static IconEntry Entry(string key, SymbolRegular symbol) => new(key, symbol);

    /// <summary>갤러리 셀에 표시할 한 아이콘 정보.</summary>
    public sealed record IconEntry(string Key, SymbolRegular Symbol)
    {
        /// <summary>WPF UI Symbol enum 이름 (예: "Book24").</summary>
        public string SymbolName => Symbol.ToString();
    }
}
