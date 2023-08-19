// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Script for system access management, it is based on the generation of Tokens, 
// for which first user groups must be created, which define their access levels, 
// the necessary users are created indicating the group they belong to and in the end, 
//an access token is generated indicating the user it is intended to assign 
//for use in all operations within the system
// Daniel() Oliver Rojas
// 2023-05-19


#load "skus.csx"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public dynamic DataMessage { get; set; }
}

AngelApiOperation api = JsonConvert.DeserializeObject<AngelApiOperation>(message);

string db_account = server_db.Prompt("VAR db_account");
string tokens_url = server_db.Prompt("VAR server_tokens_url");
string token = api.Token;

switch (api.OperationType)
{
    case "SearchSkus":
        return SearchSkus();
    default:
        return "Error: OperationType not found";
}


    // Adminin Skus class
string SearchSkus() 
{
    string result = ValidateUser("", "SearchSkus", "AUTHORIZERS, SUPERVISORS, PINSCONSUMER, CASHIER, ADMINISTRATIVE");

    if( string.IsNullOrEmpty(result) ) return "Error: SearchSkus() " + result.Replace("Error:", "" );

    dynamic d = api.DataMessage;

    if( d.Where == null) 
    {
        return "Error: SearchSkus() Where is null";
    }

    result = db.Prompt( $"SELECT * FROM skus_search WHERE {d.Where}" );

    if( result.StartsWith("Error:") ) return "Error: SearchSkus() " + result.Replace("Error:", "" );



    return result;

}


string ValidateUser(string groups, string api_name) 
{
        // Obtenemos el token de autenticación
    string result = SendToAngelPOST("tokens/admintokens", "", db_account, "GetGroupsUsingTocken", new
    {
        TokenToObtainPermission = token,
    });

    if( result.StartsWith("Error:") ) return "Error: ValidateUser() " + result.Replace( "Error:", "" );

    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

    if (responce.result.StartsWith("Error:")) return "Error: ValidateUser() " + responce.result.Replace("Error:", "");

    List<string> authorizedGroups = groups.Split(',').ToList();
    List<string> userGroups = responce.result.Split(',').ToList();

    bool found = false;

    foreach (string item in authorizedGroups)
    {
        foreach (string item2 in userGroups)
        {
            if (item.Trim() == item2.Trim())
            {
                found = true;
                break;
            }
        }

        if (found)
        {
            break;
        }
    }

    if (!found)
    {
        return $"Error: ValidateAdminUser() {api_name} User not authorized";
    }

    string token = responce.result;

}


string CreateTables() 
{
    Console.WriteLine("Creating skus catalog...");
    Skus_Catalog sku = new Skus_Catalog();
    result = db.CreateTable(sku);
    if( result.StartsWith("Error:") ) return "Error: Creating table Skus_Catalog " + result.Replace("Error:", "");

    Console.WriteLine("Creating skus catalog search table...");
    Skus_Catalog sku_search = new Skus_Catalog();
    result = db.CreateTable(sku, "skus_search", true);
    if( result.StartsWith("Error:") ) return "Error: Creating table Skus_Search " + result.Replace("Error:", "");

    Console.WriteLine("Creating Componens catalog...");
    Components component = new Components();
    result = db.CreateTable(component);
    if( result.StartsWith("Error:") ) return "Error: Creating table Components " + result.Replace("Error:", "");

    Console.WriteLine("Creating Clasifications catalog...");
    Clasifications clasifications = new Clasifications();
    result = db.CreateTable(clasifications);
    if( result.StartsWith("Error:") ) return "Error: Creating table Clasifications " + result.Replace("Error:", "");

    Console.WriteLine("Creating Makers catalog...");
    Makers maker = new Makers();
    result = db.CreateTable(maker);
    if( result.StartsWith("Error:") ) return "Error: Creating table Makers " + result.Replace("Error:", "");

    Console.WriteLine("Creating Sku_Dictionaries catalog...");
    Sku_Dictionary sku_dictionary = new Sku_Dictionary();
    result = db.CreateTable(sku_dictionary);
    if( result.StartsWith("Error:") ) return "Error: Creating table Sku_Dictionary " + result.Replace("Error:", "");

    Console.WriteLine("Creating Locations catalog...");
    Locations locations = new Locations();
    result = db.CreateTable(locations);
    if( result.StartsWith("Error:") ) return "Error: Creating table Locations " + result.Replace("Error:", "");

    Console.WriteLine("Creating Price codes catalog...");
    Price_Codes price = new Price_Codes();
    result = db.CreateTable(price);
    if( result.StartsWith("Error:") ) return "Error: Creating table Price_Codes " + result.Replace("Error:", "");

    Console.WriteLine("Creating currencies catalog...");
    Currencies currency = new Currencies();
    result = db.CreateTable(currency);
    if( result.StartsWith("Error:") ) return "Error: Creating table Currencies " + result.Replace("Error:", "");

    Console.WriteLine("Creating tokens catalog...");
    Tokens tokens = new Tokens();
    result = db.CreateTable(tokens);
    if( result.StartsWith("Error:") ) return "Error: Creating table Tokens " + result.Replace("Error:", "");

    Console.WriteLine("Creating Users catalog...");
    Users users = new Users();
    result = db.CreateTable(users);
    if( result.StartsWith("Error:") ) return "Error: Creating table Users " + result.Replace("Error:", "");    

    Console.WriteLine("Creating User Groups catalog...");
    UsersGroup usersgroup = new UsersGroup();
    result = db.CreateTable(usersgroup);
    if( result.StartsWith("Error:") ) return "Error: Creating table UsersGroup " + result.Replace("Error:", "");


}



string SendToAngelPOST(string api_name, string OPerationType, dynamic object_data)
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

    string result = db.Prompt($"POST {tokens_url} MESSAGE {JsonConvert.SerializeObject(d, Formatting.Indented)}");

    if (result.StartsWith("Error:"))
    {
        return $"Error: ApiName {api_name} Account {db_account} OperationType {OPerationType} --> Result -->" + result;
    }

    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
    return responce.result;
}

