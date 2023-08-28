namespace AngelSQL
{
    public class AngelSQLCommands
    {
        public static Dictionary<string, string> DbCommands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"SAVE ACTIVITY ON", @"SAVE ACTIVITY ON#free" },
                { @"SAVE ACTIVITY OFF", @"SAVE ACTIVITY OFF#free" },
                { @"WHITE LIST ON", @"WHITE LIST ON#free" },
                { @"WHITE LIST OFF", @"WHITE LIST OFF#free" },
                { @"BLACK LIST ON", @"WHITE LIST ON#free" },
                { @"BLACK LIST OFF", @"WHITE LIST OFF#free" },
                { @"ADD TO BLACK LIST", @"ADD TO BLACK LIST#free;COMMENT#freeoptional" },
                { @"REMOVE FROM BLACK LIST", @"REMOVE FROM BLACK LIST#free" },
                { @"ADD TO WHITE LIST", @"ADD TO WHITE LIST#free;COMMENT#freeoptional" },
                { @"REMOVE FROM WHITE LIST", @"REMOVE FROM WHITE LIST#free" },
            };

            return commands;

        }
    }
}