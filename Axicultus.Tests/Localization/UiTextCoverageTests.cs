using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Axicultus.Wpf.Tests.Localization;

/// <summary>
/// Both apps show their UI in the UI language (Korean or English, View > Language): every text comes from the
/// resources (Axicultus/Localization/Text/&lt;Area&gt;.resx, Axicultuss/Localization/Text/&lt;Area&gt;.resx, each with a
/// .ko.resx), a Korean text has no English in it and an English one no Korean. Read from the source files.
/// The NotYet lists hold the files still to be moved to the resources; a file leaves its list when it is done (a
/// listed file that no longer has anything to move fails, so the lists only shrink).
/// </summary>
public sealed class UiTextCoverageTests
{
    // ---- files still to be done (each file leaves its list when it is done) ----

    private static readonly string[] WpfXamlNotYet =
    [
    ];

    private static readonly string[] WpfCodeNotYet =
    [
    ];

    private static readonly string[] WinFormsDesignerNotYet =
    [
        "Axicultuss/Forms/FrmBackground.Designer.cs",
        "Axicultuss/Forms/FrmBibleRename.Designer.cs",
        "Axicultuss/Forms/FrmCopy.Designer.cs",
        "Axicultuss/Forms/FrmCopyMoveExternal.Designer.cs",
        "Axicultuss/Forms/FrmEditBibleItem.Designer.cs",
        "Axicultuss/Forms/FrmEditNotes.Designer.cs",
        "Axicultuss/Forms/FrmExport.Designer.cs",
        "Axicultuss/Forms/FrmFind.Designer.cs",
        "Axicultuss/Forms/FrmGenerateDoc.Designer.cs",
        "Axicultuss/Forms/FrmGenerateHtml.Designer.cs",
        "Axicultuss/Forms/FrmGetWorkingFolder.Designer.cs",
        "Axicultuss/Forms/FrmImport.Designer.cs",
        "Axicultuss/Forms/FrmImportAccessHelper.Designer.cs",
        "Axicultuss/Forms/FrmImportFolder.Designer.cs",
        "Axicultuss/Forms/FrmLaunchShow.Designer.cs",
        "Axicultuss/Forms/FrmLookupTitles.Designer.cs",
        "Axicultuss/Forms/FrmLyricsScreen.Designer.cs",
        "Axicultuss/Forms/FrmManageItemLists.Designer.cs",
        "Axicultuss/Forms/FrmMediaPlayerControl.Designer.cs",
        "Axicultuss/Forms/FrmMove.Designer.cs",
        "Axicultuss/Forms/FrmPopupText.Designer.cs",
        "Axicultuss/Forms/FrmRearrangeFolderPositions.Designer.cs",
        "Axicultuss/Forms/FrmRecoverDeleted.Designer.cs",
        "Axicultuss/Forms/FrmRegister.Designer.cs",
        "Axicultuss/Forms/FrmShowAlert.Designer.cs",
        "Axicultuss/Forms/FrmSingleMonitorAlert.Designer.cs",
        "Axicultuss/Forms/FrmSmartMerge.Designer.cs",
        "Axicultuss/Forms/FrmSplashScreen.Designer.cs",
        "Axicultuss/Forms/FrmSplashScreenOld.Designer.cs",
        "Axicultuss/Forms/FrmUpdateFileName.Designer.cs",
        "Axicultuss/Forms/FrmUsages.Designer.cs",
    ];

    private static readonly string[] WinFormsCodeNotYet =
    [
        "Axicultuss/Forms/FrmBibleRename.cs",
        "Axicultuss/Forms/FrmCopy.cs",
        "Axicultuss/Forms/FrmCopyMoveExternal.cs",
        "Axicultuss/Forms/FrmEditBibleItem.cs",
        "Axicultuss/Forms/FrmExport.cs",
        "Axicultuss/Forms/FrmFind.cs",
        "Axicultuss/Forms/FrmGenerateDoc.cs",
        "Axicultuss/Forms/FrmGenerateHtml.cs",
        "Axicultuss/Forms/FrmGetWorkingFolder.cs",
        "Axicultuss/Forms/FrmImport.cs",
        "Axicultuss/Forms/FrmImportAccessHelper.cs",
        "Axicultuss/Forms/FrmImportFolder.cs",
        "Axicultuss/Forms/FrmManageItemLists.cs",
        "Axicultuss/Forms/FrmMove.cs",
        "Axicultuss/Forms/FrmRearrangeFolderPositions.cs",
        "Axicultuss/Forms/FrmRecoverDeleted.cs",
        "Axicultuss/Forms/FrmSmartMerge.cs",
        "Axicultuss/Forms/FrmUpdateFileName.cs",
        "Axicultuss/Forms/FrmUsages.cs",
        "Axicultuss/Global/gfBible.cs",
        "Axicultuss/Global/gfConfig.cs",
        "Axicultuss/Global/gfConstants.cs",
        "Axicultuss/Global/gfDatabase.cs",
        "Axicultuss/Global/gfFileHelpers.cs",
        "Axicultuss/Global/gfFolder.cs",
        "Axicultuss/Global/gfLyrics.cs",
        "Axicultuss/Global/gfMedia.cs",
        "Axicultuss/Global/gfPowerPoint.cs",
        "Axicultuss/Global/gfUtility.cs",
        "Axicultuss/Util/MonitorPicker.cs",
    ];

    // ---- the rules ----

    /// <summary>Written the same in both languages: product, format and key names, URLs, placeholders and access keys.</summary>
    private static readonly Regex Untranslated = new(
        @"https?://\S+|www\.\S+|\{\d+(:[^}]*)?\}|\(&\w\)|\(_\w\)|&&|"
        // (bounded by non-ASCII-letters: a Korean particle may follow a name, "Axicultus를")
        + @"(?<![A-Za-z])(Axicultus|Axicultuss|EasiSlides|PowerPoint|PPTX?|OBS|Windows|Office|Word|Excel|SQLite|MySQL|MariaDB|CCLI|DirectShow|DirectX|AdminDB|HolyBibles|"
        + @"JSON|XML|HTML|CSV|RTF|PDF|EULA|URL|USB|DPI|PC|ID|IP|NDI|WPF|WinForms|InfoScreen|Copyright|DB|MB|OCR|WebSocket|LIVE|AARRGGBB|pt|px|ms|"
        + @"Ctrl|Alt|Shift|Esc|Enter|Space|Tab|PgUp|PgDn|PageUp|PageDown|Home|End|Del|Insert|F\d{1,2}|"
        // (a single letter: a key name, "M", a list mark, "A.", or a verse key, "p")
        + @"[A-Za-z])(?![A-Za-z])|"
        // (the key after a modifier: "Ctrl+S")
        + @"(?<=(Ctrl|Alt|Shift)\+)[A-Za-z0-9](?![A-Za-z])|"
        + @"\.(esw|est|esi|esp|txt|pptx?|ppsx?|docx?|rtf|jpe?g|png|gif|bmp|ico|mp[34]|wmv|avi|xml|json|db|sqlite)(?![A-Za-z])|"
        // (a text that is one key letter: the verse buttons p, c, b, e… are the keys that jump there)
        + @"^[A-Za-z]$",
        RegexOptions.CultureInvariant);

    private static bool HasHangul(string text) => text.Any(c => c is >= '가' and <= '힣' or >= 'ㄱ' and <= 'ㆎ');

    private static bool HasAsciiWord(string text) => Untranslated.Replace(text, " ").Any(char.IsAsciiLetter);

    /// <summary>A literal that needs a resource: it has a letter left after the untranslated names.</summary>
    private static bool NeedsText(string text) => Untranslated.Replace(text, " ").Any(char.IsLetter);

    // A literal whose line ends with one of these is left as it is: data the app reads or writes (a Bible book name,
    // a parsing token, a file name), or a text written the same in every language (a language's own name).
    private const string NotUiMarker = "// not UI text";
    private const string SameMarker = "// same in every language";

    private static bool Marked(string line) => line.Contains(NotUiMarker, StringComparison.Ordinal) || line.Contains(SameMarker, StringComparison.Ordinal);

    // ---- resources ----

    private static Dictionary<string, string> Resx(string path) =>
        XDocument.Load(path).Root!.Elements("data").ToDictionary(e => (string)e.Attribute("name")!, e => (string?)e.Element("value") ?? "");

    private static IEnumerable<(string Neutral, string Korean)> ResxPairs(string app) =>
        Directory.EnumerateFiles(Path.Combine(Root, app, "Localization", "Text"), "*.resx")
            .Where(path => !path.EndsWith(".ko.resx", StringComparison.OrdinalIgnoreCase))
            .Select(path => (path, Path.ChangeExtension(path, null) + ".ko.resx"));

    [Theory]
    [InlineData("Axicultus")]
    [InlineData("Axicultuss")]
    public void EveryTextIsInBothLanguagesUnmixed(string app)
    {
        var problems = new List<string>();
        foreach (var (neutral, korean) in ResxPairs(app))
        {
            string area = Path.GetFileNameWithoutExtension(neutral);
            if (!File.Exists(korean)) { problems.Add($"{area}: no .ko.resx"); continue; }
            var en = Resx(neutral);
            var ko = Resx(korean);
            problems.AddRange(en.Keys.Except(ko.Keys).Select(key => $"{area}.{key}: no Korean"));
            problems.AddRange(ko.Keys.Except(en.Keys).Select(key => $"{area}.{key}: no English"));
            foreach (var (key, text) in en)
            {
                if (HasHangul(text)) problems.Add($"{area}.{key}: Korean in English: {text}");
                if (ko.TryGetValue(key, out var k))
                {
                    if (HasAsciiWord(k)) problems.Add($"{area}.{key}: English in Korean: {k}");
                    if (Regex.Matches(text, @"\{\d+").Count != Regex.Matches(k, @"\{\d+").Count) problems.Add($"{area}.{key}: placeholders differ");
                }
            }
        }
        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }

    [Theory]
    [InlineData("Axicultus")]
    [InlineData("Axicultuss")]
    public void EveryKeyUsedHasItsText(string app)
    {
        var areas = ResxPairs(app).ToDictionary(p => Path.GetFileNameWithoutExtension(p.Neutral), p => Resx(p.Neutral).Keys.ToHashSet());
        var missing = new List<string>();
        void Check(string file, string area, string key)
        {
            if (!areas.TryGetValue(area, out var keys) || !keys.Contains(key)) missing.Add($"{Relative(file)}: {area}.{key}");
        }
        foreach (var file in SourceFiles(app, "*.cs"))
        {
            string code = File.ReadAllText(file);
            foreach (Match use in UiTextUse.Matches(code))
                Check(file, use.Groups["area"].Success ? use.Groups["area"].Value : use.Groups["type"].Value, use.Groups["key"].Value);
        }
        if (app == "Axicultus")
            foreach (var file in SourceFiles(app, "*.xaml"))
                foreach (Match use in XamlTextUse.Matches(File.ReadAllText(file)))
                    Check(file, use.Groups[1].Value, use.Groups[2].Value);
        Assert.True(missing.Count == 0, string.Join("\n", missing));
    }

    private static readonly Regex UiTextUse = new(@"UiText\.(?:Get|Format)\(\s*(?:""(?<area>\w+)""|nameof\((?<type>\w+)\))\s*,\s*""(?<key>[^""]+)""");
    private static readonly Regex XamlTextUse = new(@"\{loc:Text (\w+)\.([^}\s]+)\}");

    // ---- WPF ----

    // The attributes that show text (AutomationProperties.Name, read by screen readers and by the tests, is left out).
    private static readonly HashSet<string> TextAttributes = ["Header", "Content", "Text", "ToolTip", "Title", "Description", "PlaceholderText", "Watermark", "Label"];

    // The elements whose own text is shown (not a colour, a font name or another value).
    private static readonly HashSet<string> TextElements = ["TextBlock", "Run", "Label", "Hyperlink", "Span", "Bold", "Italic", "Underline",
        "Paragraph", "Button", "ToggleButton", "RepeatButton", "CheckBox", "RadioButton", "MenuItem", "ToolTip", "TabItem", "GroupBox",
        "Expander", "ComboBoxItem", "ListBoxItem", "ContentControl", "String"];

    /// <summary>The literal texts of a XAML file that need a resource: attribute values, setter values and element text.</summary>
    internal static IEnumerable<string> XamlLiterals(string path)
    {
        var doc = XDocument.Load(path, LoadOptions.SetLineInfo);
        foreach (var element in doc.Descendants())
        {
            foreach (var attribute in element.Attributes().Where(a => TextAttributes.Contains(a.Name.LocalName) && !a.IsNamespaceDeclaration))
                if (!attribute.Value.StartsWith('{') && NeedsText(attribute.Value) && attribute.Value is not ("한국어" or "English"))
                    yield return $"{attribute.Name.LocalName}=\"{attribute.Value}\"";
            if (element.Name.LocalName == "Setter" && TextAttributes.Contains((string?)element.Attribute("Property") ?? "")
                && (string?)element.Attribute("Value") is { } value && !value.StartsWith('{') && NeedsText(value))
                yield return $"Setter {element.Attribute("Property")!.Value}=\"{value}\"";
            foreach (var text in element.Nodes().OfType<XText>())
                if (NeedsText(text.Value) && TextElements.Contains(element.Name.LocalName))
                    yield return $"<{element.Name.LocalName}>{text.Value.Trim()}";
        }
    }

    [Fact]
    public void WpfXamlShowsNoLiteralText() => AssertOnlyListed(
        SourceFiles("Axicultus", "*.xaml").Where(file => XamlLiterals(file).Any()).Select(Relative), WpfXamlNotYet,
        file => XamlLiterals(Path.Combine(Root, file)).Take(5));

    [Fact]
    public void WpfCodeHasNoKoreanLiterals() => AssertOnlyListed(
        SourceFiles("Axicultus", "*.cs").Where(file => KoreanLiterals(file).Any()).Select(Relative), WpfCodeNotYet,
        file => KoreanLiterals(Path.Combine(Root, file)).Take(5));

    // ---- WinForms ----

    private static readonly Regex DesignerText = new(
        @"^\s*(?:this\.)?(?:(?<field>\w+)\.)?(?<property>Text|ToolTipText|HeaderText)\s*=\s*""(?<value>(?:[^""\\]|\\.)*)""\s*;", RegexOptions.Multiline);
    private static readonly Regex DesignerToolTip = new(
        @"^\s*(?:this\.)?\w+\.SetToolTip\(\s*(?:this\.)?(?<field>\w+)\s*,\s*""(?<value>(?:[^""\\]|\\.)*)""\s*\)", RegexOptions.Multiline);

    /// <summary>The resource keys a form's designer texts need ("$this.Text", "field.Text", "field.ToolTip"…).</summary>
    internal static IEnumerable<string> DesignerKeys(string designerPath)
    {
        string designer = File.ReadAllText(designerPath);
        var mainMenu = Path.GetFileName(designerPath) == "FrmMain.Designer.cs"
            ? Regex.Matches(File.ReadAllText(Path.Combine(Root, "Axicultuss", "Forms", "FrmMain.MenuText.cs")), @"(\w+)\.Text = ").Select(m => m.Groups[1].Value).ToHashSet()
            : [];
        foreach (Match text in DesignerText.Matches(designer))
        {
            string value = Regex.Unescape(text.Groups["value"].Value);
            // (a designer's own name, "label1" or "menuStrip1", is a placeholder the code replaces or nobody sees)
            if (!NeedsText(value) || Regex.IsMatch(value, @"^[a-z][A-Za-z]*\d+$")) continue;
            if (!text.Groups["field"].Success) { if (text.Groups["property"].Value == "Text") yield return "$this.Text"; continue; }
            if (!mainMenu.Contains(text.Groups["field"].Value)) yield return text.Groups["field"].Value + "." + text.Groups["property"].Value;
        }
        foreach (Match tip in DesignerToolTip.Matches(designer))
            if (NeedsText(Regex.Unescape(tip.Groups["value"].Value))) yield return tip.Groups["field"].Value + ".ToolTip";
    }

    private static IEnumerable<string> DesignerGaps(string designerPath)
    {
        string form = Path.GetFileName(designerPath)[..^".Designer.cs".Length];
        var keys = DesignerKeys(designerPath).Distinct().ToList();
        if (keys.Count == 0) yield break;
        string resx = Path.Combine(Root, "Axicultuss", "Localization", "Text", form + ".resx");
        var have = File.Exists(resx) ? Resx(resx).Keys.ToHashSet() : [];
        foreach (var key in keys.Where(key => !have.Contains(key))) yield return "no text for " + key;
        string code = string.Concat(Directory.EnumerateFiles(Path.GetDirectoryName(designerPath)!, form + "*.cs")
            .Where(f => !f.EndsWith(".Designer.cs", StringComparison.Ordinal)).Select(File.ReadAllText));
        if (!code.Contains("UiText.ApplyTo(this)", StringComparison.Ordinal)) yield return "the constructor does not call UiText.ApplyTo(this)";
    }

    [Fact]
    public void WinFormsDesignerTextsAreAllInTheResources() => AssertOnlyListed(
        SourceFiles("Axicultuss", "*.Designer.cs").Where(file => DesignerGaps(file).Any()).Select(Relative), WinFormsDesignerNotYet,
        file => DesignerGaps(Path.Combine(Root, file)).Take(5));

    /// <summary>Korean literals, and message boxes given literal text.</summary>
    private static IEnumerable<string> WinFormsCodeGaps(string path) =>
        KoreanLiterals(path).Concat(MessageBoxLiterals(path).Select(text => "message box text: " + text));

    [Fact]
    public void WinFormsCodeHasNoLiteralUiText() => AssertOnlyListed(
        SourceFiles("Axicultuss", "*.cs").Where(file => !file.EndsWith(".Designer.cs", StringComparison.Ordinal))
            .Where(file => WinFormsCodeGaps(file).Any()).Select(Relative), WinFormsCodeNotYet,
        file => WinFormsCodeGaps(Path.Combine(Root, file)).Take(5));

    // ---- C# string literals (comments left out) ----

    internal sealed record Literal(int Line, int Start, int End, string Text);

    /// <summary>The string literals of a C# file with their lines, outside comments (regular, verbatim, interpolated, raw).</summary>
    internal static List<Literal> StringLiterals(string code)
    {
        var literals = new List<Literal>();
        int line = 1;
        for (int i = 0; i < code.Length; i++)
        {
            char c = code[i];
            if (c == '\n') { line++; continue; }
            if (c == '/' && i + 1 < code.Length && code[i + 1] == '/') { while (i < code.Length && code[i] != '\n') i++; line++; continue; }
            if (c == '/' && i + 1 < code.Length && code[i + 1] == '*')
            {
                for (i += 2; i + 1 < code.Length && !(code[i] == '*' && code[i + 1] == '/'); i++) if (code[i] == '\n') line++;
                i++;
                continue;
            }
            if (c == '\'')
            {
                for (i++; i < code.Length && code[i] != '\''; i++) if (code[i] == '\\') i++;
                continue;
            }
            if (c != '"') continue;
            int start = i, startLine = line;
            bool verbatim = false;
            for (int p = i - 1; p >= 0 && code[p] is '@' or '$'; p--) verbatim |= code[p] == '@';
            var text = new StringBuilder();
            int quotes = 0;
            while (i + quotes < code.Length && code[i + quotes] == '"') quotes++;
            if (quotes >= 3 && !verbatim)
            {
                // raw string: ends at the same number of quotes
                string fence = new('"', quotes);
                int end = code.IndexOf(fence, i + quotes, StringComparison.Ordinal);
                if (end < 0) break;
                text.Append(code, i + quotes, end - i - quotes);
                line += text.ToString().Count(ch => ch == '\n');
                i = end + quotes - 1;
            }
            else if (quotes == 2 && !verbatim) { i++; } // ""
            else
            {
                for (i++; i < code.Length; i++)
                {
                    if (code[i] == '\n') { line++; if (!verbatim) break; }
                    if (!verbatim && code[i] == '\\') { text.Append(code[i]).Append(code[i + 1]); i++; continue; }
                    if (code[i] == '"')
                    {
                        if (verbatim && i + 1 < code.Length && code[i + 1] == '"') { text.Append('"'); i++; continue; }
                        break;
                    }
                    text.Append(code[i]);
                }
            }
            literals.Add(new(startLine, start, i, text.ToString()));
        }
        return literals;
    }

    private static readonly Dictionary<string, (string[] Lines, List<Literal> Literals)> Parsed = [];

    private static (string[] Lines, List<Literal> Literals) Parse(string path)
    {
        lock (Parsed)
        {
            if (Parsed.TryGetValue(path, out var parsed)) return parsed;
            string code = File.ReadAllText(path);
            return Parsed[path] = (code.Split('\n'), StringLiterals(code));
        }
    }

    /// <summary>Korean string literals outside comments, unless their line says it is not UI text (data, a log line…).</summary>
    internal static IEnumerable<string> KoreanLiterals(string path)
    {
        var (lines, literals) = Parse(path);
        return literals.Where(l => HasHangul(l.Text) && !Marked(lines[l.Line - 1]))
            .Select(l => $"line {l.Line}: \"{l.Text}\"");
    }

    private static readonly Regex MessageBoxCall = new(@"\bMessageBox\.Show\(");
    private static readonly Regex UiTextArguments = new(@"UiText\.(?:Get|Format)\(\s*(?:nameof\(\w+\)|(?<area>""\w+""))\s*,\s*(?<key>""[^""]+"")");

    /// <summary>Literal texts with letters given to MessageBox.Show (in the call, up to its end); a UiText area or key is not one.</summary>
    internal static IEnumerable<string> MessageBoxLiterals(string path)
    {
        var (lines, literals) = Parse(path);
        string code = string.Join('\n', lines);
        var resourceNames = UiTextArguments.Matches(code).SelectMany(m => new[] { m.Groups["area"], m.Groups["key"] })
            .Where(g => g.Success).Select(g => g.Index).ToHashSet();
        literals = literals.Where(l => !resourceNames.Contains(l.Start)).ToList();
        foreach (Match call in MessageBoxCall.Matches(code))
        {
            int depth = 0, end = call.Index + call.Length - 1;
            for (int i = end; i < code.Length; i++)
            {
                var literal = literals.FirstOrDefault(l => l.Start == i);
                if (literal != null) { i = literal.End; continue; }
                if (code[i] == '(') depth++;
                else if (code[i] == ')' && --depth == 0) { end = i; break; }
            }
            foreach (var literal in literals.Where(l => l.Start > call.Index && l.End <= end && NeedsText(l.Text)))
                if (!Marked(lines[literal.Line - 1]))
                    yield return $"line {literal.Line}: \"{literal.Text}\"";
        }
    }

    // ---- files ----

    private static readonly string Root = FindRoot();

    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Axicultus.sln"))) return dir.FullName;
        throw new DirectoryNotFoundException("Axicultus.sln");
    }

    private static string Relative(string path) => Path.GetRelativePath(Root, path).Replace('\\', '/');

    // The apps' own sources: not the build output, the developer-only demo and proof-of-concept windows, or the
    // localization code itself.
    private static IEnumerable<string> SourceFiles(string app, string pattern) =>
        Directory.EnumerateFiles(Path.Combine(Root, app), pattern, SearchOption.AllDirectories)
            .Where(path => !Regex.IsMatch(Relative(path), @"/(bin|obj|Demo|Poc|Localization)/"));

    private static void AssertOnlyListed(IEnumerable<string> offending, string[] notYet, Func<string, IEnumerable<string>> examples)
    {
        var found = offending.ToHashSet(StringComparer.Ordinal);
        var unlisted = found.Except(notYet).Order().ToList();
        var done = notYet.Except(found).Order().ToList();
        var message = new StringBuilder();
        foreach (var file in unlisted) message.AppendLine($"{file} has text to move to the resources:\n  " + string.Join("\n  ", examples(file)));
        foreach (var file in done) message.AppendLine($"{file} is done: take it out of the NotYet list");
        Assert.True(message.Length == 0, message.ToString());
    }
}
