using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class Translations
{
    private readonly Dictionary<string, string> spanish = new();

    public string SpanishValues()
    {
        spanish.Add("User is null", "El usuario es nulo");
        spanish.Add("ExpiryTime is null", "El tiempo de expiración es nulo");
        spanish.Add("id (Token) is null", "El id del token es nulo");
        spanish.Add("User not found", "Usuario no encontrado");
        spanish.Add("Token Id is null", "El id del token es nulo");
        spanish.Add("TokenToDelete is null", "TokenToDelete es nulo");
        spanish.Add("Token not found", "Token no encontrado");
        spanish.Add("TokenToObtainPermission is null", "TokenToObtainPermission es nulo");
        spanish.Add("TokenToGetTheUser is null", "TokenToGetTheUser es nulo");
        spanish.Add("Password is null", "Password es nulo");
        spanish.Add("Invalid password", "Contraseña inválida");
        spanish.Add("Token expired", "Token expirado");
        spanish.Add("UserGroups is null", "UserGroups es nulo");
        spanish.Add("Name is null", "Name es nulo");
        spanish.Add("Organization is null", "Organization es nulo");
        spanish.Add("Email is null", "Email es nulo");
        spanish.Add("Phone is null", "Phone es nulo");
        spanish.Add("permissions_list is null", "permissions_list es nulo");
        spanish.Add("Password is null or empty", "Password es nulo o vacío");
        spanish.Add("Auth No user group found", "Auth No se encontró el grupo de usuario");
        spanish.Add("User created successfully", "Usuario creado exitosamente");
        spanish.Add("Users not found", "Usuarios no encontrados");
        spanish.Add("UserToDelete is null", "UserToDelete es nulo");
        spanish.Add("User deleted successfully", "Usuario eliminado exitosamente");
        spanish.Add("UserGroup is null", "UserGroup es nulo");        
        spanish.Add("Name is null", "Name es nulo");
        spanish.Add("Permissions is null", "Permissions es nulo");
        spanish.Add("Users Group created successfully", "Grupo de usuarios creado exitosamente");
        spanish.Add("UserGroupToDelete is null", "UserGroupToDelete es nulo");
        spanish.Add("is a system group", "es un grupo de sistema");
        spanish.Add("The group indicated by you does not exist on this system", "El grupo indicado por usted no existe en este sistema");
        spanish.Add("User Group deleted successfully", "Grupo de usuarios eliminado exitosamente");
        spanish.Add("id is null", "id es nulo");
        spanish.Add("Name is null", "Name es nulo");
        spanish.Add("Address is null", "Address es nulo");
        spanish.Add("Phone is null", "Phone es nulo");
        spanish.Add("No user found", "No se encontró el usuario");
        spanish.Add("BranchStoreId is null", "BranchStoreId es nulo");
        spanish.Add("No branch store found", "No se encontró la sucursal");
        spanish.Add("BranchStoreToDelete is null", "BranchStoreToDelete es nulo");
        spanish.Add("Branch Store deleted successfully", "Sucursal eliminada exitosamente");
        spanish.Add( "Branchstore_id is null", "Branchstore_id es nulo" );
        spanish.Add( "Permission_id is null", "Permission_id es nulo" );
        return "Ok.";
    }

    public string Get(string key, string language)
    {        

        if( language == "" )
        {
            return key;
        }

        if( language == "es" ) 
        {
            if( !spanish.ContainsKey(key) ) 
            {
                return key;
            } 
            else 
            {
                return spanish[key];
            }
        }

        return key;
    }
}