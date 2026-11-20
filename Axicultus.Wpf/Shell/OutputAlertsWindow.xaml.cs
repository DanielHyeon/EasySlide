using System;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Controls;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Axicultus.Wpf.Shell;

public partial class OutputAlertsWindow : Window
{
    private readonly ILiveSessionService _session;

    private readonly string _historyDirectory;

    private readonly ISettingsService _settings;

    public OutputAlertsWindow(MainViewModel model, ISettingsService settings)
    {
        InitializeComponent();
        _settings = settings;
        var options = settings.Current.General;
        var workingFolder = options.WorkingFolder;
        Prefix.Text = options.ParentalAlertHeading;
        MessageFlash.IsChecked = options.MessageAlertFlash;
        MessageScroll.IsChecked = options.MessageAlertScroll;
        MessageTransparent.IsChecked = options.MessageAlertTransparent;
        ParentalFlash.IsChecked = options.ParentalAlertFlash;
        ParentalScroll.IsChecked = options.ParentalAlertScroll;
        ParentalTransparent.IsChecked = options.ParentalAlertTransparent;
        DataContext = model;
        _session = model.Session;
        _historyDirectory = string.IsNullOrWhiteSpace(workingFolder) ? "" : Path.Combine(workingFolder, "Admin", "Database");
        foreach (string entry in AlertHistory.Read(_historyDirectory, false)) Message.Items.Add(entry);
        foreach (string entry in AlertHistory.Read(_historyDirectory, true)) Parental.Items.Add(entry);
    }

    private void Message_Click(object sender, RoutedEventArgs e)
        => ShowAlert(Message, "", false, MessageFlash, MessageScroll, MessageTransparent);

    private void Parental_Click(object sender, RoutedEventArgs e)
        => ShowAlert(Parental, Prefix.Text.Trim(), true, ParentalFlash, ParentalScroll, ParentalTransparent);

    private void ShowAlert(ComboBox input, string prefix, bool parental, CheckBox flash, CheckBox scroll, CheckBox transparent)
    {
        string text = input.Text.Trim();
        if (text.Length == 0 || _session.Current.State != LiveState.Active) return;
        input.Items.Remove(text);
        input.Items.Insert(0, text);
        while (input.Items.Count > AlertHistory.Limit) input.Items.RemoveAt(input.Items.Count - 1);
        input.Text = text;
        SaveHistory(input, parental);
        var banner = new OutputAlert((prefix + " " + text).Trim(), flash.IsChecked == true, scroll.IsChecked == true, transparent.IsChecked == true,
            DateTimeOffset.UtcNow.AddSeconds(parental ? _settings.Current.General.ParentalAlertDuration : _settings.Current.General.MessageAlertDuration));
        _session.SetAudienceAlert(parental, banner);
    }

    private void ClearMessageHistory_Click(object sender, RoutedEventArgs e) => ClearHistory(Message, false);
    private void ClearParentalHistory_Click(object sender, RoutedEventArgs e) => ClearHistory(Parental, true);

    private void ClearHistory(ComboBox input, bool parental)
    {
        input.Items.Clear();
        input.Text = "";
        SaveHistory(input, parental);
    }

    private void SaveHistory(ComboBox input, bool parental)
    {
        if (!AlertHistory.Save(_historyDirectory, parental, input.Items.OfType<string>()))
            ((MainViewModel)DataContext).StatusText = "송출 알림 기록을 저장할 수 없습니다.";
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _session.SetAudienceAlert(false, null);
        _session.SetAudienceAlert(true, null);
    }
}
