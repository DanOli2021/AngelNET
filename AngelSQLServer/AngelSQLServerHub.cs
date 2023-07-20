using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;
using System.Data;
using System.Collections.Concurrent;
using DocumentFormat.OpenXml.Spreadsheet;
using AngelDB;

namespace AngelSQLServer
{
    public class AngelSQLServerHub : Hub
    {

        private readonly AngelDB.DB _mainDb;
        private readonly ConnectionMappingService _connectionMappingService;
        private Dictionary<string, string> parameters;

        public AngelSQLServerHub(AngelDB.DB mainDb, ConnectionMappingService connectionMappingService)
        {
            _mainDb = mainDb;
            _connectionMappingService = connectionMappingService;

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
                            await SendMessage(Context.ConnectionId, hubMessage.ToUser, "", "it_was_read", "it_was_read");
                            return;
                        }
                    }

                    if (_connectionMappingService.connections.ContainsKey(hubMessage.ToUser))
                    {
                        await Clients.Client(_connectionMappingService.connections[hubMessage.ToUser]).SendAsync("Send", message);
                    }
                    else                     
                    {
                        await SendMessage(Context.ConnectionId, hubMessage.ToUser, "", "Error", $"Error: SendAsync to : {hubMessage.ToUser}: user not conected");
                        return;
                    }

                }
                catch (Exception e)                
                {
                    await SendMessage(Context.ConnectionId, hubMessage.ToUser, "", "Error", $"Error: SendAsync to : {hubMessage.ToUser}: {e}");
                }

            }
            catch (Exception e)
            {
                await SendMessage(Context.ConnectionId, "", "", "Error", $"Error: SendAsync: {e}");
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
                    await SendMessage(Context.ConnectionId, hubIdentify.UserId, "", "Identify", $"Error: Identify: {result}");
                    return;
                }

                if (result == "[]")
                {
                    await SendMessage(Context.ConnectionId, hubIdentify.UserId, "", "Identify", $"Error: Identify: User not found {hubIdentify.UserId}");
                    return;
                }

                DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(result);

                if (dataTable.Rows[0]["password"].ToString().Trim() != hubIdentify.Password.Trim())
                {
                    await SendMessage(Context.ConnectionId, hubIdentify.UserId, "", "Identify", $"Error: Identify: Password not match {hubIdentify.UserId}");
                    return;
                }

                if (dataTable.Rows[0]["active"].ToString().Trim() != "1")
                {
                    await SendMessage(Context.ConnectionId, hubIdentify.UserId, "", "Identify", $"Error: Identify: Your username is not active  {hubIdentify.UserId}");
                    return;
                }

                _connectionMappingService.RemoveConnection(hubIdentify.UserId);
                _connectionMappingService.AddConnection(hubIdentify.UserId, Context.ConnectionId);

                await SendMessage(Context.ConnectionId, hubIdentify.UserId, "", "Identify", "Accepted credentials");

            }
            catch (Exception e)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Send", $"Error: Identify: {e.ToString()}");
                return;
            }

        }


        public async Task SendMessage(string connectionId, string userid, string toUser, string message_type, string message) 
        {
            HubMessage hubMessage;
            hubMessage = new HubMessage();
            hubMessage.id = Guid.NewGuid().ToString();
            hubMessage.created = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
            hubMessage.UserId = userid;
            hubMessage.ToUser = toUser;
            hubMessage.message = message;
            hubMessage.messageType = message_type;
            await Clients.Client(connectionId).SendAsync("Send", JsonConvert.SerializeObject(hubMessage, Formatting.Indented));
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
