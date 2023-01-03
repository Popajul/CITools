using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CITools
{
    internal static class DisplayUtils
    {
        internal const string CommandsDisplayMessage =
            @"Available commands :

        -> commands : Display available commands
        -> display -all: Display endpoints selected or all endpoints available if no one is selected
           -all to force displaying all endpoints
        -> select -i -k: Select endpoints by index or keyword, all selected endpoints are overwritten
        -> filter -i -k : remove selected endpoints by index or keyword
           only letters are supported for keyword parameter
        -> add -i -k: add endpoints to selection using index or keyword,
           indexes are those from endpoints selected
        -> export -format yaml/json -filename: export CI in json or yaml,
           yaml by default,
           filename is 'output' by default
        -> config -input -output -show : set input filepath and output folderpath,
           -show display actual config
        -> quit : It does what it seems!";

        internal const string CommandsDisplayInitializeMessage =
            @"Available commands :

        -> commands : Display available commands
        -> config -input -output -show : set input filepath and output folderpath,
           -show display actual config
        -> quit : It does what it seems!";
        internal static void DisplayMessage(string? message)
        {
            var sb = new StringBuilder(message);
            sb.Insert(0, "\n");
            sb.Append("\n");
            Console.WriteLine(sb.ToString());
        }

        internal static void DisplayEndpoints(IEnumerable<Endpoint> endpoints)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"Available endpoints :");
            sb.AppendLine();
            sb.AppendLine(@"    index :");
            
            foreach (var endpoint in endpoints)
            {
                    sb.AppendLine($@"    -> {endpoint.Index}{string.Join("", Enumerable.Range(1, 3 - endpoint.Index.ToString().Length).Select(i => " "))} : {endpoint.Verb.Key}{string.Join("", Enumerable.Range(1, 6 - endpoint.Verb.Key.Length).Select(i => " "))} || {endpoint.Path.Key}");
            }
            DisplayMessage(sb.ToString());
        }

        internal static void DisplayEndpointsSelected(IEnumerable<EndpointSelected> endpoints)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"Selected endpoints :");
            sb.AppendLine();
            sb.AppendLine(@"    index :");

            int index = 0;
            foreach (var endpoint in endpoints)
            {
                sb.AppendLine($@"    -> {index}{string.Join("", Enumerable.Range(1, 3 - index.ToString().Length).Select(i => " "))} : {endpoint.VerbSelected.Key}{string.Join("", Enumerable.Range(1, 6 - endpoint.VerbSelected.Key.Length).Select(i => " "))} : {endpoint.Endpoint!.Path.Key}");
                index++;
            }
            DisplayMessage(sb.ToString());
        }
    }
}
