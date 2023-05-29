namespace AngelSQL
{
    public class AngelClientsCommands
    {
        public static Dictionary<string, string> DbCommands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"CLIENTS", @"CLIENTS#free" },
                { @"COUNT CLIENTS", @"COUNT CLIENTS#free" },
                { @"KILL CLIENT", @"KILL CLIENT#free" }
        };

        return commands;

        }
    }
}