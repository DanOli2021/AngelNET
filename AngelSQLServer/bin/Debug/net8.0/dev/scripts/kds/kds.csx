// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Process to send messages to user
// Daniel() Oliver Rojas
// 2023-05-19

#load "translations.csx"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using System.IO;

public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public string User { get; set; }
    public string UserLanguage { get; set; }
    public dynamic DataMessage { get; set; }
    public string File { get; set; }
    public long FileSize { get; set; }
    public string FileType { get; set; }
}

private AngelApiOperation api = JsonConvert.DeserializeObject<AngelApiOperation>(message);

//Server parameters
private Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));
private Translations translation = new();
translation.SpanishValues();

string ConnectionString = GetVariable("ANGELSQL_MYBUSINESSPOS", @"Data Source=.\MYBUSINESSPOS;Initial catalog=MyBusiness20;User Id=sa;Password=12345678;Persist Security Info=True;");

// This is the main function that will be called by the API
return api.OperationType switch
{
    "GetSkuInfo" => GetSkuInfo(api, translation),
    _ => $"Error: No service found {api.OperationType}",
};






string GetSkuInfo(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic data = api.DataMessage;

    if( data.sku == null ) return "Error: " + translation.Get("Sku not found", api.UserLanguage);

    db.Prompt($@"SQL SERVER CONNECT {ConnectionString} ALIAS local", true);

    result = db.Prompt($"SQL SERVER QUERY SELECT articulo, descrip, precio1, impuesto, imagen FROM prods WHERE articulo = '{data.sku.ToString().Trim()}' CONNECTION ALIAS local", true);

    if( result.StartsWith("Error:") ) return result;

    var sku = new sku_data();

    if( result == "[]" ) 
    {
        sku.sku = "";
        sku.description = "Product not found";
        sku.price = "";
        sku.image = "";        
    }
    else 
    {
        DataRow dr = db.GetDataRow(result);
 
        sku.sku = dr["articulo"].ToString().Trim();
        sku.description = dr["descrip"].ToString().Trim();

        decimal price = Convert.ToDecimal(dr["precio1"]);

        result = db.Prompt($"SQL SERVER QUERY SELECT * FROM impuestos WHERE impuesto = '{dr["impuesto"].ToString()}' CONNECTION ALIAS local", true); 

        if( result.StartsWith("Error:") ) return result;

        decimal impuesto = 0;

        if( result != "[]" ) 
        {
            DataRow dr_impuesto = db.GetDataRow(result);
            impuesto = Convert.ToDecimal(dr_impuesto["valor"]) / 100;
        };

        price = price + (price * impuesto);  
        sku.price = price.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));

        if( File.Exists(dr["imagen"].ToString().Trim()) ) 
        {
            string file_name = Path.GetFileName(dr["imagen"].ToString().Trim());	
            
            if( !File.Exists( server_db.Prompt("VAR db_wwwroot") + "/pricechecker/images/" + file_name ) )
            {
                File.Copy(dr["imagen"].ToString().Trim(), server_db.Prompt("VAR db_wwwroot") + "/pricechecker/images/" + file_name);
            }

            sku.image = file_name;

        }
        else 
        {
            sku.image = "";
        }

    }

    return db.GetJson(sku);
}
 
string GetVariable(string name, string default_value)
{
    if (Environment.GetEnvironmentVariable(name) == null) return default_value;    
    Console.WriteLine($"Variable {name} found");
    return Environment.GetEnvironmentVariable(name);
}


class sku_data 
{
    public string sku = "";
    public string description = "";
    public string price = "";
    public string image = "";
}


string IsUserValid(AngelApiOperation api, Translations translation, string group = "WAITER")
{
    string result = GetGroupsUsingTocken(api.Token, api.User, api.UserLanguage);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic user_data = JsonConvert.DeserializeObject<dynamic>(result);

    if (user_data.groups == null)
    {
        return "Error: " + translation.Get("No groups found", api.UserLanguage);
    }

    if (!user_data.groups.ToString().Contains(group))
    {
        return "Error: " + translation.Get("User does not have permission to edit", api.UserLanguage );
    }

    return "Ok.";

}



private string GetGroupsUsingTocken(string token, string user, string language)
{

    var d = new
    {
        TokenToObtainPermission = token
    };

    string result = SendToAngelPOST("tokens/admintokens", user, token, "GetGroupsUsingTocken", language, d);

    if (result.StartsWith("Error:"))
    {
        return $"Error: {result}";
    }

    return result;

}



private string SendToAngelPOST(string api_name, string user, string token, string OPerationType, string Language, dynamic object_data)
{

    string db_account = user.Split("@")[1];

    var d = new
    {
        api = api_name,
        account = db_account,
        language = "C#",
        message = new
        {
            OperationType = OPerationType,
            Token = token,
            UserLanguage = Language,
            DataMessage = object_data
        }
    };

    string result = db.Prompt($"POST {server_db.Prompt("VAR server_tokens_url")} MESSAGE {JsonConvert.SerializeObject(d, Formatting.Indented)}", true);
    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
    return responce.result;

}
