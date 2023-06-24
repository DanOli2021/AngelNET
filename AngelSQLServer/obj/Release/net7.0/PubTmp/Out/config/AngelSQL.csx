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
d.Add("data_directory", "" );
d.Add("account","account1");
d.Add("account_user", "angelsql");
d.Add("account_password", "angelsql123");
d.Add("database", "database1");
d.Add("request_timeout", "4");
d.Add("wwwroot", "");

Environment.SetEnvironmentVariable("ANGELSQL_PARAMETERS", JsonConvert.SerializeObject(d, Formatting.Indented));

Dictionary<string, string> servers = new Dictionary<string, string>();
// Optional parameters
servers.Add("tockens_url", "http://localhost:5000/AngelPOST");
servers.Add("skus_url", "http://localhost:5000/AngelPOST");
servers.Add("sales_url", "http://localhost:5000/AngelPOST");
servers.Add("inventory_url", "http://localhost:5000/AngelPOST");
servers.Add("configuration_url", "http://localhost:5000/AngelPOST");

// Development time only
foreach (string key in servers.Keys)
{
    servers[key] = "https://localhost:7170/AngelPOST";
}

Environment.SetEnvironmentVariable("ANGELSQL_SERVERS", JsonConvert.SerializeObject(servers, Formatting.Indented));

return JsonConvert.SerializeObject(d);
