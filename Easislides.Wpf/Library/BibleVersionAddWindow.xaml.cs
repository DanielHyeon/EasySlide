using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Easislides.Wpf.Library;

/// <summary>
/// 성경 버전 "추가" 다이얼로그 — 추가 후보(숨김 복구 + HolyBibles 신규 파일)에서 하나를 고르고 표시 이름을 입력받는다.
/// 확인 시 호출자는 <see cref="SelectedFileName"/>·<see cref="EnteredName"/> 로 결과를 읽어 BibleViewModel.AddVersion 에 넘긴다.
/// 빈 이름·중복 이름(보이는 버전과 대소문자 무시)은 인라인으로 막고, 최종 검증은 VM 이 한 번 더 한다(이중 방어).
/// </summary>
public partial class BibleVersionAddWindow : Window
{
    private readonly HashSet<string> _existingNames;

    public BibleVersionAddWindow(IReadOnlyList<BibleAddableVersion> candidates, IEnumerable<string> existingNames)
    {
        InitializeComponent();
        _existingNames = new HashSet<string>(existingNames ?? [], StringComparer.OrdinalIgnoreCase);
        CandidateList.ItemsSource = candidates;
        if (candidates.Count > 0)
        {
            // 첫 후보를 미리 골라 두면(이름칸 자동 채움) 운영자가 바로 "추가"만 누르면 된다.
            CandidateList.SelectedIndex = 0;
        }

        Loaded += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    /// <summary>확인 시 추가할 성경 파일명(HolyBibles 내).</summary>
    public string SelectedFileName { get; private set; } = "";

    /// <summary>확인 시 입력된 표시 이름(공백 제거됨).</summary>
    public string EnteredName { get; private set; } = "";

    private void CandidateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 후보를 고르면 제안 이름으로 이름칸을 채운다(운영자가 그대로 쓰거나 고칠 수 있게).
        if (CandidateList.SelectedItem is BibleAddableVersion candidate)
        {
            NameBox.Text = candidate.SuggestedName;
            NameBox.SelectAll();
        }

        Revalidate();
    }

    private void NameBox_TextChanged(object sender, TextChangedEventArgs e) => Revalidate();

    // 후보 선택 + 비어 있지 않은 새 이름일 때만 "추가"를 켠다. 중복이면 빨간 안내.
    private void Revalidate()
    {
        // 생성자에서 InitializeComponent 도중 TextChanged 가 먼저 올 수 있어 컨트롤 null 가드.
        if (OkButton is null || NameBox is null || CandidateList is null || ErrorText is null)
        {
            return;
        }

        var name = (NameBox.Text ?? "").Trim();
        var hasCandidate = CandidateList.SelectedItem is BibleAddableVersion;

        if (!hasCandidate)
        {
            ShowError("추가할 성경 파일을 고르세요.");
            OkButton.IsEnabled = false;
            return;
        }

        if (name.Length == 0)
        {
            ShowError("");
            OkButton.IsEnabled = false;
            return;
        }

        if (_existingNames.Contains(name))
        {
            ShowError("이미 있는 버전 이름입니다. 다른 이름을 입력하세요.");
            OkButton.IsEnabled = false;
            return;
        }

        ShowError("");
        OkButton.IsEnabled = true;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (CandidateList.SelectedItem is not BibleAddableVersion candidate)
        {
            return;
        }

        var name = (NameBox.Text ?? "").Trim();
        if (name.Length == 0 || _existingNames.Contains(name))
        {
            return; // 방어적 — Revalidate 가 버튼을 막지만 한 번 더 확인.
        }

        SelectedFileName = candidate.FileName;
        EnteredName = name;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
