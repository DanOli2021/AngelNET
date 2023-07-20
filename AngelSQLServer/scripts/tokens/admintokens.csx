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

AngelApiOperation api = JsonConvert.DeserializeObject<AngelApiOperation>(message);

switch (api.OperationType)
{
    case "UpsertGroup":
        return AdminAuth.UpsertGroup(db, api);
    case "GetGroups":
        return AdminAuth.GetGroups(db, api);
    case "DeleteGroup":
        return AdminAuth.DeleteGroup(db, api);
    case "CreateUser":
        return AdminAuth.CreateUser(db, api);
    case "DeleteUser":
        return AdminAuth.DeleteUser(db, api);
    case "GetUsers":
        return AdminAuth.GetUsers(db, api);
    case "GetUser":
        return AdminAuth.GetUser(db, api);
    case "CreateNewToken":
        return AdminAuth.CreateNewToken(db, api);
    case "DeleteToken":
        return AdminAuth.DeleteToken(db, api);
    case "ValidateToken":
        return AdminAuth.ValidateToken(db, api);
    case "GetTokenFromUser":
        return AdminAuth.GetTokenFromUser(db, api);
    case "GetPermisionsUsingTocken":
        return AdminAuth.GetPermisionsUsingTocken(db, api);
    case "GetUserUsingToken":
        return AdminAuth.GetUserUsingToken(db, api);
    default:
        return $"Error: No service found {api.OperationType}";
}

public class OperationTypeClass
{
    public string OperationType;
}


public static class AdminAuth
{
    public static string CreateNewToken(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        result = db.Prompt($"SELECT * FROM users WHERE id = '{d.User}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: User not found {d.User}";
        }

        Tokens t = new Tokens();

        string token = Guid.NewGuid().ToString();

        t.id = token;
        t.User = d.User;

        int expiry_days = int.Parse(d.expiry_days.ToString());

        if (expiry_days < 0)
        {
            expiry_days = 400000;
        }

        t.ExpiryTime = DateTime.Now.AddDays( expiry_days ).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        result = db.Prompt($"INSERT INTO tokens VALUES {JsonConvert.SerializeObject(t)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() insert " + result.Replace("Error:", "");
        }

        result = db.Prompt($"DELETE FROM tokens PARTITION KEY main WHERE User = '{d["User"]}' AND id <> '{token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() delete tockens " + result.Replace("Error:", "");
        }

        return token;

    }

    public static string DeleteToken(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

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


    public static string GetPermisionsUsingTocken(AngelDB.DB db, AngelApiOperation api)
    {

        //string result = ValidateAdminUser(db, api.Token);
         //
        //if (result.StartsWith("Error:"))
        //{
        //    return result;
        //}

        var d = api.DataMessage;

        string result = db.Prompt($"SELECT * FROM tokens WHERE id = '{d.TokenToObtainPermission}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetPermisionsUsingTocken() " + result.Replace("Error:", "");
        }

        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return $"Error: GetPermisionsUsingTocken() Token {d.TokenToObtainPermission} not found";
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{dt.Rows[0]["user"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetPermisionsUsingTocken() " + result.Replace("Error:", "");
        }

        dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return "Error: User not found";
        }

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{dt.Rows[0]["UserGroup"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetPermisionsUsingTocken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: GetPermisionsUsingTocken() UserGroup not found";
        }

        DataRow r = JsonConvert.DeserializeObject<DataTable>(result).Rows[0];

        var group = new
        {
            id = r["id"].ToString(),
            user = dt.Rows[0]["id"].ToString(),
            user_name = dt.Rows[0]["name"].ToString(),
            group_name = r["name"].ToString(),
            permissions = r["Permissions"].ToString()
        };

        return JsonConvert.SerializeObject(group, Formatting.Indented);

    }


    public static string GetUserUsingToken(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

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

        result = db.Prompt($"SELECT * FROM users WHERE id = '{d.User}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetTokenFromUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: User not found";
        }

        Users[] u = JsonConvert.DeserializeObject<Users[]>(result);

        if (u[0].Password.Trim() != d.Password.ToString().Trim())
        {
            return "Error: Invalid password";
        }

        result = db.Prompt($"SELECT * FROM tokens WHERE user = '{d.User}'");

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


    public static string CreateUser(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        string[] groups = d.UserGroups.ToString().Split(',');

        foreach (string group in groups)
        {
            result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{group.Trim().ToUpper()}'");

            if (result.StartsWith("Error:"))
            {
                return "Error: CreateUser() Auth " + result.Replace("Error:", "");
            }

            if (result == "[]")
            {
                return "Error: CreateUser() Auth No user group found: " + group;
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

        result = db.Prompt($"UPSERT INTO users VALUES {JsonConvert.SerializeObject(t)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateUser() insert " + result.Replace("Error:", "");
        }

        return $"Ok. User created successfully: " + d.User;

    }


    public static string GetUsers(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        string user = result;

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

        result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

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
            UserGroups = t.Rows[0]["UserGroups"]
        };

        return JsonConvert.SerializeObject(user, Formatting.Indented);

    }


    public static string DeleteUser(AngelDB.DB db, AngelApiOperation api)
    {
        string result = "";

        result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

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

        result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{d.UserGroup}'");

        dynamic NewPermissions = d.Permissions;
        UsersGroup t = new UsersGroup();

        t.id = d.UserGroup;
        t.Name = d.Name;
        t.Permissions = JsonConvert.SerializeObject(d.Permissions);

        result = db.Prompt($"UPSERT INTO UsersGroup VALUES {JsonConvert.SerializeObject(t, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewGroup() upsert " + result.Replace("Error:", "");
        }

        return $"Ok. Users Group created successfully: {d.UserGroup}";

    }


    public static string GetGroups(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

        if (string.IsNullOrEmpty(d.Where.ToString()))
        {
            result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{d.UserGroup}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM UsersGroup WHERE {d.Where}");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteGroup() " + result.Replace("Error:", "");
        }

        return result;
    }

    public static string DeleteGroup(AngelDB.DB db, AngelApiOperation api)
    {

        string result = ValidateAdminUser(db, api.Token);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;

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


    public static string ValidateAdminUser(AngelDB.DB db, string Token)
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

        DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(result);

        if (dataTable.Rows[0]["UserGroup"].ToString().Trim() != "AUTHORIZERS") 
        {
            return "Error: ValidateAdminUser() User not authorized: " + dataTableToken.Rows[0]["User"].ToString();
        }

        return dataTableToken.Rows[0]["User"].ToString();

    }

}


