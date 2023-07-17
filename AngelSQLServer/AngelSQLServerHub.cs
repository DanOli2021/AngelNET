using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;
using System.Data;
using System.Collections.Concurrent;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AngelSQLServer
{
    public class AngelSQLServerHub : Hub
    {

        private readonly AngelDB.DB _mainDb;
        private readonly ConnectionMappingService _connectionMappingService;
        private Dictionary<string, string> parameters;
        ConcurrentDictionary<string, AngelDB.DB> _db_hub_connections;

        public AngelSQLServerHub(AngelDB.DB mainDb, ConnectionMappingService connectionMappingService, ConcurrentDictionary<string, AngelDB.DB> db_hub_connections)
        {
            _mainDb = mainDb;
            _connectionMappingService = connectionMappingService;
            _db_hub_connections = db_hub_connections;

            this.parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));
        }

        public async Task SendAsync(string message)
        {
            try
            {
                HubMessage hubMessage = JsonConvert.DeserializeObject<HubMessage>(message);

                try
                {
                    if (hubMessage.messageType.Trim().ToLower() == "it_was_read")
                    {
                        if (_connectionMappingService.connections.ContainsKey(hubMessage.ToUser)) 
                        {
                            hubMessage.status = "it_was_read";
                            hubMessage.message = "";
                            await Clients.Client(_connectionMappingService.connections[hubMessage.ToUser]).SendAsync("Send", JsonConvert.SerializeObject(hubMessage));

                            _db_hub_connections[hubMessage.ToUser].Prompt($"UPDATE hub_messages SET was_read = '{hubMessage.was_read}', status = 'it_was_read' WHERE id = '{hubMessage.id}'", true );

                            return;
                        }
                    }

                    if (_connectionMappingService.connections.ContainsKey(hubMessage.ToUser))
                    {
                        await Clients.Client(_connectionMappingService.connections[hubMessage.ToUser]).SendAsync("Send", message);
                    }
                    else                     
                    {
                        HubMessage response = new HubMessage();
                        response.id = hubMessage.id;
                        response.messageType = "chat";
                        await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: SendAsync to : {hubMessage.ToUser}: user not conected");
                        return;
                    }

                    _db_hub_connections[hubMessage.UserId].Prompt($"UPSERT INTO hub_messages VALUES {JsonConvert.SerializeObject(hubMessage)}", true);

                }
                catch (Exception e)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: SendAsync to : {hubMessage.ToUser}: {e}");
                }

            }
            catch (Exception e)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: SendAsync: {e}");
            }
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task Identify(string message)
        {
            var httpContext = Context.GetHttpContext();
            var ip = httpContext.Connection.RemoteIpAddress;

            try
            {

                HubIdentify hubIdentify = JsonConvert.DeserializeObject<HubIdentify>(message);
                string result = _mainDb.Prompt($"SELECT * FROM hub_users WHERE id = '{hubIdentify.UserId}'");

                if (result.StartsWith("Error:"))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: Identify: {result}");
                    return;
                }

                if (result == "[]")
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: Identify: User not found {hubIdentify.UserId}");
                    return;
                }

                DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(result);

                if (dataTable.Rows[0]["password"].ToString().Trim() != hubIdentify.Password.Trim())
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: Identify: Password not match {hubIdentify.UserId}");
                    return;
                }

                if (dataTable.Rows[0]["active"].ToString().Trim() != "1")
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: Identify: Your username is not active  {hubIdentify.UserId}");
                    return;
                }

                _connectionMappingService.RemoveConnection(hubIdentify.UserId);
                _connectionMappingService.AddConnection(hubIdentify.UserId, Context.ConnectionId);

                HubMessage hubMessage = new HubMessage();
                hubMessage.id = Guid.NewGuid().ToString();
                hubMessage.UserId = hubIdentify.UserId;
                hubMessage.message = "Accepted credentials";

                if (!_db_hub_connections.ContainsKey(hubMessage.UserId)) 
                {
                    AngelDB.DB db = new AngelDB.DB();
                    db.Prompt($"DB USER {parameters["master_user"]} PASSWORD {parameters["master_password"]} DATA DIRECTORY {parameters["data_directory"]}", true);
                    db.Prompt($"CREATE ACCOUNT {parameters["account"]} SUPERUSER {parameters["account_user"]} PASSWORD {parameters["account_password"]}", true);
                    db.Prompt($"USE ACCOUNT {parameters["account"]}", true);
                    db.Prompt($"CREATE DATABASE {parameters["database"]}", true);
                    db.Prompt($"USE DATABASE {parameters["database"]}", true);
                    _db_hub_connections.TryAdd(hubMessage.UserId, db);
                }

                await Clients.Client(Context.ConnectionId).SendAsync("Send", JsonConvert.SerializeObject(hubMessage, Formatting.Indented));

            }
            catch (Exception e)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: Identify: {e.ToString()}");
                return;
            }

        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;

            // Usa el ID de conexión para eliminar la conexión de la base de datos
            // Asegúrate de implementar este método en tu base de datos
            _connectionMappingService.RemoveConnection(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

    }

    public class HubIdentify
    {
        public string UserId { get; set; }
        public string Password { get; set; }

    }

    public class HubMessage
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
