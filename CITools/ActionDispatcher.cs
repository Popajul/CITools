using System.Text.RegularExpressions;

namespace CITools
{
    internal static class ActionDispatcher
    {
        private static Regex _commandreg = new Regex(@"(?<command>^\w+)");
        private static Regex _outputreg = new Regex(@"((?:-output\s?)(?<output>\S+\\*)(?:\s*))");
        private static Regex _inputreg = new Regex(@"((?:-input\s?)(?<input>\S+\\*)\s*)");
        private static Regex _indexesreg = new Regex("((?:-i\\s?)(?<indexes>(\\d+,*)*)(?:\\s*))");
        private static Regex _keywordreg = new Regex(@"((?:-k\s?)(?<keyword>\S+))");
        private static Regex _filenamereg = new Regex(@"((?:-filename\s?)(?<filename>[A-Za-z0-9]+)(?:\s*))");
        private static Regex[] _regexes = new Regex[] { _commandreg, _outputreg, _inputreg, _indexesreg, _keywordreg, _filenamereg };

        private static void DispatchInitializeConfig(string command)
        {
            var regexes = new Regex[] { _commandreg, _inputreg, _outputreg };

            var results = regexes.Select(r => r.Match(command))
                                .Where(m => m.Success)
                                .SelectMany(m => m.Groups.Values)
                                .Where(m => char.IsLetter(m.Name.First()));
            // booleans
            var isInputFilePath = command.Contains("-input");
            var isOutputFolderPath = command.Contains("-output");
            var show = command.Contains("-show");

            string absolutecommand = "";
            string? inputfilepath = null;
            string? outputfolderpath = null;
            // catch command and parameters
            try
            {
                absolutecommand = results.SingleOrDefault(r => r.Name == "command")?.Value ?? throw new InvalidOperationException("ERROR : no command name detected");
                inputfilepath = isInputFilePath ? results.SingleOrDefault(r => r.Name == "input")?.Value : null;
                outputfolderpath = isOutputFolderPath ? results.SingleOrDefault(r => r.Name == "output")?.Value : null;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message + "\n");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }


            switch (absolutecommand)
            {
                case "commands":
                    Actions.DisplayUserChoiceCommands();
                    break;
                case "quit":
                    GlobalProperties .@continue = false;
                    break;
                case "config":
                    Actions.ConfigAction(inputfilepath, outputfolderpath, show);
                    break;
                default:
                    Console.WriteLine("\nUnknown Command\n");
                    break;
            };
        }
        internal static void DispatchCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            if (!GlobalProperties._start)
            {
                DispatchInitializeConfig(command);
                return;
            }

            var results = _regexes.Select(r => r.Match(command))
                               .Where(m => m.Success)
                               .SelectMany(m => m.Groups.Values)
                               .Where(m => char.IsLetter(m.Name.First()));

            // booleans
            var isIndex = command.Contains("-i ");
            var isKeyword = command.Contains("-k ");
            var isFileName = command.Contains("-filename ");
            var isInputFilePath = command.Contains("-input ");
            var isOutputFolderPath = command.Contains("-output ");
            var show = command.Contains("-show");
            var displayAll = command.Contains("-all");
            bool wantJson = command.Contains("json", StringComparison.OrdinalIgnoreCase);

            int[]? indexes = null;
            string? keyword = null;
            string absolutecommand = "";
            string? filename = null;
            string? inputfilepath = null;
            string? outputfolderpath = null;
            // catch command and parameters
            try
            {
                keyword = isKeyword ? results.SingleOrDefault(r => r.Name == "keyword")?.Value : null;
                indexes = isIndex ? results.SingleOrDefault(r => r.Name == "indexes")?.Value?.Split(",").Select(s =>
                {
                    try
                    {
                        return Convert.ToInt32(s);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException("ERROR: index must have an integer value");
                    }
                }).ToArray() : null;
                absolutecommand = results.SingleOrDefault(r => r.Name == "command")?.Value?.ToLower() ?? throw new InvalidOperationException("ERROR : no command name detected");
                filename = isFileName ? results.SingleOrDefault(r => r.Name == "filename")?.Value : null;
                inputfilepath = isInputFilePath ? results.SingleOrDefault(r => r.Name == "input")?.Value : null;
                outputfolderpath = isOutputFolderPath ? results.SingleOrDefault(r => r.Name == "output")?.Value : null;
            }
            catch (InvalidOperationException e)
            {
                DisplayUtils.DisplayMessage(e.Message);
                return;
            }
            catch (Exception e)
            {
                DisplayUtils.DisplayMessage(e.Message);
                return;
            }

            switch (absolutecommand)
            {
                case "commands":
                    Actions.DisplayUserChoiceCommands();
                    break;
                case "display":
                    Actions.DisplayEndpointsAction(displayAll);
                    break;
                case "select":
                    Actions.SelectAction(indexes, keyword);
                    break;
                case "add":
                    Actions.AddAction(indexes, keyword);
                    break;
                case "quit":
                    GlobalProperties .@continue = false;
                    break;
                case "filter":
                    Actions.FilterAction(indexes, keyword);
                    break;
                case "export":
                    Actions.ExportAction(wantJson, filename);
                    break;
                case "config":
                    Actions.ConfigAction(inputfilepath, outputfolderpath, show);
                    break;
                default:
                    DisplayUtils.DisplayMessage("Unknown Command");
                    break;
            };
        }
    }
}
