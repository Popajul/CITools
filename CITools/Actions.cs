using Newtonsoft.Json;
using System.Configuration;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace CITools
{
    internal static class Actions
    {
        internal static void DisplayUserChoiceCommands()
        {
            if (!GlobalProperties._start)
            {
                DisplayUtils.DisplayMessage(DisplayUtils.CommandsDisplayInitializeMessage);
                return;
            }
            DisplayUtils.DisplayMessage(DisplayUtils.CommandsDisplayMessage);
        }

        internal static void DisplayEndpointsAction(bool displayAll)
        {
            if (GlobalProperties._endpointsSelected != null && GlobalProperties._endpointsSelected.Any() && !displayAll)
            {
                DisplayUtils.DisplayEndpointsSelected(GlobalProperties._endpointsSelected);
                return;
            }
            DisplayUtils.DisplayEndpoints(GlobalProperties._endpoints!);
        }
        internal static void ConfigAction(string? inputFilePath, string? outputFolderPath, bool show)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (!string.IsNullOrEmpty(inputFilePath))
            {
                config.AppSettings.Settings.Remove("inputpath");
                config.AppSettings.Settings.Add("inputpath", inputFilePath);
                config.Save(ConfigurationSaveMode.Modified);
            }
            if (!string.IsNullOrEmpty(outputFolderPath))
            {
                if (!outputFolderPath.EndsWith("\\"))
                {
                    outputFolderPath += "\\";
                }

                config.AppSettings.Settings.Remove("outputpath");
                config.AppSettings.Settings.Add("outputpath", outputFolderPath);
                config.Save(ConfigurationSaveMode.Modified);
            }

            ConfigurationManager.RefreshSection("appSettings");
            GlobalProperties.InitializeEndpoints();

            if (show)
            {
                Console.WriteLine($"Input file path : {ConfigurationManager.AppSettings["inputpath"]}");
                Console.WriteLine($"Output folder path : {ConfigurationManager.AppSettings["outputpath"]}");
            }

            return;
        }
        internal static void ExportAction(bool wantJson, string? filename)
        {
            if (!Directory.Exists(GlobalProperties._outputfolderpath))
            {
                Console.WriteLine("\nEdit config to set valid output directory path\n");
                return;
            }

            if (string.IsNullOrEmpty(filename))
                filename = "output";

            var jsonPathObject = GlobalProperties._jsonObject!.Single(p => p.Key == "paths").Value as ExpandoObject ?? throw new DeserializeException();

            // remove unselected paths
            var selectedPathKeys = GlobalProperties._endpointsSelected.Select(f => f.Endpoint!.Path.Key).ToList();
            var pathKeysToRemove = GlobalProperties._endpoints!.Where(e => !selectedPathKeys.Contains(e.Path.Key)).Select(p => p.Path.Key).ToList();
            foreach (var key in pathKeysToRemove)
            {
                _ = jsonPathObject!.Remove(key, out _);
            }

            // remove unused verbs
            foreach (var endpoint in GlobalProperties._endpoints!)
            {
                var unusedVerbs = endpoint!.Verbs!.Where(v => !GlobalProperties._endpointsSelected.Where(e => e.Endpoint!.Path.Key.Equals(endpoint.Path.Key)).Select(e => e.VerbSelected).Contains(v)).ToList();
                foreach (var unusedVerb in unusedVerbs)
                    _ = endpoint!.Path!.Value!.Remove(unusedVerb.Key, out _);
            }

            // remove unsused schema
            List<string> schemaKeysSelected = new List<string>();
            var schemas = ((GlobalProperties._jsonObject!.Single(p => p.Key == "components").Value as ExpandoObject ?? throw new DeserializeException())
                .Single(p => p.Key == "schemas").Value as ExpandoObject);

            GetSchemaKeyReferencedByExpandoObject(jsonPathObject, schemas!, schemaKeysSelected);

            var schemaKeysToRemove = schemas!.Where(p => !schemaKeysSelected.Contains(p.Key)).Select(s => s.Key).ToList();

            foreach (var key in schemaKeysToRemove)
                _ = schemas!.Remove(key, out _);

            // export json
            if (wantJson)
            {
                var json = JsonConvert.SerializeObject(GlobalProperties._jsonObject, Formatting.Indented);
                File.WriteAllText($"{GlobalProperties._outputfolderpath}{filename}.json", json);
            }
            else
            {
                // export yaml
                var serializer = new YamlDotNet.Serialization.Serializer();
                string yaml = serializer.Serialize(GlobalProperties._jsonObject!);
                File.WriteAllText($"{GlobalProperties._outputfolderpath}{filename}.yaml", yaml);
            }


            GlobalProperties.InitializeEndpoints();
            RefreshSelection();
            return;
        }
        internal static void FilterAction(IEnumerable<int>? indexes, string? keyword)
        {
            if (indexes != null && indexes.Any())
            {
                var endPointsToRemove = indexes.Select(i => GlobalProperties._endpointsSelected.ElementAt(i)).ToList();
                foreach (var endpoint in endPointsToRemove)
                    GlobalProperties._endpointsSelected.Remove(endpoint);
                return;
            }
            if (keyword != null)
            {
                foreach (var endpoint in GlobalProperties._endpointsSelected.GetEndpointSelectedByKeyword(keyword).ToList())
                    GlobalProperties._endpointsSelected.Remove(endpoint);
            }
        }
        internal static void AddAction(IEnumerable<int>? indexes, string? keyword)
        {
            if (indexes != null && indexes.Any())
            {
                GlobalProperties._endpointsSelected.AddRange(GlobalProperties._endpoints!.GetEndpointByIndex(indexes));
                return;
            }
            if (keyword != null)
            {
                GlobalProperties._endpointsSelected.AddRange(GlobalProperties._endpoints!.GetEndpointByKeyword(keyword));
            }
        }
        internal static void SelectAction(IEnumerable<int>? indexes, string? keyword)
        {
            if (indexes != null && indexes.Any())
            {
                GlobalProperties._endpointsSelected = GlobalProperties._endpoints!.GetEndpointByIndex(indexes);
                return;
            }
            if (keyword != null)
            {
                GlobalProperties._endpointsSelected = GlobalProperties._endpoints!.GetEndpointByKeyword(keyword);
            }
        }

        private static void GetSchemaKeyReferencedByExpandoObject(ExpandoObject expandoToParse, ExpandoObject schemas, List<string> schemaKeysSelected)
        {
            var schemaRefRegex = new Regex(@"\$ref"": ""#\/components\/schemas\/([a-zA-Z]{3,})");

            string json = JsonConvert.SerializeObject(expandoToParse, Formatting.Indented);

            var schemaKeysToKeep = schemaRefRegex.Matches(json).Select(e => e.Groups[1].Value).ToList();
            if (schemaKeysToKeep.Any())
            {
                schemaKeysSelected.AddRange(schemaKeysToKeep);
                foreach (var key in schemaKeysToKeep)
                {
                    var schema = schemas.Single(s => s.Key == key).Value as ExpandoObject;
                    GetSchemaKeyReferencedByExpandoObject(schema!, schemas, schemaKeysSelected);
                }
            }
        }
        private static void RefreshSelection()
        {
            GlobalProperties._endpointsSelected = GlobalProperties._endpoints!.GetEndpointByIndex(GlobalProperties._endpointsSelected.Select(e => e.InitialIndex).ToList()).ToList();
                
        }
    }
}
