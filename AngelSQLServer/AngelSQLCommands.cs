namespace AngelSQL
{
    public class AngelSQLCommands
    {
        public static Dictionary<string, string> DbCommands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"START PARAMETERS", @"START PARAMETERS#free;CONFIG FILE#freeoptional;API FILE#freeoptional" }
            };

            return commands;

        }
    }
}