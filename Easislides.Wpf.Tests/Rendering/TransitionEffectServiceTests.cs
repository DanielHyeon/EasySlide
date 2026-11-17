using System;
using System.Linq;
using System.Windows;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class TransitionEffectServiceTests
{
    [Fact]
    public void GetEffects_MatchesLegacyOrderAndDisplayNames()
    {
        var sut = new TransitionEffectService();

        var effects = sut.GetEffects();

        effects.Should().HaveCount(58);
        effects[0].Should().Be(new TransitionEffectDescriptor(TransitionEffectKind.None, "None", 0, TransitionMotionKind.None));
        effects[15].Should().Be(new TransitionEffectDescriptor(TransitionEffectKind.Fade, "Fade", 15, TransitionMotionKind.CrossFade));
        effects[17].Should().Be(new TransitionEffectDescriptor(TransitionEffectKind.FlipHorizontal, "Flip Horizontal", 17, TransitionMotionKind.Flip));
        effects[^1].Should().Be(new TransitionEffectDescriptor(TransitionEffectKind.ZoomOut, "Zoom Out", 57, TransitionMotionKind.Zoom));
        effects.Select(effect => effect.LegacyIndex).Should().BeEquivalentTo(Enumerable.Range(0, 58));
    }

    [Fact]
    public void Resolve_TrimsDisplayNameAndUnknownFallsBackToNone()
    {
        var sut = new TransitionEffectService();

        sut.Resolve(" Fade ").Should().Be(TransitionEffectKind.Fade);
        sut.Resolve("Zoom Out").Should().Be(TransitionEffectKind.ZoomOut);
        sut.Resolve("missing").Should().Be(TransitionEffectKind.None);
        sut.Resolve(null).Should().Be(TransitionEffectKind.None);
        sut.GetDisplayName((TransitionEffectKind)999).Should().Be("None");
    }

    [Fact]
    public void CreatePlan_AsFade_UsesFadeAndKeepsStoredKind()
    {
        var sut = new TransitionEffectService();

        var plan = sut.CreatePlan(new TransitionEffectRequest(
            StoredKind: TransitionEffectKind.Dissolve,
            Action: TransitionActionKind.AsFade,
            Duration: TimeSpan.FromSeconds(2),
            BackgroundMode: TransitionBackgroundMode.BothBackgrounds,
            ViewportWidth: 1920,
            ViewportHeight: 1080));

        plan.Kind.Should().Be(TransitionEffectKind.Fade);
        plan.StoredKind.Should().Be(TransitionEffectKind.Dissolve);
        plan.Duration.Should().Be(TimeSpan.FromSeconds(2));
        plan.BackgroundLayers.Should().ContainInOrder(TransitionBackgroundLayer.Current, TransitionBackgroundLayer.New);
    }

    [Fact]
    public void GetFrame_NoneAction_CompletesImmediately()
    {
        var sut = new TransitionEffectService();
        var plan = sut.CreatePlan(new TransitionEffectRequest(
            StoredKind: TransitionEffectKind.Fade,
            Action: TransitionActionKind.None,
            Duration: TimeSpan.FromSeconds(2),
            BackgroundMode: TransitionBackgroundMode.BothBackgrounds,
            ViewportWidth: 200,
            ViewportHeight: 100));

        var frame = sut.GetFrame(plan, TimeSpan.Zero);

        frame.Kind.Should().Be(TransitionEffectKind.None);
        frame.Progress.Should().Be(1);
        frame.IsComplete.Should().BeTrue();
        frame.CurrentBounds.Should().Be(new Rect(0, 0, 200, 100));
        frame.NewBounds.Should().Be(new Rect(0, 0, 200, 100));
    }

    [Fact]
    public void GetFrame_Fade_CrossFadesOpacityAndClampsProgress()
    {
        var sut = new TransitionEffectService();
        var plan = sut.CreatePlan(new TransitionEffectRequest(
            StoredKind: TransitionEffectKind.Fade,
            Action: TransitionActionKind.AsStored,
            Duration: TimeSpan.FromSeconds(2),
            BackgroundMode: TransitionBackgroundMode.BothBackgrounds,
            ViewportWidth: 200,
            ViewportHeight: 100));

        var half = sut.GetFrame(plan, TimeSpan.FromSeconds(1));
        var complete = sut.GetFrame(plan, TimeSpan.FromSeconds(3));

        half.Progress.Should().BeApproximately(0.5, 0.0001);
        half.CurrentOpacity.Should().BeApproximately(0.5, 0.0001);
        half.NewOpacity.Should().BeApproximately(0.5, 0.0001);
        half.IsComplete.Should().BeFalse();
        complete.Progress.Should().Be(1);
        complete.CurrentOpacity.Should().Be(0);
        complete.NewOpacity.Should().Be(1);
        complete.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void GetFrame_SlideInLeft_ComputesNewBoundsOffset()
    {
        var sut = new TransitionEffectService();
        var plan = sut.CreatePlan(new TransitionEffectRequest(
            StoredKind: TransitionEffectKind.InLeft,
            Action: TransitionActionKind.AsStored,
            Duration: TimeSpan.FromSeconds(4),
            BackgroundMode: TransitionBackgroundMode.BothBackgrounds,
            ViewportWidth: 200,
            ViewportHeight: 100));

        var frame = sut.GetFrame(plan, TimeSpan.FromSeconds(2));

        frame.CurrentBounds.Should().Be(new Rect(0, 0, 200, 100));
        frame.NewBounds.Should().Be(new Rect(-100, 0, 200, 100));
    }

    [Fact]
    public void GetFrame_RevealRightLeft_ComputesRevealBounds()
    {
        var sut = new TransitionEffectService();
        var plan = sut.CreatePlan(new TransitionEffectRequest(
            StoredKind: TransitionEffectKind.RevealRightLeft,
            Action: TransitionActionKind.AsStored,
            Duration: TimeSpan.FromSeconds(4),
            BackgroundMode: TransitionBackgroundMode.NewOnly,
            ViewportWidth: 200,
            ViewportHeight: 100));

        var frame = sut.GetFrame(plan, TimeSpan.FromSeconds(1));

        frame.RevealBounds.Should().Be(new Rect(150, 0, 50, 100));
        frame.BackgroundLayers.Should().ContainSingle().Which.Should().Be(TransitionBackgroundLayer.New);
    }
}
