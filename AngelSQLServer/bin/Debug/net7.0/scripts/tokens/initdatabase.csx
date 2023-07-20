// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#load "tokens.csx"
#load "users.csx"
#load "usersgroup.csx"
#load "shared.csx"

#r "DB.dll"
#r "Newtonsoft.Json.dll"

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

string db_user = db.Prompt("VAR db_user");
string db_password = db.Prompt("VAR db_password");
string db_account = db.Prompt("VAR db_account");

Console.WriteLine("Account:" + db_account);

Console.WriteLine("Creating tables...");

string result = "";

Console.WriteLine("Creating tokens catalog...");
Tokens tokens = new Tokens();
result = db.CreateTable(tokens);
Console.WriteLine(result);

Console.WriteLine("Creating Users catalog...");
Users users = new Users();
result = db.CreateTable(users);
Console.WriteLine(result);

Console.WriteLine("Creating User Groups catalog...");
UsersGroup usersgroup = new UsersGroup();
result = db.CreateTable(usersgroup);
Console.WriteLine(result);

Dictionary<string, object> servers = JsonConvert.DeserializeObject<Dictionary<string, object>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));

Dictionary<string, object> message_data = new Dictionary<string, object>();
Dictionary<string, object> permissions = new Dictionary<string, object>();

message_data.Clear();
message_data.Add("OperationType", "GetTokenFromUser");
message_data.Add("User", "authuser");
message_data.Add("Password", "mysecret");
Console.WriteLine("Account: " + db_account);
result = db.Prompt($"POST {servers["tokens_url"]} ACCOUNT {db_account} API tokens/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

string token = responce.result;
Console.WriteLine("Token ---> " + token);

// Supervisors Group
message_data.Clear();
message_data.Add("OperationType", "CreateGroup");
message_data.Add("UserGroup", "SUPERVISORS");
message_data.Add("Name", "SUPERVISORS GROUP");
message_data.Add("Token", token);

permissions.Clear();
permissions.Add("Pins", "Upsert, Delete, Query");
message_data.Add("Permissions", JsonConvert.SerializeObject(permissions, Formatting.Indented));

result = db.Prompt($"POST {servers["tokens_url"]} ACCOUNT {db_account} API tokens/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
Console.WriteLine("Supervisors Group --> " + result);

// Pinsconsumer Group
message_data.Clear();
message_data.Add("OperationType", "CreateGroup");
message_data.Add("UserGroup", "PINSCONSUMER");
message_data.Add("Name", "PINSCONSUMER GROUP");
message_data.Add("Token", token);

permissions.Clear();
permissions.Add("Pins", "Upsert, Delete, Query");
message_data.Add("Permissions", JsonConvert.SerializeObject(permissions, Formatting.Indented));
result = db.Prompt($"POST {servers["tokens_url"]} ACCOUNT {db_account} API tokens/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
Console.WriteLine("Pinsconsumer Group --> " + result);

