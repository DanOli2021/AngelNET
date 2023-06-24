// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Script for managing skus (product codes)
// Daniel() Oliver Rojas
// 2023-05-19

#load "skus.csx"
#load "shared.csx"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

AdminSkus admin_skus = new AdminSkus();
 
AngelApiOperation operation_type = JsonConvert.DeserializeObject<AngelApiOperation>(message);

Dictionary<string, string> servers = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));
 
switch (operation_type.OperationType)
{

    case "UpsertClasifications":

        return admin_skus.UpsertClasifications(db, servers, operation_type.Token, operation_type.DataMessage);

    case "DeleteClasifications":

        return admin_skus.DeleteClasifications(db, servers, operation_type.Token, operation_type.DataMessage);

    case "GetClasifications":

        return admin_skus.GetClasifications(db, servers, operation_type.Token, operation_type.DataMessage);

    case "UpsertMaker":

        return admin_skus.UpsertMaker(db, servers, operation_type.Token, operation_type.DataMessage);

    case "DeleteMaker":

        return admin_skus.DeleteMaker(db, servers, operation_type.Token, operation_type.DataMessage);

    case "GetMakers":

        return admin_skus.GetMakers(db, servers, operation_type.Token, operation_type.DataMessage);

    case "UpsertLocation":

        return admin_skus.UpsertLocation(db, servers, operation_type.Token, operation_type.DataMessage);

    case "DeleteLocation":

        return admin_skus.DeleteLocation(db, servers, operation_type.Token, operation_type.DataMessage);

    case "GetLocations":

        return admin_skus.GetLocations(db, servers, operation_type.Token, operation_type.DataMessage);

    case "UpsertCurrency":
    
            return admin_skus.UpsertCurrency(db, servers, operation_type.Token, operation_type.DataMessage);   

    case "DeleteCurrency":

        return admin_skus.DeleteCurrency(db, servers, operation_type.Token, operation_type.DataMessage);

    case "GetCurrencies":

        return admin_skus.GetCurrencies(db, servers, operation_type.Token, operation_type.DataMessage);

    case "UpsertPriceCode":

        return admin_skus.UpsertPriceCode(db, servers, operation_type.Token, operation_type.DataMessage);

    case "DeletePriceCode":
    
            return admin_skus.DeletePriceCode(db, servers, operation_type.Token, operation_type.DataMessage);   

    case "GetPriceCodes":   

        return admin_skus.GetPriceCodes(db, servers, operation_type.Token, operation_type.DataMessage);

    case "UpsertSku":

        return admin_skus.UpsertSku(db, servers, operation_type.Token, operation_type.DataMessage);

    case "DeleteSku":

        return admin_skus.DeleteSku(db, servers, operation_type.Token, operation_type.DataMessage);

    case "GetSkus":

        return admin_skus.GetSkus(db, servers, operation_type.Token, operation_type.DataMessage);

    case "SearchSkus":

        return admin_skus.SearchSkus(db, servers, operation_type.Token, operation_type.DataMessage);

    case "GetSku":

        return admin_skus.GetSku(db, servers, operation_type.Token, operation_type.DataMessage);

    default:
        return "Error: No service found";
}


public class AdminSkus
{


    public string UpsertClasifications(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Clasifications", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        Console.WriteLine("---->" + message);

        result = db.Prompt($"UPSERT INTO clasifications VALUES {JsonConvert.SerializeObject(message, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: UpsertClasifications() " + result.Replace("Error:", "");
        }

        return result;
    }

    public string DeleteClasifications(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Clasifications", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"DELETE FROM clasifications PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string GetClasifications(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Clasifications", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (message.ContainsKey("where"))
        {
            result = db.Prompt($"SELECT * FROM clasifications PARTITION KEY main WHERE {message["where"]}");
        }
        else if (message.ContainsKey("id"))
        {
            result = db.Prompt($"SELECT * FROM clasifications PARTITION KEY main WHERE id = '{message["id"]}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM clasifications PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }


    public string UpsertMaker(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Makers", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"UPSERT INTO makers VALUES {JsonConvert.SerializeObject(message, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }


    public string DeleteMaker(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Makers", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"DELETE FROM makers PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string GetMakers(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Makers", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (message.ContainsKey("where"))
        {
            result = db.Prompt($"SELECT * FROM makers PARTITION KEY main WHERE {message["where"]}");
        }
        else if (message.ContainsKey("id"))
        {
            result = db.Prompt($"SELECT * FROM makers PARTITION KEY main WHERE id = '{message["id"]}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM makers PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }


    public string UpsertLocation(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Locations", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"UPSERT INTO locations VALUES {JsonConvert.SerializeObject(message, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string DeleteLocation(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Locations", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"DELETE FROM locations PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string GetLocations(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Locations", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (message.ContainsKey("where"))
        {
            result = db.Prompt($"SELECT * FROM locations PARTITION KEY main WHERE {message["where"]}");
        }
        else if (message.ContainsKey("id"))
        {
            result = db.Prompt($"SELECT * FROM locations PARTITION KEY main WHERE id = '{message["id"]}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM locations PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string UpsertCurrency(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Currencies", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"UPSERT INTO currencies VALUES {JsonConvert.SerializeObject(message, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string DeleteCurrency(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Currencies", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"DELETE FROM Currencies PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string GetCurrencies(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Currencies", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (message.ContainsKey("where"))
        {
            result = db.Prompt($"SELECT * FROM Currencies PARTITION KEY main WHERE {message["where"]}");
        }
        else if (message.ContainsKey("id"))
        {
            result = db.Prompt($"SELECT * FROM Currencies PARTITION KEY main WHERE id = '{message["id"]}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM Currencies PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }


    public string UpsertPriceCode(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "PriceCodes", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"SELECT * FROM Currencies WHERE id = '{message["currency"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        if( result == "[]" ) 
        {
            return "Error: Currency not found";
        }

        result = db.Prompt($"UPSERT INTO price_codes VALUES {JsonConvert.SerializeObject(message, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string DeletePriceCode(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "PriceCodes", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"DELETE FROM price_codes PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }


    public string GetPriceCodes(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "PriceCodes", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (message.ContainsKey("where"))
        {
            result = db.Prompt($"SELECT * FROM price_codes PARTITION KEY main WHERE {message["where"]}");
        }
        else if (message.ContainsKey("id"))
        {
            result = db.Prompt($"SELECT * FROM price_codes PARTITION KEY main WHERE id = '{message["id"]}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM price_codes PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string UpsertSku(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Skus", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"SELECT id FROM currencies WHERE id = '{message["currency"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: currencies " + result.Replace("Error:", ",");
        }

        if( result == "[]" ) 
        {
            return "Error: Currency not found";
        }

        result = db.Prompt($"SELECT id FROM makers WHERE id = '{message["maker"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: makers " + result.Replace("Error:", ",");
        }

        if( result == "[]" ) 
        {
            return "Error: Maker not found";
        }
        
        result = db.Prompt($"SELECT id FROM locations WHERE id = '{message["location"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: locations " + result.Replace("Error:", ",");
        }

        if( result == "[]" ) 
        {
            return "Error: Location not found";
        }

        result = db.Prompt($"UPSERT INTO skus_catalog PARTITION KEY main VALUES {JsonConvert.SerializeObject(message, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        result = db.Prompt($"UPSERT INTO skus_search PARTITION KEY main VALUES {JsonConvert.SerializeObject(message, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }


        return result;
    }


    public string DeleteSku(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Skus", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"DELETE FROM skus_catalog PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        result = db.Prompt($"DELETE FROM skus_search PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }


    public string GetSkus(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Skus", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (message.ContainsKey("where"))
        {
            result = db.Prompt($"SELECT * FROM Skus_Catalog PARTITION KEY main WHERE {message["where"]}");
        }
        else if (message.ContainsKey("id"))
        {
            result = db.Prompt($"SELECT * FROM Skus_Catalog PARTITION KEY main WHERE id = '{message["id"]}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM Skus_Catalog PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string SearchSkus(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Skus", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (!message.ContainsKey("query"))
        {
            result = "Error: Missing where clause";
        }

        result = db.Prompt($"SELECT id, description, price, clasification FROM skus_search PARTITION KEY main WHERE {message["query"]}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public string GetSku(AngelDB.DB db, Dictionary<string, string> servers, string token, dynamic message)
    {

        string result = ValidatePermisions(db, servers, token, "Skus", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (!message.ContainsKey("id"))
        {
            result = "Error: Missing sku id";
        }

        result = db.Prompt($"SELECT * FROM skus_catalog PARTITION KEY main WHERE id = '{message["id"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        List<Skus_Catalog> sku = JsonConvert.DeserializeObject<List<Skus_Catalog>>(result);
        return JsonConvert.SerializeObject(sku[0]);

    }

}


