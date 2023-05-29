using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Collections;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq.Expressions;
using DocumentFormat.OpenXml.InkML;
using System.Globalization;
using AngelDBTools;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Security.Policy;

namespace AngelDB
{
    public enum ACCOUNT_TYPE
    {
        MASTER,
        ACCOUNT_MASTER,
        DATABASE_USER
    }

    public class DB : IDisposable
    {

        private ACCOUNT_TYPE _accountType;

        public ACCOUNT_TYPE accountType
        {
            get
            {
                return _accountType;
            }
            set
            {
            }
        }

        public string account = "";
        public bool IsReadOnly = false;
        public string sqliteConnectionString = "";
        public bool IsLogged = false;

        public string user = "";
        public string database = "";
        private Dictionary<string, string> config = new Dictionary<string, string>();
        public Dictionary<string, object> vars = new Dictionary<string, object>();
        public Dictionary<string, object> parameters = new Dictionary<string, object>();
        public Dictionary<string, TableInfo> table_connections = new Dictionary<string, TableInfo>();
        public Dictionary<string, DB> partitionsrules = new Dictionary<string, DB>();


        public string ChatName;
        public string apps_directory = "";

        DbLanguage language = new DbLanguage();

        public Dictionary<string, DB> dbs = new Dictionary<string, DB>();

        private string m_tables = "";
        private readonly string m_ConnectionError = "";

        public string UserTables
        {
            get { return m_tables; }
        }
        
        public string BaseDirectory
        {
            get { return _baseDirectory; }
        }
        
        public string ConnectionError
        {
            get { return m_ConnectionError; }
        }
        
        private string _Error = "";
        
        public string Error
        {
            get { return _Error; }
        }

        private string password = "";
        private string _baseDirectory = "";
        private SqliteTools sqlite;

        private PythonWraper pw = null;

        public delegate void OnReceived(string message);
        public event OnReceived OnReceivedMessage;
        public Dictionary<string, TableArea> TablesArea = new Dictionary<string, TableArea>();
        public bool use_connected_server = false;
        public string os_directory_separator = "/";
        public string Command;
        public string rule;

        public Dictionary<string, PartitionsInfo> partitions = new Dictionary<string, PartitionsInfo>();
        public Dictionary<string, QueryTools> SQLiteConnections = new Dictionary<string, QueryTools>();

        public WebForms web = new WebForms();
        public Dictionary<string, AzureTable> Azure = new Dictionary<string, AzureTable>();
        public Dictionary<string, string> SQLServer = new Dictionary<string, string>();

        public MemoryDb Grid = new MemoryDb();
        public bool speed_up = false;
        public string angel_tocken = "";
        public string angel_url = "";
        public string angel_user = "";

        public bool always_use_AngelSQL = false;
        public string ScriptMessage = "";
        public string ScriptCommandMessage = "";
        public DateTime script_file_datetime;
        public string script_file;

        public delegate void SendMessage(string message, ref string result);
        public event SendMessage OnSendMessage = null;

        public bool Development = false;
        public bool NewDatabases = true;

        public MyScript script = new MyScript();
        private bool disposedValue1;

        public PhytonScripts py;

        public bool CancelTransactions = false;

        public Dictionary<string, StatisticsAnalisis> statistics = new Dictionary<string, StatisticsAnalisis>();
        public DataTable tTables = null;
        public DataTable zTables = null;

        //Chat GPT
        public AngelDB.OpenAIChatbot GPT = null;

        /// <summary>Initializes a new instance of the <see cref="T:AngelDB.DB" /> class.</summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="baseDirectory">The base directory.</param>
        public DB(string user, string password, string baseDirectory, bool path_validation = true)
        {
            language.db = this;
            py = new PhytonScripts(this);
            _Error = StartDB(user, password, baseDirectory, path_validation);
        }

        public DB(string user, string password)
        {
            language.db = this;
            py = new PhytonScripts(this);
            _Error = StartDB(user, password, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + os_directory_separator + "Data");
        }

        public DB()
        {
            //
            language.db = this;
            py = new PhytonScripts(this);
        }

        public string StartDB(string user, string password, string baseDirectory, bool path_validation = true)
        {
            _Error = "";

            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

            try
            {

                if (baseDirectory == "null" || string.IsNullOrEmpty(baseDirectory))
                {
                    baseDirectory = Environment.CurrentDirectory + "/Data";
                }

                string result = SetBaseDirectory(baseDirectory, path_validation);

                if (result.StartsWith("Error:"))
                {
                    return result;
                }

                InitDatabases(path_validation);

                result = Login(user, password);

                if (result != "Ok.")
                {
                    return result;
                }

                if (result != "Ok.")
                {
                    return result;
                }

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }

        }

        public void RaiseOnReceived(string message)
        {
            OnReceivedMessage?.Invoke(message);
        }

        public string SetBaseDirectory(string baseDirectory, bool path_validation = true)
        {

            this._baseDirectory = baseDirectory;

            if (!path_validation) return "Ok.";

            if (!Directory.Exists(this.BaseDirectory))
            {
                if (this.NewDatabases == false)
                {
                    return "Error: Directory " + this.BaseDirectory + " does not exist.";
                }

                Directory.CreateDirectory(this.BaseDirectory);
            }

            return "Ok.";

        }

        public string GetKey(string masterkey, string key)
        {
            if (masterkey != "eytrrr67weqmnsdammhjweuasda")
            {
                return "Error: Wrong master key";
            }

            if (!this.config.ContainsKey(key))
            {
                return "Error: Key not found";
            }

            return this.config[key];
        }

        public string ChangeKey(string masterkey, string key, string data)
        {

            if (masterkey != "eytrrr67weqmnsdammhjweuasda")
            {
                return "Error: Wrong master key";
            }

            if (!this.config.ContainsKey(key))
            {
                return "Error: Key not found";
            }

            this.config[key] = data;
            return "Ok.";
        }

        public string Login(string user, string password)
        {

            this.account = "";
            this.database = "";

            if (user == config["master_user"])
            {

                if (password.Trim() != config["master_password"].Trim())
                {
                    return "Error: Invalid user or password";
                }
                else
                {
                    this.user = user;
                    this.password = password;
                    this._accountType = ACCOUNT_TYPE.MASTER;
                    this.IsLogged = true;
                    return "Ok.";
                }
            }

            DataTable t = sqlite.SQLTable($"SELECT * FROM masteraccounts WHERE user = '{user}' AND deleted IS NULL");

            if (t.Rows.Count > 0)
            {
                if (password.Trim() != t.Rows[0]["password"].ToString().Trim())
                {
                    return "Error: Invalid user or password";
                }

                this.user = user;
                this.password = password;
                this.IsLogged = true;
                this.account = t.Rows[0]["account"].ToString();
                this._accountType = ACCOUNT_TYPE.ACCOUNT_MASTER;
                return "Ok.";
            }

            string[] complexAccount = user.Split('@');

            if (complexAccount.Length < 2)
            {
                return "Error: Invalid user or password";
            }

            DataTable tAccounts = sqlite.SQLTable($"SELECT * FROM masteraccounts WHERE account = '{complexAccount[1]}' AND deleted IS NULL");

            if (tAccounts.Rows.Count == 0)
            {
                return $"Error: Account does not exist: {complexAccount[1]}";
            }

            this.account = complexAccount[1];

            DataTable u = sqlite.SQLTable($"SELECT * FROM users WHERE user = '{user}' AND deleted = 'false'");

            if (u.Rows.Count == 0)
            {
                return "Error: Invalid user or password";
            }

            if (password != u.Rows[0]["password"].ToString().Trim())
            {
                return "Error: Invalid user or password";
            }

            this.user = user;

            if (u.Rows[0]["database"].ToString().Trim() != "null")
            {
                DataTable tDataBase = sqlite.SQLTable($"SELECT * FROM databases WHERE database = '{u.Rows[0]["database"].ToString().Trim()}' AND deleted IS NULL");

                if (tDataBase.Rows.Count == 0)
                {
                    return "Error: Database does not exist";
                }
            }

            this.user = user;
            this.password = password;
            this._accountType = ACCOUNT_TYPE.DATABASE_USER;
            this.IsLogged = true;
            this.database = "";

            if (u.Rows[0]["database"].ToString().Trim() != "null")
            {
                this.database = u.Rows[0]["database"].ToString();
            }

            if (u.Rows[0]["readonly"].ToString() == "true") this.IsReadOnly = true;
            this.m_tables = u.Rows[0]["tables"].ToString();
            return "Ok.";
        }

        public string ValidateLogin(string user, string password)
        {

            if (user == config["master_user"])
            {

                if (password.Trim() != config["master_password"].Trim())
                {
                    return "Error: Invalid user or password";
                }
                else
                {
                    return ACCOUNT_TYPE.MASTER.ToString();
                }
            }

            DataTable t = sqlite.SQLTable($"SELECT * FROM masteraccounts WHERE user = '{user}' AND deleted IS NULL");

            if (t.Rows.Count > 0)
            {
                if (password.Trim() != t.Rows[0]["password"].ToString().Trim())
                {
                    return "Error: Invalid user or password";
                }

                return ACCOUNT_TYPE.ACCOUNT_MASTER.ToString();
            }

            string[] complexAccount = user.Split('@');

            if (complexAccount.Length < 2)
            {
                return "Error: Invalid user or password";
            }

            DataTable tAccounts = sqlite.SQLTable($"SELECT * FROM masteraccounts WHERE account = '{complexAccount[1]}' AND deleted IS NULL");

            if (tAccounts.Rows.Count == 0)
            {
                return $"Error: Account does not exist: {complexAccount[1]}";
            }

            DataTable u = sqlite.SQLTable($"SELECT * FROM users WHERE user = '{user}' AND deleted = 'false'");

            if (u.Rows.Count == 0)
            {
                return "Error: Invalid user or password";
            }

            if (password != u.Rows[0]["password"].ToString().Trim())
            {
                return "Error: Invalid user or password";
            }

            if (u.Rows[0]["database"].ToString().Trim() != "null")
            {
                DataTable tDataBase = sqlite.SQLTable($"SELECT * FROM databases WHERE database = '{u.Rows[0]["database"].ToString().Trim()}' AND deleted IS NULL");

                if (tDataBase.Rows.Count == 0)
                {
                    return "Error: Database does not exist";
                }
            }

            return ACCOUNT_TYPE.DATABASE_USER.ToString();
        }


        public string SaveConfig()
        {
            try
            {
                File.WriteAllText(this.BaseDirectory + os_directory_separator + "db.webmidb", AngelDBTools.CryptoString.Encrypt(JsonConvert.SerializeObject(config, Formatting.Indented), "hbjklios", "iuybncsa"));
                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }


        public Dictionary<string, string> NormalizeData(string normalize_level, string tocken1, string tocken2)
        {
            if (normalize_level.IndexOf("ddha()/%$%$$%232232244545") <= 0)
            {
                return null;
            }

            if (tocken1 != "904ffjj") return null;
            if (tocken2 != "234aslfdf") return null;

            Dictionary<string, string> config = new Dictionary<string, string>();
            config = JsonConvert.DeserializeObject<Dictionary<string, string>>(AngelDBTools.CryptoString.Decrypt(File.ReadAllText(Environment.CurrentDirectory + "/Data/db.webmidb"), "hbjklios", "iuybncsa"));
            return config;

        }

        public string InitDatabases(bool path_validation = true)
        {

            config.Clear();

            if (!File.Exists(this.BaseDirectory + os_directory_separator + "db.webmidb"))
            {
                config.Add("master_user", "db");
                config.Add("master_password", "db");
                config.Add("finger_print", System.Guid.NewGuid().ToString());
                config.Add("server", "localhost");
                config.Add("port", "11000");
                config.Add("secret", "");
                config.Add("autostart", "false");
                SaveConfig();
            }

            config = JsonConvert.DeserializeObject<Dictionary<string, string>>(AngelDBTools.CryptoString.Decrypt(File.ReadAllText(this.BaseDirectory + os_directory_separator + "db.webmidb"), "hbjklios", "iuybncsa"));

            string result;
            this.sqliteConnectionString = $"Data Source={this.BaseDirectory + os_directory_separator + "config.db"};";
            sqlite = new SqliteTools(this.sqliteConnectionString);

            if (path_validation)
            {
                result = Dataconfig.CreateInitTables(this);

                if (result != "Ok.")
                {
                    return result;
                }
            }

            return ResetPartitionRules();

        }

        public string ResetPartitionRules()
        {

            try
            {
                DataTable t = sqlite.SQLTable("SELECT * FROM partitionrules");

                string result = this.Grid.SQLExec("DROP TABLE IF EXISTS grid_partitionrules");
                result = this.Grid.SQLExec("CREATE TABLE IF NOT EXISTS grid_partitionrules ( account, database, table_name, partition, connection_string, connection_type, timestamp, PRIMARY KEY (account, database, table_name, partition) )");

                foreach (DataRow item in t.Rows)
                {
                    this.Grid.Reset();
                    this.Grid.CreateInsert("grid_partitionrules");
                    this.Grid.AddField("account", item["account"].ToString());
                    this.Grid.AddField("database", item["database"].ToString());
                    this.Grid.AddField("table_name", item["table_name"].ToString());
                    this.Grid.AddField("partition", item["partition"].ToString());
                    this.Grid.AddField("connection_string", item["connection_string"].ToString());
                    this.Grid.AddField("connection_type", item["connection_type"].ToString());
                    this.Grid.AddField("timestamp", item["timestamp"].ToString());
                    result = this.Grid.Exec();
                }

                return "Ok.";


            }
            catch (Exception e)
            {
                return $"Error {e}";
            }

        }

        public DB Clone()
        {

            DB db = new DB(this.user, this.password, this.BaseDirectory);

            if (!string.IsNullOrEmpty(this.account))
            {
                db.Prompt("USE ACCOUNT " + this.account);
            }

            if (!string.IsNullOrEmpty(this.database))
            {
                db.Prompt("USE DATABASE " + this.database);
            }

            return db;

        }

        public DbLanguage SyntaxAnalizer()
        {
            return new DbLanguage();
        }

        public string Prompt(string command, bool ThrowError = false)
        {

            string Command = command;

            foreach (var item in vars)
            {
                if (command.IndexOf("@") == -1)
                {
                    break;
                }

                command = command.Replace("@" + item.Key, item.Value.ToString());

            }

            string result;

            if (always_use_AngelSQL)
            {

                if (command.Trim().ToLower() == "angel stop")
                {
                    this.always_use_AngelSQL = false;
                    return "Ok.";
                }

                result = AngelExecute($"COMMAND {command}");

                if (result.StartsWith("Error:"))
                {
                    if (ThrowError)
                    {
                        throw new Exception(result);
                    }

                }
                if (result.StartsWith("Error:"))
                {
                    return result;
                }

                return result;

            }


            Dictionary<string, string> d = new Dictionary<string, string>();
            d = language.Interpreter(command);

            if (d == null)
            {
                if (ThrowError)
                {
                    result = language.errorString;
                    throw new Exception(result);
                }

                return language.errorString;
            }

            if (d.Count == 0)
            {
                if (ThrowError)
                {
                    result = "Error: not command found " + command; ;
                    throw new Exception(result);
                }

                return "Error: not command found " + command;

            }

            string commandkey = d.First().Key;

            if (commandkey == "run")
            {
                result = RunScript(d);
                return result;
            }

            if (!this.IsLogged)
            {
                switch (commandkey)
                {
                    case "db_user":
                        break;
                    case "=":
                        break;
                    case "script_file":
                        break;
                    case "py":
                        break;
                    case "pw":
                        break;
                    case "py_file":
                        break;
                    case "get_url":
                        break;
                    case "send_to_web":
                        break;
                    case "azure":
                        break;
                    case "angel":
                        break;
                    case "sql_server":
                        break;
                    case "always_use_server":
                        break;
                    case "stop_using_server":
                        break;
                    case "always_use_angelsql":
                        break;
                    case "set_script_message":
                        break;
                    case "get_script_message":
                        break;
                    case "get_scripts_directory":
                        break;
                    case "set_scripts_directory":
                        break;
                    case "var":
                        break;
                    case "math":
                        break;
                    case "web_form":
                        break;
                    case "read_file":
                        break;
                    case "set_development":
                        break;
                    case "set_new_databases":
                        break;
                    case "grid_insert_on":
                        break;
                    case "grid":
                        break;
                    case "to_client":
                        break;
                    case "get_enviroment":
                        break;
                    case "scripts_on_main":
                        break;
                    case "prompt":
                        break;
                    case "prompt_password":
                        break;
                    case "batch":
                        break;
                    case "batch_file":
                        break;
                    case "console":
                        break;
                    case "save_to_grid":
                        break;
                    case "create_db":
                        break;
                    case "prompt_db":
                        break;
                    case "compile":
                        break;
                    case "read_excel":
                        break;
                    case "create_excel":
                        break;
                    case "create_sync_dabase":
                        break;
                    case "sync_database":
                        break;
                    case "statistics":
                        break;
                    case "read_csv":
                        break;
                    case "gpt":
                        break;
                    case "post":
                        break;
                    case "version":
                        break;
                    default:
                        return $"Error: You have not indicated your username and password";
                }
            }

            switch (commandkey)
            {
                case "create_account":
                    result = Accounts.CreateAccount(d, this);
                    break;
                case "use_account":
                    result = Accounts.UseAccount(d, this);
                    break;
                case "delete_account":
                    result = Accounts.DeleteAccount(d, this);
                    break;
                case "undelete_account":
                    result = Accounts.UndeleteAccount(d, this);
                    break;
                case "close_account":
                    result = Accounts.CloseAccount(d, this);
                    break;
                case "account":
                    result = Accounts.ActiveAccount(this);
                    break;
                case "create_database":
                    result = Database.CreateDatabase(d, this, this.BaseDirectory);
                    break;
                case "use_database":
                    result = Database.UseDatabase(d, this);
                    break;
                case "use":

                    result = Database.Use(d, this);
                    break;

                case "delete_database":
                    result = Database.DeleteDatabase(d, this);
                    break;
                case "undelete_database":
                    result = Database.UnDeleteDatabase(d, this);
                    break;
                case "database":
                    result = Database.ActiveDatabase(this);
                    break;
                case "create_table":
                    result = Tables.CreateTable(d, this);
                    break;
                case "delete_table":
                    result = Tables.DeleteTable(d, this);
                    break;
                case "insert_into":
                    result = Tables.InsertInto(d, this);
                    break;
                case "upsert_into":
                    d["upsert"] = "true";
                    d.Add("insert_into", d["upsert_into"]);
                    result = Tables.InsertInto(d, this);
                    break;
                case "copy_to":
                    result = this.Prompt($"PY FILE copy_table.csx DATA {JsonConvert.SerializeObject(d)}");
                    break;
                //result = Tables.CopyTo( d, this );
                case "update":
                    result = Tables.Update(d, this);
                    break;
                case "delete_from":
                    result = Tables.DeleteFrom(d, this);
                    break;
                case "alter_table":
                    result = Tables.AlterTable(d, this);
                    break;
                case "select":
                    result = Tables.Select(d, this);
                    break;
                case "save_to":
                    result = SavePrompt(d);
                    break;
                case "get_tables":
                    result = Tables.GetTables(d, this);
                    break;
                case "get_structure":
                    result = Tables.GetStructure(d, this);
                    break;
                case "get_partitions":
                    result = Tables.GetPartitions(d, this);
                    break;
                case "get_accounts":
                    result = Accounts.GetsAccounts(d, this);
                    break;
                case "get_databases":
                    result = Database.GetDatabases(d, this);
                    break;
                case "who_i":

                    var v = new { local_user = this.user };
                    result = this.user;

                    break;
                case "send_to":
                    result = FileStorage.SendTo(d, this);
                    break;
                case "copy_from":
                    result = FileStorage.CopyFrom(d, this);
                    break;
                case "get_file":
                    result = FileStorage.GetFileAsBase64(d, this);
                    break;
                case "delete_file":
                    result = FileStorage.DeleteFile(d, this);
                    break;
                case "create_login":
                    result = Database.CreateLogin(d, this);
                    break;
                case "delete_login":
                    result = Database.DeleteLogin(d, this);
                    break;
                case "change_master":
                    result = Accounts.ChangeMaster(d, this);
                    break;
                case "add_master":
                    result = Accounts.AddMaster(d, this);
                    break;
                case "get_master_accounts":
                    result = Accounts.GetMasterAccounts(d, this);
                    break;
                case "update_master_account":
                    result = Accounts.UpdateMasterAccount(d, this);
                    break;
                case "delete_master_account":
                    result = Accounts.DeleteMasterAccount(d, this);
                    break;
                case "get_users":
                    result = Database.GetUsers(d, this);
                    break;
                case "get_masters":
                    result = Database.GetMasters(d, this);
                    break;
                case "stop_using_server":
                    this.use_connected_server = false;
                    result = "Ok.";
                    break;
                case "always_use_angelsql":
                    this.always_use_AngelSQL = true;
                    result = "Ok.";
                    break;
                case "get_logins":
                    result = Database.GetUsers(d, this);
                    break;
                case "validate_login":
                    result = ValidateLogin(d["validate_login"], d["password"]);
                    break;
                case "console":
                    result = FileStorage.WriteToConsole(d, this);
                    break;
                case "write_results_from":
                    result = FileStorage.WriteToFile(d, this);
                    break;
                case "cmd":
                    result = RunCMD(d["cmd"]);
                    break;
                case "load_file":
                    result = RunScript(d);
                    break;
                case "business":
                    result = Business.BusinessPrompt(this, d["business"]);
                    break;
                case "var":
                    result = SetVar(d["var"], d["="]);
                    break;
                case "get_vars":

                    result = GetVars(vars);
                    break;

                case "run":

                    result = RunScript(d);
                    break;
                    
                case "my_level":

                    result = "";

                    switch (this.accountType)
                    {
                        case ACCOUNT_TYPE.ACCOUNT_MASTER:
                            result = "ACCOUNT MASTER";
                            break;

                        case ACCOUNT_TYPE.DATABASE_USER:

                            result = "DATABASE USER";
                            break;

                        case ACCOUNT_TYPE.MASTER:

                            result = "MASTER";
                            break;

                        default:
                            break;
                    }

                    break;

                case "import_from":

                    result = this.Prompt($"PY FILE import_from.py ON APPLICATION DIRECTORY DATA {JsonConvert.SerializeObject(d)}");
                    break;

                case "who_is":

                    result = "01001101 01101001 00100000 01110000 01100001 01110000 11100001 00100000 01100101 01110011 00100000 01000100 01100001 01101110 01101001 01100101 01101100 00100000 01001111 01101100 01101001 01110110 01100101 01110010 00100000 01010010 01101111 01101010 01100001 01110011 00101100 00100000 01111001 00100000 01101101 01101001 00100000 01101101 01100001 01101101 11100001 00100000 01100101 01110011 00100000 01010010 01101111 01110011 01100001 00100000 01001101 01100001 01110010 01101001 01100001 00100000 01010100 01110010 01100101 01110110 01101001 01101100 01101100 01100001 00100000 01000111 01100001 01110010 01100011 01101001 01100001 00101100 00100000 01101101 01101001 01110011 00100000 01101000 01100101 01110010 01101101 01100001 01101110 01101111 01110011 00100000 01110011 01101111 01101110 00100000 01001111 01101100 01101001 01110110 01100101 01110010 00100000 01000111 01110101 01101001 01101100 01101100 01100101 01110010 01101101 01101111 00100000 01010010 01101111 01101010 01100001 01110011 00100000 01010100 01110010 01100101 01110110 01101001 01101100 01101100 01100001 00100000 01100101 00100000 01001001 01100001 01101110 00100000 01010010 01100001 01111010 01101001 01100101 01101100 00100000 01010010 01101111 01101010 01100001 01110011 00100000 01010100 01110010 01100101 01110110 01101001 01101100 01101100 01100001";
                    break;

                case "db_user":

                    this.account = "";
                    this.database = "";
                    this.IsLogged = false;
                    result = this.StartDB(d["db_user"], d["password"], d["data_directory"]);

                    if (result.StartsWith("Error:"))
                    {
                        break;
                    }

                    if (d["account"] == "null")
                    {
                        break;
                    }
                    else
                    {
                        result = this.Prompt("USE ACCOUNT " + d["account"]);
                    }

                    if (d["database"] == "null")
                    {
                        break;
                    }
                    else
                    {
                        result = this.Prompt("USE DATABASE " + d["database"]);
                    }


                    break;

                case "close_db":

                    this.account = "";
                    this.database = "";
                    this.IsLogged = false;
                    this.user = "";
                    result = "Ok.";
                    break;

                case "use_table":

                    result = this.UseTable(d);
                    break;

                case "(":

                    result = AnalizeParentesis(d);
                    break;

                case "create_partition_rule":

                    result = Partitions.CreatePartitionRule(d, this);
                    break;

                case "delete_partition_rule":

                    result = Partitions.DeletePartitionRule(d, this);
                    break;

                case "get_partition_rules":

                    result = Partitions.GetPartitionRules(d, this);
                    break;

                case "create_db":

                    result = CreateDB(d["create_db"], d["connection_string"]);
                    break;

                case "prompt_db":

                    result = PromptDB(d["prompt_db"], d["command"]);
                    break;

                case "get_db_list":

                    result = JsonConvert.SerializeObject(this.dbs);
                    break;

                case "remove_db":

                    result = RemoveDB(d["remove_db"]);
                    break;

                case "web_form":

                    result = this.web.WebFormsPrompt(this, d["web_form"]);
                    break;

                case "azure":

                    result = AzureExecute(d["azure"]);
                    break;

                case "sql_server":

                    result = SQLServerExecute(d["sql_server"]);
                    break;

                case "=":

                    return script.Eval(d["="], d["message"], this);

                case "script_file":

                    return script.EvalFile(d, this);

                case "set_script_message":

                    this.ScriptMessage = d["set_script_message"];
                    return "Ok.";

                case "get_script_message":

                    string message = this.ScriptMessage;
                    this.ScriptMessage = "";
                    return message;

                case "set_script_command":

                    this.ScriptCommandMessage = d["set_script_command"];
                    return "Ok.";

                case "get_script_command":

                    string script_command = this.ScriptCommandMessage;
                    this.ScriptCommandMessage = "";
                    return script_command;

                case "get_url":

                    return WebTools.ReadUrl(d["get_url"]);

                case "send_to_web":

                    if (d["source"] != "null")
                    {
                        return WebTools.SendJsonToUrl(d["send_to_web"], this.Prompt(d["source"]));
                    }

                    return WebTools.SendJsonToUrl(d["send_to_web"], d["context_data"]);

                case "save_to_grid":

                    return SaveToGrid(d);

                case "save_to_table":

                    if (d["json"] != "null")
                    {
                        result = SaveJsonToTable(d);
                        break;
                    }

                    result = SaveToTable(d);
                    break;

                case "grid":

                    return RunGrid(d);

                case "grid_insert_on":

                    return GridInsertOn(d);

                case "scripts_on_main":

                    this.apps_directory = Environment.CurrentDirectory;
                    return "Ok.";

                case "set_scripts_directory":

                    if (!Directory.Exists(d["set_scripts_directory"]))
                    {
                        return $"Error: directory does not exist {Directory.Exists(d["set_scripts_directory"])}";
                    }

                    this.apps_directory = d["set_scripts_directory"];
                    return "Ok.";

                case "get_scripts_directory":

                    return this.apps_directory;

                case "speed_up":

                    this.speed_up = true;
                    result = "Ok.";
                    break;

                case "angel":

                    result = AngelExecute(d["angel"]);
                    break;

                case "read_file":

                    try
                    {
                        result = File.ReadAllText(d["read_file"]);
                    }
                    catch (Exception e)
                    {
                        result = $"Error: {e}";
                    }

                    break;

                case "set_development":

                    if (d["set_development"] == "true")
                    {
                        this.Development = true;
                    }
                    else
                    {
                        this.Development = false;
                    }

                    result = "Ok.";
                    break;

                case "set_new_databases":

                    if (d["set_new_databases"] == "true")
                    {
                        this.Development = true;
                    }
                    else
                    {
                        this.Development = false;
                    }

                    result = "Ok.";
                    break;

                case "to_client":

                    return this.SendToClient(d["to_client"]);

                case "get_enviroment":

                    return EnviromentTools();

                case "server":

                    if (this.Prompt("MY LEVEL") != "MASTER")
                    {
                        return "Error: You don't have enough permissions";
                    }

                    Server s = new Server(this);
                    result = s.InitDataBase();

                    if (result.StartsWith("Error:"))
                    {
                        return result;
                    }

                    return s.ServerCommand(d["server"]);

                case "prompt":

                    result = Monitor.Prompt(d["prompt"]);
                    break;

                case "prompt_password":

                    result = Monitor.ReadPassword(d["prompt_password"]);
                    break;

                case "batch":

                    bool show_in_console = false;

                    if(d["show_in_console"] == "true")
                    {
                        show_in_console = true;
                    }

                    result = DBBatch.RunCode(d["batch"], show_in_console, this);
                    break;

                case "batch_file":

                    bool show_file_in_console = false;

                    if (d["show_in_console"] == "true")
                    {
                        show_file_in_console = true;
                    }

                    result = DBBatch.RunBatch(d["batch_file"], show_file_in_console, this);
                    break;

                case "create_sync":

                    result = Sync.CreateSync(d, this);
                    break;

                case "get_syncs":

                    result = Sync.GetSyncs(d, this);
                    break;

                case "sync_now":

                    result = Sync.SyncNow(d, this);
                    break;

                case "compile":

                    if (d["assembly_name"] == "null") d["assembly_name"] = "";
                    result = script.CompileFileForBlazor(d["compile"], this, d["assembly_name"]);
                    break;

                case "read_excel":

                    bool header = true;
                    if (d["first_row_as_header"] == "false") header = false;
                    result = OpenDocuments.ReadExcelAsJson(d["read_excel"], this, d["as_table"], header, d["sheet"]);
                    break;

                case "create_excel":

                    result = OpenDocuments.CreateExcelFromJson(d["create_excel"], this, d["json_values"]);
                    break;

                case "create_sync_database":

                    result = Sync.CreateSyncDataBase(d, this);
                    break;

                case "sync_database":

                    result = Sync.SyncDatabase(d, this);
                    break;

                case "get_max_sync_time":

                    result = Tables.GetMaxSyncTimeStampFromTable(d, this);
                    break;

                case "update_partition":

                    result = Tables.UpdatePartitionTimeStamp(d, this);
                    break;

                case "py":

                    result = py.EvalDictionary(d);
                    break;

                case "pw":

                    result = py.EvalDictionary(d);
                    break;

                case "py_file":

                    if (py is null)
                    {
                        this.py = new PhytonScripts(this);
                    }

                    result = "" + py.EvalFile(d, this);
                    break;

                case "py_db":

                    result = "" + py.EvalDB(d, this);
                    break;

                case "py_deploy_file":

                    result = "" + py.DeployFile(d, this);
                    break;

                case "py_deploy_to":

                    result = "" + py.DeployToProduction(d, this);
                    break;

                case "py_get_console":

                    result = "" + py.GetConsole();
                    break;

                case "statistics":

                    result = DBStatistics.StatisticsCommand(this, d["statistics"]);
                    break;

                case "read_csv":

                    bool csv_header = false;

                    if (d["read_csv"] == "true")
                    {
                        csv_header = true;
                    }
                    else 
                    {
                        csv_header = false;
                    }

                    result = AngelDBTools.StringFunctions.ReadCSV(d["read_csv"], csv_header, d["value_separator"],d["columns_as_numbers"]);
                    break;

                case "gpt":

                    if( this.GPT is null )
                    {
                        this.GPT = new OpenAIChatbot(this);
                    }

                    result = this.GPT.ProcessCommand(d["gpt"]);
                    break;


                case "post": 

                    AngelDB.AngelPOST api = new AngelDB.AngelPOST();
                    api.api = d["api"];
                    api.message = d["message"];
                    api.language = d["language"];

                    result = WebTools.SendJsonToUrl(d["post"], JsonConvert.SerializeObject(api));
                    break;


                case "lock_table":

                    result = Tables.LockTable(d, this);
                    break;

                case "unlock_table":

                    result = Tables.UnLockTable(d, this);
                    break;

                case "version":
                    result = "01.00.00 2023-04-29";
                    break;

                default:

                    result = "Error: No command given: " + commandkey;
                    break;
            }


            if (result.StartsWith("Error:"))
            {
                if (ThrowError)
                {
                    throw new Exception(result);
                }

            }

            return result;
        }


        public string CreateTable(object o)
        {
            try
            {
                string table_name = o.GetType().Name;
                string sql = "CREATE TABLE " + table_name + " FIELD LIST ";
                foreach (var prop in o.GetType().GetProperties())
                {

                    if (prop.Name.Trim().ToLower() == "id")
                    {
                        continue;
                    }

                    if (prop.Name.Trim().ToLower() == "partitionkey")
                    {
                        continue;
                    }

                    if (prop.Name.Trim().ToLower() == "timestamp")
                    {
                        continue;
                    }


                    switch (prop.PropertyType.Name)
                    {
                        case "String":
                            sql += prop.Name + " TEXT, ";
                            break;
                        case ("Int32"):
                            sql += prop.Name + " INTEGER, ";
                            break;
                        case ("Decimal"):
                            sql += prop.Name + " NUMERIC, ";
                            break;
                        case ("Boolean"):
                            sql += prop.Name + " BOOLEAN, ";
                            break;
                        case ("DateTime"):
                            sql += prop.Name + " TEXT, ";
                            break;
                        default:
                            sql += prop.Name + ", ";
                            break;
                    }
                }

                sql = sql.Substring(0, sql.Length - 2);
                string result = this.Prompt(sql);

                if( result.StartsWith("Error:"))
                {
                    Console.WriteLine(result);
                }

                return result;

            }
            catch (System.Exception e)
            {
                return $"Error: CreateTable: {e}";
            }

        }


        public string CreateTableFromJSon(string json)
        {
            try
            {
                var o = JsonConvert.DeserializeObject(json);
                string table_name = o.GetType().Name;
                string sql = "CREATE TABLE " + table_name + " FIELD LIST ";
                foreach (var prop in o.GetType().GetProperties())
                {

                    if (prop.Name.Trim().ToLower() == "id")
                    {
                        continue;
                    }

                    if (prop.Name.Trim().ToLower() == "partitionkey")
                    {
                        continue;
                    }

                    if (prop.Name.Trim().ToLower() == "timestamp")
                    {
                        continue;
                    }


                    switch (prop.PropertyType.Name)
                    {
                        case "String":
                            sql += prop.Name + " TEXT, ";
                            break;
                        case ("Int32"):
                            sql += prop.Name + " INTEGER, ";
                            break;
                        case ("Decimal"):
                            sql += prop.Name + " NUMERIC, ";
                            break;
                        case ("Boolean"):
                            sql += prop.Name + " BOOLEAN, ";
                            break;
                        case ("DateTime"):
                            sql += prop.Name + " TEXT, ";
                            break;
                        default:
                            sql += prop.Name + ", ";
                            break;
                    }
                }

                sql = sql.Substring(0, sql.Length - 2);
                return this.Prompt(sql);

            }
            catch (System.Exception e)
            {
                return $"Error: CreateTable: {e}";
            }

        }

        public string ObjectToJson(object o) 
        { 
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }

        string EnviromentTools()
        {
            string str;
            string nl = Environment.NewLine;
            //
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-- Environment members --");

            //  Invoke this sample with an arbitrary set of command line arguments.
            sb.AppendLine($"CommandLine: {Environment.CommandLine}");

            string[] arguments = Environment.GetCommandLineArgs();
            sb.AppendLine($"GetCommandLineArgs: {String.Join(", ", arguments)}");

            //  <-- Keep this information secure! -->
            sb.AppendLine($"CurrentDirectory: {Environment.CurrentDirectory}");

            sb.AppendLine("ExitCode: {Environment.ExitCode}");

            sb.AppendLine($"HasShutdownStarted: {Environment.HasShutdownStarted}");

            //  <-- Keep this information secure! -->
            sb.AppendLine($"MachineName: {Environment.MachineName}");

            sb.AppendLine($"OSVersion: {Environment.OSVersion.ToString()}");

            sb.AppendLine($"StackTrace: '{Environment.StackTrace}'");

            //  <-- Keep this information secure! -->
            sb.AppendLine($"SystemDirectory: {Environment.SystemDirectory}");

            sb.AppendLine($"TickCount: {Environment.TickCount}");

            //  <-- Keep this information secure! -->
            sb.AppendLine($"UserDomainName: {Environment.UserDomainName}");

            sb.AppendLine($"UserInteractive: {Environment.UserInteractive}");

            //  <-- Keep this information secure! -->
            sb.AppendLine($"UserName: {Environment.UserName}");

            sb.AppendLine($"Version: {Environment.Version.ToString()}");

            sb.AppendLine($"WorkingSet: {Environment.WorkingSet}");

            //  No example for Exit(exitCode) because doing so would terminate this example.

            //  <-- Keep this information secure! -->
            string query = "My system drive is %SystemDrive% and my system root is %SystemRoot%";
            str = Environment.ExpandEnvironmentVariables(query);
            sb.AppendLine($"ExpandEnvironmentVariables: {nl}  {str}");

            sb.AppendLine($"GetEnvironmentVariable: {nl}  My temporary directory is {Environment.GetEnvironmentVariable("TEMP")}.");

            sb.AppendLine($"GetEnvironmentVariables: ");
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry de in environmentVariables)
            {
                sb.AppendLine($"  {de.Key} = {de.Value}");
            }

            sb.AppendLine($"GetFolderPath: {Environment.GetFolderPath(Environment.SpecialFolder.System)}");

            string[] drives = Environment.GetLogicalDrives();
            sb.AppendLine($"GetLogicalDrives: {String.Join(", ", drives)}");

            return sb.ToString();
        }


        string RunGrid(Dictionary<string, string> d)
        {
            try
            {
                if (d["grid"].Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {

                    DataTable dataTable = new DataTable();

                    try
                    {
                        dataTable = Grid.SQLTable(d["grid"]);

                        if (d["as_json"] == "true")
                        {
                            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
                        }
                        else
                        {
                            return AngelDBTools.StringFunctions.ToCSVString(dataTable);
                        }

                    }
                    catch (Exception e)
                    {
                        return $"Error: Grid: {e}";
                    }

                }

                return Grid.SQLExec(d["grid"]);

            }
            catch (Exception e)
            {
                return $"Error: Grid {e}";
            }
        }


        string GridInsertOn(Dictionary<string, string> d)
        {

            try
            {
                DataTable t = JsonConvert.DeserializeObject<DataTable>(d["values"]);

                string ColumnList = "";

                foreach (DataColumn c in t.Columns)
                {
                    ColumnList += c.ColumnName + ",";
                }

                ColumnList = ColumnList.Substring(0, ColumnList.Length - 1);

                string result = this.Prompt("GRID CREATE TABLE IF NOT EXISTS " + d["grid_insert_on"] + " (" + ColumnList + ")");

                if (result.StartsWith("Error:"))
                {
                    return result;
                }

                if (d["id"] != "null")
                {
                    result = this.Prompt("GRID CREATE TABLE IF NOT EXISTS index_" + d["grid_insert_on"] + " ON " + d["table"] + " (" + d["id"] + ") ");

                    if (result.StartsWith("Error:"))
                    {
                        return result;
                    }

                }

                foreach (DataRow row in t.Rows)
                {

                    Grid.Reset();

                    if (d["id"] != "null")
                    {
                        result = this.Prompt("GRID SELECT * FROM " + d["grid_insert_on"] + " WHERE " + d["id"] + " = " + row[d["id"]]);

                        if (result.StartsWith("Error:"))
                        {
                            return result;
                        }

                        if (result == "[]")
                        {
                            Grid.CreateInsert(d["grid_insert_on"]);
                        }
                        else
                        {
                            Grid.CreateUpdate(d["grid_insert_on"], " WHERE " + d["id"] + " = " + row[d["id"]]);
                        }

                    }
                    else
                    {
                        Grid.CreateInsert(d["grid_insert_on"]);
                    }

                    foreach (DataColumn c in t.Columns)
                    {
                        Grid.AddField(c.ColumnName, row[c.ColumnName]);
                    }

                    result = Grid.Exec();

                    if (result.StartsWith("Error:"))
                    {
                        return result;
                    }

                }

                return result;

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }

        }


        string SaveJsonToTable(Dictionary<string, string> d)
        {
            try
            {
                string result = "";
                DataTable t = JsonConvert.DeserializeObject<DataTable>(d["json"]);

                if( t.Columns.Contains("PartitionKey"))
                {
                    t.Columns["PartitionKey"].ColumnName = "source_partitionkey";
                }

                if (t.Columns.Contains("timestamp"))
                {
                    t.Columns["timestamp"].ColumnName = "source_timestamp"; ;
                }

                StringBuilder sb = new StringBuilder();

                foreach (DataColumn item in t.Columns)
                {
                    if (d["id_column"] != "null")
                    {
                        if (item.ColumnName.Trim().ToLower() == d["id_column"].Trim().ToLower())
                        {
                            t.Columns[item.ColumnName].ColumnName = "id";
                        }
                    }

                    if (item.ColumnName.Trim().ToLower() == "union")
                    {
                        t.Columns[item.ColumnName].ColumnName = "_union";
                        sb.Append("_union" + ",");
                    }
                    else if (item.ColumnName.Trim().ToLower() != "id")
                    {
                        sb.Append(item.ColumnName.Replace("-", "_") + ",");
                    }
                }

                string field_list = sb.ToString();
                field_list = field_list.Remove(field_list.Length - 1, 1);

                string local_result = this.Prompt("CREATE TABLE " + d["save_to_table"] + " FIELD LIST " + field_list);

                if (local_result.StartsWith("Error:"))
                {
                    return local_result + "  --> " + "CREATE TABLE " + d["save_to_table"] + " FIELD LIST " + field_list;
                }

                result = this.Prompt($"UPSERT INTO {d["save_to_table"]} PARTITION KEY {d["partition_key"]} VALUES {JsonConvert.SerializeObject(t)}");

                if (result.StartsWith("Error:"))
                {
                    return "Error: SaveToTable: " + result;
                }

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: SaveToGrid {e}";
            }
        }


        string SaveToTable(Dictionary<string, string> d)
        {
            try
            {
                string result = this.Prompt(d["source"]);

                if (result.StartsWith("Error:"))
                {
                    return result;
                }

                if (result == "[]")
                {
                    return "Error: There is no data to process";
                }

                DataTable t = JsonConvert.DeserializeObject<DataTable>(result);

                StringBuilder sb = new StringBuilder();

                foreach (DataColumn item in t.Columns)
                {
                    if (d["id_column"] != "null") 
                    {
                        if (item.ColumnName.Trim().ToLower() == d["id_column"].Trim().ToLower()) 
                        {
                            t.Columns[item.ColumnName].ColumnName = "id";
                        }
                    }

                    if (item.ColumnName.Trim().ToLower() == "union")
                    {
                        t.Columns[item.ColumnName].ColumnName = "_union";
                        sb.Append("_union" + ",");
                    } 
                    else if (item.ColumnName.Trim().ToLower() != "id") 
                    {
                        sb.Append(item.ColumnName + ",");
                    }
                }

                string field_list = sb.ToString();
                field_list = field_list.Remove(field_list.Length - 1, 1);

                string local_result = this.Prompt("CREATE TABLE " + d["save_to_table"] + " FIELD LIST " + field_list);

                if (local_result.StartsWith("Error:"))
                {
                    return result;
                }

                result = this.Prompt($"UPSERT INTO {d["save_to_table"]} VALUES {JsonConvert.SerializeObject(t)}");

                if (result.StartsWith("Error:")) 
                {
                    return "Error: SaveToTable: " + result;
                }

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: SaveToGrid {e}";
            }
        }



        string SaveToGrid(Dictionary<string, string> d)
        {
            try
            {
                string local_result = this.Prompt(d["save_to_grid"]);

                if (local_result.StartsWith("Error:"))
                {
                    return "Error: SaveToGrid" + local_result.Replace("Error:", "");
                }

                if (local_result == "[]")
                {
                    return "[]";
                }

                DataTable t = JsonConvert.DeserializeObject<DataTable>( local_result );

                StringBuilder sb = new StringBuilder();

                if(t.Columns.Contains("odata.etag")) t.Columns.Remove("odata.etag");

                foreach (DataColumn item in t.Columns)
                {
                    if (item.ColumnName == "union")
                    {
                        t.Columns[item.ColumnName].ColumnName = "_union";
                        sb.Append("_union" + ",");
                    }
                    else 
                    {
                        sb.Append(item.ColumnName + ",");
                    }
                }

                string field_list = sb.ToString();
                field_list = field_list.Remove(field_list.Length - 1, 1);

                if (d["merge_data"] == "false")
                {
                    local_result = Grid.SQLExec("DROP TABLE IF EXISTS " + d["as_table"]);
                }

                local_result = Grid.SQLExec("CREATE TABLE IF NOT EXISTS " + d["as_table"] + "(row_id, " + field_list + ")");

                if (local_result.StartsWith("Error:"))
                {
                    return "Error: create_table " + local_result.Replace("Error:", "");
                }

                local_result = Grid.SQLExec($"CREATE INDEX IF NOT EXISTS index_{d["as_table"]}_row_id ON {d["as_table"]} (row_id)");

                if (local_result.StartsWith("Error:"))
                {
                    return "Error: create_index " + local_result.Replace("Error:", "");
                }

                long max_row = 0;

                if (d["merge_data"] == "true")
                {
                    DataTable t1 = Grid.SQLTable($"SELECT MAX( row_id ) AS max FROM {d["as_table"]}");

                    max_row = 0;

                    if (t1.Rows[0]["max"] != DBNull.Value)
                    {
                        max_row = (long)t1.Rows[0]["max"];
                    }

                    DataTable t2 = Grid.SQLTable($"SELECT * FROM {d["as_table"]} LIMIT 1");

                    foreach (DataColumn item in t.Columns)
                    {
                        if (!t2.Columns.Contains(item.ColumnName))
                        {
                            local_result = Grid.SQLExec($"ALTER TABLE {d["as_table"]} ADD COLUMN {item.ColumnName}");

                            if (local_result.StartsWith("Error:"))
                            {
                                return local_result;
                            }
                        }
                    }
                }

                foreach (DataColumn item in t.Columns)
                {
                    local_result = Grid.SQLExec($"CREATE INDEX IF NOT exists index_{d["as_table"]}_{item.ColumnName} ON {d["as_table"]} ({item.ColumnName})");

                    if (local_result.StartsWith("Error:"))
                    {
                        return local_result;
                    }

                }

                long n = 0;
                long i = max_row;

                foreach (DataRow r in t.Rows)
                {

                    ++n;
                    ++i;

                    Grid.Reset();
                    Grid.CreateInsert(d["as_table"]);
                    Grid.AddField("row_id", i);

                    foreach (DataColumn c in t.Columns)
                    {
                        Grid.AddField(c.ColumnName, r[c.ColumnName]);
                    }

                    local_result = Grid.Exec();

                    if (local_result.StartsWith("Error:"))
                    {
                        return local_result;
                    }
                }

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: SaveToGrid {e}";
            }
        }




        string AngelExecute(string command)
        {
            DbLanguage l = new DbLanguage();
            l.SetCommands(AngelSQL_Commands.Commands());
            Dictionary<string, string> d = l.Interpreter(command);

            if (!string.IsNullOrEmpty(l.errorString)) return l.errorString;

            switch (d.First().Key)
            {
                case "connect":

                    return Angel.Connect(d, this);

                case "command":

                    return Angel.Query(d, this);

                case "stop":

                    Angel.Disconnect(d, this);
                    this.always_use_AngelSQL = false;
                    this.angel_url = "";
                    this.angel_tocken = "";
                    return "Ok.";

                case "get_tocken":

                    return this.angel_url + "," + this.angel_tocken;

                case "set_tocken":

                    return SetTocken(d["set_tocken"]);

                case "server":

                    return Angel.ServerCommand(d["server"], this);

                default:
                    break;

            }

            return $"Error: Not AngelSQL Command found {command}";

        }


        string SetTocken(string tocken)
        {

            string[] parts = tocken.Split(',');

            if (parts.Length < 2) return "Error: Url and token are needed";

            this.angel_url = parts[0];
            this.angel_tocken = parts[1];

            return "Ok.";
        }




        string AzureExecute(string command)
        {

            DbLanguage l = new DbLanguage();
            l.SetCommands(AzureCommands.Commands());
            Dictionary<string, string> d = l.Interpreter(command);

            if (!string.IsNullOrEmpty(l.errorString)) return l.errorString;

            switch (d.First().Key)
            {
                case "connect":

                    if (!this.Azure.ContainsKey(d["alias"]))
                    {
                        try
                        {
                            this.Azure.Add(d["alias"], new AzureTable());
                            this.Azure[d["alias"]].TableServiceClient(d["connect"]);
                        }
                        catch (Exception e)
                        {
                            return $"Error: {e.ToString()}";
                        }

                    }

                    return "Ok.";

                case "select":

                    if (!this.Azure.ContainsKey(d["connection_alias"]))
                    {
                        return "Error: It is necessary to first start the Azure connection use the command CONNECT <connection_string> ALIAS <alias_name>";
                    }

                    d["where"] = CreateAzureQuery(d["where"]);

                    this.Azure[d["connection_alias"]].CreateTable(d["from"]);

                    string result = this.Azure[d["connection_alias"]].Query(d["where"]);

                    if (result.StartsWith("Error:"))
                    {
                        return result;
                    }

                    return result;

                case "get_results":

                    int page_size = 1000;
                    int.TryParse(d["page_size"], out page_size);

                    if( page_size == 0) page_size = 1000;

                    return this.Azure[d["connection_alias"]].GetQueryResults(page_size);                    

                case "save_accounts_to":

                    return SaveAzureAccountsTo(d);

                case "restore_accounts_from":

                    return RestorAzureAccounts(d);

                case "show_connections":

                    return ShowAzureConnections();

                case "get_container_from":

                    return AngelDBTools.StringFunctions.GetAccount(d["get_container_from"]);

                case "save_to_table":

                case "clear_connectios":

                    Azure.Clear();
                    return "Ok.";

                default:
                    break;

            }

            return $"Error: Not Azure Command found {command}";
        }

        string SQLServerExecute(string command)
        {

            DbLanguage l = new DbLanguage();
            l.SetCommands(SQLServerCommands.Commands());
            Dictionary<string, string> d = l.Interpreter(command);

            if (!string.IsNullOrEmpty(l.errorString)) return l.errorString;

            switch (d.First().Key)
            {
                case "connect":

                    if (!this.SQLServer.ContainsKey(d["alias"]))
                    {
                        try
                        {
                            this.SQLServer.Add(d["alias"], d["connect"]);
                        }
                        catch (Exception e)
                        {
                            return $"Error: {e.ToString()}";
                        }

                    }

                    return "Ok.";

                case "query":

                    if (!this.SQLServer.ContainsKey(d["connection_alias"]))
                    {
                        return "Error: It is necessary to first start the SQL SERVER connection use the command CONNECT <connection_string> ALIAS <alias_name>";
                    }

                    return SQLServerQuery(d["query"], d["connection_alias"]);

                case "save_accounts_to":

                    return SaveSQLServerAccountsTo(d);

                case "restore_accounts_from":

                    return RestoreSQLServerAccounts(d);

                case "show_connections":

                    return ShowAzureConnections();

                default:
                    break;

            }

            return $"Error: Not Azure Command found {command}";
        }


        public string SQLServerQuery(string command, string alias)
        {

            try
            {
                if (!this.SQLServer.ContainsKey(alias))
                {
                    return "Error: It is necessary to first start the SQL Server connection use the command CONNECT <connection_string> ALIAS <alias_name>";
                }

                SQLServerTools sql = new SQLServerTools(this.SQLServer[alias]);

                if (command.Trim().ToUpper().StartsWith("SELECT"))
                {
                    DataTable t = sql.SQLTable(command);
                    return JsonConvert.SerializeObject(t, Formatting.Indented);
                }

                string result = sql.SQLExec(command);

                if (result.StartsWith("Error:"))
                {
                    return result;
                }

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: {e.ToString()}";
            }

        }


        public string SaveAzureAccountsTo(Dictionary<string, string> d)
        {
            try
            {

                Dictionary<string, string> dic = new Dictionary<string, string>();

                foreach (string item in this.Azure.Keys)
                {
                    dic.Add(item, this.Azure[item].ConnectionString);
                }

                string json = JsonConvert.SerializeObject(dic);
                return AngelDBTools.StringFunctions.SaveEncriptedFile(d["save_accounts_to"], json, d["password"]);
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }


        public string RestorAzureAccounts(Dictionary<string, string> d)
        {
            try
            {

                if (!File.Exists(d["restore_accounts_from"]))
                {
                    return $"Error: The file does not exists {d["restore_accounts_from"]}";
                }

                string json = AngelDBTools.StringFunctions.RestoreEncriptedFile(d["restore_accounts_from"], d["password"]);

                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                foreach (string item in dic.Keys)
                {
                    string result = this.Prompt($"AZURE CONNECT {dic[item]} ALIAS {item}");

                    if (result.StartsWith("Error:"))
                    {
                        return result;
                    }
                }

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }


        public string ShowAzureConnections()
        {

            List<string> d = new List<string>();

            foreach (string item in this.Azure.Keys)
            {
                d.Add(item);
            }

            return JsonConvert.SerializeObject(d, Formatting.Indented);
        }


        public string SaveSQLServerAccountsTo(Dictionary<string, string> d)
        {
            try
            {
                return AngelDBTools.StringFunctions.SaveEncriptedFile(d["save_accounts_to"], JsonConvert.SerializeObject(this.SQLServer), d["password"]);
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }


        public string RestoreSQLServerAccounts(Dictionary<string, string> d)
        {
            try
            {
                string json = AngelDBTools.StringFunctions.RestoreEncriptedFile(d["restore_accounts_from"], d["password"]);
                this.SQLServer = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public string ShowSQLServerConnections()
        {

            List<string> d = new List<string>();

            foreach (string item in this.SQLServer.Keys)
            {
                d.Add(item);
            }

            return JsonConvert.SerializeObject(d, Formatting.Indented);
        }


        public string JsonDump(object o)
        {
            try
            {
                return JsonConvert.SerializeObject(o, Formatting.Indented);
            }
            catch (Exception e)
            {
                return $"Error : JsonSerializeObject: {e}";
                throw;
            }
        }

        string CreateAzureQuery(string query)
        {
            query = query.Replace(" = ", " eq ");
            query = query.Replace(" > ", " gt ");
            query = query.Replace(" >= ", " ge ");
            query = query.Replace(" < ", " lt ");
            query = query.Replace(" <= ", " le ");
            query = query.Replace(" <> ", " ne ");
            query = query.Replace(" AND ", " and ");
            query = query.Replace(" OR ", " or ");

            return query;

        }

        string SavePrompt(Dictionary<string, string> d)
        {
            try
            {
                string local_result = this.Prompt(d["source"]);

                if (local_result.StartsWith("Error:"))
                {
                    return local_result;
                }

                if (d["as_csv"] == "false")
                {
                    File.WriteAllText(d["save_to"], local_result, Encoding.UTF8);
                }
                else
                {
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(local_result);
                    AngelDBTools.StringFunctions.ToCSV(dt, d["save_to"], d["string_delimiter"]);
                }

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: {e.ToString()}";
            }
        }


        string CreateDB(string dbname, string ConnectionString)
        {
            if (!dbs.ContainsKey(dbname))
            {
                dbs.Add(dbname, new DB());
            }

            return dbs[dbname].Prompt(ConnectionString);

        }

        string PromptDB(string dbname, string Command)
        {
            if (!dbs.ContainsKey(dbname))
            {
                return $"Error: Database not exists; use CREAT DB command first {dbname}";
            }

            return dbs[dbname].Prompt(Command);

        }

        string RemoveDB(string dbname)
        {
            if (!dbs.ContainsKey(dbname))
            {
                return $"Error: Database not exists: {dbname}";
            }

            dbs.Remove(dbname);
            return "Ok.";
        }


        public void Close()
        {

        }

        public string AnalizeParentesis(Dictionary<string, string> d)
        {

            if (d["->"] != "null")
            {
                if (!this.TablesArea.ContainsKey(d["("]))
                {
                    return $"Error: There is no area: {d["("]}";
                }

                TableArea ta = this.TablesArea[d["("]];

                switch (d["->"].Trim().ToLower())
                {
                    case "eof()":

                        return ta.EOF();

                    case "next()":

                        return ta.Next();

                    case "update()":

                        return ta.UpdateData();

                    case "new()":

                        return ta.NewRow();

                    default:

                        return ta.Field(d["->"].Trim(), d["="].Trim());
                }

            }

            return "Ok.";

        }

        public string UseTable(Dictionary<string, string> d)
        {

            if (!this.TablesArea.ContainsKey(d["use_table"]))
            {
                this.TablesArea.Add(d["use_table"], null);
            }

            TableArea ta = new TableArea();
            string result = ta.UseTable(d, this);
            this.TablesArea[d["use_table"]] = ta;

            return result;
        }


        public DataTable GetDataTable( string Data )
        {

            try
            {
                return JsonConvert.DeserializeObject<DataTable>(Data);
            }
            catch (Exception)
            {
                this._Error = Data;
                return null;
            }
        }

        public string GetVars(Dictionary<string, object> d)
        {
            return JsonConvert.SerializeObject(d, Formatting.Indented);
        }

        public string RunCMD(string command)
        {

            var processInfo = new ProcessStartInfo("cmd.exe", command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = this.BaseDirectory + "scripts"                
            };

            StringBuilder sb = new();
            Process p = Process.Start(processInfo);
            p.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            p.BeginOutputReadLine();
            p.WaitForExit(1000);
            return sb.ToString();
        }


        public string RunScript(Dictionary<string, string> d)
        {

            if (!File.Exists(d["run"]))
            {
                return "Error: The file does not exists";
            }

            return ProcessFile(d["run"], d["verbose"]);

        }

        public string ProcessFile(string fileName, string verbose)
        {

            string result = "";
            StringBuilder sb = new StringBuilder();

            if (File.Exists(fileName))
            {
                int counter = 0;
                string line;

                StreamReader file = new StreamReader(fileName);

                string command = "";

                while ((line = file.ReadLine()) != null)
                {

                    ++counter;

                    if (line.Trim().StartsWith(@"//")) continue;
                    if (line.Trim() == "") continue;

                    command += line;

                    if (!line.Trim().EndsWith(";"))
                    {
                        continue;
                    }

                    command = command.Trim();
                    command = command.Substring(0, command.Length - 1);

                    if (command.Trim().StartsWith("IGNORE ERROR"))
                    {
                        result = Prompt(command.Trim()[12..]);
                        sb.AppendLine(result);
                        command = "";
                        continue;
                    }

                    if (command.Trim().StartsWith("QUIT"))
                    {
                        file.Close();
                        return sb.ToString();
                    }

                    if (verbose == "true")
                    {
                        Monitor.ShowLine(command, ConsoleColor.Yellow);
                    }

                    result = Prompt(command);
                    sb.AppendLine(result);
                    command = "";

                    if (result.StartsWith("Error:"))
                    {
                        Monitor.ShowError(result);
                        Monitor.ShowError($"In line {counter}");
                        file.Close();
                        return sb.ToString();
                    }
                }

                file.Close();
            }
            else
            {
                Monitor.ShowError($"File does not exists {fileName}");
            }

            return "Ok.";
        }

        public string SetVar(string var_name, string value)
        {

            try
            {

                value = value.Trim();

                if (value.StartsWith("'") && value.EndsWith("'"))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (!vars.ContainsKey(var_name) && value == "null")
                {
                    return "";
                }
                else if (vars.ContainsKey(var_name) && value == "null")
                {
                    return vars[var_name].ToString();
                }
                else if (vars.ContainsKey(var_name) && value != "null")
                {
                    vars[var_name] = value;
                    return vars[var_name].ToString();
                }
                else
                {
                    vars.Add(var_name, value);
                    return vars[var_name].ToString();
                }
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }


        public string SendToClient(string message)
        {
            if (this.OnSendMessage is null) return "";
            string result = "";
            this.OnSendMessage(message, ref result);
            return result;
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue1)
            {
                if (disposing)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(this.angel_url))
                        {
                            this.Prompt("ANGEL STOP");
                        }

                        this.CancelTransactions = true;

                        foreach (string key in this.dbs.Keys)
                        {
                            this.dbs[key].CancelTransactions = true;
                        }

                        //this.py.m_engine.Runtime.Shutdown();
                        //this.py.m_engine = null;
                        //this.py = null;

                        config = null;
                        vars = null;
                        parameters = null;
                        partitionsrules = null;
                        language = null;
                        dbs = null;
                        sqlite = null;
                        TablesArea = null;
                        partitions = null;
                        SQLiteConnections = null;
                        web = null;
                        Azure = null;
                        SQLServer = null;
                        Grid = null;
                        script = null;


                    }
                    catch (Exception)
                    {
                    }
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                disposedValue1 = true;
            }
        }

        public string jSonSerialize(object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }

        public object jSonDeserialize(string jSon)
        {
            return JsonConvert.DeserializeObject(jSon);
        }






        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~DB()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class PartitionsInfo
    {
        public string account_database_table_partition { get; set; } = "";
        public string ConnectionString { get; set; }
        public string table_type = "";
        public string partition_name { get; set; }
        public QueryTools sqlite { get; set; } = null;

        public string UpdateTimeStamp()
        {
            try
            {
                return sqlite.ExecSQLDirect($"UPDATE partitions SET timestamp = '{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff")}' WHERE partition = '{this.partition_name}'");
            }
            catch (Exception e)
            {
                return $"Error: UpdateTimeStamp {e}";
            }
        }

    }

    public class TableInfo
    {
        public string ConnectionString { get; set; }
        public string table_directory { get; set; }
        public string table_type { get; set; }        
    }


}
