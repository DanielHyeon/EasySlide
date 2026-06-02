using Easislides.Wpf.Shell;
using Wpf.Ui.Controls;

namespace Easislides.Wpf.Theme;

/// <summary>
/// 예배 순서 항목의 종류(곡/성경/PPT/미디어/공지)에 맞는 Fluent 아이콘을 고른다 — 목록에서 한눈에 종류를 구분하게(현대 UI 업그레이드).
/// 순수 함수라 IO·UI 없이 단위 테스트가 쉽다. 실제 바인딩은 <c>KindToSymbolConverter</c> 가 이 함수에 위임한다.
/// </summary>
public static class WorshipItemIcon
{
    /// <summary>
    /// 항목 종류 문자열(<see cref="LiveItemKinds"/>)을 Fluent <see cref="SymbolRegular"/> 아이콘으로 매핑한다.
    /// 모르는/빈 종류는 일반 문서 아이콘(<see cref="EsIcons.GenericItem"/>)으로 폴백한다(빈 화면 방지).
    /// </summary>
    public static SymbolRegular SymbolForKind(string? kind) => kind switch
    {
        LiveItemKinds.Song => EsIcons.Notebook,        // 찬양 가사 — 노트북 아이콘
        LiveItemKinds.Bible => EsIcons.Bible,          // 성경 — 책 아이콘
        LiveItemKinds.PowerPoint => EsIcons.PowerPointSlide, // PPT — 슬라이드 아이콘
        LiveItemKinds.Media => EsIcons.MediaFile,      // 미디어 — 비디오 아이콘
        LiveItemKinds.Notice => EsIcons.Notice,        // 공지(텍스트) — 말풍선 아이콘
        _ => EsIcons.GenericItem,                      // 그 외(Item 등) — 일반 문서 아이콘
    };
}
