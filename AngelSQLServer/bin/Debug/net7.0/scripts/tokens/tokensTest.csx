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


// Creamos el grupo de ventas
var d2 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new 
    {
        OperationType = "UpsertGroup",
        Token = token,
        DataMessage = new
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
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d2, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Creating group sales ---> " + responce.result);


// Obtenemos los grupos
var d3 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetGroups",
        Token = token,
        DataMessage = new
        {
            UserGroup = "SALES",
            Where = ""
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d3, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Obteniendo todos los grupos ---> " + responce.result);


// Creamos un usuario
var d4 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "CreateUser",
        Token = token,
        DataMessage = new
        {
            User = "salesuser",
            UserGroups = "AUTHORIZERS, SALES",
            Name = "Sales User",
            Password = "mysecret",
            Organization = "SALES",
            Email = "",
            Phone = ""
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d4, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Creando usuario ---> " + responce.result);


// Borramos el grupo de ventas
var d5 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "DeleteGroup",
        Token = token,
        DataMessage = new
        {
            UserGroupToDelete = "SALES"
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d5, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Borrando grupo ---> " + responce.result);



// Borramos el usuario creado anteriormente
var d6 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "DeleteUser",
        Token = token,
        DataMessage = new
        {
            UserToDelete = "salesuser"
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d6, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Eliminando usuario ---> " + responce.result);


// Obtenemos los usuarios
var d7 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetUsers",
        Token = token,
        DataMessage = new
        {
            Where = ""
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d7, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Lista de usuarios ---> " + responce.result);


// Obtenemos la info de un usuario especifico
var d8 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetUser",
        Token = token,
        DataMessage = new
        {
            User = "myuser"
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d8, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Info de usuario myuser ---> " + responce.result);


// Creamos un nuevo token
var d9 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "CreateNewToken",
        Token = token,
        DataMessage = new
        {
            User = "myuser",
            expiry_days = -1
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d9, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
string new_token = responce.result;
Console.WriteLine("Creando un nuevo token ---> " + responce.result);



// Borramos el token creado anteriormente
var d10 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "DeleteToken",
        Token = token,
        DataMessage = new
        {
            TokenToDelete = new_token
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d10, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Borrando token ---> " + responce.result);


// Validamos el token 
var d11 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "ValidateToken",
        Token = token,
        DataMessage = new
        {
            TokenToValidate = token
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d11, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Validando token ---> " + responce.result);


// Permissions using Token
var d12 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetPermisionsUsingTocken",
        Token = token,
        DataMessage = new
        {
            TokenToObtainPermission = token
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d12, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("Permissions using Token ---> " + responce.result);


// Permissions using Token
var d13 = new
{
    api = "tokens/admintokens",
    account = db_account,
    language = "C#",
    message = new
    {
        OperationType = "GetUserUsingToken",
        Token = token,
        DataMessage = new
        {
            TokenToGetTheUser = token
        }
    }
};

result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d13, Formatting.Indented)}");
responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
Console.WriteLine("User using Token ---> " + responce.result);

