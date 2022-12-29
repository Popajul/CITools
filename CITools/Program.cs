using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Configuration;
using System.Dynamic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace CITools
{
    internal class Program
    {
        internal static string? _inputfilepath { get; set; }
        internal static string? _outputfolderpath { get; set; }
        internal static ExpandoObject? _jsonObject { get; set; }
        internal static IEnumerable<Endpoint>? _endpoints { get; set; }

        private static List<EndpointSelected> _endpointsSelected = new List<EndpointSelected>();
        private static bool @continue = true;

        static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            string welcome = $"CITools {version}\n\nFirst time here, use first \'config -input {{inputfilepath}} -output {{outputfolderpath}}\'\n";
            Console.WriteLine(welcome);
            InitializeEndpoints();
            DisplayUserChoiceCommands();
            while (@continue)
            {
                var command = Console.ReadLine();
                if (string.IsNullOrEmpty(command)) continue;
                DispatchCommand(command);
            }
        }


        #region UI Logic
        private static void DisplayUserChoiceCommands()
        {
            Console.WriteLine("\nAvailable commands : \n");
            Console.WriteLine("\t-> commands : Display available commands");
            Console.WriteLine("\t-> display -all: Display endpoints selected or all endpoints available if no one is selected" +
                "\n\t   -all to force displaying all endpoints");
            Console.WriteLine("\t-> select -i -k: Select endpoints by index or keyword, all selected endpoints are overwritten");
            Console.WriteLine("\t-> filter -i -k : remove selected endpoints by index or keyword" +
                "\n\t   only letters are supported for keyword parameter");
            Console.WriteLine("\t-> add -i -k: add endpoints to selection using index or keyword," +
                "\n\t   indexes are those from endpoints selected");
            Console.WriteLine("\t-> export -format yaml/json -filename: export CI in json or yaml," +
                "\n\t   yaml by default," +
                "\n\t   filename is \'output\' by default");
            Console.WriteLine("\t-> config -input -output -show : set input filepath and output folderpath," +
                "\n\t   -show display actual config");
            Console.WriteLine("\t-> quit : It does what it seems!\n");
        }
        private static void DispatchCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            // set regex to catch command and parameters
            var commandRegex = new Regex(@"(?<command>^\w+)");
            var indexRegex = new Regex(@"(\d{1,})");
            var keywordRegex = new Regex(@"-k\s([a-zA-Z]{1,})");
            var filenameRegex = new Regex(@"-filename\s([a-zA-Z]{3,})");
            var inputfilepathRegex = new Regex(@"-input\s(?<input1>(.+)\\([^\\]+))\s-|-input\s(?<input2>(.+)\\([^\\]+))");
            var outputfolderpathRegex = new Regex(@"-output\s(?<output1>(.+)\\([^\\]+))\s-|-output\s(?<output2>(.+)\\([^\\]+))");

            // booleans
            var isIndex = command.Contains("-i");
            var isKeyword = command.Contains("-k");
            var isFileName = command.Contains("-filename");
            var isInputFilePath = command.Contains("-input");
            var isOutputFolderPath = command.Contains("-output");
            var show = command.Contains("-show");
            var displayAll = command.Contains("-all");
            bool wantJson = command.Contains("json", StringComparison.OrdinalIgnoreCase);

            IEnumerable<int>? indexes;
            string? keyword;
            string? absolutecommand;
            string? filename;
            string? inputfilepath;
            string? outputfolderpath;
            // catch command and parameters
            try
            {
                keyword = isKeyword ? keywordRegex.Matches(command).Single().Groups[1].Value : null;
                indexes = isIndex ? indexRegex.Matches(command).SelectMany(m => m.Captures, (m, c) => Convert.ToInt32(c.Value)) : null;
                absolutecommand = commandRegex.Matches(command).Single(m => m.Groups.ContainsKey("command")).Groups.Values.Single(v => v.Name.Equals("command")).Value.ToLower();
                filename = isFileName ? filenameRegex.Matches(command).Single().Groups[1].Value : null;
                inputfilepath = isInputFilePath ? inputfilepathRegex.Matches(command).Single(m => m.Groups.ContainsKey("input1") || m.Groups.ContainsKey("input2")).Groups.Values.Where(v => (v.Name == "input1" || v.Name == "input2") && v.Value != "").Single().Value : null;
                outputfolderpath = isOutputFolderPath ? outputfolderpathRegex.Matches(command).Single(m => m.Groups.ContainsKey("output1") || m.Groups.ContainsKey("output2")).Groups.Values.Where(v => (v.Name == "output1" || v.Name == "output2") && v.Value != "").Single().Value : null;
            }
            catch
            {
                Console.WriteLine("Something went wrong while reading command");
                return;
            }
            

            switch (absolutecommand)
            {
                case "commands":
                    DisplayUserChoiceCommands();
                    break;
                case "display":
                    DisplayEndpointsAction(displayAll);
                    break;
                case "select":
                    SelectAction(indexes, keyword);
                    break;
                case "add":
                    AddAction(indexes, keyword);
                    break;
                case "quit":
                    @continue = false;
                    break;
                case "filter":
                    FilterAction(indexes, keyword);
                    break;
                case "export":
                    ExportAction(wantJson, filename);
                    break;
                case "config":
                    ConfigAction(inputfilepath, outputfolderpath, show);
                    break;
                default:
                    Console.WriteLine("\nUnknown Command\n");
                    break;

            };
        }
        #endregion
        #region Actions
        private static void ConfigAction(string? inputFilePath, string? outputFolderPath, bool show)
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
            InitializeEndpoints();

            if (show)
            {
                Console.WriteLine($"Input file path : {ConfigurationManager.AppSettings["inputpath"]}");
                Console.WriteLine($"Output folder path : {ConfigurationManager.AppSettings["outputpath"]}");
            }

            return;
        }
        private static void ExportAction(bool wantJson, string? filename)
        {
            if (string.IsNullOrEmpty(filename))
                filename = "output";
            var jsonPathObject = _jsonObject!.Single(p => p.Key == "paths").Value as ExpandoObject ?? throw new DeserializeException();
            // remove unselected paths
            var selectedPathKeys = _endpointsSelected.Select(f => f.Endpoint!.Path.Key).ToList();
            var pathKeysToRemove = _endpoints!.Where(e => !selectedPathKeys.Contains(e.Path.Key)).Select(p => p.Path.Key).ToList();
            foreach (var key in pathKeysToRemove)
            {
                _ = jsonPathObject!.Remove(key, out _);
            }
            // remove unused verbs
            foreach (var endpoint in _endpoints!)
            {
                var unusedVerbs = endpoint!.Verbs!.Where(v => !_endpointsSelected.Where(e => e.Endpoint!.Path.Key.Equals(endpoint.Path.Key)).Select(e => e.VerbSelected).Contains(v)).ToList();
                foreach (var unusedVerb in unusedVerbs)
                    _ = endpoint!.Path!.Value!.Remove(unusedVerb.Key, out _);
            }

            // remove unsused schema
            List<string> schemaKeysSelected = new List<string>();
            var schemas = ((_jsonObject!.Single(p => p.Key == "components").Value as ExpandoObject ?? throw new DeserializeException())
                .Single(p => p.Key == "schemas").Value as ExpandoObject);

            GetSchemaKeyReferencedByExpandoObject(jsonPathObject, schemas!, schemaKeysSelected);

            var schemaKeysToRemove = schemas!.Where(p => !schemaKeysSelected.Contains(p.Key)).Select(s => s.Key).ToList();

            foreach (var key in schemaKeysToRemove)
                _ = schemas!.Remove(key, out _);

            // export json
            if (wantJson)
            {
                var json = JsonConvert.SerializeObject(_jsonObject, Formatting.Indented);
                File.WriteAllText($"{_outputfolderpath}{filename}.json", json);
            }
            else
            {
                // export yaml
                var serializer = new YamlDotNet.Serialization.Serializer();
                string yaml = serializer.Serialize(_jsonObject!);
                File.WriteAllText($"{_outputfolderpath}{filename}.yaml", yaml);
            }
            

            InitializeEndpoints();

            return;
        }
        private static void FilterAction(IEnumerable<int>? indexes, string? keyword)
        {

            if (indexes != null && indexes.Any())
            {
                var endPointsToRemove = indexes.Select(i => _endpointsSelected.ElementAt(i)).ToList();
                foreach (var endpoint in endPointsToRemove)
                    _endpointsSelected.Remove(endpoint);
                return;
            }
            if (keyword != null)
            {
                foreach (var endpoint in GetEndpointSelectedByKeyword(keyword).ToList())
                    _endpointsSelected.Remove(endpoint);
            }
        }
        private static void AddAction(IEnumerable<int>? indexes, string? keyword)
        {
            if (indexes != null && indexes.Any())
            {
                _endpointsSelected.AddRange(GetEndpointByIndex(indexes));
                return;
            }
            if (keyword != null)
            {
                _endpointsSelected.AddRange(GetEndpointByKeyword(keyword));
            }
        }
        private static void SelectAction(IEnumerable<int>? indexes, string? keyword)
        {
            if (indexes != null && indexes.Any())
            {
                SelectEndpointByIndexes(indexes);
                return;
            }
            if (keyword != null)
            {
                SelectEndpointByKeyword(keyword);
            }
        }
        #endregion
        #region Utils
        private static void InitializeEndpoints()
        {
            _inputfilepath = ConfigurationManager.AppSettings["inputpath"];
            _outputfolderpath = ConfigurationManager.AppSettings["outputpath"];
            SetJsonObject();
            SetEndPoints();
        }
        private static void SetJsonObject()
        {
            try
            {
                string json = File.ReadAllText(_inputfilepath!, System.Text.Encoding.UTF8) ?? throw new NullReferenceException("Invalid json");
                var expConverter = new ExpandoObjectConverter();

                _jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(json, expConverter) ?? throw new DeserializeException();
            }
            catch
            {

            }

        }

        private static void SetEndPoints()
        {
            if (_jsonObject == null)
                _endpoints = Enumerable.Empty<Endpoint>();
            _endpoints =
            ((_jsonObject!.Single(p => p.Key == "paths").Value as ExpandoObject) ?? throw new DeserializeException())
                .Select(p => new Endpoint() { Path = new KeyValuePair<string, ExpandoObject>(key: p.Key, value: (p.Value as ExpandoObject) ?? throw new DeserializeException()) });
        }
        private static void DisplayEndpoints()
        {
            Console.WriteLine("\nAvailable endpoints :\n");
            int index = 0;
            foreach (var endpoint in _endpoints!)
            {
                foreach (var verb in endpoint.Verbs!)
                {
                    Console.WriteLine($"\t-> {index} : {verb.Key} : {endpoint.Path.Key}");
                    index++;
                }
            }
            Console.WriteLine("\n");
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

        private static void SelectEndpointByIndexes(IEnumerable<int> indexes)
        {
            _endpointsSelected = GetEndpointByIndex(indexes);
        }
        private static void SelectEndpointByKeyword(string keyword)
        {
            _endpointsSelected = GetEndpointByKeyword(keyword);
        }
        private static IEnumerable<EndpointSelected> GetEndpointSelectedByKeyword(string keyword)
        {
            return _endpointsSelected.Where(e => e.Endpoint!.Path.Key.Contains(keyword));

        }
        private static void DisplayEndpointsSelected()
        {
            Console.WriteLine("\nSelected endpoints : \n");
            var index = 0;
            foreach (var endpoint in _endpointsSelected)
            {
                Console.WriteLine($"\t-> {index} : {endpoint.VerbSelected.Key} : {endpoint.Endpoint!.Path.Key}");
                index++;
            }
            Console.WriteLine("\n");
        }
        private static List<EndpointSelected> GetEndpointByIndex(IEnumerable<int> indexes)
        {
            return indexes.Select(i => _endpoints!.SelectMany(e => e.Verbs!, (e, v) => new { Endpoint = e, Verb = v }).ElementAt(i)).Select(e => new EndpointSelected() { Endpoint = e.Endpoint, VerbSelected = e.Verb }).ToList();
        }
        private static List<EndpointSelected> GetEndpointByKeyword(string keyword)
        {
            return _endpoints!.Where(e => e.Path.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .SelectMany(e => e.Verbs!, (e, v) => new { Endpoint = e, Verb = v })
                .Select(e => new EndpointSelected() { Endpoint = e.Endpoint, VerbSelected = e.Verb }).ToList();
        }
        private static void DisplayEndpointsAction(bool displayAll)
        {
            if (_endpointsSelected != null && _endpointsSelected.Any() && !displayAll)
            {
                DisplayEndpointsSelected();
                return;
            }
            DisplayEndpoints();
        }
        #endregion
    }

    internal class Endpoint
    {
        // To get EndPoint Path select Key
        internal KeyValuePair<string, ExpandoObject> Path { get; set; }
        // To get Endpoint verb select Key of an element
        internal IEnumerable<KeyValuePair<string, ExpandoObject>>? Verbs { get => Path.Value.Select(a => new KeyValuePair<string, ExpandoObject>(a.Key, (a.Value! as ExpandoObject) ?? throw new DeserializeException())); }
    }

    internal class EndpointSelected
    {
        // To get EndPoint Path select Key
        internal Endpoint? Endpoint { get; set; }
        internal KeyValuePair<string, ExpandoObject> VerbSelected { get; set; }
    }

    internal class DeserializeException : NullReferenceException
    {
        public override string Message { get; }
        internal DeserializeException()
        {
            Message = "Error occurs while deserializing json";
        }
    }

}