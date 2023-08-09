// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Script for managing Branch Stores and Authorizers
// Daniel() Oliver Rojas
// 2023-07-03

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));
Dictionary<string, string> servers = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_SERVERS"));
AngelApiOperation operation = JsonConvert.DeserializeObject<AngelApiOperation>(message);

switch (operation.OperationType)
{
    case "CreateAccount":

        return CreateAccount(db, main_db, operation.Token, operation.DataMessage);

    case "SendPinToEmail":

        return SendPinToEmail(db, operation.DataMessage);

    default:

        return "Error: No service found";
}


public string CreateAccount(AngelDB.DB db, AngelDB.DB main_db, string token, dynamic message)
{

    if (string.IsNullOrEmpty(message["pin"]))
    {
        return "Error: PIN is required";
    }

    if (string.IsNullOrEmpty(message["account"]))
    {
        return "Error: Account Name is required";
    }

    if (string.IsNullOrEmpty(message["user"]))
    {
        return "Error: User is required";
    }

    if (string.IsNullOrEmpty(message["password"]))
    {
        return "Error: Password is required";
    }

    if (string.IsNullOrEmpty(message["retype_password"]))
    {
        return "Error: Retype Password is required";
    }

    if (message["password"].ToString().Trim() != message["retype_password"].ToString().Trim())
    {
        return "Error: Passwords do not match";
    }

    if (string.IsNullOrEmpty(message["email"]))
    {
        return "Error: Email is required";
    }

    string email = message["email"].ToString().Trim().ToLower();

    if (EmailValidator.IsValidEmail(email))
    {
        return "Error: Email is not valid";
    }

    if (string.IsNullOrEmpty(message["name"]))
    {
        return "Error: Name is required";
    }

    string account = message["account"].ToString().Trim().ToLower();

    if (!ValidarCadena(account))
    {
        return "Error: Account Name is not valid";
    }

    string user = message["user"].ToString().Trim().ToLower();

    if (!ValidarCadena(user))
    {
        return "Error: USER Name is not valid";
    }

    string result = main_db.Prompt("SELECT * FROM accounts WHERE id = '" + account + "'");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    if (result != "[]")
    {
        return "Error: Account already exists";
    }

    var db_account = new
    {
        // db_user, name, email, connection_string, db_password, database, data_directory, account, super_user, super_user_password, active, created
        id = account,
        db_user = message["user"].ToString().Trim(),
        name = message["name"].ToString().Trim(),
        email = message["email"].ToString().Trim(),
        connection_string = $"DB USER {message["user"].ToString().Trim()} PASSWORD {message["password"].ToString().Trim()} DATA DIRECTORY C:/accounts/{message["account"].ToString().Trim()}",
        db_password = message["password"].ToString().Trim(),
        data_directory = $"C:/accounts/{message["account"].ToString().Trim()}",
        account = message["account"].ToString().Trim(),
        super_user = "main_" + message["user"].ToString().Trim(),
        super_user_password = message["password"].ToString().Trim(),
        active = "true",
        database = "auth",
        created = DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
    };

    result = main_db.Prompt($"CREATE TABLE accounts FIELD LIST db_user, name, email, connection_string, db_password, database, data_directory, account, super_user, super_user_password, active, created");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = main_db.Prompt($"UPSERT INTO accounts VALUES {JsonConvert.SerializeObject(db_account, Formatting.Indented)}");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt(db_account.connection_string, true);

    db.Prompt($"VAR db_user = '{db_account.db_user}'");
    db.Prompt($"VAR db_password = '{db_account.db_password}'");
    db.Prompt($"VAR db_account = '{db_account.id}'");

    db.Prompt($"CREATE ACCOUNT {db_account.id} SUPERUSER {db_account.super_user} PASSWORD {db_account.db_password}", true);
    db.Prompt($"USE ACCOUNT {db_account.id}", true);
    db.Prompt($"CREATE DATABASE {db_account.database}", true);
    db.Prompt($"USE DATABASE {db_account.database}", true);

    result = db.Prompt("SCRIPT FILE S:/mybusinesspos/tokens/Scripts/tokens/initdatabase.csx RECOMPILE");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt("SCRIPT FILE S:/mybusinesspos/tokens/Scripts/auth/initdatabase_auth.csx RECOMPILE");
    return result;

}

public string SendPinToEmail(AngelDB.DB db, dynamic message)
{

    string email = message["email"].ToString().Trim().ToLower();

    if (string.IsNullOrEmpty(email))
    {
        return "Error: Email is required";
    }

    if (EmailValidator.IsValidEmail(email))
    {
        return "Error: Email is not valid";
    }

    Dictionary<string, string> dataMessage = new Dictionary<string, string>();
    dataMessage.Add("permision_id", "Register");
    dataMessage.Add("branchstore_id", "AUTHAPP");
    string result = sendToAngelPOST(db, servers, "register", "Ventana1$$$", "auth/adminbranchstores", "CreatePermission", dataMessage);

    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

    responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);

    if (responce.result.StartsWith("Error:"))
    {
        return responce.result;
    }

    RegisterEmail registerEmail = JsonConvert.DeserializeObject<RegisterEmail>(responce.result);

    if (!parameters.ContainsKey("wwwroot"))
    {
        return "Error: wwwroot parameter is required";
    }

    string wwwroot = parameters["wwwroot"].ToString().Trim();

    if (!Directory.Exists(wwwroot))
    {
        return "Error: wwwroot directory not found";
    }

    string images_directory = Path.Combine(wwwroot, "auth/pins/images");

    if (!Directory.Exists(images_directory))
    {
        Directory.CreateDirectory(images_directory);
    }

    string pins_directory = Path.Combine(wwwroot, "auth/pins");

    if (!Directory.Exists(pins_directory))
    {
        Directory.CreateDirectory(pins_directory);
    }

    string image_name = images_directory + "/" + registerEmail.pin_number + ".png";

    result = CreateImageFromText(registerEmail.pin_number, image_name);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    string htmlCode = $@"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <title>Confirmation PIN</title>
    </head>
    <body>
        <h1>Confirmation PIN</h1>
        <p>Dear User,</p>
        <p>Here is your confirmation PIN:</p>
        <img src='{"https://tokens.mybusinesspos.net/auth/pins/images/" + registerEmail.pin_number + ".png" }' alt='Confirmation PIN Image'>
        <p>If you are unable to view the image, please click on the following link:</p>
        <a href='https://tokens.mybusinesspos.net/auth/pins/{registerEmail.pin_number}.html'>Click here</a>
        <p>Thank you!</p>
    </body>
    </html>";

    string html_file = Path.Combine(wwwroot, "auth/pins/" + registerEmail.pin_number + ".html");
    File.WriteAllText(html_file, htmlCode);

    result = EmailSender.SendEmail(parameters["email_address"], 
                                    parameters["email_password"],
                                    "Tokens Administration PIN", 
                                    email, 
                                    "", 
                                    "Tokens MyBusiness POS Confirmation PIN", 
                                    htmlCode,
                                    parameters["smtp"].ToString().Trim(), 
                                    int.Parse(parameters["smtp_port"].ToString().Trim()), 
                                    false);
    
    return result;

}



public static bool ValidarCadena(string cadena)
{
    // Comprobando que el largo del string esté dentro del rango permitido
    if (cadena.Length < 4 || cadena.Length > 32)
    {
        return false;
    }

    // Creando la expresión regular para comprobar que el string solo contenga caracteres y números
    Regex patron = new Regex("^[a-zA-Z0-9]*$");

    // Si el string cumple con el patrón, devolver True, si no, devolver False
    return patron.IsMatch(cadena);
}



public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public dynamic DataMessage { get; set; }
}


public string sendToAngelPOST(AngelDB.DB db, Dictionary<string, string> servers, string user, string password, string api_name, string OperationType, Dictionary<string, string> data_message)
{

    Dictionary<string, string> message_data = new Dictionary<string, string>();

    message_data.Clear();
    message_data.Add("OperationType", "GetTokenFromUser");
    message_data.Add("User", user);
    message_data.Add("Password", password);

    string result = db.Prompt($"POST {servers["tockens_url"]} ACCOUNT mybusinesspos API tokens/admintokens MESSAGE {JsonConvert.SerializeObject(message_data, Formatting.Indented)}");

    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
    responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
    string token = responce.result;

    Dictionary<string, object> data = new Dictionary<string, object>();
    data.Add("OperationType", OperationType);
    data.Add("Token", token);
    data.Add("DataMessage", data_message);

    result = db.Prompt($"POST {servers["auth_url"]} ACCOUNT mybusinesspos API {api_name} MESSAGE {JsonConvert.SerializeObject(data, Formatting.Indented)}");
    return result;

}


public class RegisterEmail
{
    public string id { get; set; }
    public string authorizer { get; set; }
    public string authorizer_name { get; set; }
    public string branch_store { get; set; }
    public string pin_number { get; set; }
    public string message { get; set; }
    public string date { get; set; }
    public string permisions { get; set; }
    public string confirmed_date { get; set; }
    public string user { get; set; }
    public string user_name { get; set; }
    public string app_user { get; set; }
    public string app_user_name { get; set; }
    public string authuser { get; set; }
    public string status { get; set; }

}

public static class EmailValidator
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // La siguiente expresión regular se basa en la definición de los RFC 5322 Official Standard y RFC 5321 SMTP
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<=\w)@"
                + @"((?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\["
                + @"((?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|"
                + @"\[((a|b|c|d|e|f|g|h|j|k|l|m|n|o|p|r|s|t|u|v|w|y|z)|(a|b|c|d|e|f|g|h|j|k|l|m|n|o|p|r|s|t|u|v|w|y|z)"
                + @"(a|b|c|d|e|f|g|h|j|k|l|m|n|o|p|r|s|t|u|v|w|y|z))\])])$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            // Si la expresión regular falla, simplemente devolvemos falso
            return false;
        }
    }
}


public static string CreateImageFromText(string text, string filename)
{

    try
    {
        // Define el tipo de fuente y tamaño
        Font font = new Font("Arial", 50, FontStyle.Regular);

        // Crea un bitmap en base al texto y la fuente
        SizeF textSize;
        using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
        {
            textSize = graphics.MeasureString(text, font);
        }

        // Crea una nueva imagen del tamaño necesario para el texto
        using (Bitmap image = new Bitmap((int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height)))
        {
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // Define el color de fondo y el color del texto
                graphics.Clear(Color.White);
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString(text, font, brush, 0, 0);
                }
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            // Guarda la imagen a un archivo
            image.Save(filename, ImageFormat.Png);
        }

        return "Ok.";

    }
    catch (System.Exception e)
    {
        return "Error: CreateImageFromText() " + e.Message;
    }

}


public static class EmailSender
{
    public static string SendEmail(
        string fromEmail,
        string fromPassword,
        string fromName,
        string toEmail,
        string cc,
        string subject,
        string bodyHtml, string host, 
        int port, 
        bool enableSsl = true, bool useDefaultCredentials = false)
    {
        try
        {
            var fromAddress = new MailAddress(fromEmail, fromName);
            var toAddress = new MailAddress(toEmail);

            NetworkCredential credentials = new NetworkCredential(fromAddress.Address, fromPassword);

            if (useDefaultCredentials)
            {
                credentials = CredentialCache.DefaultNetworkCredentials;
            }

            var smtp = new SmtpClient
            {
                Host = "67.205.92.88", // especifica el servidor SMTP aquí
                Port = 25, // especifica el puerto SMTP aquí
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,                
                Credentials = credentials
            };

            
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            })
            {
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    message.CC.Add(cc);
                }

                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                smtp.Send(message);
                ServicePointManager.ServerCertificateValidationCallback = null;
            }

            return "Ok.";

        }
        catch (System.Exception e)
        {
            return "Error: " + e.Message;
        }

    }




}

public static string ConvertImageToBase64(string imagePath)
{
    byte[] imageBytes = File.ReadAllBytes(imagePath);
    string base64String = Convert.ToBase64String(imageBytes);
    return base64String;
}

