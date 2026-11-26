using System.Collections;
using System.Globalization;
using System.Linq;
using System.Resources;
using Axicultus.Localization;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// The menu texts both apps share (Axicultus.Core/Localization/MenuText*.resx): every key in both languages, and a
// Korean menu shows Korean, an English one English (no mixing).
public class MenuTextTests
{
    private static string[] Keys(CultureInfo culture) =>
        [.. MenuText.ResourceManager.GetResourceSet(culture, true, false)!.Cast<DictionaryEntry>().Select(e => (string)e.Key).Order()];

    [Fact]
    public void BothLanguagesHaveEveryKey() => Assert.Equal(Keys(CultureInfo.InvariantCulture), Keys(CultureInfo.GetCultureInfo("ko")));

    // Kept as they are in both: product and format names, and a Korean text's access key "(&E)".
    private static string WithoutNames(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text, @"\(&\w\)|\{0\}|PowerPoint|PPTX|InfoScreen|WPF|\.esw|Language", "");

    [Fact]
    public void AKoreanMenuIsKoreanAndAnEnglishOneEnglish()
    {
        foreach (var key in Keys(CultureInfo.InvariantCulture).Where(key => key != nameof(MenuText.View_Language)))
        {
            string korean = MenuText.ResourceManager.GetString(key, CultureInfo.GetCultureInfo(UiLanguage.Korean))!;
            string english = MenuText.ResourceManager.GetString(key, CultureInfo.GetCultureInfo(UiLanguage.English))!;
            Assert.True(korean.Any(c => c is >= '가' and <= '힣'), $"{key}: {korean}");
            Assert.False(WithoutNames(korean).Any(char.IsAsciiLetter), $"{key} has English in Korean: {korean}");
            Assert.False(english.Any(c => c is >= '가' and <= '힣'), $"{key} has Korean in English: {english}");
            Assert.Equal(english.Count(c => c == '{'), korean.Count(c => c == '{')); // the same placeholders
        }
        // The language menu reads in both, so it is found whichever language is on.
        Assert.Contains("언어", MenuText.ResourceManager.GetString(nameof(MenuText.View_Language), CultureInfo.GetCultureInfo(UiLanguage.English)));
        Assert.Contains("Language", MenuText.ResourceManager.GetString(nameof(MenuText.View_Language), CultureInfo.GetCultureInfo(UiLanguage.Korean)));
    }

    [Theory]
    [InlineData(null, UiLanguage.Korean)]
    [InlineData("", UiLanguage.Korean)]
    [InlineData("ko-KR", UiLanguage.Korean)]
    [InlineData("Korean", UiLanguage.Korean)]
    [InlineData("en-US", UiLanguage.English)]
    [InlineData("en", UiLanguage.English)]
    [InlineData("English", UiLanguage.English)]
    [InlineData("fr-FR", UiLanguage.Korean)]
    public void AKeptLanguageIsOneOfTheTwo(string? kept, string language) => Assert.Equal(language, UiLanguage.Resolve(kept));
}
