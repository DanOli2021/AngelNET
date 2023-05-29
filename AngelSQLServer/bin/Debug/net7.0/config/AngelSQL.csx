// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#r "DB.dll"
#r "Newtonsoft.Json.dll"

using Newtonsoft.Json;
using System.Collections.Generic;

Dictionary<string, string> d = new Dictionary<string, string>();
d.Add("certificate", "");
d.Add("password", "");
d.Add("bind_ip", "");
d.Add("bind_port", "12000");
d.Add("urls", "");
d.Add("cors", "https://localhost:11000");
d.Add("master_user", "db" );
d.Add("master_password", "db");
d.Add("data_directory", "c:/posdata" );
d.Add("account","account1");
d.Add("account_user", "angelsql");
d.Add("account_password", "angelsql123");
d.Add("database", "database1");
d.Add("request_timeout", "4");


return JsonConvert.SerializeObject(d);
