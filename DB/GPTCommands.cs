using System.Collections.Generic;

namespace AngelDB
{
    public static class GPTCommands
    {
        public static Dictionary<string, string> Commands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"SET API KEY", @"SET API KEY#free" },
                { @"SET END POINT", @"SET END POINT#free" },
                { @"SET MAX TOCKENS", @"SET MAX TOCKENS#numeric" },
                { @"SAVE ACCOUNT TO", @"SAVE ACCOUNT TO#free;PASSWORD#password" },
                { @"RESTORE ACCOUNT FROM", @"RESTORE ACCOUNT FROM#free;PASSWORD#password" },
                { @"PROMPT", @"PROMPT#free" },
                { @"PROMPT PREVIEW", @"PROMPT PREVIEW#free" },
                { @"START CHAT", @"START CHAT#free" },
                { @"ADD USER INPUT", @"ADD USER INPUT#free" },
                { @"ADD EXAMPLE OUTPUT", @"ADD EXAMPLE OUTPUT#free" },
                { @"GET CHAT", @"GET CHAT#free" },
                { @"GET MODELS", @"GET MODELS#free" }
            };

            return commands;

        }
    }
}
