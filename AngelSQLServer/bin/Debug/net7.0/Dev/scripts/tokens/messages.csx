// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Process to send messages to user
// Daniel() Oliver Rojas
// 2023-05-19


#load "pins.csx"
#load "translations.csx"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Xml.Linq;
using System.Text;


public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public string UserLanguage { get; set; }
    public dynamic DataMessage { get; set; }    
}

AngelApiOperation api = JsonConvert.DeserializeObject<AngelApiOperation>(message);

//Server parameters
Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));

Translations translation;

if( !db.Globals.ContainsKey("translations") )
{
    translation = new();
    translation.SpanishValues();
    db.Globals.TryAdd("translations", translation);
}
else  
{
    translation = (Translations)db.Globals["translations"];
} 

// This is the main function that will be called by the API
return api.OperationType switch
{
    "SendPinToEmail" => AdminMessages.SendPinToEmail(api, parameters, server_db, translation ),
    "RecoverMasterPassword" => AdminMessages.RecoverMasterPassword(db, api, parameters, server_db, translation),
    _ => $"Error: No service found {api.OperationType}",
};


// This class is used to store the tokens in the database
public static class AdminMessages
{


    public static string RecoverMasterPassword(AngelDB.DB db, AngelApiOperation api, Dictionary<string, string> parameters, AngelDB.DB server_db, Translations translation)
    {
        dynamic d = api.DataMessage;

        string language = "en";

        if( api.UserLanguage != null )
        {
            language = api.UserLanguage;
        }

        if (d.Email == null)
        {
            return $"Error: SendPinToEmail() {translation.Get("Email is null", language)}";
        }

        string Email = d.Email.ToString().Trim().ToLower();

        if( string.IsNullOrEmpty(Email))
        {
            return $"Error: {translation.Get("Email is required", language)}";
        }

        if (EmailValidator.IsValidEmail(Email))
        {
            return $"Error: {translation.Get("Email is not valid", language)} {Email}";
        }

        string result = server_db.Prompt($"SELECT * FROM accounts WHERE email = '{Email}'");

        if( result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        if( result == "[]")
        {
            return $"Error: RecoverMasterPassword() {translation.Get("Email not found", language)}";
        }

        DataTable tAccounts = db.GetDataTable(result);
        DataRow r = tAccounts.Rows[0];

        if (string.IsNullOrEmpty(r["connection_string"].ToString()))
        {
            result = db.Prompt($"DB USER {r["db_user"]} PASSWORD {r["db_password"]} DATA DIRECTORY {r["data_directory"]}");
        }
        else
        { 
            result = db.Prompt(r["connection_string"].ToString());
        }

        if (result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        result = db.Prompt($"USE {r["account"]} DATABASE {r["database"]}");

        if (result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        result = db.Prompt("SELECT * FROM users WHERE master = 'true'");

        if (result.StartsWith("Error:"))
        {
            return "Error: RecoverMasterPassword() " + result.Replace("Error:", "");
        }

        string user = "";
        string password = "";

        if (result != "[]") 
        {
            DataTable tUsers = db.GetDataTable(result);
            DataRow rUser = tUsers.Rows[0];

            user = rUser["id"].ToString();
            password = rUser["password"].ToString();
        }

        string htmlCode = $@"<!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>MyBusiness POS Authorizer</title>
                            </head>
                            <body>
                                <h1>MyBusiness POS Authorizer</h1>
                                <p></p>
                                <h2>Account     : {r["account"]}</h2>   
                                <h2>DB User     : {r["db_user"]}</h2>
                                <h2>DB Password : {r["db_password"]}</h2>
                                <h2>User        : {user}</h2>
                                <h2>Password    : {password}</h2>
                                <p></p>
                            </body>
                            </html>";

        // result = EmailSender.SendEmail(parameters["email_address"],
        //                                 parameters["email_password"],
        //                                 "MyBusiness POS Authorizer (Recover)",
        //                                 email,
        //                                 "",
        //                                 "MyBusiness POS Authorizer (Recover)",
        //                                 htmlCode,
        //                                 parameters["smtp"].ToString().Trim(),
        //                                 int.Parse(parameters["smtp_port"].ToString().Trim()),
        //                                 false);

        //if (result.StartsWith("Error:"))
        //{
        //    return result;
        //}

        result = SendMailFromSoap(htmlCode, Email, parameters["email_address"], parameters["email_password"],"MyBusiness POS Authorizer (Recover)").GetAwaiter().GetResult();

        XDocument doc = XDocument.Parse(result);
        XNamespace ns = "http://wsCorreo.mybusinesspos.com/";
        result = doc.Descendants(ns + "EnviaCorreoHResult").First().Value;
        return result;

    }


    public static string SendPinToEmail(AngelApiOperation api, Dictionary<string, string> parameters, AngelDB.DB server_db, Translations translation)
    {

        dynamic d = api.DataMessage;

        string language = "en";

        if( api.UserLanguage != null )
        {
            language = api.UserLanguage;
        }

        if (d.Email == null)
        {
            return "Error: SendPinToEmail() Email is null";
        }

        string email = d.Email.ToString().Trim().ToLower();

        if (string.IsNullOrEmpty(email))
        {
            return $"Error: {translation.Get("Email is required", language)}";
        }

        if (EmailValidator.IsValidEmail(email))
        {
            return $"Error: {translation.Get("Email is not valid", language)} {email}";
        }

        string result = server_db.Prompt($"SELECT * FROM accounts WHERE email = '{email}'", true);

        if (result != "[]")
        {
            return $"Error: CreateAccount() {translation.Get("Email already exists in another account", language)} {email}";
        }

        Pin pin = new()
        {
            Id = Guid.NewGuid().ToString(),
            Pin_number = RandomNumberGenerator.GenerateRandomNumber(4),
            Authorizer = "SYSTEM",
            Authorizer_name = "SYSTEM",
            Branch_store = "SYSTEM",
            Date = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
            Expirytime = DateTime.Now.AddMinutes(30).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
            Permissions = "Create account",
            Status = "pending",
            User = email,
            Minutes = 30,
            Authorizer_message = "Created by SendPinToEmail()",
            Pintype = "touser"
        };

        //result = db.CreateTable(pin, "pins");
        //if (result.StartsWith("Error:")) return "Error: Creating table pins " + result.Replace("Error:", "");

        string wwwroot = parameters["wwwroot"].ToString().Trim();

        if (!Directory.Exists(wwwroot))
        {
            return $"Error: {translation.Get("wwwroot directory not found", language)}";
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

        string image_name = images_directory + "/" + pin.Pin_number + ".png";

        result = CreateImageFromText(pin.Pin_number, image_name);

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        result = server_db.CreateTable(pin, "pins");
        if (result.StartsWith("Error:")) return $"Error: {translation.Get("Creating table pins", language)} " + result.Replace("Error:", "");

        // string htmlCode = $@"<!DOCTYPE html>
        //                     <html>
        //                     <head>
        //                         <meta charset='UTF-8'>
        //                         <title>Confirmation PIN</title>
        //                     </head>
        //                     <body>
        //                         <h1>Confirmation PIN</h1>
        //                         <p>Dear User,</p>
        //                         <p>Here is your confirmation PIN:</p>
        //                         <img src='{"https://tokens.mybusinesspos.net/auth/pins/images/" + pin.pin_number + ".png"}' alt='Confirmation PIN Image'>
        //                         <p>If you are unable to view the image, please click on the following link:</p>
        //                         <a href='https://tokens.mybusinesspos.net/auth/pins/{pin.pin_number}.html'>Click here</a>
        //                         <p>Thank you!</p>
        //                     </body>
        //                     </html>";

        // string html_file = Path.Combine(wwwroot, "auth/pins/" + pin.pin_number + ".html");
        // File.WriteAllText(html_file, htmlCode);
        // result = SendMailFromSoap(htmlCode, email, parameters["email_address"], parameters["email_password"]).GetAwaiter().GetResult();

        // result = EmailSender.SendEmail(parameters["email_address"],
        //                                 parameters["email_password"],
        //                                 "Tokens Administration PIN",
        //                                 email,
        //                                 "",
        //                                 "Tokens MyBusiness POS Confirmation PIN",
        //                                 htmlCode,
        //                                 parameters["smtp"].ToString().Trim(),
        //                                 int.Parse(parameters["smtp_port"].ToString().Trim()),
        //                                 false);

        // if (result.StartsWith("Error:"))
        // {
        //     return result;
        // }

        // XDocument doc = XDocument.Parse(result);
        // XNamespace ns = "http://wsCorreo.mybusinesspos.com/";
        // result = doc.Descendants(ns + "EnviaCorreoHResult").First().Value;

        // if (result == "Ok.")
        // {
        server_db.Prompt($"UPSERT INTO pins VALUES {JsonConvert.SerializeObject(pin, Formatting.Indented)}");
        // }

        return "Ok.->Pin->" + pin.Pin_number.ToString();
    }

    static async Task<string> SendMailFromSoap(string html, string email, string fromAddress, string password, string default_subject = "Tokens Administration PIN")
    {
        string fromAddressName = fromAddress;
        string fromAddressPass = password;
        string toAddress = email;        
        string toAddressName = email;
        string subject = default_subject;
        string alternate = html;

        string soapEnvelope = $@"
            <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                <soap12:Body>
                    <EnviaCorreoH xmlns=""http://wsCorreo.mybusinesspos.com/"">
                        <fromAddress>{System.Security.SecurityElement.Escape(fromAddressName)}</fromAddress>
                        <fromAddressPass>{System.Security.SecurityElement.Escape(fromAddressPass)}</fromAddressPass>
                        <fromAddressName>{System.Security.SecurityElement.Escape(fromAddressName)}</fromAddressName>                        
                        <toAddress>{System.Security.SecurityElement.Escape(toAddress)}</toAddress>
                        <toAddressName>{System.Security.SecurityElement.Escape(toAddressName)}</toAddressName>
                        <Subjet>{System.Security.SecurityElement.Escape(subject)}</Subjet>
                        <alternate>{System.Security.SecurityElement.Escape(alternate)}</alternate>                                                
                        <Trick></Trick>
                    </EnviaCorreoH>
                </soap12:Body>
            </soap12:Envelope>";

        var url = "https://wscorreoa.mybusinesspos.net/WSCorreo.asmx";
        return await PostSOAPRequestAsync(url, soapEnvelope);

    }


    private static async Task<string> PostSOAPRequestAsync(string url, string text)
    {
        var httpClient = new HttpClient();

        using HttpContent content = new StringContent(text, Encoding.UTF8, "text/xml");
        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.Add("SOAPAction", "http://wsCorreo.mybusinesspos.com/EnviaCorreoH");
        request.Content = content;
        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode(); // throws an Exception if 404, 500, etc.
        return await response.Content.ReadAsStringAsync();
    }


    public static class RandomNumberGenerator
    {
        private static readonly Random random = new();

        public static string GenerateRandomNumber(int digitCount)
        {
            int randomNumber = random.Next((int)Math.Pow(10, digitCount));
            return randomNumber.ToString().PadLeft(digitCount, '0');
        }
    }


    public static bool HasExpired(string expiryTime)
    {
        // Parsea las fechas dadas usando un formato de fecha y hora específico
        DateTime parsedNow = DateTime.Now.ToUniversalTime();
        DateTime parsedExpiryTime = DateTime.ParseExact(expiryTime, "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
        // Si la fecha de expiración calculada es anterior o igual a la fecha/hora de expiración dada, retorna verdadero
        return parsedNow > parsedExpiryTime;
    }

    public static string ConvertToDateTimeWithMaxTime(string inputDate)
    {
        // Parse the date
        if (DateTime.TryParseExact(inputDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            // Set the time to 23:59:59.000000
            DateTime maxTimeDate = parsedDate.Date.Add(new TimeSpan(23, 59, 59)).AddTicks(9999999); // 10 million ticks per second - 1 tick = 1 second
            return maxTimeDate.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        }
        else
        {
            return $"Error: Invalid date format. {inputDate}";
        }
    }

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
        Font font = new("Arial", 50, FontStyle.Regular);

        // Crea un bitmap en base al texto y la fuente
        SizeF textSize;
        using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
        {
            textSize = graphics.MeasureString(text, font);
        }

        // Crea una nueva imagen del tamaño necesario para el texto
        using Bitmap image = new((int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));
        using (Graphics graphics = Graphics.FromImage(image))
        {
            // Define el color de fondo y el color del texto
            graphics.Clear(Color.White);
            using Brush brush = new SolidBrush(Color.Black);
            graphics.DrawString(text, font, brush, 0, 0);
        }

        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        // Guarda la imagen a un archivo
        image.Save(filename, ImageFormat.Png);

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

            NetworkCredential credentials = new(fromAddress.Address, fromPassword);

            if (useDefaultCredentials)
            {
                credentials = CredentialCache.DefaultNetworkCredentials;
            }

            var smtp = new SmtpClient
            {
                Host = host, // especifica el servidor SMTP aquí
                Port = port, // especifica el puerto SMTP aquí
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = credentials
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            if (!string.IsNullOrWhiteSpace(cc))
            {
                message.CC.Add(cc);
            }

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            smtp.Send(message);
            ServicePointManager.ServerCertificateValidationCallback = null;

            return "Ok.";

        }
        catch (System.Exception e)
        {
            return "Error: " + e.Message;
        }

    }

}



