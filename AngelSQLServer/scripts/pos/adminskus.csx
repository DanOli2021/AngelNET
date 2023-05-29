// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Script for managing skus (product codes)
// Daniel() Oliver Rojas
// 2023-05-19

#load "skus.csx"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

AdminSkus adminAuth = new AdminSkus();

OperationTypeClass operation_type = JsonConvert.DeserializeObject<OperationTypeClass>(message);

switch (operation_type.OperationType)
{
    
    case "UpsertSku":
    default:
        return "Error: No service found";
}


public class OperationTypeClass 
{
    public string OperationType;
}



public class AdminSkus
{
}


