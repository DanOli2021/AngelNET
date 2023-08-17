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

#load "tokens.csx"
#load "users.csx"
#load "usersgroup.csx"
#load "branch_stores.csx" 
#load "pins.csx"

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data;

public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public dynamic DataMessage { get; set; }
}

AngelApiOperation api = JsonConvert.DeserializeObject<AngelApiOperation>(message);

//Server parameters
Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));

switch (api.OperationType)
{
    case "CreateAccount":
        return CreateNewAccount( server_db, api );
    default:
        return $"Error: No service found {api.OperationType}";
}


// CreateAccount
// This method is used to create a new user account in the system,
string CreateNewAccount(AngelDB.DB server_db, AngelApiOperation api)
{
    var d = api.DataMessage;

    if (d.Pin == null) return "Error: Pin is required";

    if (d.AccountName == null) return "Error: AccountName is required";

    if (d.Password == null) return "Error: Password is required";

    if (d.Name == null) return "Error: Name is required";

    if (d.User == null) return "Error: User is required";

    string result = server_db.Prompt($"SELECT * FROM pins WHERE pin_number = '{d.Pin}'");

    if (result == "[]")
    {
        return "Error: CreateAccount() Pin does not exist";
    }

    if (result.StartsWith("Error:"))
    {
        return "Error: CreateAccount() " + result.Replace("Error:", "");
    }

    DataTable p = server_db.GetDataTable(result);

    Pin my_pin = new Pin()
    {
        id = p.Rows[0]["id"].ToString(),
        pin_number = p.Rows[0]["pin_number"].ToString(),
        user = p.Rows[0]["user"].ToString(),
        status = p.Rows[0]["status"].ToString(),
        expirytime = p.Rows[0]["expirytime"].ToString(),
        date = p.Rows[0]["date"].ToString()
    };

    DateTime expirytime = DateTime.ParseExact( my_pin.expirytime, "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture );

    if (DateTime.Now.ToUniversalTime() > expirytime)
    {
        return "Error: CreateAccount() Pin has expired";
    }

    if (my_pin.status != "pending")
    {
        return "Error: CreateAccount() status is not pending"; 
    }

    if (my_pin.pin_number != d.Pin.ToString())
    {
        return "Error: CreateAccount() Pin does not match";
    }

    //result = server_db.Prompt($"SELECT * FROM accounts WHERE email = '{my_pin.user}'", true);

    //if (result != "[]")
    //{
    //    return "Error: CreateAccount() Email already exists in another account";
    //}

    result = server_db.Prompt($"SELECT * FROM accounts WHERE id = '{d.AccountName}'");

    if (result != "[]")
    {
        return "Error: CreateAccount() AccountName already exists take another";
    }

    if (!StringValidator.IsValid(d.AccountName.ToString()))
    {
        return "Error: CreateAccount() AccountName is not valid, only letters and numbers are allowed, minimun 5 characters, maximun 30 characters";
    }

    AngelDB.DB db = new AngelDB.DB();

    result = db.Prompt($"DB USER db PASSWORD db DATA DIRECTORY {parameters["accounts_directory"]}/{d.AccountName}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt($"CHANGE MASTER TO USER {d.User} PASSWORD {d.Password}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt($"CREATE ACCOUNT {d.AccountName} SUPERUSER user_{d.AccountName} PASSWORD {d.Password}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt($"USE ACCOUNT {d.AccountName}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt($"CREATE DATABASE {d.AccountName}_data1");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt($"USE DATABASE {d.AccountName}_data1");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    var account = new
    {
        id = d.AccountName,
        db_user = d.User.ToString(),
        name = d.Name,
        email = my_pin.user,
        connection_string = $"DB USER {d.User} PASSWORD {d.Password} DATA DIRECTORY {parameters["accounts_directory"]}/{d.AccountName}",
        db_password = d.Password.ToString(),
        database = $"{d.AccountName}_data1",
        data_directory = $"{parameters["accounts_directory"]}/{d.AccountName}",
        account = d.AccountName.ToString(),
        super_user = $"user_{d.AccountName}",
        super_user_password = d.Password.ToString(),
        active = "true",
        created = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
    };

    result = server_db.Prompt($"UPSERT INTO accounts VALUES {JsonConvert.SerializeObject(account)}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    Tokens tokens = new Tokens();
    result = db.CreateTable(tokens);
    if (result.StartsWith("Error:")) return "Error: Creating table tokens " + result.Replace( "Error:", "" );

    Users users = new Users();
    result = db.CreateTable(users);
    if (result.StartsWith("Error:")) return "Error: Creating table users " + result.Replace( "Error:", "" );

    UsersGroup usersgroup = new UsersGroup();
    result = db.CreateTable(usersgroup);
    if (result.StartsWith("Error:")) return "Error: Creating table UsersGroup " + result.Replace("Error:", "");

    branch_stores branch_store = new branch_stores();
    result = db.CreateTable(branch_store);
    if (result.StartsWith("Error:")) return "Error: Creating table branch_stores " + result.Replace("Error:", "");

    Pin pin = new Pin();
    result = db.CreateTable(my_pin, "pins");
    if (result.StartsWith("Error:")) return "Error: Creating table pins " + result.Replace("Error:", "");


    List<UsersGroup> groups = new List<UsersGroup>();

    UsersGroup g = new UsersGroup();
    g.id = "AUTHORIZERS";
    g.Name = "AUTHORIZERS";
    g.Permissions = "";
    groups.Add(g);

    g = new UsersGroup();
    g.id = "PINSCONSUMER";
    g.Name = "PINSCONSUMER";
    g.Permissions = "";
    groups.Add(g);

    g = new UsersGroup();
    g.id = "SUPERVISORS";
    g.Name = "SUPERVISORS";
    g.Permissions = "";
    groups.Add(g);

    g = new UsersGroup();
    g.id = "ADMINISTRATIVE";
    g.Name = "ADMINISTRATIVE";
    g.Permissions = "";
    groups.Add(g);

    g = new UsersGroup();
    g.id = "ADMINISTRATIVE";
    g.Name = "ADMINISTRATIVE";
    g.Permissions = "";
    groups.Add(g);

    g = new UsersGroup();
    g.id = "CASHIER";
    g.Name = "CASHIER";
    g.Permissions = "";
    groups.Add(g);

    result = db.Prompt("UPSERT INTO usersgroup VALUES " + JsonConvert.SerializeObject(groups));
    if (result.StartsWith("Error:")) return "Error: insert group ADMINISTRATIVE " + result.Replace("Error:", "");

    Users u = new Users();
    u.id = d.User.ToString();
    u.Name = d.Name.ToString();
    u.Password = d.Password.ToString();
    u.UserGroups = "AUTHORIZERS, SUPERVISORS, PINSCONSUMER, ADMINISTRATIVE, CASHIER";
    u.Organization = "";
    u.Email = my_pin.user;
    u.Phone = "";
    u.permissions_list = "Any";
    u.master = "true";

    result = db.Prompt("UPSERT INTO users VALUES " + JsonConvert.SerializeObject(u));
    if (result.StartsWith("Error:")) return "Error: insert user " + result.Replace("Error:", "");

    Tokens t = new Tokens();
    t.id = System.Guid.NewGuid().ToString();
    t.User = d.User.ToString();
    t.ExpiryTime = DateTime.Now.AddYears(1).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    result = db.Prompt("UPSERT INTO tokens VALUES " + JsonConvert.SerializeObject(t));

    return "Ok.";

}


public static class StringValidator
{
    private static readonly Regex ValidStringPattern = new Regex("^[a-zA-Z0-9]{1,30}$");

    public static bool IsValid(string input)
    {

        if (input.Length > 30) return false;
        if (input.Length < 5) return false;

        return ValidStringPattern.IsMatch(input);
    }

}

