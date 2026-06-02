using System.Windows.Input;

namespace Easislides.Wpf.Input;

/// <summary>
/// 키 → 절(섹션) 라벨 매핑(레거시 FrmMain PreviewBtnVerse_Click 1~9·c·t·b·w·e·p·q).
/// 운영자가 라이브 중 숫자/문자 키로 곡의 해당 절·후렴 라벨로 즉시 점프하게 한다.
/// 순수 함수라 단위 테스트가 쉽고, 실제 점프 여부는 호출 측이 JumpToLyricsSectionCommand.CanExecute 로 판단한다
/// (라벨이 현재 곡에 없으면 무시되므로, 매핑은 "후보 라벨 문자"만 돌려주면 된다).
/// </summary>
public static class VerseJumpKeyMap
{
    /// <summary>
    /// 키 하나를 절 라벨 문자열로 매핑한다. 숫자 키(상단/넘패드 1~9)는 "1".."9", 약속된 문자 키는 소문자 라벨,
    /// 그 외는 <c>null</c>(점프 대상 아님). 라벨 비교는 대소문자 무시이므로 소문자로 돌려준다.
    /// </summary>
    public static string? MapKeyToLabel(Key key) => key switch
    {
        // 숫자(절 번호) — 상단 숫자열과 넘패드 모두.
        Key.D1 or Key.NumPad1 => "1",
        Key.D2 or Key.NumPad2 => "2",
        Key.D3 or Key.NumPad3 => "3",
        Key.D4 or Key.NumPad4 => "4",
        Key.D5 or Key.NumPad5 => "5",
        Key.D6 or Key.NumPad6 => "6",
        Key.D7 or Key.NumPad7 => "7",
        Key.D8 or Key.NumPad8 => "8",
        Key.D9 or Key.NumPad9 => "9",
        // 약속된 섹션 문자(레거시 PreviewBtnVerse): c=후렴, b=브릿지, p=전주, q=간주, t=탁(태그), w/e 등.
        Key.C => "c",
        Key.T => "t",
        Key.B => "b",
        Key.W => "w",
        Key.E => "e",
        Key.P => "p",
        Key.Q => "q",
        _ => null,
    };
}
