using System.Text.RegularExpressions;
using System.Xml;

namespace Localizer
{
    internal class Program
    {
        private static List<string> _usedLocalizationIDs = null!;
        private static Random _random = null!;

        private static void Main(string[] args)
        {
            _usedLocalizationIDs = new List<string>();
            _random = new Random();

            Console.WriteLine("Path to Source folder:");
            var sourceFolder = new DirectoryInfo(Console.ReadLine()!);
            if (!sourceFolder.Exists)
            {
                Exit("Directory does not exist.");
                return;
            }

            Console.WriteLine("\nPath to Modules folder:");
            var modulesFolder = new DirectoryInfo(Console.ReadLine()!);
            if (!modulesFolder.Exists)
            {
                Console.WriteLine("Directory does not exist. External IDs won't be considered.");
            }

            var localizationFile = Directory.GetFiles(sourceFolder.FullName, "std_module_strings_xml.xml", SearchOption.AllDirectories).FirstOrDefault();
            if (localizationFile is null)
            {
                Exit("std_module_strings_xml.xml not found.");
                return;
            }
        
            var allLocalizationFiles = new[] {localizationFile}.Concat(Directory.GetFiles(modulesFolder.FullName, "std_module_strings_xml.xml", SearchOption.AllDirectories)).ToList();
            Console.WriteLine($"\nLoaded {allLocalizationFiles.Count} Localization files");
        
            LoadUsedLocalizationIDs(allLocalizationFiles);
            Console.WriteLine($"Loaded {_usedLocalizationIDs.Count} IDs");
        
            var files = Directory.GetFiles(sourceFolder.FullName, "*.cs", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Exit("No .cs files found.");
                return;
            }
            Console.WriteLine($"Loaded {files.Length} files to localize");
        
            LocalizeTexts(localizationFile, files);
            Console.WriteLine("\nAll texts got localized! Press any key to exit..");
            Console.ReadKey();
        }

        private static void Exit(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
        }

        private static void LoadUsedLocalizationIDs(IEnumerable<string> localizationFiles)
        {
            foreach (var localizationFile in localizationFiles)
            {
                var localizationDocument = new XmlDocument();
                localizationDocument.Load(localizationFile);

                var stringNodes = localizationDocument.SelectNodes("/base/strings/*");
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

            var filesToLocalize = files.ToList();

            var filesToRemove = new List<string>();
            var texts = new List<string>();
            foreach (var file in filesToLocalize)
            {
                var textsToLocalize = GetTextsToLocalize(file).ToList();
                if (!textsToLocalize.Any())
                {
                    filesToRemove.Add(file);
                }

                texts.AddRange(textsToLocalize);
            }

            filesToLocalize.RemoveAll(f => filesToRemove.Contains(f));
            Console.WriteLine($"Removed {filesToRemove.Count} files without text to localize");

            var initialTextCount = texts.Count;
            Console.WriteLine($"Found {initialTextCount} texts to localize");

            var duplicates = texts.RemoveAll(t => string.IsNullOrWhiteSpace(t) || t.Contains("img src="));
            Console.WriteLine($"Removed {duplicates} bad (empty or image) texts");

            texts = texts.Distinct().ToList();
            Console.WriteLine($"Removed {initialTextCount - texts.Count} duplicated texts");

            Console.WriteLine($"\nLocalizing {texts.Count} texts in {filesToLocalize.Count} files..");
            for (var textIndex = 1; textIndex < texts.Count; textIndex++)
            {
                if (textIndex % 100 == 0)
                {
                    Console.WriteLine($"Localized {textIndex}/{texts.Count} texts..");
                }

                var text = texts[textIndex - 1];
                var textToLocalize = $"{{=!}}{text}";
                var localizedText = GetLocalizedText(textToLocalize);

                if (string.IsNullOrWhiteSpace(localizedText.Text))
                {
                    continue;
                }

                AddTextToLocalization(localizationDocument, localizedText.ID, text);

                foreach (var file in filesToLocalize)
                {
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
            const int idLength = 8;
            var chars = new char[idLength];

            for (var i = 0; i < idLength; i++)
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
}