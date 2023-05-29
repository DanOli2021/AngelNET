// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#r "AngelSQL.dll"
#r "DB.dll"
#r "Newtonsoft.Json.dll"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;

Dictionary<string, string> my_data = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

switch (my_data["service"])
{
    case "skus":

        return db.Prompt($"SCRIPT FILE scripts/pos/skus.csx DATA {message}");

    default:
        return "Error: No service found";
}

