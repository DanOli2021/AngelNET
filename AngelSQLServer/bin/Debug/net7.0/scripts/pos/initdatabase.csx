// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#load "skus.csx"
#load "tokens.csx"
#load "users.csx"
#load "usersgroup.csx"

#r "DB.dll"
#r "Newtonsoft.Json.dll"

using System;

Console.WriteLine("Creating tables...");

string result = "";

Console.WriteLine("Creating skus catalog...");
Skus_Catalog sku = new Skus_Catalog();
result = db.CreateTable(sku);
Console.WriteLine(result);

Console.WriteLine("Creating Componens catalog...");
Components component = new Components();
result = db.CreateTable(component);
Console.WriteLine(result);

Console.WriteLine("Creating Clasificacions catalog...");
Clasificacions clasificacion = new Clasificacions();
result = db.CreateTable(clasificacion);
Console.WriteLine(result);

Console.WriteLine("Creating Makers catalog...");
Makers maker = new Makers();
result = db.CreateTable(maker);
Console.WriteLine(result);

Console.WriteLine("Creating Sku_Dictionaries catalog...");
Sku_Dictionary sku_dictionary = new Sku_Dictionary();
result = db.CreateTable(sku_dictionary);
Console.WriteLine(result);

Console.WriteLine("Creating Locations catalog...");
Locations locations = new Locations();
result = db.CreateTable(locations);
Console.WriteLine(result);

Console.WriteLine("Creating Price codes catalog...");
Price_Codes price = new Price_Codes();
result = db.CreateTable(price);
Console.WriteLine(result);

Console.WriteLine("Creating Currencys catalog...");
Currencys currency = new Currencys();
result = db.CreateTable(currency);
Console.WriteLine(result);

Console.WriteLine("Creating Tockens catalog...");
Tokens tokens = new Tokens();
result = db.CreateTable(tokens);
Console.WriteLine(result);

Console.WriteLine("Creating Users catalog...");
Users users = new Users();
result = db.CreateTable(users);
Console.WriteLine(result);

Console.WriteLine("Creating Users catalog...");
UsersGroup usersgroup  = new UsersGroup();
result = db.CreateTable(usersgroup);
Console.WriteLine(result);

