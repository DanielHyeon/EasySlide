using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Input;

public sealed record CommandDescriptor(
    string Id,
    string Category,
    string DisplayName,
    string Description,
    bool IsDangerous,
    IReadOnlyList<Shortcut> DefaultShortcuts);

public interface ICommandCatalog
{
    IReadOnlyList<CommandDescriptor> All { get; }

    CommandDescriptor? FindById(string commandId);
    IReadOnlyList<Shortcut> GetDefaultShortcuts();
}

public sealed class CommandCatalog : ICommandCatalog
{
    private static readonly IReadOnlyList<CommandDescriptor> DefaultCommands = new[]
    {
        Command(
            MainCommandIds.OutputOpen,
            "Output",
            "출력 창 열기",
            "선택한 모니터에 WPF 출력 창을 엽니다.",
            isDangerous: false),
        Command(
            MainCommandIds.OutputClose,
            "Output",
            "출력 창 닫기",
            "출력 창을 닫습니다. 라이브 중이면 확인이 필요합니다.",
            isDangerous: true),
        Command(
            MainCommandIds.LiveGo,
            "Live",
            "Go Live",
            "선택한 항목을 라이브 출력으로 전환합니다.",
            isDangerous: true,
            new Shortcut(Key.L, ModifierKeys.Control, MainCommandIds.LiveGo, IsGlobal: false, "Go Live")),
        Command(
            MainCommandIds.LiveStop,
            "Live",
            "라이브 중지",
            "현재 라이브 송출을 중지합니다.",
            isDangerous: true),
        Command(
            MainCommandIds.LiveNext,
            "Live",
            "다음 항목",
            "다음 큐 항목으로 이동합니다.",
            isDangerous: false,
            new Shortcut(Key.Space, ModifierKeys.None, MainCommandIds.LiveNext, IsGlobal: false, "다음 항목"),
            new Shortcut(Key.F5, ModifierKeys.None, MainCommandIds.LiveNext, IsGlobal: true, "다음 항목")),
        Command(
            MainCommandIds.LivePrevious,
            "Live",
            "이전 항목",
            "이전 큐 항목으로 이동합니다.",
            isDangerous: false,
            new Shortcut(Key.Space, ModifierKeys.Shift, MainCommandIds.LivePrevious, IsGlobal: false, "이전 항목"),
            new Shortcut(Key.F4, ModifierKeys.None, MainCommandIds.LivePrevious, IsGlobal: true, "이전 항목")),
        Command(
            MainCommandIds.LiveHide,
            "Live",
            "출력 숨김",
            "현재 라이브 출력을 숨김 상태로 전환합니다.",
            isDangerous: true,
            new Shortcut(Key.H, ModifierKeys.Control, MainCommandIds.LiveHide, IsGlobal: false, "출력 숨김")),
        Command(
            MainCommandIds.LiveBlack,
            "Live",
            "검은 화면",
            "현재 라이브 출력을 검은 화면으로 전환합니다.",
            isDangerous: true,
            new Shortcut(Key.B, ModifierKeys.Control, MainCommandIds.LiveBlack, IsGlobal: false, "검은 화면")),
    };

    public CommandCatalog()
        : this(DefaultCommands)
    {
    }

    internal CommandCatalog(IEnumerable<CommandDescriptor> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        All = commands.ToList().AsReadOnly();
    }

    public IReadOnlyList<CommandDescriptor> All { get; }

    public CommandDescriptor? FindById(string commandId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        return All.FirstOrDefault(command => string.Equals(command.Id, commandId, StringComparison.Ordinal));
    }

    public IReadOnlyList<Shortcut> GetDefaultShortcuts()
        => All.SelectMany(command => command.DefaultShortcuts).ToList().AsReadOnly();

    private static CommandDescriptor Command(
        string id,
        string category,
        string displayName,
        string description,
        bool isDangerous,
        params Shortcut[] defaultShortcuts)
        => new(id, category, displayName, description, isDangerous, Array.AsReadOnly(defaultShortcuts.ToArray()));
}
