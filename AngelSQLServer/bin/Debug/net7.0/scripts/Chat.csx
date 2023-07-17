// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS
// Script for managing Chats
// Daniel() Oliver Rojas
// 2023-07-03

using System;
using Newtonsoft.Json;
using System.Collections.Generic;

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

HUbMessage chat_message = JsonConvert.DeserializeObject<HUbMessage>(message);
HUbMessage message_response;

// Configurar las opciones de serialización
var configuracion = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore
};

switch (chat_message.messageType.Trim().ToLower())
{
    case "command":

        Console.WriteLine();
        Console.WriteLine("Message Id: " + chat_message.id);
        Console.WriteLine("Message: " + chat_message.created + " --> " + chat_message.UserId + " --> COMMAND: " + chat_message.message);
        Console.WriteLine();

        message_response = new HUbMessage();

        message_response.id = System.Guid.NewGuid().ToString();
        message_response.UserId = chat_message.ToUser;
        message_response.ToUser = chat_message.UserId;
        message_response.messageType = "chat";
        message_response.message = db.Prompt(chat_message.message);
        message_response.status = "sent";
        message_response.was_read = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");

        return JsonConvert.SerializeObject(message_response, Formatting.Indented);

    case "chat":

        Console.WriteLine();
        Console.WriteLine("Message Id: " + chat_message.id);
        Console.WriteLine("Message: " + chat_message.created + " --> " + chat_message.UserId + " --> " + chat_message.message);
        Console.WriteLine();

        message_response = new HUbMessage();

        // Configurar las opciones de serialización
        var configuracion = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        message_response.id = chat_message.id;
        message_response.UserId = chat_message.ToUser;
        message_response.ToUser = chat_message.UserId;
        message_response.messageType = "it_was_read";
        message_response.status = "was_read";
        message_response.was_read = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");

        return JsonConvert.SerializeObject(message_response, Formatting.Indented);

    case "it_was_read":

        Console.WriteLine();
        Console.WriteLine("Message Id: " + chat_message.id);
        Console.WriteLine("Read by " + chat_message.UserId + " " + chat_message.was_read + " --> " + "It was read");
        Console.WriteLine();
        return "Ok.";

    default:

        Console.WriteLine();
        Console.WriteLine("Message: " + message);
        Console.WriteLine();
        return "Ok.";

}