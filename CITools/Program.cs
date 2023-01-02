namespace CITools
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("CITools 1.0.0.29\n");
            GlobalProperties.InitializeEndpoints();
            if (!GlobalProperties._start)
            {
                DisplayUtils.DisplayMessage("You must set input path to a valid json file using this command :\n'config -input {inputfilepath} -output {outputfolderpath}'");
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