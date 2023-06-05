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

AdminAuth adminAuth = new AdminAuth();
TokenOperation token_operation;
UserOperation user_operation;
UserGroupOperation user_group_operation;

OperationTypeClass operation_type = JsonConvert.DeserializeObject<OperationTypeClass>(message);

switch (operation_type.OperationType)
{

    case "CreateNewToken":
        token_operation = JsonConvert.DeserializeObject<TokenOperation>(message);
        return adminAuth.CreateNewToken(db, token_operation);
    case "DeleteToken":
        token_operation = JsonConvert.DeserializeObject<TokenOperation>(message);
        return adminAuth.DeleteToken(db, token_operation);
    case "ValidateToken":
        token_operation = JsonConvert.DeserializeObject<TokenOperation>(message);
        return adminAuth.ValidateToken(db, token_operation);
    case "GetTokenFromUser":
        token_operation = JsonConvert.DeserializeObject<TokenOperation>(message);
        return adminAuth.GetTokenFromUser(db, token_operation);
    case "GetPermisionsUsingTocken":
        token_operation = JsonConvert.DeserializeObject<TokenOperation>(message);
        return adminAuth.GetPermisionsUsingTocken(db, token_operation);

    case "CreateUser":
        user_operation = JsonConvert.DeserializeObject<UserOperation>(message);
        return adminAuth.CreateNewUser(db, user_operation);
    case "DeleteUser":
        user_operation = JsonConvert.DeserializeObject<UserOperation>(message);
        return adminAuth.DeleteUser(db, user_operation);
    case "GetUsers":
        user_operation = JsonConvert.DeserializeObject<UserOperation>(message);
        return adminAuth.GetUsers(db, user_operation);

    case "CreateGroup":
        user_group_operation = JsonConvert.DeserializeObject<UserGroupOperation>(message);
        return adminAuth.CreateNewGroup(db, user_group_operation);
    case "GetGroups":
        user_group_operation = JsonConvert.DeserializeObject<UserGroupOperation>(message);
        return adminAuth.GetGroups(db, user_group_operation);
    case "DeleteGroup":
        user_group_operation = JsonConvert.DeserializeObject<UserGroupOperation>(message);
        return adminAuth.DeleteGroup(db, user_group_operation);
    default:
        return "Error: No service found";
}


public class OperationTypeClass
{
    public string OperationType;
}

public class TokenOperation
{
    public string OperationType { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
    public int expiry_days { get; set; }
    public string db_user { get; set; }
    public string db_password { get; set; }

}

public class UserOperation
{
    public string OperationType { get; set; }
    public string User { get; set; }
    public string UserGroup { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Organization { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Where { get; set; }
    public string db_user { get; set; }
    public string db_password { get; set; }
}

public class UserGroupOperation
{
    public string OperationType;
    public string UserGroup { get; set; }
    public string Name { get; set; }
    public string Permissions { get; set; }
    public string Where { get; set; }
    public string db_user { get; set; }
    public string db_password { get; set; }
}


public class AdminAuth
{
    public string CreateNewToken(AngelDB.DB db, TokenOperation to)
    {
        string result = "";

        result = ValidateAdminUser(db, to.db_user, to.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{to.User}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: CreateNewToken() User not found {to.User}";
        }

        Tokens t = new Tokens();

        string token = Guid.NewGuid().ToString();

        t.id = token;
        t.User = to.User;

        if (to.expiry_days < 0)
        {
            to.expiry_days = 400000;
        }

        t.ExpiryTime = DateTime.Now.AddDays(to.expiry_days).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        result = db.Prompt($"INSERT INTO tokens VALUES {JsonConvert.SerializeObject(t)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() insert " + result.Replace("Error:", "");
        }

        result = db.Prompt($"DELETE FROM tokens PARTITION KEY main WHERE User = '{to.User}' AND id <> '{token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() delete tockens " + result.Replace("Error:", "");
        }

        return token;

    }

    public string DeleteToken(AngelDB.DB db, TokenOperation to)
    {
        string result = "";

        result = ValidateAdminUser(db, to.db_user, to.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"SELECT * FROM tokens WHERE id = '{to.Token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteToken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return $"Error: DeleteToken() Token not found {to.User}";
        }

        result = db.Prompt($"DELETE FROM tokens PARTITION KEY main WHERE id = '{to.Token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteToken() delete tockens " + result.Replace("Error:", "");
        }

        return "Ok. Token deleted: " + to.Token;

    }


    public string GetPermisionsUsingTocken(AngelDB.DB db, TokenOperation to) 
    {

        string result = ValidateToken(db, to);

        if (result.StartsWith("Error:"))
        {
            return result;
        }
        
        result = db.Prompt($"SELECT * FROM tokens WHERE id = '{to.Token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetPermisionsUsingTocken() " + result.Replace("Error:", "");
        }
        
        DataTable dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return "Error: GetPermisionsUsingTocken() Token not found";
        }

        result = db.Prompt($"SELECT * FROM users WHERE id = '{dt.Rows[0]["user"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetPermisionsUsingTocken() " + result.Replace("Error:", "");
        }

        dt = JsonConvert.DeserializeObject<DataTable>(result);

        if (dt.Rows.Count == 0)
        {
            return "Error: GetPermisionsUsingTocken() User not found";
        }

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{dt.Rows[0]["UserGroup"]}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetPermisionsUsingTocken() " + result.Replace("Error:", "");
        }

        if( result == "[]")
        {
            return "Error: GetPermisionsUsingTocken() UserGroup not found";
        }

        dt = JsonConvert.DeserializeObject<DataTable>(result);

        return dt.Rows[0]["permissions"].ToString();

    }


    public string ValidateToken(AngelDB.DB db, TokenOperation to)
    {
        string result = "";

        result = db.Prompt($"SELECT * FROM tokens WHERE id = '{to.Token}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: ValidateToken() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: ValidateToken() Token not found";
        }

        Tokens[] t = JsonConvert.DeserializeObject<Tokens[]>(result);

        if (t[0].ExpiryTime.CompareTo(DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff")) < 0)
        {
            return "Error: ValidateToken() Token expired";
        }

        return "Ok.";

    }


    public string GetTokenFromUser(AngelDB.DB db, TokenOperation to)
    {
        string result = "";


        result = db.Prompt($"SELECT * FROM users WHERE id = '{to.User}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: GetTokenFromUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: GetTokenFromUser() User not found";
        }

        Users[] u = JsonConvert.DeserializeObject<Users[]>(result);

        if (u[0].Password.Trim() != to.Password.Trim())
        {
            return "Error: GetTokenFromUser() Invalid password";
        }

        result = db.Prompt($"SELECT * FROM tokens WHERE user = '{to.User}'");

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


    public string CreateNewUser(AngelDB.DB db, UserOperation uo)
    {
        string result = "";

        result = ValidateAdminUser(db, uo.db_user, uo.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{uo.UserGroup}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewUser() Auth " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: CreateNewUser() Auth No user group found: " + uo.UserGroup;
        }

        Users t = new Users();

        t.id = uo.User;
        t.UserGroup = uo.UserGroup;
        t.Name = uo.Name;
        t.Password = uo.Password;
        t.Organization = uo.Organization;
        t.Email = uo.Email;
        t.Phone = uo.Phone;

        result = db.Prompt($"UPSERT INTO users VALUES {JsonConvert.SerializeObject(t)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() insert " + result.Replace("Error:", "");
        }

        return $"Ok. User created successfully: {uo.User}";

    }


    public string GetUsers(AngelDB.DB db, UserOperation uo)
    {
        string result = "";

        result = ValidateAdminUser(db, uo.db_user, uo.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (string.IsNullOrEmpty(uo.Where))
        {
            result = db.Prompt($"SELECT * FROM users WHERE id = '{uo.User}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM users WHERE {uo.Where}");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteUser() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: DeleteUser() User not found";
        }

        DataTable t = JsonConvert.DeserializeObject<DataTable>(result);
        t.Columns.Remove("Password");

        return JsonConvert.SerializeObject(t, Formatting.Indented);

    }

    public string DeleteUser(AngelDB.DB db, UserOperation uo)
    {
        string result = "";

        result = ValidateAdminUser(db, uo.db_user, uo.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        Users t = new Users();

        result = db.Prompt($"SELECT * FROM users WHERE id = '{uo.User}'");

        if (result == "[]")
        {
            return "Error: DeleteUser() User not found";
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteUser() " + result.Replace("Error:", "");
        }

        result = db.Prompt($"DELETE FROM users PARTITION KEY main WHERE id = '{uo.User}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewToken() insert " + result.Replace("Error:", "");
        }

        return $"Ok. User deleted successfully: {uo.User}";

    }


    public string CreateNewGroup(AngelDB.DB db, UserGroupOperation go)
    {
        string result = "";

        result = ValidateAdminUser(db, go.db_user, go.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        Dictionary<string, bool> permissions = new Dictionary<string, bool>();
        permissions.Add("Sales", false);
        permissions.Add("Sales_POS", false);
        permissions.Add("Sales_Kiosk", false);
        permissions.Add("Sales_Customers", false);
        permissions.Add("Sales_X_Report", false);
        permissions.Add("Sales_Z_Report", false);
        permissions.Add("Sales_cash_reconciliation", false);
        permissions.Add("Sales_counting_bills_and_coins", false);
        permissions.Add("Sales_giving_a_refund", false);
        permissions.Add("Sales_void_transaction", false);
        permissions.Add("Sales_tender_the_transaction", false);
        permissions.Add("Sales_void_item", false);
        permissions.Add("Purchases", false);
        permissions.Add("Inventory", false);
        permissions.Add("Inventory_skus", false);
        permissions.Add("Inventory_skus_create", false);
        permissions.Add("Inventory_skus_modify", false);
        permissions.Add("Inventory_skus_delete", false);
        permissions.Add("Inventory_skus_offers", false);
        permissions.Add("Inventory_skus_offers_create", false);
        permissions.Add("Inventory_skus_offers_modify", false);
        permissions.Add("Inventory_skus_offers_delete", false);
        permissions.Add("Inventory_skus_clasificacions", false);
        permissions.Add("Inventory_skus_clasificacions_create", false);
        permissions.Add("Inventory_skus_clasificacions_modify", false);
        permissions.Add("Inventory_skus_clasificacions_delete", false);
        permissions.Add("Inventory_inbound_outbound", false);
        permissions.Add("Inventory_inbound_outbound_create", false);
        permissions.Add("Inventory_inbound_outbound_modify", false);
        permissions.Add("Inventory_inbound_outbound_delete", false);
        permissions.Add("Inventory_inbound_outbound_apply", false);
        permissions.Add("Physical_inventory", false);
        permissions.Add("Physical_inventory_create", false);
        permissions.Add("Physical_inventory_modify", false);
        permissions.Add("Physical_inventory_delete", false);
        permissions.Add("Physical_inventory_apply", false);
        permissions.Add("Physical_inventory_shrinkage", false);
        permissions.Add("BusinessManager", false);
        permissions.Add("Configuration", false);

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{go.UserGroup}'");

        Dictionary<string, bool> getPermissions = new Dictionary<string, bool>();

        if (result != "[]")
        {
            DataTable g = JsonConvert.DeserializeObject<DataTable>(result);
            getPermissions = JsonConvert.DeserializeObject<Dictionary<string, bool>>(g.Rows[0]["Permissions"].ToString());
        }

        foreach (string key in permissions.Keys)
        {
            if (!getPermissions.ContainsKey(key))
            {
                getPermissions.Add(key, permissions[key]);
            }
        }

        Dictionary<string, bool> NewPermissions = JsonConvert.DeserializeObject<Dictionary<string, bool>>(go.Permissions);

        foreach (string key in NewPermissions.Keys)
        {
            if (getPermissions.ContainsKey(key))
            {
                getPermissions[key] = NewPermissions[key];
            }
            else
            {
                return "Error: CreateNewGroup() The permission indicated by you does not exist on this system: " + key;
            }
        }

        UsersGroup t = new UsersGroup();

        t.id = go.UserGroup;
        t.Name = go.Name;
        t.Permissions = JsonConvert.SerializeObject(getPermissions, Formatting.Indented);

        result = db.Prompt($"UPSERT INTO UsersGroup VALUES {JsonConvert.SerializeObject(t, Formatting.Indented)}");

        if (result.StartsWith("Error:"))
        {
            return "Error: CreateNewGroup() upsert " + result.Replace("Error:", "");
        }

        return $"Ok. Users Group created successfully: {go.UserGroup}";

    }


    public string GetGroups(AngelDB.DB db, UserGroupOperation go)
    {

        string result = ValidateAdminUser(db, go.db_user, go.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        if (string.IsNullOrEmpty(go.Where))
        {
            result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{go.UserGroup}'");
        }
        else
        {
            result = db.Prompt($"SELECT * FROM UsersGroup WHERE {go.Where}");
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteGroup() " + result.Replace("Error:", "");
        }

        return result;
    }

    public string DeleteGroup(AngelDB.DB db, UserGroupOperation go)
    {

        string result = ValidateAdminUser(db, go.db_user, go.db_password);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = db.Prompt($"SELECT * FROM UsersGroup WHERE id = '{go.UserGroup}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteGroup() " + result.Replace("Error:", "");
        }

        if (result == "[]")
        {
            return "Error: DeleteGroup() The group indicated by you does not exist on this system: " + go.UserGroup;
        }

        result = db.Prompt($"DELETE FROM UsersGroup PARTITION KEY main WHERE id = '{go.UserGroup}'");

        if (result.StartsWith("Error:"))
        {
            return "Error: DeleteGroup() " + result.Replace("Error:", "");
        }

        return "Ok.";
    }


    public string ValidateAdminUser(AngelDB.DB db, string user, string password)
    {

        if (db.Prompt("MY LEVEL") == "DATABASE USER")
        {
            return "Error: ValidateAdminUser() Auth You are not a master user or account administrator";
        }

        string result = db.Prompt($"VALIDATE LOGIN {user} PASSWORD {password}");

        if (result.StartsWith("Error:"))
        {
            return "Error: ValidateAdminUser() Auth " + result.Replace("Error:", "");
        }

        if (result == "DATABASE USER")
        {
            return "Error: ValidateAdminUser() Auth You are not a master user or account administrator";
        }

        return "Ok.";

    }

}


