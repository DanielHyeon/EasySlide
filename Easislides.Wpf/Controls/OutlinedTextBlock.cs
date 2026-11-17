using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Easislides.Wpf.Controls;

/// <summary>
/// 글자에 외곽선(Outline)을 그리는 텍스트 표시 컨트롤(§7.3-A 폰트 효과 — Outline Font).
/// 일반 <see cref="System.Windows.Controls.TextBlock"/> 은 외곽선을 못 그리므로,
/// <see cref="FormattedText.BuildGeometry(Point)"/> 로 글자 형상을 만든 뒤 채움(Fill)+테두리(Stroke)로 직접 렌더한다.
/// 출력 창에서 외곽선 효과 on 일 때만 본문 대신 이 컨트롤을 쓰며(상호배타), off 일 때는 기존 본문 그대로라 회귀가 없다.
/// 어두운/영상 배경 위 가사 가독성을 높이는 용도.
/// <para>
/// 폰트 패밀리는 <see cref="TextElement.FontFamilyProperty"/> 를 AddOwner(상속)하여 일반 본문 TextBlock 과
/// 똑같이 주변(앱/창) 폰트를 물려받는다 — 외곽선 on/off 시 글꼴이 달라지지 않도록 보장(code-review CRITICAL 반영).
/// FormattedText/Geometry 는 텍스트·폰트 관련 속성이 바뀔 때만 다시 만들어 캐시한다(성능, code-review MAJOR 반영).
/// </para>
/// </summary>
public sealed class OutlinedTextBlock : FrameworkElement
{
    // 외곽선·밑줄이 측정 박스 가장자리에서 잘리지 않도록 두는 고정 여백.
    // 외곽선 두께 2px + 밑줄(stroke 포함)이 글자 하단 가까이 그려지므로, 하단 잘림을 막도록 3px 여백을 둔다.
    // 더 두꺼운 외곽선을 쓰게 되면 두께 이상으로 키울 것(밑줄 stroke 하단 ~1px 돌출분 포함).
    private const double Padding = 3d;

    // 텍스트/폰트/정렬/폭/줄간격 등 형상에 영향을 주는 속성이 바뀌면 캐시를 비운다.
    private static readonly FrameworkPropertyMetadataOptions ShapeChange =
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender;

    // 채움/테두리 색·두께는 형상(Geometry)에는 영향 없고 렌더에만 영향.
    private static readonly FrameworkPropertyMetadataOptions PaintChange =
        FrameworkPropertyMetadataOptions.AffectsRender;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(string.Empty, ShapeChange, OnShapeChanged));

    public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
        nameof(FontSize), typeof(double), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(48d, ShapeChange, OnShapeChanged));

    // 일반 TextBlock 과 동일하게 주변 폰트를 상속받는다(리터럴 폰트명 금지 — EasiDS001, 글꼴 정합 보장).
    public static readonly DependencyProperty FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner(
            typeof(OutlinedTextBlock),
            new FrameworkPropertyMetadata(
                TextElement.FontFamilyProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.Inherits | ShapeChange,
                OnShapeChanged));

    public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
        nameof(FontWeight), typeof(FontWeight), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(FontWeights.SemiBold, ShapeChange, OnShapeChanged));

    public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
        nameof(FontStyle), typeof(FontStyle), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(FontStyles.Normal, ShapeChange, OnShapeChanged));

    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(Brush), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(Brushes.White, PaintChange));

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
        nameof(Stroke), typeof(Brush), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(Brushes.Black, PaintChange));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(2d, PaintChange));

    public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
        nameof(TextAlignment), typeof(TextAlignment), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(TextAlignment.Center, ShapeChange, OnShapeChanged));

    // 밑줄(§7.3-A) — on 이면 형상(Geometry)에 밑줄을 포함해 외곽선·채움과 같은 색으로 그린다.
    // 일반 TextBlock 의 TextDecorations 에 대응(외곽선 모드에서도 밑줄을 렌더하기 위함). 형상에 영향 → ShapeChange.
    public static readonly DependencyProperty UnderlineProperty = DependencyProperty.Register(
        nameof(Underline), typeof(bool), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(false, ShapeChange, OnShapeChanged));

    public static readonly DependencyProperty MaxTextWidthProperty = DependencyProperty.Register(
        nameof(MaxTextWidth), typeof(double), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(1160d, ShapeChange, OnShapeChanged));

    public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
        nameof(LineHeight), typeof(double), typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(0d, ShapeChange, OnShapeChanged));

    // 형상 캐시 — 텍스트/폰트 속성이 바뀔 때만(OnShapeChanged) 무효화한다.
    private FormattedText? _formatted;
    private Geometry? _geometry;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public bool Underline
    {
        get => (bool)GetValue(UnderlineProperty);
        set => SetValue(UnderlineProperty, value);
    }

    public double MaxTextWidth
    {
        get => (double)GetValue(MaxTextWidthProperty);
        set => SetValue(MaxTextWidthProperty, value);
    }

    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var formatted = EnsureFormatted();
        if (formatted is null)
        {
            return new Size(0, 0);
        }

        // 텍스트 실제 크기에 외곽선 여백(양쪽)을 더한 만큼 차지. 가용 영역을 넘지 않게 클램프.
        var width = Math.Min(formatted.Width + Padding * 2, availableSize.Width);
        var height = formatted.Height + Padding * 2;
        if (!double.IsInfinity(availableSize.Height))
        {
            height = Math.Min(height, availableSize.Height);
        }

        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var formatted = EnsureFormatted();
        if (formatted is null)
        {
            return;
        }

        // 글자 형상(Geometry)은 텍스트/폰트가 바뀔 때만 다시 만들어 캐시(고비용 연산 — 매 렌더 재생성 방지).
        if (_geometry is null)
        {
            _geometry = formatted.BuildGeometry(new Point(Padding, Padding));
            if (_geometry.CanFreeze)
            {
                _geometry.Freeze();
            }
        }

        // 채움(Fill)+테두리(Stroke)로 그린다. Pen LineJoin=Round 로 모서리가 뾰족해지지 않게(가독성·미관).
        var pen = new Pen(Stroke ?? Brushes.Black, StrokeThickness)
        {
            LineJoin = PenLineJoin.Round,
        };
        pen.Freeze();
        drawingContext.DrawGeometry(Fill ?? Brushes.White, pen, _geometry);
    }

    // 텍스트/폰트/정렬/폭/줄간격 변경 시 캐시 무효화 — 다음 측정·렌더에서 다시 만든다.
    private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (OutlinedTextBlock)d;
        control._formatted = null;
        control._geometry = null;
    }

    // 현재 속성으로 FormattedText 를 만들어 캐시한다(텍스트 없으면 null).
    private FormattedText? EnsureFormatted()
    {
        if (_formatted is not null)
        {
            return _formatted;
        }

        if (string.IsNullOrEmpty(Text) || FontSize <= 0)
        {
            return null;
        }

        var family = FontFamily ?? SystemFonts.MessageFontFamily;
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var typeface = new Typeface(family, FontStyle, FontWeight, FontStretches.Normal);
        var formatted = new FormattedText(
            Text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            Fill ?? Brushes.White,
            dpi)
        {
            TextAlignment = TextAlignment,
            Trimming = TextTrimming.CharacterEllipsis,
        };

        if (MaxTextWidth > 0)
        {
            formatted.MaxTextWidth = MaxTextWidth;
        }

        // 밑줄 on 이면 형상에 밑줄을 포함시킨다 — BuildGeometry 가 밑줄 사각형까지 만들어 외곽선·채움과 같은 색으로 그려진다.
        if (Underline)
        {
            formatted.SetTextDecorations(TextDecorations.Underline);
        }

        // 줄 간격(설정 기반)을 적용하되, FormattedText.LineHeight 는 줄 박스를 강제해
        // 폰트 자연 줄높이보다 작으면 글자가 잘린다(일반 TextBlock=MaxHeight 전략과 다름).
        // 그래서 자연 줄높이를 하한으로 둬 좁은 줄간격에서도 글자가 잘리지 않게 한다(code-review MAJOR 반영).
        if (LineHeight > 0)
        {
            var naturalLineHeight = family.LineSpacing * FontSize;
            formatted.LineHeight = Math.Max(LineHeight, naturalLineHeight);
        }

        _formatted = formatted;
        return _formatted;
    }
}
