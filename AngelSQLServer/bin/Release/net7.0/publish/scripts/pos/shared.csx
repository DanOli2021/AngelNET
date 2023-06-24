// Script for managing skus (product codes)
// Daniel() Oliver Rojas
// 2023-05-19

#r "C:\AngelSQLNet\AngelSQL\db.dll"
#r "C:\AngelSQLNet\AngelSQL\Newtonsoft.Json.dll"
#r "C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\7.0.5\Microsoft.AspNetCore.Components.dll"
#r "C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\7.0.5\Microsoft.AspNetCore.Components.Web.dll"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

static public string GetPermisions(AngelDB.DB db, string server_url, string token)
{

    string result = "";

    AngelApiOperation operationTypeClass = new AngelApiOperation();
    operationTypeClass.OperationType = "GetPermisionsUsingTocken";
    operationTypeClass.Token = token;
    result = db.Prompt($"POST {server_url} API pos/admintokens MESSAGE {JsonConvert.SerializeObject(operationTypeClass, Formatting.Indented)}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    AngelDB.AngelResponce api = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

    if (api.result.StartsWith("Error:"))
    {
        return api.result;
    }

    return api.result;

}



static public string ValidatePermisions(AngelDB.DB db, Dictionary<string, string> servers, string token, string permision, string permision_name)
{
    string result = "";

    result = GetPermisions(db, servers["tockens_url"], token);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    Dictionary<string, string> permisions = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

    if (!permisions[permision].Contains(permision_name))
    {
        return $"Error: You don't have {permision} {permision_name} permision";
    }

    return "Ok.";

}



public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public dynamic DataMessage { get; set; }
}


public class OperationTypeObject
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public object Message { get; set; }
}

