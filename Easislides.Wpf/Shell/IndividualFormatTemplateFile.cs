using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Easislides.Wpf.Shell;

/// <summary>
/// FrmMain Ind_LoadTemplate/Ind_SaveTemplate 호환 .est 파일의 ListHeader/FormatData 를 읽고 쓴다.
/// </summary>
public static class IndividualFormatTemplateFile
{
    public const string DialogFilter = "EasiSlides Template File (*.est)|*.est|All files (*.*)|*.*";

    public static string LoadFormatData(string filePath)
    {
        var document = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
        var header = document.Descendants("ListHeader").FirstOrDefault();
        return header?.Element("FormatData")?.Value ?? string.Empty;
    }

    public static void SaveFormatData(string filePath, string? formatData)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(
                "EasiSlides",
                new XElement(
                    "ListItem",
                    new XElement(
                        "ListHeader",
                        new XElement("SystemID", "EasiSlides"),
                        new XElement("FormatData", formatData ?? string.Empty),
                        new XElement("Notes", string.Empty)))));

        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
        };

        using var writer = XmlWriter.Create(filePath, settings);
        document.Save(writer);
    }
}
