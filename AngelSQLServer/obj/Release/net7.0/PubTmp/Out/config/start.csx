// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#r "Newtonsoft.Json.dll"

using Newtonsoft.Json;
using System.Collections.Generic;

//Server parameters
Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));
Dictionary<string, string> servers = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));

// Create the master database
server_db.Prompt($"DB USER {parameters["master_user"]} PASSWORD {parameters["master_password"]} DATA DIRECTORY {parameters["data_directory"]}", true);
server_db.Prompt($"CREATE ACCOUNT {parameters["account"]} SUPERUSER {parameters["account_user"]} PASSWORD {parameters["account_password"]}", true);
server_db.Prompt($"USE ACCOUNT {parameters["account"]}", true);
server_db.Prompt($"CREATE DATABASE {parameters["database"]}", true);
server_db.Prompt($"USE DATABASE {parameters["database"]}", true);

// Create the accounts table
server_db.Prompt($"CREATE TABLE accounts FIELD LIST db_user, name, email, connection_string, db_password, database, data_directory, account, super_user, super_user_password, active, created", true);
// Create the hub users table
server_db.Prompt($"CREATE TABLE hub_users FIELD LIST account, name, email, phone, password, role, active, last_access, created", true);
// Create the table pins
server_db.Prompt($"CREATE TABLE pins FIELD LIST authorizer TEXT, authorizer_name TEXT, branch_store TEXT, pin_number TEXT, message TEXT, authorizer_message TEXT, pintype TEXT, date TEXT, expirytime TEXT, minutes INTEGER, permissions TEXT, confirmed_date TEXT, user TEXT, app_user TEXT, app_user_name TEXT, status TEXT", true);

foreach (string key in parameters.Keys)
{
    server_db.Prompt( "VAR db_" + key + " = " + parameters[key] );    
}

foreach (string key in server.Keys)
{
    server_db.Prompt("VAR server_" + key + " = " + server[key]);
}
