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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
    case "UpsertUser":
        return AdminAuth.UpsertUser(db, api);
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
    case "GetGroupsUsingTocken":
        return AdminAuth.GetGroupsUsingTocken(db, api);
    case "GetUserUsingToken":
        return AdminAuth.GetUserUsingToken(db, api);
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

        result = ValidateAdminUser(db, api.Token, "AUTHORIZERS", "CreateNewToken");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        var d = api.DataMessage;


        if (d.User == null)
        {
            return "Error: CreateNewToken() User is null";
        }

        if (d.expiry_days == null)
        {
            return "Error: CreateNewToken() expiry_days is null";
        }



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

        t.ExpiryTime = DateTime.Now.AddDays(expiry_days).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
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

        if (d.Password == null)
        {
            return "Error: GetTokenFromUser() Password is null";
        }



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
            return "Error: CreateUser() User is null";
        }

        if (d.Password == null)
        {
            return "Error: CreateUser() Password is null";
        }

        if (d.UserGroups == null)
        {
            return "Error: CreateUser() userGroups is null";
        }

        if (d.Name == null)
        {
            return "Error: CreateUser() Name is null";
        }

        if (d.Organization == null)
        {
            return "Error: CreateUser() Organization is null";
        }

        if (d.Email == null)
        {
            return "Error: CreateUser() Email is null";
        }

        if (d.Phone == null)
        {
            return "Error: CreateUser() Phone is null";
        }

        if (d.permissions_list == null)
        {
            return "Error: CreateUser() permissions_list is null";
        }

        if (string.IsNullOrEmpty(d.Password.ToString()))
        {
            return "Error: CreateUser() password is null or empty";
        }

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
        t.permissions_list = d.permissions_list;

        result = db.Prompt($"UPSERT INTO users VALUES {JsonConvert.SerializeObject(t)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateUser() insert " + result.Replace("Error:", "");
        }

        api.DataMessage.expiry_days = 30;

        result = CreateNewToken(db, api);

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateUser() " + result.Replace("Error:", "");
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

        List<string> system_groups = new List<string> { "AUTHORIZERS", "SUPERVISORS", "PINSCONSUMER" };

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
            authorizer = user.Rows[0]["id"].ToString(),
            authorizer_name = user.Rows[0]["Name"].ToString(),
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

        if (!user.Rows[0]["UserGroups"].ToString().Contains("AUTHORIZERS"))
        {
            result = db.Prompt($"SELECT * FROM pins WHERE date >= '{d.InitialDate}' AND date <= '{d.FinalDate} 24:00:00' AND authorizer = '{user.Rows[0]["id"]}' ORDER BY date DESC");
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


}


