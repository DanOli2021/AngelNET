import json
import locale

locale.setlocale( locale.LC_ALL, "" )

class universal:

    #db -> Database access variable
    #data -> Json data passed by the application 
    def __init__(self, db, data):
            self.db = db
            self.data = data

    def add_universal_user(self):
            self.db.Prompt("USE accounts DATABASE accountsdb" )
            self.db.Prompt("CREATE TABLE users FIELD LIST name, email, account, password, confirmed, enabled STORAGE main", True)
            userdata = json.loads(self.data)

            #sysconfig.get_path

            userdata["to_account"] = userdata["to_account"].strip().lower()

            partition_key = userdata["to_account"][0:3]
            result = self.db.Prompt("SELECT id, password FROM accounts PARTITION KEY " + partition_key + " WHERE id = '" + userdata["to_account"] + "'", True)

            if result == "{}":
                raise Exception("Error: Account does not exist: " + userdata["to_account"])

            accountdata = json.loads(self.data)

            if accountdata["master_password"].strip() != userdata["master_password"]:
                raise Exception( "Error: Wrong master password, account: " + userdata["to_account"] )

            user = userdata["add_universal_user"].strip() + "@" + userdata["to_account"].strip()
            user = user.lower()

            newuser = {}
            newuser["id"] = user
            newuser["name"] = userdata["name"]
            newuser["email"] = userdata["email"]
            newuser["account"] = userdata["to_account"]
            newuser["password"] = userdata["password"]
            newuser["confirmed"] = 0
            newuser["enabled"] = 1
            self.db.Prompt("INSERT INTO users PARTITION KEY " + partition_key + " VALUES " + json.dumps(newuser,4), True )
            self.db.Prompt("USE " + userdata["to_account"] + " DATABASE " + userdata["to_account"], True )
            self.db.Prompt("CREATE LOGIN " + userdata["add_universal_user"].strip() + " PASSWORD " + userdata["password"], True)
            self.db.Prompt("CREATE LOGIN commander_" + userdata["add_universal_user"].strip() + " PASSWORD " + userdata["password"], True)

            return "Ok. User created: " + user


    def get_universal_users(self):
            maindata = json.loads(self.data)
            account = maindata["account"]
            partition_key = account[0:3]

            result = self.db.Prompt("SELECT * FROM accounts PARTITION KEY " + partition_key + " WHERE id = '" + account + "'", True)    

            if result == "{}":
                return "Error: Account does not exist"

            accountdata = json.loads( result )

            if accountdata[partition_key][0]["confirmed"] == None:
                return "Error: The account has not been confirmed"

            if accountdata[partition_key][0]["enabled"] == None:
                return "Error: Account is not active"

            return self.db.Prompt("SELECT * FROM users PARTITION KEY " + partition_key + " WHERE account = '" + account + "'", True)    
        

    def create_universal_account(self):
            self.db.Prompt("USE accounts DATABASE accountsdb" )
            self.db.Prompt("CREATE TABLE accounts FIELD LIST name, email, master_user, password, secret, confirmed, enabled, account_data, account_type STORAGE main", True)
            universalaccount = json.loads(self.data)
            universalaccount["create_universal_account"] = universalaccount["create_universal_account"].strip().lower()
            partition_key = universalaccount["create_universal_account"][0:3]
            result = self.db.Prompt("SELECT * FROM accounts PARTITION KEY " + partition_key + " WHERE id = '" + universalaccount["create_universal_account"] + "'", True)

            if result != "{}":
               return "Error: Account already exists: " + universalaccount["create_universal_account"];

            account = {}
            account["id"] = universalaccount["create_universal_account"]
            account["name"] = universalaccount["name"]
            account["email"] = universalaccount["email"]
            account["master_user"] = universalaccount["user"]
            account["password"] = universalaccount["password"]
            account["secret"] = universalaccount["secret"]
            account["account_data"] = universalaccount["account_data"]
            account["account_type"] = universalaccount["type"]
            account["confirmed"] = "False"
            account["enabled"] = "True"

            self.db.Prompt( "INSERT INTO accounts PARTITION KEY " + partition_key + " VALUES " + json.dumps(account,4), True )       
            self.db.Prompt( "CREATE ACCOUNT " + universalaccount["create_universal_account"] + " SUPERUSER " + universalaccount["master_user"] + " PASSWORD " + universalaccount["password"], True ) 
            self.db.Prompt( "USE ACCOUNT " + universalaccount["create_universal_account"], True ) 
            self.db.Prompt( "CREATE DATABASE " + universalaccount["create_universal_account"], True ) 

            return "Ok. Account created: " + universalaccount["create_universal_account"]



    def set_universal_user(self):
            user = json.loads(self.data)
            account = user["set_universal_user"].split("@")[1]
            partition_key = account[0:3]
 
            self.db.Prompt("USE accounts DATABASE accountsdb" )
            result = self.db.Prompt("SELECT * FROM users PARTITION KEY " + partition_key + " WHERE id = '" + user["set_universal_user"] + "'", True)    
            
            if result == "{}":
                return "Error: User not located"

            userdata = json.loads( result )

            if userdata[partition_key][0]["confirmed"] == "False":
                return "Error: The user has not yet been confirmed"

            result = self.db.Prompt("SELECT * FROM accounts PARTITION KEY " + partition_key + " WHERE id = '" + account + "'", True)    

            if result == "{}":
                return "Error: Account does not exist"

            accountdata = json.loads( result )

            if userdata[partition_key][0]["password"] != user["password"]:
                return "Error: Wrong password"

            if accountdata[partition_key][0]["confirmed"] == 0:
                return "Error: The account has not been confirmed"

            if accountdata[partition_key][0]["enabled"] == 0:
                return "Error: Account is not active"
            
            universaluser = {}
            universaluser["secret"] = accountdata[partition_key][0]["secret"]
            universaluser["name"] = userdata[partition_key][0]["name"]
            universaluser["email"] = userdata[partition_key][0]["email"]
            universaluser["user"] = userdata[partition_key][0]["id"]
            universaluser["password"] = user["password"]
            return json.dumps(universaluser,4)


    def validate_universal_user(self):
            maindata = json.loads(self.data)
            account = maindata["account"].split("@")[1]
            partition_key = account[0:3]

            self.db.Prompt("USE accounts DATABASE accountsdb", True )
            result = self.db.Prompt("SELECT * FROM users PARTITION KEY " + partition_key + " WHERE id = '" + maindata["account"] + "'", True)    

            if result == "{}":
                return "Error: Account does not exist: " + account

            return "Ok."

