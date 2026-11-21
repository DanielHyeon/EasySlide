using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Captions;

internal sealed class QueueRecoveryJournal : IDisposable
{
    private readonly MainViewModel _main;
    private readonly CaptionRecoveryStore _store;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(600) };

    public QueueRecoveryJournal(MainViewModel main, CaptionRecoveryStore store)
    {
        _main = main;
        _store = store;
        _timer.Tick += TimerTick;
        _main.Queue.CollectionChanged += QueueChanged;
        _main.QueueReplacing += Flush;
    }

    private void QueueChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _timer.Stop();
        _timer.Start();
    }

    private void TimerTick(object? sender, EventArgs e) => Flush();

    private async void Flush()
    {
        if (!_timer.IsEnabled) return;
        _timer.Stop();
        if (!_main.WorshipListHasUnsavedChanges) return;
        var plan = _main.CurrentWorshipListName;
        var items = _main.Queue.Select(CaptionRecoveryStore.Capture).ToArray();
        var selected = _main.SelectedItem == null ? -1 : _main.Queue.IndexOf(_main.SelectedItem);
        try { await _store.SaveQueueAsync(plan, items, selected); }
        catch (Exception ex) { _main.StatusText = "예배 순서 복구본 저장 실패 · " + ex.Message; }
    }

    public void Dispose()
    {
        Flush();
        _timer.Tick -= TimerTick;
        _main.Queue.CollectionChanged -= QueueChanged;
        _main.QueueReplacing -= Flush;
    }
}
