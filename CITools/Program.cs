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
        static void Main()
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            DisplayUtils.DisplayMessage(name);
            GlobalProperties.InitializeEndpoints();
            if (!GlobalProperties._start)
            {
                DisplayUtils.DisplayMessage("You must Set input path to a valid json file using this command :\n'config -input {inputfilepath} -output {outputfolderpath}'");
            }
            Actions.DisplayUserChoiceCommands();
            
            while (GlobalProperties.@continue)
            {
                var command = Console.ReadLine();
                if (string.IsNullOrEmpty(command)) continue;
                ActionDispatcher.DispatchCommand(command);
            }
        }
    }

}