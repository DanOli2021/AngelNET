﻿// GLOBALS
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

//Server parameters
Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));

switch (api.OperationType)
{
    case "UpsertGroup":
        return AdminAuth.UpsertGroup(db, api);
    case "GetGroups":
        return AdminAuth.GetGroups(db, api);
    case "DeleteGroup":
        return AdminAuth.DeleteGroup(db, api);
    case "UpsertUser":
        return AdminAuth.UpsertUser(db, api);
    case "DeleteUser":
        return AdminAuth.DeleteUser(db, api);
    case "GetUsers":
        return AdminAuth.GetUsers(db, api);
    case "GetUser":
        return AdminAuth.GetUser(db, api);
    case "SaveToken":
        return AdminAuth.SaveToken(db, api);
    case "DeleteToken":
        return AdminAuth.DeleteToken(db, api);
    case "ValidateToken":
        return AdminAuth.ValidateToken(db, api);
    case "GetTokenFromUser":
        return AdminAuth.GetTokenFromUser(db, api);
    case "GetGroupsUsingTocken":
        return AdminAuth.GetGroupsUsingTocken(db, api);
    case "GetUserUsingToken":
        return AdminAuth.GetUserUsingToken(db, api);
    case "GetTokens":
        return AdminAuth.GetTokens(db, api);
    case "GetToken":
        return AdminAuth.GetToken(db, api);
    case "UpsertBranchStore":
        return AdminAuth.UpsertBranchStore(db, api);
    case "GetBranchStores":
        return AdminAuth.GetBranchStores(db, api);
    case "DeleteBranchStore":
        return AdminAuth.DeleteBranchStore(db, api);
    case "GetBranchStore":
        return AdminAuth.GetBranchStore(db, api);
    case "GetBranchStoresByUser":
        return AdminAuth.GetBranchStoresByUser(db, api);
    case "CreatePermission":
        return AdminAuth.CreatePermission(db, api);
    case "GetPins":
        return AdminAuth.GetPins(db, api);
    case "OperatePin":
        return AdminAuth.OperatePin(db, api);
    case "SendPinToEmail":
        return AdminAuth.SendPinToEmail(db, api, parameters, server_db);
    case "RecoverMasterPassword":
        return AdminAuth.RecoverMasterPassword(db, api, parameters, server_db);
    default:
        return $"Error: No service found {api.OperationType}";
}

public class OperationTypeClass
{
    public string OperationType;
}


public static class AdminAuth
{
    public static string SaveToken(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "SaveToken");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;


        if (d.User == null)
        {
            return "Error: SaveToken() User is null";
        }

        d.User = d.User.ToString().Split('@')[0];

        if (d.UsedFor == null)
        {
            d.UsedFor = "App Access";
        }

        if (d.ExpiryTime == null)
        {
            return "Error: SaveToken() ExpiryTime is null";
        }

        if (d.id == null)
        {
            return "Error: SaveToken() id (Token) is null"; ;
        }

        d.ExpiryTime = ConvertToDateTimeWithMaxTime(d.ExpiryTime.ToString());

        if (d.Observations == null)
        {
            d.Observations = "";
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{d.User}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: SaveToken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: User not found {d.User}";
        }

        Tokens t = new Tokens();

        if (d.id == "New")
        {
            d.id = System.Guid.NewGuid().ToString();
        }

        t.id = d.id;
        t.User = d.User;
        t.UsedFor = d.UsedFor;
        t.CreationTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        t.Observations = d.Observations;
        t.ExpiryTime = d.ExpiryTime;

        result = db.Prompt($"UPSERT INTO tokens VALUES {JsonConvert.SerializeObject(t)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: SaveToken() insert " + result.Replace("Error:", "");
        }

        return t.id;

    }

    public static string GetTokens(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "GetTokens");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.Where == null)
        {
            d.Where = "";
        }

        if (!string.IsNullOrEmpty(d.Where.ToString()))
        {
            result = db.Prompt($"SELECT * FROM tokens PARTITION KEY main WHERE {d.Where}");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM tokens PARTITION KEY main");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        DataTable t = JsonConvert.DeserializeObject<DataTable>(result);

        DataColumn newColumn = new DataColumn("ServerTime", typeof(System.String));
        newColumn.DefaultValue = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        t.Columns.Add(newColumn);

        return JsonConvert.SerializeObject(t);
    }


    public static string GetToken(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "GetToken");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.TokenId == null)
        {
            return "Error: GetToken() TokenId is null";
        }

        result = db.Prompt($"SELECT * FROM tokens PARTITION KEY main WHERE id = '{d.TokenId}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: No token found";
        }

        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);

        Tokens t = new Tokens();
        t.id = dt.Rows[0]["id"].ToString();
        t.User = dt.Rows[0]["User"].ToString();
        t.ExpiryTime = dt.Rows[0]["ExpiryTime"].ToString();
        t.UsedFor = dt.Rows[0]["UsedFor"].ToString();
        t.Observations = dt.Rows[0]["Observations"].ToString();
        t.CreationTime = dt.Rows[0]["CreationTime"].ToString();

        return JsonConvert.SerializeObject(t, Formatting.Indented);

    }


    public static string DeleteToken(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "DeleteToken");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;


        if (d.TokenToDelete == null)
        {
            return "Error: DeleteToken() TokenToDelete is null";
        }

        result = db.Prompt($"SELECT * FROM tokens WHERE id = '{d.TokenToDelete}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteToken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: DeleteToken() Token not found {d["TokenToDelete"]}";
        }

        result = db.Prompt($"DELETE FROM tokens PARTITION KEY main WHERE id = '{d["TokenToDelete"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteToken() delete tockens " + result.Replace("Error:", "");
        }

        return "Ok. Token deleted: " + d.TokenToDelete;

    }


    public static string GetGroupsUsingTocken(AngelDB.DB db, AngelApiOperation api)
    {

        //string result = ValidateAdminUser(db, api.Token);
        //
        //if (result.StartsWith("Error:"))
        //{
        //    return result;
        //}

        var d = api.DataMessage;



        if (d.TokenToObtainPermission == null)
        {
            return "Error: GetGroupsUsingTocken() TokenToObtainPermission is null";
        }


        string result = db.Prompt($"SELECT * FROM tokens WHERE id = '{d.TokenToObtainPermission}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetGroupsUsingTocken() " + result.Replace("Error:", "");
        }

        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return $"Error: GetGroupsUsingTocken() Token {d.TokenToObtainPermission} not found";
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{dt.Rows[0]["user"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetGroupsUsingTocken() " + result.Replace("Error:", "");
        }

        dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return "Error: User not found";
        }

        DataRow r = JsonConvert.DeserializeObject<DataTable>(result).Rows[0];

        var group = new
        {
            groups = dt.Rows[0]["UserGroups"],
            user = dt.Rows[0]["id"].ToString(),
            user_name = dt.Rows[0]["name"].ToString(),
            permissions_list = dt.Rows[0]["permissions_list"].ToString(),
        };

        return JsonConvert.SerializeObject(group, Formatting.Indented);

    }


    public static string GetUserUsingToken(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS", "GetUserUsingToken");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;


        if (d.TokenToGetTheUser == null)
        {
            return "Error: GetUserUsingTocken() TokenToGetTheUser is null";
        }


        result = ValidateToken(db, d.TokenToGetTheUser.ToString());

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"SELECT * FROM tokens WHERE id = '{d.TokenToGetTheUser}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetUserUsingTocken() " + result.Replace("Error:", "");
        }

        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return "Error: GetUserUsingTocken() Token not found";
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{dt.Rows[0]["user"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetUserUsingTocken() " + result.Replace("Error:", "");
        }

        dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return "Error: User not found";
        }

        return dt.Rows[0]["id"].ToString();

    }


    public static string ValidateToken(AngelDB.DB db, string token)
    {
        string result = "";

        result = db.Prompt($"SELECT * FROM tokens WHERE id = '{token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: ValidateToken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: ValidateToken() {token} Token not found";
        }

        Tokens[] t = JsonConvert.DeserializeObject<Tokens[]>(result);

        if (t[0].ExpiryTime.CompareTo(DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff")) < 0)
        {
            return "Error: ValidateToken() Token expired";
        }

        return $"Ok. Token is valid: {token}";

    }



    public static string ValidateToken(AngelDB.DB db, AngelApiOperation api)
    {
        var d = api.DataMessage;
        return ValidateToken(db, d.TokenToValidate.ToString());
    }



    public static string GetTokenFromUser(AngelDB.DB db, AngelApiOperation api)
    {

        string result = "";

        //result = ValidateAdminUser(db, to.Token);

        //if (result.StartsWith("Error:"))
        //{
        //    return result;
        //}

        var d = api.DataMessage;

        if (d.User == null)
        {
            return "Error: GetTokenFromUser() User is null";
        }

        d.User = d.User.ToString().Split('@')[0];

        if (d.Password == null)
        {
            return "Error: GetTokenFromUser() Password is null";
        }

        d.User = d.User.ToString().Split('@')[0];

        result = db.Prompt($"SELECT * FROM users WHERE id = '{d.User}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetTokenFromUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: User not found {d.User}";
        }

        Users[] u = JsonConvert.DeserializeObject<Users[]>(result);

        if (u[0].Password.Trim() != d.Password.ToString().Trim())
        {
            return "Error: Invalid password";
        }

        result = db.Prompt($"SELECT * FROM tokens WHERE user = '{d.User}' AND ExpiryTime > '{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff")}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetTokenFromUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: GetTokenFromUser() Token not found";
        }

        Tokens[] t = JsonConvert.DeserializeObject<Tokens[]>(result);

        if (t[0].ExpiryTime.CompareTo(DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff")) < 0)
        {
            return "Error: GetTokenFromUser() Token expired";
        }

        return t[0].id;

    }


    public static string UpsertUser(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "UpsertUser");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.User == null)
        {
            return "Error: UpsertUser() User is null";
        }

        d.User = d.User.ToString().Split('@')[0];

        if (d.Password == null)
        {
            return "Error: UpsertUser() Password is null";
        }

        if (d.UserGroups == null)
        {
            return "Error: UpsertUser() userGroups is null";
        }

        if (d.Name == null)
        {
            return "Error: UpsertUser() Name is null";
        }

        if (d.Organization == null)
        {
            return "Error: UpsertUser() Organization is null";
        }

        if (d.Email == null)
        {
            return "Error: UpsertUser() Email is null";
        }

        if (d.Phone == null)
        {
            return "Error: UpsertUser() Phone is null";
        }

        if (d.permissions_list == null)
        {
            return "Error: UpsertUser() permissions_list is null";
        }

        if (string.IsNullOrEmpty(d.Password.ToString()))
        {
            return "Error: UpsertUser() password is null or empty";
        }

        string[] groups = d.UserGroups.ToString().Split(',');

        foreach (string group in groups)
        {
            result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{group.Trim().ToUpper()}'");

            if (result.StartsWith("Error:"))
            {
                return "Error: UpsertUser() Auth " + result.Replace("Error:", "");
            }

            if (result == "[]")
            {
                return "Error: UpsertUser() Auth No user group found: " + group;
            }
        }

        Users t = new Users();

        t.id = d.User;
        t.UserGroups = d.UserGroups;
        t.Name = d.Name;
        t.Password = d.Password;
        t.Organization = d.Organization;
        t.Email = d.Email;
        t.Phone = d.Phone;
        t.permissions_list = d.permissions_list;

        result = db.Prompt($"UPSERT INTO users VALUES {JsonConvert.SerializeObject(t)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: UpsertUser() insert " + result.Replace("Error:", "");
        }

        result = GetTokenFromUser(db, api);

        if (result.StartsWith("Error:"))
        {
            api.DataMessage.ExpiryTime = DateTime.Now.AddDays(30).ToUniversalTime().ToString("yyyy-MM-dd");

            var new_token = new Tokens();
            new_token.id = "New";
            new_token.User = d.User;
            new_token.ExpiryTime = DateTime.Now.AddDays(30).ToUniversalTime().ToString("yyyy-MM-dd");
            new_token.UsedFor = "App Login";
            new_token.Observations = "Created by UpsertUser()";
            api.DataMessage = new_token;
            result = SaveToken(db, api);

            if (result.StartsWith("Error:"))
            {
                return "Error: UpsertUser() " + result.Replace("Error:", "");
            }
        }

        return $"Ok. User created successfully: " + d.User;

    }


    public static string GetUsers(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "GetUsers");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        string user = result;

        if (d.Where == null)
        {
            d.Where = "";
        }

        if (string.IsNullOrEmpty(d.Where.ToString()))
        {
            result = db.Prompt($"SELECT * FROM users ORDER BY name ");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM users WHERE {d.Where} ORDER BY name");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: GetUsers() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: GetUsers() Users not found";
        }

        DataTable t = JsonConvert.DeserializeObject<DataTable>(result);
        t.Columns.Remove("Password");

        return JsonConvert.SerializeObject(t, Formatting.Indented);

    }

    public static string GetUser(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS", "GetUser");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;


        if (d.User == null)
        {
            return "Error: GetUser() User is null";
        }

        d.User = d.User.ToString().Split('@')[0];

        result = db.Prompt($"SELECT * FROM users WHERE id = '{d.User}' ORDER BY name");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: GetUser() User not found";
        }

        DataTable t = JsonConvert.DeserializeObject<DataTable>(result);
        t.Columns.Remove("Password");

        var user = new
        {
            User = t.Rows[0]["id"],
            Name = t.Rows[0]["Name"],
            Organization = t.Rows[0]["Organization"],
            Email = t.Rows[0]["Email"],
            Phone = t.Rows[0]["Phone"],
            UserGroups = t.Rows[0]["UserGroups"],
            permissions_list = t.Rows[0]["permissions_list"]
        };

        return JsonConvert.SerializeObject(user, Formatting.Indented);

    }


    public static string DeleteUser(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "DeleteUser");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.UserToDelete == null)
        {
            return "Error: DeleteUser() UserToDelete is null";
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{d.UserToDelete}'");

        if (result == "[]")
        {
            return "Error: DeleteUser() User not found";
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteUser() " + result.Replace("Error:", "");
        }

        result = db.Prompt($"DELETE FROM users PARTITION KEY main WHERE id = '{d["UserToDelete"]}'");


        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() insert " + result.Replace("Error:", "");
        }

        return $"Ok. User deleted successfully: {d.UserToDelete}";

    }


    public static string UpsertGroup(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "UpsertGroup");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;



        if (d.UserGroup == null)
        {
            return "Error: UpsertGroup() UserGroup is null";
        }

        if (d.Name == null)
        {
            return "Error: UpsertGroup() Name is null";
        }

        if (d.Permissions == null)
        {
            return "Error: UpsertGroup() Permissions is null";
        }

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{d.UserGroup}'");

        dynamic NewPermissions = d.Permissions;
        UsersGroup t = new UsersGroup();

        t.id = d.UserGroup.ToString().ToUpper().Trim();
        t.Name = d.Name;
        t.Permissions = d.Permissions;

        result = db.Prompt($"UPSERT INTO UsersGroup VALUES {JsonConvert.SerializeObject(t, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewGroup() upsert " + result.Replace("Error:", "");
        }

        return $"Ok. Users Group created successfully: {d.UserGroup}";

    }


    public static string GetGroups(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "GetGroups");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.Where == null)
        {
            d.Where = "";
        }

        if (string.IsNullOrEmpty(d.Where.ToString()))
        {
            result = db.Prompt($"SELECT * FROM UsersGroup ORDER BY name");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM UsersGroup WHERE {d.Where}");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: GetGroups() " + result.Replace("Error:", "");
        }

        return result;
    }

    public static string DeleteGroup(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "DeleteGroup");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;


        if (d.UserGroupToDelete == null)
        {
            return "Error: DeleteGroup() UserGroupToDelete is null";
        }
        
        List<string> system_groups = new List<string> { "AUTHORIZERS", "SUPERVISORS", "PINSCONSUMER", "CASHIER", "ADMINISTRATIVE" };

        foreach (string item in system_groups)
        {
            if (item == d.UserGroupToDelete.ToString().ToUpper().Trim())
            {
                return $"Error: UpsertGroup() UserGroup {d.UserGroupToDelete.ToString().Trim().ToUpper()} is a system group";
            }
        }

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{d.UserGroupToDelete.ToString().Trim().ToUpper()}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteGroup() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: DeleteGroup() The group indicated by you does not exist on this system: " + d.UserGroupToDelete;
        }

        result = db.Prompt($"DELETE FROM UsersGroup PARTITION KEY main WHERE id = '{d.UserGroupToDelete.ToString().Trim().ToUpper()}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteGroup() " + result.Replace("Error:", "");
        }

        return $"Ok. User Group deleted successfully {d.UserGroupToDelete}";
    }



    public static string UpsertBranchStore(AngelDB.DB db, AngelApiOperation api)
    {
        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "UpsertBranchStore");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.id == null)
        {
            return "Error: UpsertBranchStores() id is null";
        }

        if (d.name == null)
        {
            return "Error: UpsertBranchStores() Name is null";
        }

        if (d.address == null)
        {
            return "Error: UpsertBranchStores() Address is null";
        }

        if (d.phone == null)
        {
            return "Error: UpsertBranchStores() Phone is null";
        }

        if (d.authorizer == null)
        {
            d.authorizer = "";
        }

        branch_stores branch_store = new branch_stores
        {
            id = d.id.ToString().Trim().ToUpper(),
            name = d.name.ToString(),
            address = d.address.ToString(),
            phone = d.phone.ToString(),
            authorizer = d.authorizer.ToString().Trim()
        };

        if (!string.IsNullOrEmpty(d.authorizer.ToString()))
        {
            result = db.Prompt($"SELECT * FROM users WHERE id = '{d.authorizer.ToString().Trim()}'");

            if (result.StartsWith("Error:"))
            {
                return "Error: UpsertBranchStores() " + result.Replace("Error:", "");
            }

            if (result == "[]")
            {
                return $"Error: No user found: {d.authorizer}";
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


    public static string GetBranchStores(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS", "GetBranchStores");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.Where == null)
        {
            d.Where = "";
        }

        if (!string.IsNullOrEmpty(d.Where.ToString()))
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


    public static string GetBranchStore(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS", "GetBranchStore");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.BranchStoreId == null)
        {
            return "Error: GetBranchStore() BranchStoreId is null";
        }

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

    public static string DeleteBranchStore(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "DeleteBranchStore");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.BranchStoreToDelete == null)
        {
            return "Error: DeleteBranchStore() BranchStoreToDelete is null";
        }

        result = db.Prompt($"DELETE FROM branch_stores PARTITION KEY main WHERE id = '{d.BranchStoreToDelete}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return $"Ok. Branch Store deleted successfully: {d.BranchStoreToDelete}";
    }


    public static string GetBranchStoresByUser(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS", "GetBranchStoresByUser");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        api.DataMessage.TokenToGetTheUser = api.Token;

        result = GetUserUsingToken(db, api);

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        result = db.Prompt($"SELECT * FROM branch_stores PARTITION KEY main WHERE authorizer = '{result}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;

    }


    public static string CreatePermission(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS", "CreatePermission");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.Branchstore_id == null)
        {
            return "Error: CreatePermission() Branchstore_id is null";
        }

        if (d.Permission_id == null)
        {
            return "Error: CreatePermission() Permission_id is null";
        }

        if (d.User == null)
        {
            d.User = "";
        }

        d.User = d.User.ToString().Split('@')[0];

        if (d.PinType == null)
        {
            d.PinType = "Generic";
        }

        if (d.PinType == "touser")
        {

            if (d.User == "")
            {
                return "Error: CreatePermission() User is null";
            }

            result = db.Prompt($"SELECT * FROM users WHERE id = '{d.User}'");

            if (result.StartsWith("Error:"))
            {
                return result;
            }

            if (result == "[]")
            {
                return $"Error: User {d.User} not found";
            }
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{result}'");
        DataTable user = JsonConvert.DeserializeObject<DataTable>(result);

        result = db.Prompt($"SELECT * FROM branch_stores PARTITION KEY main WHERE id = '{d.Branchstore_id.ToString()}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            if (d.Branchstore_id.ToString() != "SYSTEM")
            {
                return $"Error: Branch Store {d.Branchstore_id.ToString()} not found";
            }
        }

        if (d.Minutes == null)
        {
            d.Minutes = 30;
        }

        int minutes = 30;
        int.TryParse(d.Minutes.ToString(), out minutes);

        Pin p = new Pin()
        {
            id = Guid.NewGuid().ToString(),
            authorizer = user.Rows[0]["id"].ToString(),
            authorizer_name = user.Rows[0]["Name"].ToString(),
            branch_store = d.Branchstore_id.ToString(),
            date = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
            expirytime = DateTime.Now.AddMinutes(minutes).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
            permissions = d.Permission_id.ToString(),
            status = "pending",
            pin_number = RandomNumberGenerator.GenerateRandomNumber(4),
            user = d.User.ToString(),
            pintype = d.PinType.ToString(),
            minutes = minutes,
            authorizer_message = d.AuthorizerMessage.ToString(),
        };

        result = db.Prompt($"UPSERT INTO pins VALUES {JsonConvert.SerializeObject(p, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return JsonConvert.SerializeObject(p, Formatting.Indented);

    }

    public static string GetPins(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS", "GetPins");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.InitialDate == null)
        {
            return "Error: GetPins() InitialDate is null";
        }

        if (d.FinalDate == null)
        {
            return "Error: GetPins() FinalDate is null";
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{result}'");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (result == "[]")
        {
            return $"Error: User {result} not found";
        }

        DataTable user = JsonConvert.DeserializeObject<DataTable>(result);

        DateTime parsedDate;

        DateTime.TryParseExact(d.InitialDate.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
        d.InitialDate = parsedDate.ToUniversalTime().ToString("yyyy-MM-dd");
        d.FinalDate = d.FinalDate + " 23:59:59";

        DateTime.TryParseExact(d.FinalDate.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
        d.FinalDate = parsedDate.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

        if (!user.Rows[0]["UserGroups"].ToString().Contains("AUTHORIZERS"))
        {
            result = db.Prompt($"SELECT * FROM pins WHERE date >= '{d.InitialDate}' AND date <= '{d.FinalDate}' AND authorizer = '{user.Rows[0]["id"]}' ORDER BY date DESC");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM pins WHERE date >= '{d.InitialDate}' AND date <= '{d.FinalDate}' ORDER BY date DESC");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: " + result.Replace("Error:", "");
        }

        return result;

    }


    public static string OperatePin(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token, "AUTHORIZERS, SUPERVISORS, PINSCONSUMER", "OperatePin");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (d.Pin == null)
        {
            return "Error: OperatePin() Pin is null";
        }

        if (d.Permission == null)
        {
            return "Error: OperatePin() Permission is null";
        }

        if (d.BranchStore == null)
        {
            return "Error: OperatePin() BranchStore is null";
        }

        if (d.AppUser == null)
        {
            return "Error: OperatePin() AppUser is null";
        }

        if (d.AppUserName == null)
        {
            return "Error: OperatePin() AppUserName is null";
        }

        if (d.PinType == null)
        {
            d.PinType = "Generic";
        }

        string PartitionKey = DateTime.Now.ToUniversalTime().ToString("yyyy-MM");
        result = db.Prompt($"SELECT * FROM pins PARTION KEY {PartitionKey} WHERE pin_number = '{d.Pin}' AND permissions = '{d.Permission}' AND branch_store = '{d.BranchStore}' AND status = 'pending' ORDER BY date DESC");

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

        if (d.PinType.ToString().Trim().ToLower() == "touser")
        {
            if (r["d.User"].ToString().Trim().ToLower() != d.User.ToString().Trim().ToLower())
            {
                return "Error: This pin is not for the user who is trying to confirm it";
            }
        }

        DateTime expiry = DateTime.Parse(r["expirytime"].ToString());

        if (DateTime.Now.ToUniversalTime() > expiry)
        {
            return $"Error: Pin expired {dt.Rows[0]["id"].ToString()}";
        }

        Pin pin = new Pin()
        {
            id = dt.Rows[0]["id"].ToString(),
            confirmed_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            status = "confirmed",
            user = d.User,
            message = d.Message,
            app_user = d.AppUser,
            app_user_name = d.AppUserName,
            pintype = d.PinType
        };

        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        result = db.Prompt($"UPSERT INTO {PartitionKey} pins VALUES {JsonConvert.SerializeObject(pin, Formatting.Indented, settings)}");

        return result;
    }


    public static string RecoverMasterPassword(AngelDB.DB db, AngelApiOperation api, Dictionary<string, string> parameters, AngelDB.DB server_db)
    {
        var d = api.DataMessage;

        if (d.Email == null)
        {
            return "Error: SendPinToEmail() Email is null";
        }

        string Email = d.Email.ToString().Trim().ToLower();

        if( string.IsNullOrEmpty(Email))
        {
            return "Error: Email is required";
        }

        if (EmailValidator.IsValidEmail(Email))
        {
            return $"Error: Email is not valid {Email}";
        }

        string result = server_db.Prompt($"SELECT * FROM accounts WHERE email = '{Email}'");

        if( result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        if( result == "[]")
        {
            return "Error: RecoverMasterPassword() Email not found";
        }

        DataTable tAccounts = db.GetDataTable(result);
        DataRow r = tAccounts.Rows[0];

        if (string.IsNullOrEmpty(r["connection_string"].ToString()))
        {
            result = db.Prompt($"DB USER {r["db_user"]} PASSWORD {r["db_password"]} DATA DIRECTORY {r["data_directory"]}");
        }
        else
        { 
            result = db.Prompt(r["connection_string"].ToString());
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        result = db.Prompt($"USE {r["account"]} DATABASE {r["database"]}");

        if (result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        result = db.Prompt("SELECT * FROM users WHERE master = 'true'");

        if (result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        string user = "";
        string password = "";

        if (result != "[]") 
        {
            DataTable tUsers = db.GetDataTable(result);
            DataRow rUser = tUsers.Rows[0];

            user = rUser["id"].ToString();
            password = rUser["password"].ToString();
        }

        string htmlCode = $@"<!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>MyBusiness POS Authorizer</title>
                            </head>
                            <body>
                                <h1>MyBusiness POS Authorizer</h1>
                                <p></p>
                                <h2>Account     : {r["account"]}</h2>   
                                <h2>DB User     : {r["db_user"]}</h2>
                                <h2>DB Password : {r["db_password"]}</h2>
                                <h2>User        : {user}</h2>
                                <h2>Password    : {password}</h2>
                                <p></p>
                            </body>
                            </html>";

        return "Ok.";

        //result = SendMailFromSoap(htmlCode, Email, "MyBusiness POS Authorizer (Recover)").GetAwaiter().GetResult();

        // result = EmailSender.SendEmail(parameters["email_address"],
        //                                 parameters["email_password"],
        //                                 "MyBusiness POS Authorizer (Recover)",
        //                                 email,
        //                                 "",
        //                                 "MyBusiness POS Authorizer (Recover)",
        //                                 htmlCode,
        //                                 parameters["smtp"].ToString().Trim(),
        //                                 int.Parse(parameters["smtp_port"].ToString().Trim()),
        //                                 false);

        //if (result.StartsWith("Error:"))
        //{
        //    return result;
        //}

        //XDocument doc = XDocument.Parse(result);
        //XNamespace ns = "http://wsCorreo.mybusinesspos.com/";
        //result = doc.Descendants(ns + "EnviaCorreoHResult").First().Value;
        //return result;

    }


    public static string SendPinToEmail(AngelDB.DB db, AngelApiOperation api, Dictionary<string, string> parameters, AngelDB.DB server_db)
    {

        var d = api.DataMessage;

        if (d.Email == null)
        {
            return "Error: SendPinToEmail() Email is null";
        }

        string email = d.Email.ToString().Trim().ToLower();

        if (string.IsNullOrEmpty(email))
        {
            return "Error: Email is required";
        }

        if (EmailValidator.IsValidEmail(email))
        {
            return $"Error: Email is not valid {email}";
        }

        string result = server_db.Prompt($"SELECT * FROM accounts WHERE email = '{email}'", true);

        if (result != "[]")
        {
            return "Error: CreateAccount() Email already exists in another account";
        }

        Pin pin = new Pin()
        {
            id = Guid.NewGuid().ToString(),
            pin_number = RandomNumberGenerator.GenerateRandomNumber(4),
            authorizer = "SYSTEM",
            authorizer_name = "SYSTEM",
            branch_store = "SYSTEM",
            date = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
            expirytime = DateTime.Now.AddMinutes(30).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
            permissions = "Create account",
            status = "pending",
            user = email,
            minutes = 30,
            authorizer_message = "Created by SendPinToEmail()",
            pintype = "touser"
        };

        //result = db.CreateTable(pin, "pins");
        //if (result.StartsWith("Error:")) return "Error: Creating table pins " + result.Replace("Error:", "");

        string wwwroot = parameters["wwwroot"].ToString().Trim();

        if (!Directory.Exists(wwwroot))
        {
            return "Error: wwwroot directory not found";
        }

        string images_directory = Path.Combine(wwwroot, "auth/pins/images");

        if (!Directory.Exists(images_directory))
        {
            Directory.CreateDirectory(images_directory);
        }

        string pins_directory = Path.Combine(wwwroot, "auth/pins");

        if (!Directory.Exists(pins_directory))
        {
            Directory.CreateDirectory(pins_directory);
        }

        string image_name = images_directory + "/" + pin.pin_number + ".png";

        result = CreateImageFromText(pin.pin_number, image_name);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        string htmlCode = $@"<!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>Confirmation PIN</title>
                            </head>
                            <body>
                                <h1>Confirmation PIN</h1>
                                <p>Dear User,</p>
                                <p>Here is your confirmation PIN:</p>
                                <img src='{"https://tokens.mybusinesspos.net/auth/pins/images/" + pin.pin_number + ".png"}' alt='Confirmation PIN Image'>
                                <p>If you are unable to view the image, please click on the following link:</p>
                                <a href='https://tokens.mybusinesspos.net/auth/pins/{pin.pin_number}.html'>Click here</a>
                                <p>Thank you!</p>
                            </body>
                            </html>";

        result = "Ok.";

        //string html_file = Path.Combine(wwwroot, "auth/pins/" + pin.pin_number + ".html");
        //File.WriteAllText(html_file, htmlCode);

        //result = SendMailFromSoap(htmlCode, email).GetAwaiter().GetResult();

        // result = EmailSender.SendEmail(parameters["email_address"],
        //                                 parameters["email_password"],
        //                                 "Tokens Administration PIN",
        //                                 email,
        //                                 "",
        //                                 "Tokens MyBusiness POS Confirmation PIN",
        //                                 htmlCode,
        //                                 parameters["smtp"].ToString().Trim(),
        //                                 int.Parse(parameters["smtp_port"].ToString().Trim()),
        //                                 false);

        //if (result.StartsWith("Error:"))
        //{
        //    return result;
        //}

        //XDocument doc = XDocument.Parse(result);
        //XNamespace ns = "http://wsCorreo.mybusinesspos.com/";
        //result = doc.Descendants(ns + "EnviaCorreoHResult").First().Value;

        //result = server_db.CreateTable(pin, "pins");
        //if (result.StartsWith("Error:")) return "Error: Creating table pins " + result.Replace("Error:", "");

        if (result == "Ok.")
        {
            result = server_db.Prompt($"UPSERT INTO pins VALUES {JsonConvert.SerializeObject(pin, Formatting.Indented)}");
        }

        return result;

    }

    static async Task<string> SendMailFromSoap(string html, string email, string default_subject = "Tokens Administration PIN")
    {
        string fromAddressName = "tokens@mybusinesspos.net";
        //string fromAddressPassword = "TokenNewService3456()";
        string toAddress = email;
        string toAddressName = "";
        string subject = default_subject;
        string alternate = html;
        string password = "Your mail password from mybusiness email service";

        string soapEnvelope = $@"
            <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                <soap12:Body>
                    <EnviaCorreoH xmlns=""http://wsCorreo.mybusinesspos.com/"">
                        <fromAddressName>{System.Security.SecurityElement.Escape(fromAddressName)}</fromAddressName>
                        <toAddress>{System.Security.SecurityElement.Escape(toAddress)}</toAddress>
                        <toAddressName>{System.Security.SecurityElement.Escape(toAddressName)}</toAddressName>
                        <Subjet>{System.Security.SecurityElement.Escape(subject)}</Subjet>
                        <alternate>{System.Security.SecurityElement.Escape(alternate)}</alternate>
                        <password>{System.Security.SecurityElement.Escape(password)}</password>
                    </EnviaCorreoH>
                </soap12:Body>
            </soap12:Envelope>";

        var url = "https://wscorreo.mybusinesspos.com:25100/wscorreo.asmx";
        return await PostSOAPRequestAsync(url, soapEnvelope);

    }


    private static async Task<string> PostSOAPRequestAsync(string url, string text)
    {
        var httpClient = new HttpClient();

        using (HttpContent content = new StringContent(text, Encoding.UTF8, "text/xml"))
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
        {
            request.Headers.Add("SOAPAction", "http://wsCorreo.mybusinesspos.com/EnviaCorreoH");
            request.Content = content;
            using (HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode(); // throws an Exception if 404, 500, etc.
                return await response.Content.ReadAsStringAsync();
            }
        }
    }

    public static string ValidateAdminUser(AngelDB.DB db, string Token, string groups = "AUTHORIZERS", string api_name = "")
    {

        string result = db.Prompt($"SELECT * FROM tokens WHERE id = '{Token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: ValidateAdminUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: ValidateAdminUser() Token not found: " + Token;
        }

        DataTable dataTableToken = JsonConvert.DeserializeObject<DataTable>(result);

        result = db.Prompt($"SELECT * FROM users WHERE id = '{dataTableToken.Rows[0]["User"].ToString()}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: ValidateAdminUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: ValidateAdminUser() User not found: " + dataTableToken.Rows[0]["User"].ToString();
        }

        if (HasExpired(dataTableToken.Rows[0]["ExpiryTime"].ToString()))
        {
            return "Error: ValidateAdminUser() The Token has expired: " + dataTableToken.Rows[0]["User"].ToString();
        }

        DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(result);

        List<string> authorizedGroups = groups.Split(',').ToList();
        List<string> userGroups = dataTable.Rows[0]["UserGroups"].ToString().Split(',').ToList();

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
            return $"Error: ValidateAdminUser() {api_name} User not authorized: " + dataTableToken.Rows[0]["User"].ToString();
        }

        return dataTableToken.Rows[0]["User"].ToString();

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


    public static bool HasExpired(string expiryTime)
    {
        // Parsea las fechas dadas usando un formato de fecha y hora específico
        DateTime parsedNow = DateTime.Now.ToUniversalTime();
        DateTime parsedExpiryTime = DateTime.ParseExact(expiryTime, "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
        // Si la fecha de expiración calculada es anterior o igual a la fecha/hora de expiración dada, retorna verdadero
        return parsedNow > parsedExpiryTime;
    }

    public static string ConvertToDateTimeWithMaxTime(string inputDate)
    {
        // Parse the date
        DateTime parsedDate;
        if (DateTime.TryParseExact(inputDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
        {
            // Set the time to 23:59:59.000000
            DateTime maxTimeDate = parsedDate.Date.Add(new TimeSpan(23, 59, 59)).AddTicks(9999999); // 10 million ticks per second - 1 tick = 1 second
            return maxTimeDate.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        }
        else
        {
            return $"Error: Invalid date format. {inputDate}";
        }
    }

}


public static class EmailValidator
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // La siguiente expresión regular se basa en la definición de los RFC 5322 Official Standard y RFC 5321 SMTP
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<=\w)@"
                + @"((?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\["
                + @"((?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|"
                + @"\[((a|b|c|d|e|f|g|h|j|k|l|m|n|o|p|r|s|t|u|v|w|y|z)|(a|b|c|d|e|f|g|h|j|k|l|m|n|o|p|r|s|t|u|v|w|y|z)"
                + @"(a|b|c|d|e|f|g|h|j|k|l|m|n|o|p|r|s|t|u|v|w|y|z))\])])$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            // Si la expresión regular falla, simplemente devolvemos falso
            return false;
        }
    }
}


public static string CreateImageFromText(string text, string filename)
{

    try
    {
        // Define el tipo de fuente y tamaño
        Font font = new Font("Arial", 50, FontStyle.Regular);

        // Crea un bitmap en base al texto y la fuente
        SizeF textSize;
        using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
        {
            textSize = graphics.MeasureString(text, font);
        }

        // Crea una nueva imagen del tamaño necesario para el texto
        using (Bitmap image = new Bitmap((int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height)))
        {
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // Define el color de fondo y el color del texto
                graphics.Clear(Color.White);
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString(text, font, brush, 0, 0);
                }
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            // Guarda la imagen a un archivo
            image.Save(filename, ImageFormat.Png);
        }

        return "Ok.";

    }
    catch (System.Exception e)
    {
        return "Error: CreateImageFromText() " + e.Message;
    }

}


public static class EmailSender
{
    public static string SendEmail(
        string fromEmail,
        string fromPassword,
        string fromName,
        string toEmail,
        string cc,
        string subject,
        string bodyHtml, string host,
        int port,
        bool enableSsl = true, bool useDefaultCredentials = false)
    {
        try
        {
            var fromAddress = new MailAddress(fromEmail, fromName);
            var toAddress = new MailAddress(toEmail);

            NetworkCredential credentials = new NetworkCredential(fromAddress.Address, fromPassword);

            if (useDefaultCredentials)
            {
                credentials = CredentialCache.DefaultNetworkCredentials;
            }

            var smtp = new SmtpClient
            {
                Host = host, // especifica el servidor SMTP aquí
                Port = port, // especifica el puerto SMTP aquí
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = credentials
            };


            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            })
            {
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    message.CC.Add(cc);
                }

                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                smtp.Send(message);
                ServicePointManager.ServerCertificateValidationCallback = null;
            }

            return "Ok.";

        }
        catch (System.Exception e)
        {
            return "Error: " + e.Message;
        }

    }

}


