using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Easislides.Wpf.Composites;

/// <summary>
/// SafetyConfirm — 계획서 §5.2 + §6.2.1, 라이브 운영 안전장치.
///
/// 책임:
///   라이브 상태에서 위험 액션(Black/Hide/Delete 등) 실행 전, 작은 인라인 확인.
///   모달 다이얼로그가 아닌 인라인 — 라이브 운영 흐름 끊지 않음.
///
/// 사용:
///   var ok = await SafetyConfirm.AskAsync(parentVisual, "지금 송출을 검정으로?", "5초 안에 확인 안 하면 자동 취소");
///
/// 동작:
///   - 부모 visual 아래에 작은 알림 카드로 나타남
///   - "확인" + "취소" 버튼 두 개 (확인은 IsDefault=Enter, 취소는 IsCancel=Esc 자동 라우팅)
///   - 5초 타임아웃 시 자동 취소 (실수 방지) — CancellationToken으로 즉시 클릭 시 정상 취소
///   - 외부 클릭(StaysOpen=false)으로 즉시 취소
///   - 단일 TrySetResult 진입점 보장 — 이중 결과 방지
/// </summary>
public static class SafetyConfirm
{
    /// <summary>비차단 확인 — 사용자 응답까지 Task로 대기. 타임아웃 시 false 반환.</summary>
    public static Task<bool> AskAsync(
        FrameworkElement anchor,
        string question,
        string? subtext = null,
        TimeSpan? timeout = null)
    {
        if (anchor is null) throw new ArgumentNullException(nameof(anchor));

        var tcs = new TaskCompletionSource<bool>();
        var dismissAfter = timeout ?? TimeSpan.FromSeconds(5);
        var cts = new CancellationTokenSource();

        // 단일 결과 진입점 — 이중 호출 안전, 부수 리소스 정리.
        Popup? popup = null;
        void Complete(bool result)
        {
            if (!tcs.TrySetResult(result)) return; // 이미 결과 설정됨 — no-op
            cts.Cancel();
            cts.Dispose();
            if (popup is not null && popup.IsOpen) popup.IsOpen = false;
        }

        popup = new Popup
        {
            Placement = PlacementMode.Center,
            PlacementTarget = anchor,
            AllowsTransparency = true,
            PopupAnimation = PopupAnimation.Fade,
            StaysOpen = false,
            Focusable = true, // 키 이벤트 수신 가능
        };

        var card = BuildCard(question, subtext, Complete);
        popup.Child = card;

        // Popup이 열리고 나서 카드에 포커스 — 그래야 Enter/Esc 키가 IsDefault/IsCancel 버튼으로 라우팅됨
        popup.Opened += (_, _) =>
        {
            card.Focus();
            FocusManager.SetFocusedElement(popup, card);
        };

        // 외부 클릭(StaysOpen=false)으로 닫힘 → 취소
        popup.Closed += (_, _) => Complete(false);

        popup.IsOpen = true;

        // 타임아웃 — CancellationToken 전달로 Complete() 시 즉시 중단
        _ = Task.Delay(dismissAfter, cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled) return; // 사용자가 이미 응답 — 정상
            anchor.Dispatcher.InvokeAsync(() => Complete(false));
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    private static StackPanel BuildCard(string question, string? subtext, Action<bool> onAnswer)
    {
        // 카드는 StackPanel로 — Focusable=true 설정해 Esc 키 이벤트 수신 가능.
        var stack = new StackPanel
        {
            Focusable = true,
            Margin = new Thickness(0, 4, 0, 0),
        };

        var border = new Border
        {
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            MinWidth = 280,
            MaxWidth = 360,
        };
        border.SetResourceReference(Border.BackgroundProperty, "Brush.Surface.Card");
        border.SetResourceReference(Border.BorderBrushProperty, "Brush.Status.Warning");

        var inner = new StackPanel();
        var titleText = new TextBlock
        {
            Text = question,
            TextWrapping = TextWrapping.Wrap,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14,
        };
        titleText.SetResourceReference(TextBlock.ForegroundProperty, "Brush.Text.Primary");
        titleText.SetResourceReference(TextBlock.FontFamilyProperty, "Font.Primary");
        inner.Children.Add(titleText);

        if (!string.IsNullOrEmpty(subtext))
        {
            var subtextBlock = new TextBlock
            {
                Text = subtext,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0),
            };
            subtextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Brush.Text.Secondary");
            subtextBlock.SetResourceReference(TextBlock.FontFamilyProperty, "Font.Primary");
            inner.Children.Add(subtextBlock);
        }

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
        };

        // 취소 버튼 — IsCancel=true로 WPF가 Esc 키를 자동 라우팅 (포커스 없어도 동작)
        var cancelBtn = new Button
        {
            Content = "취소",
            Margin = new Thickness(0, 0, 8, 0),
            MinWidth = 70,
            IsCancel = true,
        };
        AutomationProperties.SetName(cancelBtn, "Live safety cancel");
        cancelBtn.SetResourceReference(Button.StyleProperty, "EsButton.Secondary");
        cancelBtn.Click += (_, _) => onAnswer(false);

        // 확인 버튼 — IsDefault=true로 Enter 키 자동 라우팅
        var confirmBtn = new Button
        {
            Content = "확인",
            MinWidth = 70,
            IsDefault = true,
        };
        AutomationProperties.SetName(confirmBtn, "Live safety confirm");
        confirmBtn.SetResourceReference(Button.StyleProperty, "EsButton.Danger");
        confirmBtn.Click += (_, _) => onAnswer(true);

        buttons.Children.Add(cancelBtn);
        buttons.Children.Add(confirmBtn);
        inner.Children.Add(buttons);

        border.Child = inner;
        stack.Children.Add(border);
        return stack;
    }
}
