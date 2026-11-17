using System;
using System.Collections.Generic;

namespace Easislides.Wpf.Shell;

public sealed record CommandTelemetryEntry(
    DateTimeOffset Timestamp,
    string CommandName,
    bool Succeeded,
    string Detail);

public interface ICommandTelemetry
{
    IReadOnlyList<CommandTelemetryEntry> Entries { get; }

    void Record(string commandName, bool succeeded, string detail = "");
}

public sealed class InMemoryCommandTelemetry : ICommandTelemetry
{
    private readonly List<CommandTelemetryEntry> _entries = new();

    public IReadOnlyList<CommandTelemetryEntry> Entries => _entries;

    public void Record(string commandName, bool succeeded, string detail = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        _entries.Add(new CommandTelemetryEntry(
            DateTimeOffset.Now,
            commandName,
            succeeded,
            detail));
    }
}
