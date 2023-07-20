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

AngelDB.HUbMessage chat_message = JsonConvert.DeserializeObject<AngelDB.HUbMessage>(message);
AngelDB.HUbMessage message_response;

// Configurar las opciones de serialización
var configuracion = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore
};

switch (chat_message.messageType.Trim().ToLower())
{
    case "command":

        message_response = new AngelDB.HUbMessage();
        message_response.id = System.Guid.NewGuid().ToString();
        message_response.UserId = chat_message.ToUser;
        message_response.ToUser = chat_message.UserId;
        message_response.messageType = "chat";
        message_response.message = db.Prompt(chat_message.message);
        message_response.status = "sent";
        message_response.was_read = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");

        return JsonConvert.SerializeObject(message_response, Formatting.Indented);

    case "chat":

        message_response = new AngelDB.HUbMessage();

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

    default:

        return "Ok.";

}