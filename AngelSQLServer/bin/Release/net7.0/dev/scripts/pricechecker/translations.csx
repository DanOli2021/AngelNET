using System;
using System.Collections.Generic;

public class Translations
{
    private readonly Dictionary<string, string> spanish = new Dictionary<string, string>();

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
        spanish.Add("Permissions is null", "Permissions es nulo");
        spanish.Add("Users Group created successfully", "Grupo de usuarios creado exitosamente");
        spanish.Add("UserGroupToDelete is null", "UserGroupToDelete es nulo");
        spanish.Add("is a system group", "es un grupo de sistema");
        spanish.Add("The group indicated by you does not exist on this system", "El grupo indicado por usted no existe en este sistema");
        spanish.Add("User Group deleted successfully", "Grupo de usuarios eliminado exitosamente");
        spanish.Add("id is null", "id es nulo");
        spanish.Add("Address is null", "Address es nulo");
        spanish.Add("No user found", "No se encontró el usuario");
        spanish.Add("BranchStoreId is null", "BranchStoreId es nulo");
        spanish.Add("No branch store found", "No se encontró la sucursal");
        spanish.Add("BranchStoreToDelete is null", "BranchStoreToDelete es nulo");
        spanish.Add("Branch Store deleted successfully", "Sucursal eliminada exitosamente");
        spanish.Add("Branchstore_id is null", "Branchstore_id es nulo" );
        spanish.Add("Permission_id is null", "Permission_id es nulo" );
        spanish.Add("Branch Store not found", "Sucursal no encontrada" );
        spanish.Add("InitialDate is null", "InitialDate es nulo" );
        spanish.Add("FinalDate is null", "FinalDate es nulo" );
        spanish.Add("Pin is null", "Pin es nulo");
        spanish.Add("Permission is null", "Permission es nulo");
        spanish.Add("BranchStore is null", "BranchStore es nulo");
        spanish.Add("AppUser is null", "AppUser es nulo");
        spanish.Add("AppUserName is null", "AppUserName es nulo");
        spanish.Add("Pin not found", "Pin no encontrado");
        spanish.Add("This pin is not for the user who is trying to confirm it", "Este pin no es para el usuario que está intentando confirmarlo");
        spanish.Add("Pin expired", "Pin expirado");
        spanish.Add("Email is required", "Email es requerido");
        spanish.Add("Email is not valid", "Email no es válido");
        spanish.Add("Email not found", "Email no encontrado");
        spanish.Add("Email already exists in another account", "Email ya existe en otra cuenta");
        spanish.Add("wwwroot directory not found", "No se encontró el directorio wwwroot");
        spanish.Add("Creating table pins", "Creando tabla pins");
        spanish.Add("Id is required", "Id es requerido");
        spanish.Add("Topic is required", "Tema es requerido");
        spanish.Add("Description is required", "Descripción es requerida");
        spanish.Add("Topic_id is required", "Topic_id es requerido");
        spanish.Add("Subtopic is required", "Subtema es requerido");
        spanish.Add("Topic_id does not exist", "Topic_id no existe");
        spanish.Add("Subtopic_id is required", "Subtopic_id es requerido");
        spanish.Add("Content_title is required", "Content_title es requerido");
        spanish.Add("Status is required", "Status es requerido");
        spanish.Add("Version is required", "Version es requerido");
        spanish.Add("IsPublic is required", "IsPublic es requerido");
        spanish.Add("Content_id is required", "Content_id es requerido");
        spanish.Add("Content_type is required", "Content_type es requerido");
        spanish.Add("Content_order is required", "Content_order es requerido");
        spanish.Add("Content is required", "Content es requerido");
        spanish.Add("Subtopic_id does not exist", "Subtopic_id no existe");
        spanish.Add("You first need to delete the content details and content header in order to delete this item", "Primero debe eliminar los detalles de contenido y el encabezado de contenido para eliminar este elemento");
        spanish.Add("No content found for Content_id:", "No se encontró contenido para Content_id:");
        spanish.Add("No Subtopic found for Subtopic_id:", "No se encontró subtema para Subtopic_id:");
        spanish.Add("No Topic found for Topic_id:", "No se encontró tema para Topic_id:");
        spanish.Add("Search is required", "Search es requerido");
        spanish.Add("No groups found", "No se encontraron grupos");
        spanish.Add("User does not have permission to edit", "El usuario no tiene permiso para editar");   
        spanish.Add("File is required", "File es requerido");
        spanish.Add("You first need to delete the subtopics and content header in order to delete this item", "Primero debe eliminar los subtemas y el encabezado de contenido para eliminar este elemento");
        return "Ok.";
    }

    public string Get(string key, string language)
    {        

        if( language == "" || language == "en" || language == null )
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