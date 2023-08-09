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
using System.Security.Cryptography;

static public string GetPermisions(AngelDB.DB db, string server_url, string token)
{

    string result = "";

    string account = db.Prompt("VAR db_account");

    AngelApiOperation operationTypeClass = new AngelApiOperation();
    operationTypeClass.OperationType = "GetPermisionsUsingTocken";
    operationTypeClass.Token = token;
    result = db.Prompt($"POST {server_url} ACCOUNT {account} API tokens/admintokens MESSAGE {JsonConvert.SerializeObject(operationTypeClass, Formatting.Indented)}");

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

static public string ValidatePermisions(AngelDB.DB db, Dictionary<string, string> servers, string token, string group_name, string permision, string permision_detail, bool validate_only_group = false)
{
    string result = "";

    result = GetPermisions(db, servers["tokens_url"], token);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    Dictionary<string, string> group = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

    if (group_name.Trim().ToUpper().IndexOf(group["id"].Trim().ToUpper()) < 0)
    {
        return $"Error: Does not belong to the {group_name} group";
    }

    if (validate_only_group)
    {
        return JsonConvert.SerializeObject(group, Formatting.Indented);
    }

    Dictionary<string, string> permisions = JsonConvert.DeserializeObject<Dictionary<string, string>>(group["permissions"]);

    if (!permisions[permision].Contains(permision_detail))
    {
        return $"Error: You don't have {permision} {permision_detail} permision";
    }

    return JsonConvert.SerializeObject(group, Formatting.Indented);

}


public static class RandomNumberGenerator
{
    private static Random random = new Random();

    public static string GenerateRandomNumber(int digitCount)
    {
        int randomNumber = random.Next((int)Math.Pow(10, digitCount));
        return randomNumber.ToString().PadLeft(digitCount, '0');
    }
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


