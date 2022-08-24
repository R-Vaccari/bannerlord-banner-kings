using System.Text.RegularExpressions;
using System.Xml;

namespace Localizer;

internal class Program
{
    private static List<string> _usedLocalizationIDs = null!;
    private static Random _random = null!;

    private static void Main(string[] args)
    {
        _usedLocalizationIDs = new List<string>();
        _random = new Random();

        Console.WriteLine("Path to Source folder:");
        var sourceFolder = Console.ReadLine();

        if (!Directory.Exists(sourceFolder))
        {
            Exit("Directory does not exist.");
            return;
        }

        var localizationFile = Directory.GetFiles(sourceFolder, "std_module_strings_xml.xml", SearchOption.AllDirectories).FirstOrDefault();
        if (localizationFile is null)
        {
            Exit("std_module_strings_xml.xml not found.");
            return;
        }

        var allLocalizationFiles = GetAllLocalizationFiles(localizationFile);

        var files = Directory.GetFiles(sourceFolder, "*.cs", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            Exit("No .cs files found.");
            return;
        }

        LoadUsedLocalizationIDs(allLocalizationFiles);
        LocalizeTexts(localizationFile, files);
    }

    private static void Exit(string message)
    {
        Console.WriteLine(message);
        Console.ReadKey();
    }

    private static IEnumerable<string> GetAllLocalizationFiles(string localizationFile)
    {
        Console.WriteLine("Path to Modules folder:");
        var modulesFolder = Console.ReadLine();
        if (!Directory.Exists(modulesFolder))
        {
            Console.WriteLine("Directory does not exist. External IDs won't be considered.");
            return new[] {localizationFile};
        }

        return new[] {localizationFile}.Concat(Directory.GetFiles(modulesFolder, "std_module_strings_xml.xml", SearchOption.AllDirectories));
    }

    private static void LoadUsedLocalizationIDs(IEnumerable<string> localizationFiles)
    {
        foreach (var localizationFile in localizationFiles)
        {
            var localizationDocument = new XmlDocument();
            localizationDocument.Load(localizationFile);

            var stringNodes = localizationDocument.SelectNodes("/base/strings");
            if (stringNodes is null || stringNodes.Count == 0)
            {
                return;
            }

            foreach (XmlNode stringNode in stringNodes)
            {
                if (stringNode is null)
                {
                    continue;
                }

                _usedLocalizationIDs.Add(stringNode.Attributes!["id"]?.Value!);
            }
        }
    }

    private static void LocalizeTexts(string localizationFile, IEnumerable<string> files)
    {
        var localizationDocument = new XmlDocument();
        localizationDocument.Load(localizationFile);

        foreach (var file in files)
        {
            var texts = GetTextsToLocalize(file);
            foreach (var text in texts)
            {
                if (string.IsNullOrWhiteSpace(text) || text.Contains("img src="))
                {
                    continue;
                }

                var textToLocalize = $"{{=!}}{text}";
                var localizedText = GetLocalizedText(textToLocalize);

                if (string.IsNullOrWhiteSpace(localizedText.Text))
                {
                    continue;
                }

                AddTextToLocalization(localizationDocument, localizedText.ID, text);
                ReplaceTextInSource(file, textToLocalize, localizedText.Text);
            }
        }

        localizationDocument.Save(localizationFile);
    }

    private static IEnumerable<string> GetTextsToLocalize(string file)
    {
        var regex = new Regex("(?<=\"{=!}).*?(?=\")");
        var text = GetFileContent(file);

        return regex.Matches(text).Select(match => match.Value);
    }

    private static string GetFileContent(string file)
    {
        return File.ReadAllText(file);
    }

    private static (string ID, string Text) GetLocalizedText(string text)
    {
        var localizationID = GetLocalizationID();

        return (localizationID, text.Replace("{=!}", $"{{={localizationID}}}"));
    }

    private static string GetLocalizationID()
    {
        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
        var chars = new char[9];

        for (var i = 0; i < 9; i++)
        {
            chars[i] = allowedChars[_random.Next(0, allowedChars.Length)];
        }

        var guid = new string(chars);

        return _usedLocalizationIDs.Contains(guid)
            ? GetLocalizationID() 
            : guid;
    }

    private static void AddTextToLocalization(XmlDocument localizationDocument, string localizationID, string localizedText)
    {
        var stringNode = localizationDocument.CreateElement("string");

        var idAttribute = localizationDocument.CreateAttribute("id");
        idAttribute.Value = localizationID;

        var textAttribute = localizationDocument.CreateAttribute("text");
        textAttribute.Value = localizedText;

        stringNode.Attributes.Append(idAttribute);
        stringNode.Attributes.Append(textAttribute);

        var stringsElement = localizationDocument.SelectSingleNode("/base/strings");
        stringsElement!.AppendChild(stringNode);
    }

    private static void ReplaceTextInSource(string file, string textToLocalize, string localizedTextText)
    {
        var fileContent = GetFileContent(file);
        fileContent = fileContent.Replace(textToLocalize, localizedTextText);

        File.WriteAllText(file, fileContent);
    }
}