//AngelSQLServer

using AngelDB;
using AngelSQL;
using AngelSQLServer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.WindowsServices;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

string commandLine = string.Join(" ", args);
string api_file = Environment.CurrentDirectory + "/config/AngelAPI.csx";
string config_file = Environment.CurrentDirectory + "/config/AngelSQL.csx";

if (!string.IsNullOrEmpty(commandLine))
{
    DbLanguage language = new DbLanguage();
    language.SetCommands(AngelSQL.AngelSQLCommands.DbCommands());
    Dictionary<string, string> d = language.Interpreter(commandLine.Trim());

    if (d is null)
    {
        LogFile.Log(language.errorString);
    }
    else
    {
        if (d.First().Key == "start_parameters")
        {
            if (d["api_file"] != "null") 
            {
                api_file = d["api_file"];
            }

            if (d["config_file"] != "null")
            {
                config_file = d["config_file"];
            }

            if (!File.Exists(api_file)) 
            {
                LogFile.Log($"Error: api file {api_file} does not exists");
                Environment.Exit(0);
                return;
            }

            if (!File.Exists(config_file))
            {
                LogFile.Log($"Error: config file {config_file} does not exists");
                Environment.Exit(0);
                return;
            }

        }
    }

}

// Using dot as decimal separator
NumberFormatInfo nfi = new NumberFormatInfo();
nfi.NumberDecimalSeparator = ".";
CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

//if is a Windows service, set the current directory to the same as the executable
if (WindowsServiceHelpers.IsWindowsService())
{
    Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
}

// Create a bulder for the web app
var builder = WebApplication.CreateBuilder(args);
// End Create a builder for the web app


//if is a Windows service, set the current directory to the same as the executable
builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) : default
});

if (WindowsServiceHelpers.IsWindowsService())
{
    builder.Host.UseWindowsService();
}


// AngelSQLServer parameters
Dictionary<string, string> parameters = new Dictionary<string, string>();
parameters.Add("certificate", "");
parameters.Add("password", "");
parameters.Add("bind_ip", "");
parameters.Add("bind_port", "12000");
parameters.Add("urls", "");
parameters.Add("cors", "https://localhost:11000");
parameters.Add("master_user", "db");
parameters.Add("master_password", "db");
parameters.Add("data_directory", "null");
parameters.Add("account", "account1");
parameters.Add("account_user", "angelsql");
parameters.Add("account_password", "angelsql123");
parameters.Add("database", "database1");
parameters.Add("request_timeout", "4");


//Our AngelDB database 
AngelDB.DB main_db = new AngelDB.DB();

builder.Services.AddSingleton<AngelDB.DB>(main_db);

string result = main_db.Prompt($"SCRIPT FILE {config_file}");

if (!result.StartsWith("Error:"))
{
    parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
}
else
{
    LogFile.Log(result);
}

main_db.Prompt($"DB USER {parameters["master_user"]} PASSWORD {parameters["master_password"]} DATA DIRECTORY {parameters["data_directory"]}", true);
main_db.Prompt($"CREATE ACCOUNT {parameters["account"]} SUPERUSER {parameters["account_user"]} PASSWORD {parameters["account_password"]}", true);
main_db.Prompt($"USE ACCOUNT {parameters["account"]}", true);
main_db.Prompt($"CREATE DATABASE {parameters["database"]}", true);
main_db.Prompt($"USE DATABASE {parameters["database"]}", true);

// Add cors support
if (parameters["cors"] == "true")
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
                       builder =>
                       {
                           builder.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();
                       });
    });
}


builder.WebHost.ConfigureKestrel(options =>
{

    // Establece tu tiempo límite deseado aquí (en milisegundos)

    if (!parameters.ContainsKey("request_timeout"))
    {
        parameters.Add("request_timeout", "10");
    }

    int request_timeout = 10;
    int.TryParse(parameters["request_timeout"], out request_timeout);
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);

    // Add pfx certificate
    if (!string.IsNullOrEmpty(parameters["certificate"]))
    {
        try
        {
            if (!string.IsNullOrEmpty(parameters["bind_port"]))
            {
                parameters["bind_port"] = "443";
            }

            if (!string.IsNullOrEmpty(parameters["bind_ip"]))
            {
                options.Listen(System.Net.IPAddress.Parse(parameters["bind_ip"]), int.Parse(parameters["bind_port"]), listenOptions =>
                {
                    listenOptions.UseHttps(parameters["certificate"], parameters["password"]);
                });
            }
            else
            {

                if (string.IsNullOrEmpty(parameters["bind_ip"]))
                {
                    parameters["bind_ip"] = "443";
                }

                options.Listen(System.Net.IPAddress.Any, int.Parse(parameters["bind_port"]), listenOptions =>
                {
                    listenOptions.UseHttps(parameters["certificate"], parameters["password"]);
                });

            }
        }
        catch (Exception e)
        {
            LogFile.Log($"Error: {e}");
        }
    }

});


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Add cors support
app.UseCors(options => options.SetIsOriginAllowed(x => _ = true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();

//Using html static pages
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");

app.UseDefaultFiles(defaultFilesOptions);

string wwww_directory = Path.Combine(Environment.CurrentDirectory, "wwwroot");

if (parameters.ContainsKey("wwwroot"))
{
    if (!string.IsNullOrEmpty(parameters["wwwroot"])) 
    {
        wwww_directory = parameters["wwwroot"];
    }
}

if(!Directory.Exists(wwww_directory))
{
    Directory.CreateDirectory(wwww_directory);

    string index_html = Path.Combine(wwww_directory, "index.html");
    string content = $"<html><head><title>AngelSQLServer</title></head><body><h1>AngelSQLServer</h1><p>AngelSQLServer is running</p><p>{index_html}</p></body></html>";
    File.WriteAllText(index_html, content);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        wwww_directory),
    RequestPath = ""
});

Dictionary<string, AngelSQL.DBConnections> connections = new Dictionary<string, AngelSQL.DBConnections>();

string Identification(AngelSQL.Query query)
{

    AngelDB.DB db_local = new AngelDB.DB();
    db_local.NewDatabases = false;

    if (string.IsNullOrEmpty(query.data_directory))
    {
        query.data_directory = parameters["data_directory"];
    }

    string result = db_local.Prompt($"DB USER {query.User} PASSWORD {query.password} ACCOUNT {query.account} DATABASE {query.database} DATA DIRECTORY {query.data_directory}");

    AngelSQL.Responce responce = new AngelSQL.Responce();

    if (result.StartsWith("Error:"))
    {
        responce.tocken = "";
        responce.result = result;
        responce.type = "ERROR";
        return JsonConvert.SerializeObject(responce);
    }

    AngelSQL.DBConnections connection = new AngelSQL.DBConnections();
    connection.CreationDate = DateTime.Now;
    connection.expiration_days = 30;
    connection.date_of_last_access = DateTime.Now;
    connection.User = query.User;
    connection.db = db_local;

    string tocken = Guid.NewGuid().ToString();

    connections.Add(tocken, connection);

    responce.tocken = tocken;
    responce.result = result;
    responce.type = "SUCCESS";
    return JsonConvert.SerializeObject(responce);

}


string QueryResponce(AngelSQL.Query query)
{

    AngelSQL.Responce responce = new AngelSQL.Responce();

    if (!connections.ContainsKey(query.tocken))
    {
        responce.tocken = "";
        responce.result = "Error: You need to identify yourself first.";
        responce.type = "ERROR";
        return JsonConvert.SerializeObject(responce);
    }

    responce.tocken = query.tocken;
    connections[query.tocken].date_of_last_access = DateTime.Now;

    if (query.type == "SERVERCOMMAND")
    {
        if (connections[query.tocken].db.Prompt("MY LEVEL") == "MASTER")
        {

            DbLanguage language = new DbLanguage();
            language.SetCommands(AngelSQL.AngelClientsCommands.DbCommands());
            Dictionary<string, string> d = language.Interpreter(query.command.Trim());

            if (d is null)
            {
                responce.result = language.errorString;
                return JsonConvert.SerializeObject(responce);
            }

            Dictionary<string, AngelDB.DB> local = new Dictionary<string, AngelDB.DB>();

            foreach (string key in connections.Keys)
            {
                if (connections[key].db.BaseDirectory == connections[query.tocken].db.BaseDirectory)
                {
                    local.Add(key, connections[key].db);
                }

            }

            switch (d.First().Key)
            {
                case "clients":

                    responce.result = JsonConvert.SerializeObject(connections, Formatting.Indented);
                    return JsonConvert.SerializeObject(responce);

                case "count_clients":

                    responce.result = connections.Count.ToString();
                    return JsonConvert.SerializeObject(responce);

                case "kill_client":

                    if (connections.ContainsKey(d["kill_client"]))
                    {
                        connections.Remove(d["kill_client"]);
                        responce.result = "Ok.";
                    }
                    else
                    {
                        responce.result = "Error: Client not found.";
                    }

                    return JsonConvert.SerializeObject(responce);

                default:
                    break;
            }

        }

    }

    responce.result = connections[query.tocken].db.Prompt(query.command);

    if (responce.result.StartsWith("Error:"))
    {
        responce.type = "ERROR";
    }
    else
    {
        responce.type = "SUCCESS";
    }

    return JsonConvert.SerializeObject(responce);

}


//Mapping the AngelSQL API
AngelDB.DB angel_api_db = new AngelDB.DB();

app.MapGet("/AngelAPI", (string data) =>
{
    try
    {
        AngelSQL.ApiClass api = JsonConvert.DeserializeObject<AngelSQL.ApiClass>(data);
        string ApiCommand;

        if (parameters.ContainsKey("angel_api"))
        {
            ApiCommand = parameters["angel_api"];
        }
        else
        {
            ApiCommand = api_file;
        }

        ApiCommand = ApiCommand.Trim();

        if (ApiCommand.EndsWith(".cxs")) 
        {
            return angel_api_db.Prompt($"SCRIPT FILE {ApiCommand} MESSAGE " + data);
        }

        if (ApiCommand.EndsWith(".py"))
        {
            return angel_api_db.Prompt($"PY FILE {ApiCommand} MESSAGE " + data);
        }

        return "Error: Invalid API file.";

    }
    catch (Exception e)
    {
        return "Error: " + e.Message;
    }
});
// End of mapping the AngelSQL API


//Mapping the AngelPOS API (POST)

AngelDB.DB angel_post_db = new AngelDB.DB();
angel_post_db.Prompt($"DB USER {parameters["master_user"]} PASSWORD {parameters["master_password"]} DATA DIRECTORY {parameters["data_directory"]}", true);
angel_post_db.Prompt($"CREATE ACCOUNT {parameters["account"]} SUPERUSER {parameters["account_user"]} PASSWORD {parameters["account_password"]}", true);
angel_post_db.Prompt($"USE ACCOUNT {parameters["account"]}", true);
angel_post_db.Prompt($"CREATE DATABASE {parameters["database"]}", true);
angel_post_db.Prompt($"USE DATABASE {parameters["database"]}", true);

app.MapPost("/AngelPOST", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
        try
        {
            string jsonstring = await reader.ReadToEndAsync();
            AngelSQL.AngelPOST api = JsonConvert.DeserializeObject<AngelSQL.AngelPOST>(jsonstring);

            string scripts_directory = Path.Combine( Environment.CurrentDirectory, "scripts" );

            if( parameters.ContainsKey("scripts_directory"))
            {
                if(!string.IsNullOrEmpty(parameters["scripts_directory"]))
                {
                    scripts_directory = parameters["scripts_directory"];
                }
            }

            if (string.IsNullOrEmpty(api.language))
            {
                result = angel_post_db.Prompt($"SCRIPT FILE {scripts_directory}/{api.api}.csx MESSAGE " + api.message, true);
            }
            else
            {
                switch (api.language)
                {
                    case "C#":
                        result = angel_post_db.Prompt($"SCRIPT FILE {scripts_directory}/{api.api}.csx MESSAGE " + api.message, true);
                        break;
                    case "CSHARP":
                        result = angel_post_db.Prompt($"SCRIPT FILE {scripts_directory}/{api.api}.csx MESSAGE " + api.message, true);
                        break;
                    case "null":
                        result = angel_post_db.Prompt($"SCRIPT FILE {scripts_directory}/{api.api}.csx MESSAGE " + api.message, true);
                        break;
                    case "PYTHON":
                        result = angel_post_db.Prompt($"PY FILE {scripts_directory}/{api.api}.py MESSAGE " + api.message, true);
                        break;
                    default:
                        break;
                }
            }

            AngelSQL.Responce responce = new AngelSQL.Responce
            {
                type = "SUCCESS",
                result = result
            };

            return JsonConvert.SerializeObject(responce);

        }
        catch (global::System.Exception e)
        {
            AngelSQL.Responce responce = new AngelSQL.Responce();
            responce.type = "ERROR";
            responce.result = $"Error: {e}";
            return JsonConvert.SerializeObject(responce);
        }
    }
});

// End of mapping AngelPOST communications protocol


//Mapping AngelSQL communications protocol
app.MapPost("/AngelSQL", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
        try
        {
            string jsonstring = await reader.ReadToEndAsync();
            AngelSQL.Query query = JsonConvert.DeserializeObject<AngelSQL.Query>(jsonstring);

            switch (query.type.Trim().ToLower())
            {
                case "identification":

                    return Identification(query);

                case "query":

                    return QueryResponce(query);

                case "disconnect":

                    if (connections.ContainsKey(query.tocken))
                    {
                        connections.Remove(query.tocken);
                    }

                    AngelSQL.Responce responce = new AngelSQL.Responce();
                    responce.tocken = "";
                    responce.result = "Disconnected";
                    responce.type = "SUCCESS";
                    return JsonConvert.SerializeObject(responce);

                case "servercommand":

                    return QueryResponce(query);

                default:
                    break;
            }

            return jsonstring;
        }
        catch (global::System.Exception e)
        {
            AngelSQL.Responce responce = new AngelSQL.Responce();
            responce.type = "ERROR";
            responce.result = $"Error: {e}";
            return JsonConvert.SerializeObject(responce); ;
        }
    }
});

//End of mapping AngelSQL communications protocol


// Adding urls to the server
if (!string.IsNullOrEmpty(parameters["urls"]))
{
    string[] urls = parameters["urls"].Split(",");

    foreach (string url in urls)
    {
        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                app.Urls.Add(url);
            }
            catch (Exception e)
            {
                LogFile.Log($"Error: Add urls {e}");
            }
        }
    }
}


// Remove garbage
int garbage_cicle = 0;

#pragma warning disable CS4014 // Dado que no se esperaba esta llamada, la ejecución del método actual continuará antes de que se complete la llamada
Task.Run(() =>
{
    try
    {
        if (!parameters.ContainsKey("connection_timeout"))
        {
            parameters.Add("connection_timeout", "12");
        }


        while (true)
        {

            GC.Collect();

            ++garbage_cicle;

            if (garbage_cicle == 120)
            {
                foreach (string item in connections.Keys)
                {
                    if ((DateTime.Now - connections[item].date_of_last_access).Hours > decimal.Parse(parameters["connection_timeout"]))
                    {
                        connections.Remove(item);
                    }
                }

                garbage_cicle = 0;

            }

            Thread.Sleep(5000);
        }

    }
    catch (Exception e)
    {
        LogFile.Log("Error: " + e);
    }

});


Task.Run(() =>
{

    try
    {
        if (!parameters.ContainsKey("service_command"))
        {
            parameters.Add("service_command", Environment.CurrentDirectory + "/config/tasks.csx");
        }

        if (!parameters.ContainsKey("service_delay"))
        {
            parameters.Add("service_delay", "300000");
        }


        while (true)
        {
            int delay = 300000;
            string local_result = main_db.Prompt($"SCRIPT FILE {parameters["service_command"]}");

            if (result.StartsWith("Error:"))
            {
                LogFile.Log(result);
            }

            if (int.TryParse(parameters["service_delay"], out delay))
            {
                Thread.Sleep(delay);
            }
            else
            {
                Thread.Sleep(delay);
                Thread.Sleep(300000);
            }

        }
    }
    catch (Exception e)
    {
        LogFile.Log("Error: " + e);
    }


});

// End of running Tasks script


// Our Header
void PutHeader()
{
    //Console.Clear();
    AngelDB.Monitor.ShowLine("===================================================================", ConsoleColor.Magenta);
    AngelDB.Monitor.ShowLine(" =>  DataBase software, powerful and simple at the same time", ConsoleColor.Magenta);
    AngelDB.Monitor.ShowLine(" =>  We explain it to you in 20 words or fewer:", ConsoleColor.Magenta);
    AngelDB.Monitor.ShowLine(" =>  AngelSQL", ConsoleColor.Green);
    AngelDB.Monitor.ShowLine("===================================================================", ConsoleColor.Magenta);

}


// Agrega el middleware personalizado para capturar solicitudes a '/swagger/index.html'.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        // Responde con el código de estado HTTP 404 (No encontrado)
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        // Opcional: Redirige a otra página, por ejemplo, la página principal de tu aplicación
        context.Response.Redirect("/");
    }
    else
    {
        await next();
    }
});



// Running the server, as a service or as a console application
if (WindowsServiceHelpers.IsWindowsService())
{
    app.Run();
}
else
{
    app.RunAsync();

    PutHeader();

    for (; ; )
    {
        // All operations are done here
        string line;
        string prompt = "";

        if (main_db.always_use_AngelSQL == true)
        {
            prompt = main_db.angel_url + ">" + main_db.angel_user;
        }
        else
        {
            if (main_db.IsLogged)
            {
                prompt = new DirectoryInfo(main_db.BaseDirectory + "/").Name + ">" + main_db.account + ">" + main_db.database + ">" + main_db.user;
            }
        }

        line = AngelDB.Monitor.Prompt(prompt + " $> ");

        if (string.IsNullOrEmpty(line))
        {
            continue;
        }

        if (line.Trim().ToUpper() == "QUIT")
        {
            Environment.Exit(0);
            return;
        }

        if (line.Trim().ToUpper() == "CLEAR")
        {
            Console.Clear();
            PutHeader();
            continue;
        }

        if (line.Trim().ToUpper() == "LISTEN ON")
        {
            try
            {
                foreach (string item in app.Urls)
                {
                    Console.WriteLine(item);
                }
            }
            catch (Exception e)
            {
                AngelDB.Monitor.ShowError($"Error: {e.ToString()}");
            }
            continue;
        }

        string result_db = main_db.Prompt(line);

        if (result_db.StartsWith("Error:"))
        {
            AngelDB.Monitor.ShowError(result_db);
        }
        else
        {
            AngelDB.Monitor.Show(result_db);
        }

    }

}