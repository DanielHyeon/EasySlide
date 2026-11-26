using System;
using System.Linq;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Localization;

/// <summary>
/// The texts looked up with a key made in code (an enum value, a choice name), which UiTextCoverageTests cannot
/// read from the source: each one must be in the resources.
/// </summary>
public sealed class ComputedTextKeysTests
{
    private static bool Missing(string text) => text.StartsWith('[');

    [Fact]
    public void EveryTransitionEffectHasALabel()
        => new TransitionEffectService().GetEffects().Where(effect => Missing(effect.Label)).Select(effect => effect.Kind).Should().BeEmpty();

    [Fact]
    public void EveryEnumValueShownInAListHasAText()
    {
        Type[] shown = [typeof(BibleSearchMatchMode), typeof(ExternalFileItemKind), typeof(ExternalFileOperationKind),
            typeof(ExternalFileDestinationKind), typeof(ImportDuplicatePolicy), typeof(ExportFormat), typeof(ColorTheme)];
        shown.SelectMany(type => Enum.GetValues(type).Cast<Enum>())
            .Where(value => Missing(EnumTextConverter.Text(value)))
            .Select(value => value.GetType().Name + "." + value)
            .Should().BeEmpty();
    }

    [Fact]
    public void EveryOptionChoiceHasALabel()
        => OptionChoices.Horizontal.Concat(OptionChoices.Vertical).Concat(OptionChoices.Italics).Concat(OptionChoices.Headings)
            .Where(choice => Missing(choice.Label)).Select(choice => choice.Name).Should().BeEmpty();

    [Fact]
    public void EveryMediaStateHasAStatusText()
        => Enum.GetValues<MediaPlaybackState>().Where(state => UiText.Catalog.Find("MediaPlaybackViewModel", state.ToString()) is null)
            .Should().BeEmpty();
}
