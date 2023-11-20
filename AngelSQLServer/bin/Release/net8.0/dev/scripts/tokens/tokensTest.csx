// GLOBALS
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
result = SendToAngelPOST("tokens/admintokens", "", db_account, "GetTokenFromUser", new
{
    User = "authuser",
    Password = "mysecret"
});

Console.WriteLine(result);
AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

string token = responce.result;

dynamic param_data = new
{
    User = "authuser",
    Password = "mysecret"
};

// Obtenemos el token de autenticación
result = SendToAngelPOST("tokens/admintokens", "", db_account, "GetTokenFromUser", new
{
    User = "authuser",
    Password = "mysecret"
});


// Agregamos un Branch Store
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "UpsertGroup", new
{
    UserGroup = "SALES",
    Name = "SALES USERS",
    Permissions = new
    {
        Sales = "Upsert, Delete, Query",
        Sales_POS = "Create, Modify, Delete, Consult",
        Sales_Kiosk = "Create, Modify, Delete, Consult",
        Customers = "Upsert, Delete, Query",
        Sales_X_Report = "true",
        Sales_Z_Report = "true",
        Sales_cash_reconciliation = "true",
        Sales_giving_a_refund = "true",
        Sales_void_transaction = "true",
        Sales_tender_the_transaction = "true",
        Sales_void_item = "true",
        Sales_change_price = "true",
        Purchases = "Upsert, Delete, Query",
        Inventory = "Upsert, Delete, Query",
        Skus = "Upsert, Delete, Query",
        Skus_offers = "Upsert, Delete, Query",
        Currencies = "Upsert, Delete, Query",
        PriceCodes = "Upsert, Delete, Query",
        Clasifications = "Upsert, Delete, Query",
        Makers = "Upsert, Delete, Query",
        Locations = "Upsert, Delete, Query",
        Inventory_inbound_outbound = "Upsert, Delete, Query",
        Physical_inventory = "Upsert, Delete, Query, Apply",
        Physical_inventory_shrinkage = "Upsert, Delete, Query",
        BusinessManager = "Reports,CEO Reports",
        Configuration = "Modify"
    }
});

Console.WriteLine("Creating group SALES " + result);


// Obtenemos los grupos
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetGroups", new
{
    UserGroup = "SALES",
    Where = ""
});

Console.WriteLine("Get groups " + result);

// Borramos el grupo de ventas
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "DeleteGroup", new
{
    UserGroupToDelete = "SALES"
});

Console.WriteLine("Delete group " + result);


// Creamos un usuario
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "CreateUser", new
{
    User = "salesuser",
    UserGroups = "AUTHORIZERS, SALES",
    Name = "Sales User",
    Password = "mysecret",
    Organization = "SALES",
    Email = "",
    Phone = ""
});

Console.WriteLine("Create user " + result);


// Creamos un usuario
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "DeleteUser", new
{
    UserToDelete = "salesuser"
});

Console.WriteLine("Delete user " + result);

// Obtenemos los usuarios
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetUsers", new
{
    Where = ""
});

Console.WriteLine("Get users " + result);


// Obtenemos la info de un usuario especifico
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetUser", new
{
    User = "myuser"
});

Console.WriteLine("GetUser " + result);

// Creamos un nuevo token
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "CreateNewToken", new
{
    User = "myuser",
    expiry_days = -1
});

Console.WriteLine("CreateNewTocken " + result);


// Borramos el token creado anteriormente
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "DeleteToken", new
{
    TokenToDelete = result
});

Console.WriteLine("DeleteToken " + result);

// Validamos el token 
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "ValidateToken", new
{
    TokenToValidate = token
});

Console.WriteLine("ValidateToken " + result);


// Permissions using Token
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetPermisionsUsingTocken", new
{
    TokenToObtainPermission = token
});

Console.WriteLine("GetPermisionsUsingTocken " + result);


// Permissions using Token
result = SendToAngelPOST("auth/adminbranchstores", token, db_account, "GetUserUsingToken", new
{
    TokenToGetTheUser = token
});

Console.WriteLine("GetUserUsingToken " + result);


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

    if (result.StartsWith("Error:"))
    {
        return $"Error: ApiName {api_name} Account {db_account} OperationType {OPerationType} --> Result -->" + result;
    }

    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
    return responce.result;
}