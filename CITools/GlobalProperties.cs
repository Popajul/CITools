using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITools
{
    internal static class GlobalProperties
    {
        internal static bool @continue = true;
        internal static string? _inputfilepath { get; set; }
        internal static string? _outputfolderpath { get; set; }
        internal static ExpandoObject? _jsonObject { get; set; }
        internal static IEnumerable<Endpoint>? _endpoints { get; set; }

        internal static List<EndpointSelected> _endpointsSelected = new List<EndpointSelected>();
        internal static bool _start { get; set; }

        internal static void InitializeEndpoints()
        {
            _inputfilepath = ConfigurationManager.AppSettings["inputpath"];
            _outputfolderpath = ConfigurationManager.AppSettings["outputpath"];
            SetJsonObject();
            SetEndPoints();
            _start = _endpoints!.Any();
        }
        private static void SetJsonObject()
        {
            try
            {
                string json = File.ReadAllText(_inputfilepath!, System.Text.Encoding.UTF8) ?? throw new NullReferenceException("Invalid json");
                var expConverter = new ExpandoObjectConverter();

                _jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(json, expConverter) ?? throw new DeserializeException();
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message + "\n");
            }

        }

        private static void SetEndPoints()
        {
            if (_jsonObject == null)
            {
                _endpoints = Enumerable.Empty<Endpoint>();
                return;
            }

            _endpoints =
            ((_jsonObject!.Single(p => p.Key == "paths").Value as ExpandoObject) ?? throw new DeserializeException())
                .Select(p => new Endpoint() { Path = new KeyValuePair<string, ExpandoObject>(key: p.Key, value: (p.Value as ExpandoObject) ?? throw new DeserializeException()) });
        }
    }
}
