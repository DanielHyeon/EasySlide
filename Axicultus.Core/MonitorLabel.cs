namespace Axicultus.Core;

public static class MonitorLabel
{
    public static string Connected(string deviceId, bool isPrimary, int width, int height)
    {
        string role = isPrimary ? "주 모니터" : "출력 모니터";
        return $"{role} {deviceId} ({width}x{height})";
    }

    public static string Disconnected(string deviceId) => $"{deviceId} (연결 해제)";
}
