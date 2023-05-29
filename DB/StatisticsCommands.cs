using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelDB
{
    public static class StatisticsCommands
    {
        public static Dictionary<string, string> Commands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"ANALYSIS", @"ANALYSIS#free;FROM#free;VARIABLE#freeoptional" },
                { @"SHOW ANALYSIS", @"SHOW ANALYSIS#free" },
                { @"CONFIDENCE INTERVAL OF", @"CONFIDENCE INTERVAL OF#free;CONFIDENCE LEVEL#free;PROPORTION#freeoptional" },                
                { @"CONFIDENCE INTERVAL BETWEEN", @"CONFIDENCE INTERVAL BETWEEN#free;AND#free;CONFIDENCE LEVEL#free" },
                { @"PROPORTION COMPARE", @"PROPORTION COMPARE#free;BETWEEN#free;AND#free;CONFIDENCE LEVEL#free" },
                { @"SET VARIABLES", @"SET VARIABLES#free;JSON#freeoptional;VARIABLE#freeoptional;VALUE#freeoptional" },
                { @"SAMPLE SIZE", @"SAMPLE SIZE#free;POPULATION SIZE#number;CONFIDENCE LEVEL#number;MARGIN OF ERROR#number" }
            };

            return commands;

        }

    }
}
