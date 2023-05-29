using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AngelDB
{
    public static class Angel
    {
        public static string Connect(Dictionary<string, string> d, DB mainClass ) 
        {

            AngelQuery query = new AngelQuery();
            query.type = "IDENTIFICATION";
            query.User = d["user"];
            query.password = d["password"];
            query.account = d["account"];
            query.database = d["database"];
            query.data_directory = d["data_directory"];

            string result = WebTools.SendJsonToUrl(d["connect"], JsonConvert.SerializeObject(query));

            if (result.StartsWith("Error:")) 
            {
                return result;
            }

            AngelResponce angelResponce = JsonConvert.DeserializeObject<AngelResponce>(result);

            if (angelResponce.type.Trim() == "ERROR") 
            {
                return angelResponce.result;
            }

            mainClass.angel_tocken = angelResponce.tocken;
            mainClass.angel_url = d["connect"];
            mainClass.angel_user = d["user"];

            return "Ok.";
        }



        public static string Query(Dictionary<string, string> d, DB mainClass) 
        {

            string url = d["url"];
            string tocken = d["tocken"];

            if (url == "null")
            {
                url = mainClass.angel_url;
            }

            if (tocken == "null")
            {
                tocken = mainClass.angel_tocken;
            }

            if (string.IsNullOrEmpty(url)) 
            {
                return "Error: You have not indicated the URL to which you see to connect, please use ANGEL CONNECT first";
            }

            if (string.IsNullOrEmpty(tocken))
            {
                return "Error: You have not indicated the TOCKEN to which you see to connect, please use ANGEL CONNECT first";
            }

            AngelQuery query = new AngelQuery();
            query.type = "QUERY";
            query.tocken = tocken;
            query.command = d["command"];
            string result = WebTools.SendJsonToUrl(url, JsonConvert.SerializeObject(query));

            if (result.StartsWith("Error:"))
            {
                return result;
            }

            AngelResponce angelResponce = JsonConvert.DeserializeObject<AngelResponce>(result);
            return angelResponce.result;
        }

        public static string Disconnect(Dictionary<string, string> d, DB mainClass)
        {

            string url = mainClass.angel_url;
            string tocken = mainClass.angel_tocken;

            if (url == "null")
            {
                url = mainClass.angel_url;
            }

            if (tocken == "null")
            {
                tocken = mainClass.angel_tocken;
            }

            if (string.IsNullOrEmpty(url))
            {
                return "Error: You have not indicated the URL to which you see to connect, please use ANGEL CONNECT first";
            }

            if (string.IsNullOrEmpty(tocken))
            {
                return "Error: You have not indicated the TOCKEN to which you see to connect, please use ANGEL CONNECT first";
            }

            AngelQuery query = new AngelQuery();
            query.type = "DISCONNECT";
            query.tocken = tocken;
            query.command = "DISCONNECT";
            string result = WebTools.SendJsonToUrl(url, JsonConvert.SerializeObject(query));

            if (result.StartsWith("Error:"))
            {
                return result;
            }

            AngelResponce angelResponce = JsonConvert.DeserializeObject<AngelResponce>(result);
            return angelResponce.result;
        }

        public static string ServerCommand(string command, DB mainClass)
        {

            string url = mainClass.angel_url;
            string tocken = mainClass.angel_tocken;

            if (url == "null")
            {
                url = mainClass.angel_url;
            }

            if (tocken == "null")
            {
                tocken = mainClass.angel_tocken;
            }

            if (string.IsNullOrEmpty(url))
            {
                return "Error: You have not indicated the URL to which you see to connect, please use ANGEL CONNECT first";
            }

            if (string.IsNullOrEmpty(tocken))
            {
                return "Error: You have not indicated the TOCKEN to which you see to connect, please use ANGEL CONNECT first";
            }

            AngelQuery query = new AngelQuery();
            query.type = "SERVERCOMMAND";
            query.tocken = tocken;
            query.command = command;
            string result = WebTools.SendJsonToUrl(url, JsonConvert.SerializeObject(query));

            if (result.StartsWith("Error:"))
            {
                return result;
            }

            AngelResponce angelResponce = JsonConvert.DeserializeObject<AngelResponce>(result);
            return angelResponce.result;
        }

    }

    public class AngelQuery
    {
        public string type { get; set; } = "";
        public string User { get; set; } = "";
        public string password { get; set; } = "";
        public string account { get; set; } = "";
        public string database { get; set; } = "";
        public string data_directory { get; set; } = "";
        public string tocken { get; set; } = "";
        public bool on_iis { get; set; } = false;        
        public string command { get; set; } = "";

    }


    public class AngelResponce
    {
        public string type { get; set; } = "";
        public string tocken { get; set; } = "";
        public string result { get; set; } = "";
    }

    public class AngelPOST
    {
        public string api = "";
        public string message = "";
        public string language = "";
    }


}
