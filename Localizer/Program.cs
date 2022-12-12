using System.Text.RegularExpressions;
using System.Xml;

namespace Localizer
{
    internal class Program
    {
        private static List<string> _usedLocalizationIDs = null!;
        private static Dictionary<string, string> _existingLocalizations = null!;
        private static Random _random = null!;

        private static void Main(string[] args)
        {
            _usedLocalizationIDs = new List<string>();
            _existingLocalizations = new Dictionary<string, string>();
            _random = new Random();

            Console.WriteLine("Path to Source folder:");
            var sourceFolder = new DirectoryInfo(@"G:\Dev\BannerLordMods\TXP4\src\TournamentsXPanded4"); //new DirectoryInfo(Console.ReadLine()!);
            if (!sourceFolder.Exists)
            {
                Exit("Directory does not exist.");
                return;
            }

            Console.WriteLine("\nPath to Modules folder:");
            var modulesFolder = new DirectoryInfo(@"G:\Dev\BannerLordMods\TXP4\src\TournamentsXPanded4\_Module");
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

            var allLocalizationFiles = GetAllLocalizationFiles(modulesFolder.FullName);
            Console.WriteLine($"Loaded {allLocalizationFiles.Count} Localization files");
        
            LoadUsedLocalizationIDs(allLocalizationFiles);
            Console.WriteLine($"Loaded {_usedLocalizationIDs.Count} IDs");
        
            var files = Directory.GetFiles(sourceFolder.FullName, "*.cs", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Exit("No .cs files found.");
                return;
            }
            Console.WriteLine($"Loaded {files.Length} files to localize");
        
            LoadExistingLocalizations(localizationFile);
            Console.WriteLine($"Loaded {_existingLocalizations.Count} own/existing localizations to reuse");

            LocalizeTexts(localizationFile, files);
            Console.WriteLine("\nAll texts got localized! Press any key to exit..");
            Console.ReadKey();
        }

        private static void Exit(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
        }

        private static void LoadExistingLocalizations(string localizationFile)
        {
            var localizationDocument = new XmlDocument();
            localizationDocument.Load(localizationFile);

            var stringNodes = localizationDocument.SelectNodes("/base/strings/*");
            if (stringNodes is null || stringNodes.Count == 0)
            {
                return;
            }

            var duplicatedNodes = new List<XmlNode>();
            var duplicatedIDs = new List<XmlNode>();
            var duplicatedTexts = new List<XmlNode>();

            foreach (XmlNode stringNode in stringNodes)
            {
                if (stringNode is null)
                {
                    continue;
                }

                var id = stringNode.Attributes!["id"]?.Value!;
                var text = stringNode.Attributes!["text"]?.Value!;

                if (_existingLocalizations.ContainsKey(id) && _existingLocalizations.ContainsValue(text))
                {
                    duplicatedNodes.Add(stringNode);
                    continue;
                }

                if (_existingLocalizations.ContainsKey(id) && !_existingLocalizations.ContainsValue(text))
                {
                    duplicatedIDs.Add(stringNode);
                    continue;
                }

                if (!_existingLocalizations.ContainsKey(id) && _existingLocalizations.ContainsValue(text))
                {
                    duplicatedTexts.Add(stringNode);
                    continue;
                }

                _existingLocalizations.Add(id, text);
            }

            foreach (var xmlNode in duplicatedNodes)
            {
                localizationDocument.SelectSingleNode("/base/strings")!.RemoveChild(xmlNode);
            }
            Console.WriteLine($"Fixed {duplicatedNodes.Count} duplicated localizations");

            //foreach (var duplicatedID in duplicatedIDs)
            //{
            //    var text = duplicatedID.Attributes!["text"]?.Value!;
            //    var localizedText = GetLocalizedText($"{{=!}}{text}");
            //    AddTextToLocalization(localizationDocument, localizedText.ID, localizedText.Text);
            //}
            //Console.WriteLine($"Fixed {duplicatedIDs.Count} duplicated IDs");

            //foreach (var duplicatedText in duplicatedTexts)
            //{
            //    var localizedText = GetLocalizedText();
            //}
            //Console.WriteLine($"Fixed {duplicatedTexts.Count} duplicated texts");

            localizationDocument.Save(localizationFile);
        }

        private static List<string> GetAllLocalizationFiles(string modulesFolder)
        {
            var localizationFiles = new List<string>();

            localizationFiles.AddRange(Directory.GetFiles(modulesFolder, "module_strings.xml", SearchOption.AllDirectories).ToList());
            localizationFiles.AddRange(Directory.GetFiles(modulesFolder, "std_*.xml", SearchOption.AllDirectories).ToList());

            return localizationFiles;
        }

        private static void LoadUsedLocalizationIDs(IEnumerable<string> localizationFiles)
        {
            foreach (var localizationFile in localizationFiles)
            {
                var fileContent = File.ReadAllText(localizationFile);

                var localizationDocument = new XmlDocument();
                localizationDocument.LoadXml(fileContent);

                var stringNodes = localizationDocument.SelectNodes("/base/strings/*");
                if (stringNodes is null || stringNodes.Count == 0)
                {
                    continue;
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

            var allFilesToLocalize = files.ToList();
            var filesToLocalize = allFilesToLocalize.ToList();

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
            Console.WriteLine($"Skipped {filesToRemove.Count} files without text to localize");

            var initialTextCount = texts.Count;
            Console.WriteLine($"Loaded {initialTextCount} texts to localize");

            var duplicates = texts.RemoveAll(t => string.IsNullOrWhiteSpace(t) || t.Contains("img src="));
            Console.WriteLine($"Skipped {duplicates} bad (empty or image) texts");

            texts = texts.Distinct().ToList();
            Console.WriteLine($"Removed {initialTextCount - texts.Count} duplicated texts");

            Console.WriteLine($"Localizing {texts.Count} texts in {filesToLocalize.Count} files..");
            var resUsedTexts = 0;
            for (var textIndex = 1; textIndex < texts.Count; textIndex++)
            {
                if (textIndex % 100 == 0)
                {
                    Console.WriteLine($"Localized {textIndex}/{texts.Count} texts..");
                }

                var text = texts[textIndex - 1];
                var textToLocalize = $"{{=!}}{text}";
                (string ID, string Text) localizedText;

                if (_existingLocalizations.ContainsValue(text))
                {
                    var localizationID = _existingLocalizations.FirstOrDefault(el => el.Value == text).Key;
                    localizedText = (localizationID, text.Replace("{=!}", $"{{={localizationID}}}"));

                    resUsedTexts++;
                }
                else
                {
                    localizedText = GetLocalizedText(textToLocalize);
                }

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

            Console.WriteLine($"Reused {resUsedTexts} texts");

            localizationDocument.Save(localizationFile);

            var initialReUseableTexts = _existingLocalizations.Count;
            RemoveUnusedLocalizations(localizationFile, allFilesToLocalize);
            Console.WriteLine($"Removed {initialReUseableTexts - _existingLocalizations.Count} existing localizations, which were not used anymore");
        }

        private static void RemoveUnusedLocalizations(string localizationFile, IEnumerable<string> files)
        {
            var localizationDocument = new XmlDocument();
            localizationDocument.Load(localizationFile);

            var stringNodes = localizationDocument.SelectNodes("/base/strings/*");
            if (stringNodes is null || stringNodes.Count == 0)
            {
                return;
            }

            var regex = new Regex("(?<=\"{=).{8}(?=})");
            var texts = files.Select(File.ReadAllText).ToList();
            var ids = texts.SelectMany(t => regex.Matches(t).Select(m => m.Value)).ToList();

            var nodesToRemove = new List<XmlNode>();
            foreach (XmlNode stringNode in stringNodes)
            {
                if (stringNode is null)
                {
                    continue;
                }

                var id = stringNode.Attributes!["id"]?.Value!;
                if (ids.Any(i => i == id))
                {
                    continue;
                }

                _existingLocalizations.Remove(id);
                nodesToRemove.Add(stringNode);
            }

            foreach (var xmlNode in nodesToRemove)
            {
                localizationDocument.SelectSingleNode("/base/strings")!.RemoveChild(xmlNode);
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