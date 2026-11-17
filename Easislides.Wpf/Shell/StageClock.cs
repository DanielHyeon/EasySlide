using System;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 스테이지(Preview) 모니터 시계 표시 형식 — 워십 리더가 시간을 한눈에 보도록 24시간 HH:mm 로 보여 준다.
/// 순수 함수(주어진 시각을 포맷만)라 테스트하기 쉽다. 실제 "지금 시각"은 창 코드비하인드의 타이머가 넣어 준다.
/// </summary>
public static class StageClock
{
    /// <summary>주어진 시각을 스테이지 시계 문구("HH:mm", 예: 14:05)로 만든다.</summary>
    public static string Format(DateTime now) => now.ToString("HH:mm");
}
