namespace AngelSQL
{
    public class Query
    {
        public string type { get; set; } = "";
        public string User { get; set; } = "";
        public string password { get; set; } = "";
        public string account { get; set; } = "";
        public string database { get; set; } = "";
        public string data_directory { get; set; } = "";
        public string token { get; set; } = "";
        public bool on_iis { get; set; } = false;
        public string command { get; set; } = "";
    }


    public class Responce
    {
        public string type { get; set; } = "";
        public string token { get; set; } = "";
        public string result { get; set; } = "";
    }


    public class DBConnections
    {
        public AngelDB.DB db { get; set; } = null;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime date_of_last_access { get; set; } = DateTime.Now;
        public int expiration_days { get; set; } = 30;
        public string User { get; set; } = "";
    }

    public class ApiClass 
    {
        public string token { get; set; } = "";
        public string type { get; set; } = "";
        public string data { get; set; } = "";
    }

    public class AngelPOST
    {
        public string api = "";
        public dynamic message = "";
        public string language = "";
        public string account = "";
        public string db_user = "";
        public string db_password = "";
    }



}


