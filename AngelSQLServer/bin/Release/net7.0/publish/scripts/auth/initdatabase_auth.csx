// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#load "shared.csx"
#load "branch_stores.csx"
#load "authorizer.csx"
#load "pins.csx"

#r "DB.dll"
#r "Newtonsoft.Json.dll"

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

string db_account = db.Prompt("VAR db_account");
Console.WriteLine("Account: " + db_account);

Console.WriteLine("Creating tables...");

string result = "";

Console.WriteLine("Creating Branch Stores catalog...");
branch_stores branch_Stores = new branch_stores();
result = db.CreateTable(branch_Stores);
Console.WriteLine(result);

Console.WriteLine("Creating Authorizer catalog...");
Authorizer autorizer = new Authorizer();
result = db.CreateTable(autorizer, "authorizers");
Console.WriteLine(result);

Console.WriteLine("Creating pins catalog...");
Pin pin = new Pin();
result = db.CreateTable(pin, "pins");
Console.WriteLine(result);

Dictionary<string, object> servers = JsonConvert.DeserializeObject<Dictionary<string, object>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));
AngelApiOperation operation = new AngelApiOperation();
Dictionary<string, object> message_data = new Dictionary<string, object>();
 
message_data.Clear();
message_data.Add("OperationType", "GetTokenFromUser");
message_data.Add("User", "authuser");
message_data.Add("Password", "mysecret");

Console.WriteLine("Account: " + db_account);

result = db.Prompt($"POST {servers["tokens_url"]} ACCOUNT {db_account} API tokens/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

string token = responce.result;

Console.WriteLine("Token ---> " + token);

// Insert Authorizer
Dictionary<string, object> authorizer = new Dictionary<string, object>();
authorizer.Add("id", "authuser");
authorizer.Add("name", "Root user");
authorizer.Add("phone", "");
authorizer.Add("email", "");
authorizer.Add("password", "mysecret");
authorizer.Add("retype_password", "mysecret");
authorizer.Add("permissions_list", "Cancel Sale, Delete Sale, Inventory transfers, Delete item");
authorizer.Add("user_group", "AUTHORIZERS");

operation.OperationType = "UpsertAuthorizer";
operation.Token = token;
operation.DataMessage = authorizer;

result = db.Prompt($"POST {servers["auth_url"]} ACCOUNT {db_account} API auth/adminbranchstores MESSAGE {JsonConvert.SerializeObject(operation, Formatting.Indented)}");
Console.WriteLine("Insert Authorizer --> " + result);

message_data.Clear();
message_data.Add("OperationType", "GetTokenFromUser");
message_data.Add("User", "authuser");
message_data.Add("Password", "mysecret");

result = db.Prompt($"POST {servers["tokens_url"]} ACCOUNT {db_account} API tokens/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

token = responce.result;
Console.WriteLine("Token ---> " + token);
