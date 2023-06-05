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

OperationTypeClass operation_type = JsonConvert.DeserializeObject<OperationTypeClass>(message);
Dictionary<string, string> servers = JsonConvert.DeserializeObject<Dictionary<string,string>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));


switch (operation_type.OperationType)
{
    
    case "UpsertClasificacions":

        return admin_skus.UpsertClasificacion(db, servers, operation_type.Token, message);

    default:
        return "Error: No service found";
}


public class AdminSkus
{
    public string UpsertClasificacion(AngelDB.DB db, Dictionary<string,string> servers, string token, string message) 
    {

        string result = "";
        
        result = GetPermisions(db, servers["tockens_url"], token, message);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        Clasificacions clasificacion = JsonConvert.DeserializeObject<Clasificacions>(result);
        result = db.Prompt($"UPSERT INTO clasifications VALUES '{ JsonConvert.SerializeObject(clasificacion, Formatting.Indented) }'");
        return result;
    }
}


