// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#r "Newtonsoft.Json.dll"

using Newtonsoft.Json;
using System.Collections.Generic;

string main_url = GetVariable( "ANGELSQL_URLS", "http://localhost:11000" );

Dictionary<string, string> parameters = new Dictionary<string, string>
{
    { "certificate", GetVariable( "ANGELSQL_CERTIFICATE", "" ) },
    { "password", GetVariable( "ANGELSQL_CERTIFICATE_PASSWORD", "" ) },
    { "urls", GetVariable( "ANGELSQL_URLS", "http://localhost:11000" ) },
    { "cors", GetVariable( "ANGELSQL_CORS", "http://localhost:11000" ) },
    { "master_user", GetVariable( "ANGELSQL_MASTER_USER", "db" ) },
    { "master_password", GetVariable( "ANGELSQL_MASTER_PASSWORD", "db" ) },
    { "data_directory", GetVariable( "ANGELSQL_DATA_DIRECTORY", "" ) },
    { "account", GetVariable( "ANGELSQL_ACCOUNT", "account1" ) },
    { "account_user", GetVariable( "ANGELSQL_ACCOUNT_USER", "angelsql" ) },
    { "account_password", GetVariable( "ANGELSQL_ACCOUNT_PASSWORD", "angelsql123" ) },
    { "database", GetVariable( "ANGELSQL_DATABASE", "database1" ) },
    { "request_timeout", GetVariable( "ANGELSQL_REQUEST_TIMEOUT", "4" ) },
    { "wwwroot", GetVariable( "ANGELSQL_WWWROOT", "dev/wwwroot" ) },
    { "scripts_directory", GetVariable( "ANGELSQL_SCRIPTS_DIRECTORY", "dev/scripts" ) },
    { "accounts_directory", GetVariable( "ANGELSQL_ACCOUNTS_DIRECTORY", "" ) },
    { "smtp", GetVariable( "ANGELSQL_SMPT", "" ) },
    { "smtp_port", GetVariable( "ANGELSQL_PORT", "" ) },
    { "email_address", GetVariable( "ANGELSQL_EMAIL_ADDRESS", "" ) },
    { "email_password", GetVariable( "ANGELSQL_EMAIL_PASSWORD", "" ) },
    { "angel_api", GetVariable( "ANGELSQL_API", "config/AngelAPI.csx" ) },
    { "python_path", GetVariable( "ANGELSQL_PYTHON_PATH", "C:/Program Files/Python311/python311.dll" ) },
    { "service_command", GetVariable( "ANGELSQL_SERVICE_COMMAND", "config/Tasks.csx" ) },
    { "save_activity", "false" },
    { "use_black_list", "false" },
    { "use_white_list", "false" },
    { "service_delay", "300000" },
};

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


string GetVariable(string name, string default_value)
{
    if (Environment.GetEnvironmentVariable(name) == null) return default_value;    
    return Environment.GetEnvironmentVariable(name);
}
