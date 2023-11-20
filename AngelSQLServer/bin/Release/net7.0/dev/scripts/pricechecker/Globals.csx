#r "C:\AngelSQLNet\AngelSQL\db.dll"
#r "C:\AngelSQLNet\AngelSQL\Newtonsoft.Json.dll"
#r "C:\AngelSQLNet\AngelSQL\System.Drawing.Common.dll"
#r "C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\7.0.5\Microsoft.AspNetCore.Components.dll"
#r "C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\7.0.5\Microsoft.AspNetCore.Components.Web.dll"

AngelDB.DB db = new();
AngelDB.DB server_db = new();

string data = "";
string message = "";