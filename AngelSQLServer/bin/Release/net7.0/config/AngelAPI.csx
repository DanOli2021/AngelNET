// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));
Dictionary<string, string> messaje_dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

Console.WriteLine($"AngelAPI.csx: {message}");

switch (messaje_dic["service"])
{
    case "help":

        return db.Prompt($"SCRIPT FILE {parameters["scripts_directory"]}/help.csx MESSAGE {messaje_dic["command"]}");

    default:
        return "Error: No service found";
}
