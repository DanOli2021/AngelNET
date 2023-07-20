// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Script for managing Branch Stores and Authorizers
// Daniel() Oliver Rojas
// 2023-07-03

#load "branch_stores.csx"
#load "authorizer.csx"
#load "pins.csx"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public dynamic DataMessage { get; set; }
}

Dictionary<string, string> servers = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));
AngelApiOperation api = JsonConvert.DeserializeObject<AngelApiOperation>(message);


switch (api.OperationType.ToString())
{
    case "UpsertBranchStores":

        return AdminBranchStores.UpsertBranchStores(db, servers, api);

    case "DeleteBranchStore":

        return AdminBranchStores.DeleteBranchStore(db, servers, api);

    case "GetBranchStores":

        return AdminBranchStores.GetBranchStores(db, servers, api);

    case "GetBranchStore":

        return AdminBranchStores.GetBranchStore(db, servers, api);

    case "UpsertAuthorizer":

        return AdminBranchStores.UpsertAuthorizer(db, servers, api);

    case "DeleteAuthorizer":

        return AdminBranchStores.DeleteAuthorizer(db, servers, api);

    case "GetAuthorizers":

        return AdminBranchStores.GetAuthorizers(db, servers, api);

    case "GetAuthorizer":

        return AdminBranchStores.GetAuthorizer(db, servers, api);

    case "GetBranchStoreByUser":

        return AdminBranchStores.GetBranchStoreByUser(db, servers, api);

    case "CreatePermission":

        return AdminBranchStores.CreatePermission(db, servers, api);

    case "GetPins":

        return AdminBranchStores.GetPins(db, servers, api);

    case "OperatePin":

        return AdminBranchStores.OperatePin(db, servers, api);

    default:

        return $"Error: No service found {api.OperationType}";
}


public static class AdminBranchStores
{
    public static string UpsertBranchStores(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {
        string result = ValidatePermisions(db, servers, api.Token, "AUTHORIZERS", "BranchStores", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        branch_stores branch_store = new branch_stores
        {
            id = d.id.ToString().Trim().ToUpper(),
            name = d.Name.ToString(),
            address = d.Address.ToString(),
            phone = d.Phone.ToString()
        };

        if (!string.IsNullOrEmpty(d.Authorizer.ToString()))
        {
            result = db.Prompt($"SELECT * FROM authorizers WHERE id = '{d.Authorizer.ToString().Trim()}'");

            if (result.StartsWith("Error:"))
            {
                return "Error: UpsertBranchStores() " + result.Replace("Error:", "");
            }

            if (result == "[]")
            {
                return "Error: No autorizer found";
            }
        }

        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        result = db.Prompt($"UPSERT INTO branch_stores VALUES {JsonConvert.SerializeObject(branch_store, Formatting.Indented, settings)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: UpsertBranchStores() " + result.Replace("Error:", "");
        }

        return result;
    }



    public static string DeleteBranchStore(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "AUTHORIZERS", "BranchStores", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        result = db.Prompt($"DELETE FROM branch_stores PARTITION KEY main WHERE id = '{d.BranchStoreToDelete}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return $"Ok. Branch Store deleted successfully: {d.BranchStoreToDelete}";
    }

    public static string GetBranchStores(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "AUTHORIZERS", "BranchStores", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (!string.IsNullOrEmpty( d.Where.ToString() ))
        {
            result = db.Prompt($"SELECT * FROM branch_stores PARTITION KEY main WHERE {d.Where}");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM branch_stores PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }

    public static string GetBranchStore(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "AUTHORIZERS, SUPERVISORS", "", "", true);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        result = db.Prompt($"SELECT * FROM branch_stores PARTITION KEY main WHERE id = '{d.BranchStoreId}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        if (result == "[]") 
        {
            return "Error: No branch store found";
        }

        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);

        branch_stores b = new branch_stores();
        b.id = dt.Rows[0]["id"].ToString();
        b.name = dt.Rows[0]["name"].ToString();
        b.address = dt.Rows[0]["address"].ToString();
        b.phone = dt.Rows[0]["phone"].ToString();
        b.authorizer = dt.Rows[0]["authorizer"].ToString();

        return JsonConvert.SerializeObject(b, Formatting.Indented);

    }

    public static string UpsertAuthorizer(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {
        string result = ValidatePermisions(db, servers, api.Token.ToString(), "AUTHORIZERS", "Authorizer", "Upsert");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (string.IsNullOrEmpty(d.RetypePassword.ToString()))
        {
            return "Error: No RetypePassword found";
        }

        if (string.IsNullOrEmpty(d.Password.ToString()))
        {
            return "Error: No password found";
        }

        if (d.Password.ToString().Trim() != d.Password.ToString().Trim())
        {
            return "Error: Passwords do not match";
        }

        string db_account = db.Prompt("VAR db_account");

        // Creamos un usuario
        var user = new
        {
            api = "tokens/admintokens",
            account = db_account,
            language = "C#",
            message = new
            {
                OperationType = "CreateUser",
                Token = api.Token,
                DataMessage = new
                {
                    User = d.id.ToString().Trim().ToLower(),
                    Password = d.Password.ToString().Trim(),
                    UserGroups = d.UserGroups.ToString().Trim().ToLower(),
                    Name = d.Name.ToString(),
                    Email = d.Email.ToString().ToLower(),
                    Phone = d.Phone.ToString(),
                    Organization = ""
                }
            }
        };

        result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(user, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: UpsertAuthorizer() 1 " + result.Replace("Error:", "");
        }

        AngelDB.AngelResponce response = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

        if (response.result.ToString().StartsWith("Error:"))
        {
            return "Error: UpsertAuthorizer() 2 " + response.result;
        }

        // Creamos un nuevo token
        var new_token = new
        {
            api = "tokens/admintokens",
            account = db_account,
            language = "C#",
            message = new
            {
                OperationType = "CreateNewToken",
                Token = api.Token,
                DataMessage = new
                {
                    User = d.id.ToString().Trim().ToLower(),
                    expiry_days = -1
                }
            }
        };

        result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(new_token, Formatting.Indented)}");
        response = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

        if (result.StartsWith("Error:"))
        {
            return "Error: UpsertAuthorizer() " + result.Replace("Error:", "");
        }

        response = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

        if (response.result.ToString().StartsWith("Error:"))
        {
            return "Error: UpsertAuthorizer() " + response.result;
        }

        Authorizer authorizer = new Authorizer()
        {
            id = d.id.ToString().ToLower(),
            name = d.Name.ToString().Trim(),
            phone = d.Phone.ToString().Trim(),
            email = d.Email.ToString().Trim(),
            password = d.Password.ToString().Trim(),
            permissions_list = d.PermissionsList.ToString().Trim(),
            user_group = d.UserGroups.ToString().Trim()
        };

        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        result = db.Prompt($"UPSERT INTO authorizers VALUES {JsonConvert.SerializeObject(authorizer, Formatting.Indented, settings)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: UpsertAuthorizer() " + result.Replace("Error:", "");
        }

        return result;

    }

    public static string DeleteAuthorizer(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "AUTHORIZERS", "Authorizer", "Delete");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        result = db.Prompt($"DELETE FROM authorizers PARTITION KEY main WHERE id = '{d.AuthorizerToDelete}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        result = db.Prompt($"UPDATE branch_stores PARTITION KEY main SET authorizer = NULL WHERE authorizer = '{d.AuthorizerToDelete}'");

        return result;
    }

    public static string GetAuthorizers(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "AUTHORIZERS", "Authorizer", "Query");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (string.IsNullOrEmpty(d.Where.ToString()))
        {
            result = db.Prompt($"SELECT id, name, phone, email, permissions_list, user_group FROM authorizers PARTITION KEY main WHERE {d.Where}");
        }
        if (string.IsNullOrEmpty(d.id.ToString()))
        {
            result = db.Prompt($"SELECT id, name, phone, email, permissions_list, user_group FROM authorizers PARTITION KEY main WHERE id = '{d.id}'");
        }
        else
        {
            result = db.Prompt($"SELECT id, name, phone, email, permissions_list, user_group FROM authorizers PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;
    }


    public static string GetAuthorizer(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "AUTHORIZERS, SUPERVISORS", "", "", true);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        result = db.Prompt($"SELECT * FROM authorizers PARTITION KEY main WHERE id = '{d.Authorizer}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);

        Authorizer a = new Authorizer()
        {
            id = dt.Rows[0]["id"].ToString(),
            name = dt.Rows[0]["name"].ToString(),
            phone = dt.Rows[0]["phone"].ToString(),
            email = dt.Rows[0]["email"].ToString(),
            permissions_list = dt.Rows[0]["permissions_list"].ToString(),
            user_group = dt.Rows[0]["user_group"].ToString()
        };

        return JsonConvert.SerializeObject(a, Formatting.Indented);

    }




    public static string GetBranchStoreByUser(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "SUPERVISORS", "", "", true);
        string db_account = db.Prompt("VAR db_account");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        // Permissions using Token
        var d13 = new
        {
            api = "tokens/admintokens",
            account = db_account,
            language = "C#",
            message = new
            {
                OperationType = "GetUserUsingToken",
                Token = api.Token,
                DataMessage = new
                {
                    TokenToGetTheUser = api.Token
                }
            }
        };

        result = db.Prompt($"POST {servers["tokens_url"]} MESSAGE {JsonConvert.SerializeObject(d13, Formatting.Indented)}");
        AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        if (responce.result.StartsWith("Error:"))
        {
            return "Error: " + responce.result.Replace("Error:", "");
        }

        result = db.Prompt($"SELECT * FROM branch_stores PARTITION KEY main WHERE authorizer = '{responce.result}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;

    }


    public static string CreatePermission(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "SUPERVISORS", "", "", true);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        Dictionary<string, string> group = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

        result = db.Prompt($"SELECT * FROM branch_stores PARTITION KEY main WHERE id = '{d.Branchstore_id.ToString()}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: Branch Store {d.Branchstore_id.ToString()} not found";
        }

        Pin p = new Pin()
        {
            id = Guid.NewGuid().ToString(),
            authorizer = group["user"],
            authorizer_name = group["user_name"],
            branch_store = d.Branchstore_id.ToString(),
            date = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
            permissions = d.Permission_id.ToString(),
            status = "pending",
            pin_number = RandomNumberGenerator.GenerateRandomNumber(4)
        };

        result = db.Prompt($"UPSERT INTO pins VALUES {JsonConvert.SerializeObject(p, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return JsonConvert.SerializeObject(p, Formatting.Indented);

    }

    public static string GetPins(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "SUPERVISORS, AUTHORIZERS", "", "", true);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        Dictionary<string, string> group = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

        if (group["id"] == "SUPERVISORS")
        {
            result = db.Prompt($"SELECT * FROM pins WHERE date >= '{d.InitialDate}' AND date <= '{d.FinalDate} 24:00:00' AND authorizer = '{group["user"]}' ORDER BY date DESC");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM pins WHERE date >= '{d.InitialDate}' AND date <= '{d.FinalDate} 24:00:00' ORDER BY date DESC");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;

    }


    public static string OperatePin(AngelDB.DB db, Dictionary<string, string> servers, AngelApiOperation api)
    {

        string result = ValidatePermisions(db, servers, api.Token, "PINSCONSUMER", "", "", true);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        Dictionary<string, string> group = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

        string PartitionKey = DateTime.Now.ToString("yyyy-MM");

        result = db.Prompt($"SELECT * FROM pins WHERE pin_number = '{d.Pin}' AND permissions = '{d.Permission}' AND branch_store = '{d.BranchStore}' AND status = 'pending' ORDER BY date DESC");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: Pin not found";
        }

        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);
        DataRow r = dt.Rows[0];

        Pin pin = new Pin()
        {
            id = dt.Rows[0]["id"].ToString(),
            confirmed_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            status = "confirmed",
            user = d.User,
            user_name = d.UserName,
            message = d.Message,
            app_user = d.AppUser,
            app_user_name = d.AppUserName
        };

        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        result = db.Prompt($"UPSERT INTO pins VALUES {JsonConvert.SerializeObject(pin, Formatting.Indented, settings)}");

        return result;
    }



    static public string GetPermisions(AngelDB.DB db, string server_url, string token)
    {

        string result = "";
        string db_account = db.Prompt("VAR db_account");

        // Permissions using Token
        var m = new
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

        result = db.Prompt($"POST {server_url} MESSAGE {JsonConvert.SerializeObject(m, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

        if (responce.result.StartsWith("Error:"))
        {
            return responce.result;
        }

        return responce.result;

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


}


