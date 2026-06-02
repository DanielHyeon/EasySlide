using System.Collections.Generic;
using System.Linq;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 출력 글꼴 콤보에 보여줄 글꼴 이름 목록을 만든다(순수 계산 — 시스템 글꼴 열거는 View 가 넘겨준다).
/// <para>
/// 규칙: <b>자주 쓰는 추천 글꼴(favorites)을 맨 앞</b>에 그 순서대로 두고(운영자가 한/영 대표 글꼴을 바로 찾게),
/// 그 뒤에 <b>설치된 나머지 글꼴</b>을 가나다·ABC 순으로 붙인다. 대소문자 무시로 중복을 없애고, 빈 이름은 버린다.
/// 설치 글꼴 열거가 비면(헤드리스·열거 실패) 추천 목록만 돌려준다(무회귀).
/// </para>
/// <para>
/// 시스템 글꼴 열거(<c>Fonts.SystemFontFamilies</c>)는 환경마다 달라 단위 테스트가 흔들리므로, 이 순수 함수는
/// 설치 목록을 인자로 받아 결정적으로 동작한다(테스트는 가짜 목록 주입). View 가 실제 설치 글꼴을 넘긴다.
/// </para>
/// </summary>
public static class SystemFontCatalog
{
    /// <summary>
    /// 기본 추천 글꼴(맨 앞에 그 순서대로) — 한/영 대표 글꼴을 운영자가 바로 찾게 한다.
    /// 출력 가사 글꼴 콤보·공지(InfoScreen) 글꼴 콤보가 함께 쓴다(중복 정의 방지).
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultFavorites =
    [
        "맑은 고딕",
        "Malgun Gothic",
        "나눔고딕",
        "본고딕",
        "Noto Sans CJK KR",
        "굴림",
        "바탕",
        "Segoe UI",
        "Arial",
        "Calibri",
    ];

    /// <summary>
    /// 추천 글꼴(앞, 순서 유지) + 설치된 나머지 글꼴(정렬)을 합쳐 콤보용 목록을 만든다.
    /// </summary>
    /// <param name="installedFamilies">설치된 글꼴 이름들(순서 무관, 널 허용). 보통 <c>Fonts.SystemFontFamilies</c> 의 이름.</param>
    /// <param name="favorites">맨 앞에 둘 추천 글꼴(순서 유지). 설치 여부와 무관하게 제안으로 남긴다(편집 콤보라 직접 입력도 됨).</param>
    public static IReadOnlyList<string> BuildFontFamilyList(
        IEnumerable<string>? installedFamilies,
        IReadOnlyList<string> favorites)
    {
        var result = new List<string>();
        // 대소문자 무시 중복 제거용 집합 — "Malgun Gothic" 이 추천·설치 양쪽에 있어도 한 번만.
        var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        // 1) 추천 글꼴 먼저(순서 유지). 빈 이름은 건너뛴다.
        foreach (var name in favorites ?? System.Array.Empty<string>())
        {
            var trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length > 0 && seen.Add(trimmed))
            {
                result.Add(trimmed);
            }
        }

        // 2) 설치된 나머지 글꼴을 가나다·ABC 순으로 붙인다(추천에 이미 있는 건 제외).
        // 중복 제거는 오직 seen(대소문자 무시) 한 곳에서만 — 정렬(문화권)과 다른 잣대로 두 번 거르지 않아
        // "거의 같은 이름"이 두 줄로 새는 일이 없다. 빈 이름은 버리고 앞뒤 공백은 다듬는다.
        if (installedFamilies is not null)
        {
            var rest = installedFamilies
                .Select(name => (name ?? string.Empty).Trim())
                .Where(name => name.Length > 0)
                .OrderBy(name => name, System.StringComparer.CurrentCulture);

            foreach (var name in rest)
            {
                if (seen.Add(name)) // 추천에 이미 있거나(1단계) 설치 목록에 또 나온 같은 이름이면 건너뛴다.
                {
                    result.Add(name);
                }
            }
        }

        return result;
    }
}
