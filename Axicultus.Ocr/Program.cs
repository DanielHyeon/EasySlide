using System.Text;
using System.Text.Json;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;

// Axicultus.Ocr <language> <image>...
// Reads each image with the Windows OCR engine and writes one JSON array to stdout:
// [{ "File": "...", "Error": null, "Lines": [{ "Words": [{ "Text": "주", "X": 1, "Y": 2, "Width": 3, "Height": 4 }] }] }]
// Exit codes: 0 done (per-file errors are in the JSON), 2 bad arguments, 3 the language has no OCR engine.

if (args.Length < 2)
{
    Console.Error.WriteLine("usage: Axicultus.Ocr <language> <image>...");
    return 2;
}

Console.OutputEncoding = new UTF8Encoding(false);
var engine = Language.IsWellFormed(args[0]) ? OcrEngine.TryCreateFromLanguage(new Language(args[0])) : null;
if (engine == null)
{
    Console.Error.WriteLine($"No OCR engine for '{args[0]}'. Installed: " +
        string.Join(", ", OcrEngine.AvailableRecognizerLanguages.Select(l => l.LanguageTag)));
    return 3;
}

var pages = new List<OcrPage>();
foreach (var path in args.Skip(1))
{
    try
    {
        var file = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(path));
        using var stream = await file.OpenAsync(FileAccessMode.Read);
        var decoder = await BitmapDecoder.CreateAsync(stream);
        using var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        if (bitmap.PixelWidth > OcrEngine.MaxImageDimension || bitmap.PixelHeight > OcrEngine.MaxImageDimension)
        {
            pages.Add(new(path, $"image larger than {OcrEngine.MaxImageDimension} pixels", []));
            continue;
        }
        var result = await engine.RecognizeAsync(bitmap);
        pages.Add(new(path, null, [.. result.Lines.Select(line => new OcrLine([.. line.Words.Select(word =>
            new OcrWord(word.Text, (int)word.BoundingRect.X, (int)word.BoundingRect.Y, (int)word.BoundingRect.Width, (int)word.BoundingRect.Height))]))]));
    }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or System.Runtime.InteropServices.COMException)
    {
        pages.Add(new(path, error.Message, []));
    }
}

Console.Out.Write(JsonSerializer.Serialize(pages));
return 0;

internal sealed record OcrWord(string Text, int X, int Y, int Width, int Height);
internal sealed record OcrLine(OcrWord[] Words);
internal sealed record OcrPage(string File, string? Error, OcrLine[] Lines);
