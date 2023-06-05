// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Script for managing skus (product codes)
// Daniel() Oliver Rojas
// 2023-05-19

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;


public string GetPermisions(AngelDB.DB db, string server_url, string token, string message)
{

    string result = "";

    OperationTypeClass operationTypeClass = new OperationTypeClass();
    operationTypeClass.OperationType = "GetPermisionsUsingTocken";
    operationTypeClass.Token = token;
    result = db.Prompt($"POST {server_url} API pos/admintokens MESSAGE {JsonConvert.SerializeObject(operationTypeClass, Formatting.Indented)}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    AngelDB.AngelPOST api = JsonConvert.DeserializeObject<AngelDB.AngelPOST>(result);

    if (api.message.StartsWith("Error:"))
    {
        return api.message;
    }

    return api.message;

}

public class OperationTypeClass
{
    public string OperationType;
    public string Token;
}


