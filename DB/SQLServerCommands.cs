using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelDB
{
    internal class SQLServerCommands
    {
        public static Dictionary<string, string> Commands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"CONNECT", @"CONNECT#free;ALIAS#free" },
                { @"QUERY", @"QUERY#free;CONNECTION ALIAS#free" },
                { @"SAVE ACCOUNTS TO", @"SAVE ACCOUNTS TO#free;PASSWORD#password" },
                { @"RESTORE ACCOUNTS FROM", @"RESTORE ACCOUNTS FROM#free;PASSWORD#password" },
                { @"SHOW CONNECTIONS", @"SHOW CONNECTIONS#free" },
            };

            return commands;

        }
    }
}
