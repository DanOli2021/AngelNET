// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#r "Newtonsoft.Json.dll"

using Newtonsoft.Json;
using System.Collections.Generic;

string main_url = "";

Dictionary<string, string> parameters = new Dictionary<string, string>
{
    { "certificate", "" },
    { "password", "" },
    { "urls", main_url },
    { "cors", main_url },
    { "master_user", "db" },
    { "master_password", "db" },
    { "data_directory", "" },
    { "account", "account1" },
    { "account_user", "angelsql" },
    { "account_password", "angelsql123" },
    { "database", "database1" },
    { "request_timeout", "4" },
    { "wwwroot", "dev/wwwroot" },
    { "scripts_directory", "dev/scripts" },
    { "smtp", "" },
    { "smtp_port", "" },
    { "email_address", "" },
    { "email_password", "" },
    { "angel_api", "config/AngelAPI.csx" },
    { "python_path", "C:/Program Files/Python311/python311.dll" },
    { "service_command", "config/Tasks.csx" },
    { "service_delay", "300000" },
};


Environment.SetEnvironmentVariable("ANGELSQL_PARAMETERS", JsonConvert.SerializeObject(parameters, Formatting.Indented));

Dictionary<string, string> servers = new Dictionary<string, string>
{
    { "tokens_url", $"{main_url}/AngelPOST" },
    { "skus_url", $"{main_url}/AngelPOST" },
    { "sales_url", $"{main_url}/AngelPOST" },
    { "inventory_url", $"{main_url}/AngelPOST" },
    { "configuration_url", $"{main_url}/AngelPOST" },
    { "auth_url", $"{main_url}/AngelPOST" }
};

Environment.SetEnvironmentVariable("ANGELSQL_SERVERS", JsonConvert.SerializeObject(servers, Formatting.Indented));

return JsonConvert.SerializeObject(parameters);
