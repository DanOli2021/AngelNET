
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#r "DB.dll"
#r "Newtonsoft.Json.dll"

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

string db_account = "KIOSK";
Console.WriteLine("Account: " + db_account);

Dictionary<string, object> servers = JsonConvert.DeserializeObject<Dictionary<string, object>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));

// Obtenemos el token de autenticación
var d1 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetTokenFromUser",
        DataMessage = new
        {
            User = "authuser",
            Password = "mysecret"
        }
    }
};

string result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d1, Formatting.Indented)}");
AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
string token = responce.result;
Console.WriteLine("Token ---> " + token);

// Agregamos un Branch Store
var d2 = new
{
    api = "auth/adminbranchstores",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "UpsertBranchStores",
        Token = token,
        DataMessage = new
        {
            id = "BRANCHSTORE01",
            Name = "BRANCH STORE 01",
            Address = "Calle 1 # 1-1",
            Phone = "1234567",
            Authorizer = "authuser"
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d2, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Create branch store ---> " + responce.result);

// Borramos un Branch Store
var d3 = new
{
    api = "auth/adminbranchstores",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "DeleteBranchStore",
        Token = token,
        DataMessage = new
        {
            BranchStoreToDelete = "BRANCHSTORE01",
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d3, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Delete branch store ---> " + responce.result);

// Obtenemos Branch Stores 
var d4 = new
{
    api = "auth/adminbranchstores",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetBranchStores",
        Token = token,
        DataMessage = new
        {
            Where = ""
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d4, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Get branch stores ---> " + responce.result);


// Obtenemos Un Branch Store 
var d5 = new
{
    api = "auth/adminbranchstores",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetBranchStore",
        Token = token,
        DataMessage = new
        {
            BranchStoreId = "BRANCHSTORE01"
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d5, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Get branch store ---> " + responce.result);


// Obtenemos Un Autorizador
var d6 = new
{
    api = "auth/adminbranchstores",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "UpsertAuthorizer",
        Token = token,
        DataMessage = new
        {
            id = "auth01",
            Password = "mysecret",
            RetypePassword = "mysecret",
            Name = "AUTHORIZER 01",
            Email = "",
            Phone = "",
            Organization = "",
            PermissionsList = "Sales, Confirm PIN",
            UserGroups = "KIOSK"
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d6, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Create Authorizer ---> " + responce.result);


