using AngelDB;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using OpenAI_API.Moderation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AngelDB
{
    public static class HubCommands
    {
        public static Dictionary<string, string> Commands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"CONNECT", @"CONNECT#free" },
                { @"RESTORE ACCOUNTS FROM", @"RESTORE ACCOUNTS FROM#free;PASSWORD#password" },
                { @"SHOW CONNECTIONS", @"SHOW CONNECTIONS#free" },
                { @"IDENTIFY", @"IDENTIFY#free;USER#free;PASSWORD#free" },
                { @"CHAT TO USER", @"CHAT TO USER#free;TYPE#free;MESSAGE#free" },
                { @"QUERY", @"QUERY#free" },
                { @"GET MESSAGE", @"GET MESSAGE#free" },
                { @"WHO I AM", @"WHO I AM#free" },
                { @"DISCONNECT", @"DISCONNECT#free" }
            };

            return commands;

        }

    }


    public class HubOperation: IDisposable
    {
        string url = "";
        HubConnection hub = null;
        AngelDB.DB main_db;
        string main_user = "";
        MemoryDb mem_db = new MemoryDb();

        public static event Action<string> OnMessageReceived = delegate { };

        public HubOperation(AngelDB.DB db)
        {
            main_db = db;
            mem_db.SQLExec("CREATE TABLE IF NOT EXISTS chats (id INTEGER PRIMARY KEY AUTOINCREMENT, id_chat TEXT, from_user TEXT, to_user TEXT, message TEXT, created TEXT, was_read TEXT, status TEXT)");
            mem_db.SQLExec("CREATE UNIQUE INDEX IF NOT EXISTS index_id ON chats (id_chat)");
            mem_db.SQLExec("CREATE UNIQUE INDEX IF NOT EXISTS index_id ON chats (created)");
            OnMessageReceived += ReceiveData;

        }


        public async Task<string> HubExecute(string command)
        {
            DbLanguage l = new DbLanguage();
            l.SetCommands(HubCommands.Commands());
            Dictionary<string, string> d = l.Interpreter(command);

            if (!string.IsNullOrEmpty(l.errorString)) return l.errorString;

            switch (d.First().Key)
            {
                case "connect":

                    return await this.ConnectHub(d);

                case "disconnect":

                    return this.DisconnectHub();

                case "identify":

                    return this.IdentifyAsync(d["user"], d["password"]).Result;

                case "chat_to_user":

                    return this.MessageAsync(d).Result;

                case "query":

                    try
                    {
                        return JsonConvert.SerializeObject( mem_db.SQLTable(d["query"]), Formatting.Indented );
                    }
                    catch (Exception e)
                    {
                        return $"Error: Query: {e}";
                    }

                case "who_i_am":

                    return this.main_user;

                default:

                    return "Error: Query: Command not found.";

               case "get_message":

                    try
                    {
                        DataTable t = mem_db.SQLTable($"SELECT * FROM chats WHERE id = {d["get_message"]}");

                        if (t.Rows.Count == 1)
                        {
                            return t.Rows[0]["message"].ToString();
                        }

                        return "Error: No message found";

                    }
                    catch (Exception)
                    {
                        return "Error: Query: Command not found.";
                    }


            }

        }

        public async Task<string> ConnectHub(Dictionary<string, string> d)
        {

            try
            {
                this.url = d["connect"];

                hub = new HubConnectionBuilder()
                    .WithUrl(url)
                    .Build();

                await hub.StartAsync();

                hub.On<string>("Send", (message) =>
                {
                    OnMessageReceived.Invoke(message);
                });

                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: ConnectHub {e}";
            }
        }



        public async void ReceiveData(string message) 
        {

            try
            {

                HUbMessage hUbMessage = JsonConvert.DeserializeObject<HUbMessage>(message);

                mem_db.Reset();
                mem_db.CreateInsert("chats");
                mem_db.AddField("id_chat", hUbMessage.id);
                mem_db.AddField("from_user", hUbMessage.UserId);
                mem_db.AddField("to_user", hUbMessage.ToUser);
                mem_db.AddField("message", hUbMessage.message);
                mem_db.AddField("created", hUbMessage.created);
                mem_db.AddField("was_read", hUbMessage.was_read);
                mem_db.AddField("status", hUbMessage.status);
                string result_query = mem_db.Exec();

                string result = main_db.Prompt($"SCRIPT FILE scripts/chat/chat.csx ON APPLICATION DIRECTORY MESSAGE {message}");

                if (result.StartsWith("Error:"))
                {
                    mem_db.Reset();
                    mem_db.CreateInsert("chats");
                    mem_db.AddField("id_chat", System.Guid.NewGuid().ToString());
                    mem_db.AddField("from_user", "");
                    mem_db.AddField("to_user", "");
                    mem_db.AddField("message", result);
                    mem_db.AddField("created", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    mem_db.AddField("was_read", "");
                    mem_db.AddField("status", "Error");
                    result_query = mem_db.Exec();
                    return;
                }

                if (result == "Ok.")
                {
                    return;
                }

                await hub.SendAsync("SendAsync", result);

            }
            catch (Exception e)
            {
                mem_db.Reset();
                mem_db.CreateInsert("chats");
                mem_db.AddField("id_chat", System.Guid.NewGuid().ToString());
                mem_db.AddField("from_user", "");
                mem_db.AddField("to_user", "");
                mem_db.AddField("message", $"Error: ReceiveData {e}");
                mem_db.AddField("created", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"));
                mem_db.AddField("was_read", "");
                mem_db.AddField("status", "Error");
                string result_query = mem_db.Exec();
            }
        }


        public string DisconnectHub()
        {
            try
            {
                hub.StopAsync();
                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: ConnectHub {e}";
            }
        }


        public async Task<string> IdentifyAsync(string user, string password)
        {
            try
            {

                if( hub is null) return "Error: Identify: The hub is not connected use the following command: HUB CONNECT <url>";

                this.main_user = user;

                var identify = new { UserId = user, password = password };
                await hub.SendAsync("Identify", JsonConvert.SerializeObject(identify));
                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: Identify {e}";
            }
        }


        public async Task<string> MessageAsync(Dictionary<string,string> d)
        {
            try
            {
                HUbMessage message = new HUbMessage();
                message.id = Guid.NewGuid().ToString();
                message.UserId = this.main_user;
                message.ToUser = d["chat_to_user"];
                message.messageType = d["type"];
                message.message = d["message"];
                message.created = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
                message.status = "sent";
                await hub.SendAsync("SendAsync", JsonConvert.SerializeObject(message));
                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: Message {e}";
            }
        }

        public void Dispose()
        {
            if (hub != null) 
            {
                OnMessageReceived -= (message) => ReceiveData(message);
                this.DisconnectHub();
            }
        }
    }

    public class HUbMessage
    {
        public string id { get; set; }
        public string UserId { get; set; }
        public string ToUser { get; set; }
        public string messageType { get; set; }
        public string message { get; set; }
        public string created { get; set; }
        public string status { get; set; }
        public string was_read { get; set; }
    }


}
