namespace Easislides.Wpf.Shell;

/// <summary>
/// 예배 순서 한 항목이 "지금 라이브 송출 중"인지 판단하는 순수 함수 — 목록의 LIVE 표시에 쓴다.
/// 항목 Id 가 현재 라이브 항목 Id(<see cref="MainViewModel.LiveItemId"/>)와 같을 때만 라이브.
/// 둘 다 비었으면(라이브 미시작) 라이브 아님 — 빈 값끼리 우연히 같다고 표시되지 않게 가드한다.
/// </summary>
public static class WorshipLiveState
{
    /// <summary><paramref name="itemId"/> 가 <paramref name="liveItemId"/> 와 같고 비어 있지 않으면 true(이 항목이 라이브).</summary>
    public static bool IsLive(string? itemId, string? liveItemId)
        => !string.IsNullOrEmpty(itemId) && string.Equals(itemId, liveItemId, System.StringComparison.Ordinal);
}
