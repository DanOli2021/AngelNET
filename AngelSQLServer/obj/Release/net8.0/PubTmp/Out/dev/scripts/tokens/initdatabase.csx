// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

#load "tokens.csx"
#load "users.csx"
#load "usersgroup.csx"
#load "branch_stores.csx" 
#load "pins.csx"

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

Console.WriteLine("Creating tables...");

string result = "";
Console.WriteLine("Creating tokens catalog...");
Tokens tokens = new();
result = db.CreateTable(tokens);
Console.WriteLine(result);

Console.WriteLine("Creating Users catalog...");
Users users = new();
result = db.CreateTable(users);
Console.WriteLine(result);

Console.WriteLine("Creating User Groups catalog...");
UsersGroup usersgroup = new();
result = db.CreateTable(usersgroup);
Console.WriteLine(result);

Console.WriteLine("Creating BranchStore catalog...");
Branch_stores branch_store = new();
result = db.CreateTable(branch_store);
Console.WriteLine(result);

Console.WriteLine("Creating Pins catalog...");
Pin pin = new();
result = db.CreateTable(pin, "pins");
Console.WriteLine(result);

UsersGroup g = new();

g.id = "AUTHORIZERS";
g.Name = "AUTHORIZERS";
g.Permissions = "";

result = db.Prompt("UPSERT INTO usersgroup VALUES " + JsonConvert.SerializeObject(g));
Console.WriteLine("Upsert group: " + result); 

g.id = "PINSCONSUMER";
g.Name = "PINSCONSUMER";
g.Permissions = "";

result = db.Prompt("UPSERT INTO usersgroup VALUES " + JsonConvert.SerializeObject(g));
Console.WriteLine("Upsert group: " + result); 

g.id = "SUPERVISORS";
g.Name = "SUPERVISORS";
g.Permissions = "";

result = db.Prompt("UPSERT INTO usersgroup VALUES " + JsonConvert.SerializeObject(g));
Console.WriteLine("Upsert group: " + result);

g.id = "CASHIER";
g.Name = "CASHIER";
g.Permissions = "";

result = db.Prompt("UPSERT INTO usersgroup VALUES " + JsonConvert.SerializeObject(g));
Console.WriteLine("Upsert group: " + result);

g.id = "ADMINISTRATIVE";
g.Name = " ADMINISTRATIVE";
g.Permissions = "";

result = db.Prompt("UPSERT INTO usersgroup VALUES " + JsonConvert.SerializeObject(g));
Console.WriteLine("Upsert group: " + result);

Users u = new();
u.Id = "authuser";
u.Name = "authuser";
u.Password = "mysecret";
u.UserGroups = "AUTHORIZERS, SUPERVISORS, PINSCONSUMER";
u.Organization = "AUTHORIZERS";
u.Email = "";
u.Phone = "";
u.Permissions_list = "Delete Sale, Inventory Transfer, Delete Item";

result = db.Prompt("UPSERT INTO users VALUES " + JsonConvert.SerializeObject(u));
Console.WriteLine("Upsert users: " + result); 

Tokens t = new();
t.Id = "5c242c01-39f4-43bf-8f63-bf4b19dbe8e3";
t.User = "authuser";
t.ExpiryTime = "2050-12-31 23:59:59.9999999";
result = db.Prompt("UPSERT INTO tokens VALUES " + JsonConvert.SerializeObject(t));
Console.WriteLine("Upsert tokens: " + result);



