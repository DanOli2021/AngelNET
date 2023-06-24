// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#load "skus.csx"
#load "tokens.csx"
#load "users.csx"
#load "usersgroup.csx"
#load "shared.csx"


#r "DB.dll"
#r "Newtonsoft.Json.dll"

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

Console.WriteLine("Creating tables...");

string result = "";

Console.WriteLine("Creating skus catalog...");
Skus_Catalog sku = new Skus_Catalog();
result = db.CreateTable(sku);
Console.WriteLine(result);

Console.WriteLine("Creating skus catalog search table...");
Skus_Catalog sku_search = new Skus_Catalog();
result = db.CreateTable(sku, "skus_search", true);
Console.WriteLine(result);

Console.WriteLine("Creating Componens catalog...");
Components component = new Components();
result = db.CreateTable(component);
Console.WriteLine(result);

Console.WriteLine("Creating Clasifications catalog...");
Clasifications clasifications = new Clasifications();
result = db.CreateTable(clasifications);
Console.WriteLine(result);

Console.WriteLine("Creating Makers catalog...");
Makers maker = new Makers();
result = db.CreateTable(maker);
Console.WriteLine(result);

Console.WriteLine("Creating Sku_Dictionaries catalog...");
Sku_Dictionary sku_dictionary = new Sku_Dictionary();
result = db.CreateTable(sku_dictionary);
Console.WriteLine(result);

Console.WriteLine("Creating Locations catalog...");
Locations locations = new Locations();
result = db.CreateTable(locations);
Console.WriteLine(result);

Console.WriteLine("Creating Price codes catalog...");
Price_Codes price = new Price_Codes();
result = db.CreateTable(price);
Console.WriteLine(result);

Console.WriteLine("Creating currencies catalog...");
Currencies currency = new Currencies();
result = db.CreateTable(currency);
Console.WriteLine(result);

Console.WriteLine("Creating Tockens catalog...");
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


Dictionary<string, object> message_data = new Dictionary<string, object>();
message_data.Add("OperationType", "CreateGroup");
message_data.Add("UserGroup", "SALES");
message_data.Add("Name", "SALES GROUP");
message_data.Add("db_user", "db");
message_data.Add("db_password", "db");

Dictionary<string, object> permissions = new Dictionary<string, object>();
permissions.Add("Sales", "Upsert, Delete, Query");
permissions.Add("Sales_POS", "Create, Modify, Delete, Consult");
permissions.Add("Sales_Kiosk", "Create, Modify, Delete, Consult");
permissions.Add("Customers", "Upsert, Delete, Query");
permissions.Add("Sales_X_Report", "true");
permissions.Add("Sales_Z_Report", "true");
permissions.Add("Sales_cash_reconciliation", "true");
permissions.Add("Sales_giving_a_refund", "true");
permissions.Add("Sales_void_transaction", "true");
permissions.Add("Sales_tender_the_transaction", "true");
permissions.Add("Sales_void_item", "true");
permissions.Add("Sales_change_price", "true");
permissions.Add("Purchases", "Upsert, Delete, Query");
permissions.Add("Inventory", "Upsert, Delete, Query");
permissions.Add("Skus", "Upsert, Delete, Query");
permissions.Add("Skus_offers", "Upsert, Delete, Query");
permissions.Add("Currencies", "Upsert, Delete, Query");
permissions.Add("PriceCodes", "Upsert, Delete, Query");
permissions.Add("Clasifications", "Upsert, Delete, Query");
permissions.Add("Makers", "Upsert, Delete, Query");
permissions.Add("Locations", "Upsert, Delete, Query");
permissions.Add("Inventory_inbound_outbound", "Upsert, Delete, Query");
permissions.Add("Physical_inventory", "Upsert, Delete, Query, Apply");
permissions.Add("Physical_inventory_shrinkage", "Upsert, Delete, Query");
permissions.Add("BusinessManager", "Reports,CEO Reports");
permissions.Add("Configuration", "Modify");

message_data.Add("Permissions", JsonConvert.SerializeObject(permissions, Formatting.Indented));
Dictionary<string, object> servers = JsonConvert.DeserializeObject<Dictionary<string, object>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));


Console.WriteLine("Inserting basic data...");
result = db.Prompt($"POST {servers["tockens_url"]} API pos/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
Console.WriteLine("User Groups --> " + result);

message_data.Clear();
message_data.Add("OperationType", "CreateUser");
message_data.Add("User", "myposuser");
message_data.Add("Password", "mysecret");
message_data.Add("UserGroup", "SALES");
message_data.Add("Name", "SALES USER");
message_data.Add("Organization", "My Organization Name");
message_data.Add("Email", "myemail@mycompany.name");
message_data.Add("Phone", "55 55055555");
message_data.Add("db_user", "db");
message_data.Add("db_password", "db");

result = db.Prompt($"POST {servers["tockens_url"]} API pos/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
Console.WriteLine("Basic User --> " + result);

message_data.Clear();
message_data.Add("OperationType", "GetTokenFromUser");
message_data.Add("User", "myposuser");
message_data.Add("Password", "mysecret");

result = db.Prompt($"POST {servers["tockens_url"]} API pos/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
Console.WriteLine("Get Tocken --> " + result);

AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

if (responce.result.StartsWith("Error:"))
{
    message_data.Clear();
    message_data.Add("OperationType", "CreateNewToken");
    message_data.Add("User", "myposuser");
    message_data.Add("expiry_days", 30);
    message_data.Add("db_user", "db");
    message_data.Add("db_password", "db");
    result = db.Prompt($"POST {servers["tockens_url"]} API pos/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");
    Console.WriteLine("Create Tocken --> " + result);
}

responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
string token = responce.result;

AngelApiOperation operation = new AngelApiOperation();
Console.WriteLine("Token --> " + token);

Clasifications c = new Clasifications();
c.id = "Unknown";
c.description = "Unknown Clasification";

operation.OperationType = "UpsertClasifications";
operation.Token = token;
operation.DataMessage = c;

result = db.Prompt($"POST {servers["skus_url"]} API pos/adminskus MESSAGE {JsonConvert.SerializeObject(operation, Formatting.Indented)}");

Makers m = new Makers();
m.id = "Unknown";
m.description = "Unknown Maker";

operation.OperationType = "UpsertMaker";
operation.Token = token;
operation.DataMessage = m;

result = db.Prompt($"POST {servers["skus_url"]} API pos/adminskus MESSAGE {JsonConvert.SerializeObject(operation, Formatting.Indented)}");
Console.WriteLine("Insert standard maker for skus --> " + result);


Locations l = new Locations();
l.id = "Unknown";
l.description = "Unknown Location";

operation.OperationType = "UpsertLocation";
operation.Token = token;
operation.DataMessage = l;

result = db.Prompt($"POST {servers["skus_url"]} API pos/adminskus MESSAGE {JsonConvert.SerializeObject(operation, Formatting.Indented)}");
Console.WriteLine("Insert standard location for skus --> " + result);

Currencies cu = new Currencies();
cu.id = "USD";
cu.description = "US Dollar";
cu.symbol = "$";
cu.exchange_rate = 1;

operation.OperationType = "UpsertCurrency";
operation.Token = token;
operation.DataMessage = cu;

result = db.Prompt($"POST {servers["skus_url"]} API pos/adminskus MESSAGE {JsonConvert.SerializeObject(operation, Formatting.Indented)}");
Console.WriteLine("Insert standard maker for skus --> " + result);

Skus_Catalog sc = new Skus_Catalog();
sc.id = "750";
sc.description = "Sku 750";
sc.clasification = "Unknown";
sc.price = 2;
sc.cost = 1;
sc.consumption_tax = 0.16M;
sc.consumption_tax1 = 0;
sc.consumption_tax2 = 0;
sc.currency = "USD";
sc.maker = "Unknown";
sc.location = "Unknown";
sc.requires_inventory = true;
sc.image_name = "";
sc.is_for_sale = true;
sc.bulk_sale = true;
sc.require_series = false;
sc.require_lots = false;
sc.kit = false;
sc.sell_below_cost = false;
sc.locked = false;
sc.weight_request = false;
sc.weight = 0;
sc.ClaveProdServ = "01010101";
sc.ClaveUnidad = "H87";
sc.unit = "Unit";

operation.OperationType = "UpsertSku";
operation.Token = token;
operation.DataMessage = sc;

result = db.Prompt($"POST {servers["skus_url"]} API pos/adminskus MESSAGE {JsonConvert.SerializeObject(operation, Formatting.Indented)}");
Console.WriteLine("Insert standard sku --> " + result);

sc.id = "751";
sc.description = "Sku 751";

result = db.Prompt($"POST {servers["skus_url"]} API pos/adminskus MESSAGE {JsonConvert.SerializeObject(operation, Formatting.Indented)}");
Console.WriteLine("Insert standard sku --> " + result);
