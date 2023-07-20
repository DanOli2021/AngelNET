// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#r "Newtonsoft.Json.dll"

using Newtonsoft.Json;
using System.Collections.Generic;

Dictionary<string, string> parameters = new Dictionary<string, string>
{
    { "certificate", "" },
    { "password", "" },
    { "urls", "" },
    { "cors", "http://localhost:11000" },
    { "master_user", "db" },
    { "master_password", "db" },
    { "data_directory", "" },
    { "account", "account1" },
    { "account_user", "angelsql" },
    { "account_password", "angelsql123" },
    { "database", "database1" },
    { "request_timeout", "4" },
    { "wwwroot", "" },
    { "scripts_directory", "" },
    { "smtp", "" },
    { "smtp_port", "" },
    { "email_address", "" },
    { "email_password", "" },
};


Environment.SetEnvironmentVariable("ANGELSQL_PARAMETERS", JsonConvert.SerializeObject(parameters, Formatting.Indented));

Dictionary<string, string> servers = new Dictionary<string, string>
{
    { "tokens_url", "http://localhost:5000/AngelPOST" },
    { "skus_url", "http://localhost:5000/AngelPOST" },
    { "sales_url", "http://localhost:5000/AngelPOST" },
    { "inventory_url", "http://localhost:5000/AngelPOST" },
    { "configuration_url", "http://localhost:5000/AngelPOST" },
    { "auth_url", "http://localhost:5000/AngelPOST" }
};

// Development time only

if ( Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" )
{
    foreach (string key in servers.Keys)
    {
        servers[key] = "https://localhost:7170/AngelPOST";
    }
}

Environment.SetEnvironmentVariable("ANGELSQL_SERVERS", JsonConvert.SerializeObject(servers, Formatting.Indented));

return JsonConvert.SerializeObject(parameters);
