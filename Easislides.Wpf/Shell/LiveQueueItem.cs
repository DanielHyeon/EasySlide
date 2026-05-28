namespace Easislides.Wpf.Shell;

public sealed record LiveQueueItem(string Id, string Title, string Kind = "Item")
{
    public override string ToString() => Title;
}
