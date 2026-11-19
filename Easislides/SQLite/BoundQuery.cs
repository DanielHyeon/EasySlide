namespace Easislides.SQLite;

/// <summary>SQL and the values that must accompany it when replaying a saved search.</summary>
internal sealed record BoundQuery(string Text, params (string Name, object Value)[] Parameters);
