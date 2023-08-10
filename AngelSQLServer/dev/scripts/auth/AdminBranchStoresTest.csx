
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

string result = "";

// Obtenemos el token de autenticación
result = SendToAngelPOST("tokens/admintokens", db_account, "GetTokenFromUser", new 
{
    User = "authuser",
    Password = "mysecret"
});

Console.WriteLine(result);
AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

string token = responce.result;

// Agregamos un Branch Store
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "UpsertBranchStores", new
{
    id = "BRANCHSTORE01",
    Name = "BRANCH STORE 01",
    Address = "Calle 1 # 1-1",
    Phone = "1234567",
    Authorizer = "authuser"
});

Console.WriteLine(result);

// Borramos un Branch Store
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "DeleteBranchStore", new
{
    BranchStoreToDelete = "BRANCHSTORE01"
});

Console.WriteLine(result);


// Obtenemos Branch Stores 
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetBranchStores", new
{
    Where = ""
});

Console.WriteLine(result);

// Obtenemos Un Branch Store 
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetBranchStore", new
{
    BranchStoreId = "BRANCHSTORE01"
});

Console.WriteLine(result);

// Creamos o actualizamos Un Autorizador
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "UpsertAuthorizer", new
{
    id = "auth01",
    Password = "mysecret",
    Name = "AUTHORIZER 01",
    Email = "",
    Phone = "",
    Organization = "",
    PermissionsList = "Sales, Co nfirm PIN",
    UserGroups = "KIOSK"
});

Console.WriteLine(result);

// Borramos Un Autorizador
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "DeleteAuthorizer", new
{
    AuthorizerToDelete = "auth01"
});

Console.WriteLine(result);

// Obtenemos los autorizadores
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetAuthorizers", new
{
    Where = ""
});

Console.WriteLine(result);

// Obtenemos un autorizador
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetAuthorizer", new
{
    Authorizer = "authuser"
});

Console.WriteLine(result);



string SendToAngelPOST(string api_name, string token, string db_account, string OPerationType, dynamic object_data) 
{ 
    var d = new
    {
        api = api_name,
        account = db_account,
        language = "C#",
        message = new
        {
            OperationType = OPerationType,
            Token = token,
            DataMessage = object_data
        }
    };

    string result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d, Formatting.Indented)}");

    if (result.StartWith("Error:")) 
    {
        return $"Error: ApiName {api_name} Account {db_account} OperationType {OPerationType} --> Result -->" + result;
    }

    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
    return responce.result;
}