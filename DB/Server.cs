using AngelDBTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelDB
{

    public class Server
    {
        AngelDB.DbLanguage language = new AngelDB.DbLanguage();
        string result = "";
        AngelDB.DB db;

        public Server(AngelDB.DB db) 
        {
            language.SetCommands(AngelDB.AngelServerCommands.DbCommands());
            this.db = db;            
        }


        public string ServerCommand(string command) 
        {
            Dictionary<string, string> local_d = language.Interpreter(command);

            if (!string.IsNullOrEmpty(language.errorString))
            {
                return language.errorString;
            }

            return ServerProcessCommands(local_d);

        }

        string ServerProcessCommands(Dictionary<string, string> d)
        {
            string commandkey = d.First().Key;
            string result = "";

            switch (commandkey)
            {
                case "add_app":

                    if (string.IsNullOrEmpty(d["add_app"]))
                    {
                        return "Error: You need to specify the app name";
                    }

                    Dictionary<string, string> app = new Dictionary<string, string>();
                    app.Add("id", d["add_app"]);
                    app.Add("app_directory", d["directory"]);
                    app.Add("domain", d["domain"]);
                    return db.Prompt($"INSERT INTO apps VALUES {JsonConvert.SerializeObject(app)}");

                case "delete_app":

                    result = db.Prompt($"SELECT * FROM apps WHERE id = '{d["delete_app"]}'");

                    if (result == "[]")
                    {
                        return $"Error: the App {d["delete_app"]} does not exist";
                    }

                    return db.Prompt($"DELETE FROM apps PARTITION KEY main WHERE id = '{d["delete_app"]}'");

                case "show_apps":

                    return db.Prompt($"SELECT * FROM apps WHERE {d["where"]}");

                case "set_certificate":

                    db.Prompt($"DELETE FROM certificates PARTITION KEY main WHERE 1 = 1 ");
                    Dictionary<string, string> certificate = new Dictionary<string, string>();
                    certificate.Add("id", "" + d["set_certificate"]);
                    certificate.Add("certificate", d["set_certificate"]);
                    certificate.Add("password", CryptoString.Encrypt(d["password"], "minregpo", "stocolmo"));
                    certificate.Add("listen_ip", d["listen_ip"]);
                    certificate.Add("listen_port", d["port"]);
                    return db.Prompt($"INSERT INTO certificates VALUES {JsonConvert.SerializeObject(certificate)}");

                case "delete_certificate":

                    return db.Prompt($"DELETE FROM certificates PARTITION KEY main WHERE id = '{d["delete_certificate"]}'");

                case "show_certificates":

                    return db.Prompt($"SELECT certificate, listen_ip, listen_port FROM certificates");

               case "show_params":

                    return db.Prompt($"SELECT * FROM params");

                case "init_database":

                    return InitDataBase();

                case "set_parameter":

                    return SetParameter(d["set_parameter"], d["value"]);
                    
                default:
                    return "Error: unknown command";
            }
        }


        string SetParameter(string parameter, string value)
        {
            result = db.Prompt($"SELECT * FROM params");

            if (result.StartsWith("Error:")) return result;

            if (result == "[]")
            {
                Dictionary<string, string> d = new Dictionary<string, string>();
                d.Add("urls", d["set_host"]);
                return db.Prompt($"INSERT INTO params VALUES {JsonConvert.SerializeObject(d)}");
            }
            else
            {
                return db.Prompt($"UPDATE params SET {parameter} = '{value}' WHERE 1 = 1");
            }

        }

        public string InitDataBase()
        {
            string result = "";
            result = db.Prompt("CREATE ACCOUNT angelsql_main SUPERUSER angelsql PASSWORD angelsql_main_user");
            if (result.StartsWith("Error:")) return result;
            result = db.Prompt("USE angelsql_main");
            if (result.StartsWith("Error:")) return result;
            result = db.Prompt("CREATE DATABASE angelsql_main");
            if (result.StartsWith("Error:")) return result;
            result = db.Prompt("USE angelsql_main");
            if (result.StartsWith("Error:")) return result;
            result = db.Prompt("CREATE TABLE certificates FIELD LIST listen_ip, listen_port, certificate, password");
            if (result.StartsWith("Error:")) return result;
            result = db.Prompt("CREATE TABLE params FIELD LIST service_command, service,  service_delay, connection_timeout, urls, cors, scripts_on_memory");
            if (result.StartsWith("Error:")) return result;
            result = db.Prompt("CREATE TABLE apps FIELD LIST app_directory, domain, version");
            if (result.StartsWith("Error:")) return result;
            result = db.Prompt("CREATE TABLE angel_log FIELD LIST value");
            if (result.StartsWith("Error:")) return result;

            return "Ok.";
        }



    }
}
