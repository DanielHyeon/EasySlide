using System;

namespace Easislides.Wpf.Shell;

/// <summary>메인 창의 크기·위치·최대화 상태(저장/복원용 순수 값). View 와 설정 사이를 잇는다.</summary>
public readonly record struct WindowPlacement(double Left, double Top, double Width, double Height, bool Maximized);

/// <summary>
/// 저장된 창 배치를 화면(가상 데스크톱) 안으로 보정하는 순수 계산기(레거시 FrmMain 창 상태 저장 대응).
/// View(MainWindow) 가 이 결과로 Left/Top/Width/Height·WindowState 를 설정한다 — 여기엔 WPF 의존이 없어 단위 테스트가 쉽다.
/// </summary>
public static class WindowPlacementCalculator
{
    /// <summary>
    /// 저장된 창 배치를 화면 안으로 보정해 돌려준다. 저장된 적이 없으면(폭/높이 ≤ 0) <c>null</c>(기본 크기·중앙 사용).
    /// 모니터 분리·해상도 변경으로 저장 위치가 화면 밖이 돼도, 창 전체가 보이도록 위치·크기를 화면 안으로 끌어온다.
    /// </summary>
    /// <param name="savedLeft">저장된 왼쪽 좌표(px).</param>
    /// <param name="savedTop">저장된 위쪽 좌표(px).</param>
    /// <param name="savedWidth">저장된 너비(px). 0 이하면 "저장된 적 없음".</param>
    /// <param name="savedHeight">저장된 높이(px). 0 이하면 "저장된 적 없음".</param>
    /// <param name="maximized">저장 당시 최대화 상태였는지.</param>
    /// <param name="screenLeft">가상 데스크톱 왼쪽(SystemParameters.VirtualScreenLeft).</param>
    /// <param name="screenTop">가상 데스크톱 위쪽(SystemParameters.VirtualScreenTop).</param>
    /// <param name="screenWidth">가상 데스크톱 너비.</param>
    /// <param name="screenHeight">가상 데스크톱 높이.</param>
    /// <param name="minWidth">창 최소 너비(이보다 작게 복원하지 않음).</param>
    /// <param name="minHeight">창 최소 높이.</param>
    public static WindowPlacement? ComputeRestore(
        int savedLeft,
        int savedTop,
        int savedWidth,
        int savedHeight,
        bool maximized,
        double screenLeft,
        double screenTop,
        double screenWidth,
        double screenHeight,
        double minWidth,
        double minHeight)
    {
        // 저장된 적이 없으면(또는 손상된 0/음수 크기) 복원하지 않는다 — 호출 측이 기본 크기·중앙을 쓰게 한다.
        if (savedWidth <= 0 || savedHeight <= 0)
        {
            return null;
        }

        // 화면이 비정상(0 이하)이면 보정 기준이 없으므로 복원 포기(안전).
        if (screenWidth <= 0 || screenHeight <= 0)
        {
            return null;
        }

        var screenRight = screenLeft + screenWidth;
        var screenBottom = screenTop + screenHeight;

        // 크기: 최소 크기 이상, 화면 크기 이하로 클램프(화면보다 큰 창이 복원돼 잘리지 않도록).
        var maxWidth = Math.Max(minWidth, screenWidth);
        var maxHeight = Math.Max(minHeight, screenHeight);
        var width = Math.Clamp((double)savedWidth, minWidth, maxWidth);
        var height = Math.Clamp((double)savedHeight, minHeight, maxHeight);

        // 위치: 창 전체가 화면 안에 들어오도록 보정. 오른쪽/아래 한계는 (화면 끝 − 창 크기).
        var maxLeft = Math.Max(screenLeft, screenRight - width);
        var maxTop = Math.Max(screenTop, screenBottom - height);
        var left = Math.Clamp((double)savedLeft, screenLeft, maxLeft);
        var top = Math.Clamp((double)savedTop, screenTop, maxTop);

        return new WindowPlacement(left, top, width, height, maximized);
    }
}
