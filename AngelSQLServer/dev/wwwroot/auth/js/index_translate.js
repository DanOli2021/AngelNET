const app_name = "MyBusiness POS Control de Acceso";

function translate_login( language ) 
{

    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("auth_title").innerHTML = app_name;
        document.getElementById("auth_for_username").innerHTML = "Usuario";
        document.getElementById("auth_for_password").innerHTML = "Contraseña";
        document.getElementById("login_button").value = "Iniciar Sesión (F2)";

        if( document.getElementById("button_recover_password") != null ) 
        {
            document.getElementById("button_recover_password").value = "Recuperar Contraseña";
        }
        
        document.getElementById("button_register").value = "Registrarse";
    }        

}


function translate_sendpin( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("auth_title").innerHTML = app_name;
        document.getElementById("auth_send_pin").innerHTML = "Obtener PIN para recuperar contraseña";
        document.getElementById("auth_button_send_pin").value = "Obtener PIN";
    }        

}


function translate_menu( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("auth_title").innerHTML = app_name;
        document.getElementById("menu_log_out").innerHTML = "Cerrar Sesión";
    }        

}


function translate_pins( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("pins_menu").innerHTML = "Menú";
        document.getElementById("pins_refresh").innerHTML = "Refrescar (F4)";
        document.getElementById("pins_start_date").innerHTML = "Fecha inicial";
        document.getElementById("pins_end_date").innerHTML = "Fecha final";
        document.getElementById("pins_authorizer").innerHTML = "Authorizador";
        document.getElementById("pins_branch_store").innerHTML = "Sucursal";
        document.getElementById("pins_date").innerHTML = "Fecha";
        document.getElementById("pins_expiry_time").innerHTML = "Fecha de Expiración";
        document.getElementById("pins_confirmated_date").innerHTML = "Fecha de Confirmación";
        document.getElementById("pins_user").innerHTML = "Usuario";
        document.getElementById("pins_user_name").innerHTML = "Nombre de Usuario";
    }
}


function translate_supervisor( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("supervisor_menu").innerHTML = "Menú";
        document.getElementById("supervisor_user").innerHTML = "Usuario";
        document.getElementById("supervisor_permission").innerHTML = "Permiso";
        document.getElementById("supervisor_pin_message").innerHTML = "Mensaje de PIN";
        document.getElementById("supervisor_button_save").innerHTML = "Salvar";
        document.getElementById("supervisor_button_close").innerHTML = "Cerrar";
    }
}


function translate_branch_stores( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("branch_stores_menu").innerHTML = "Menú";
        document.getElementById("branch_stores_branch_stores").innerHTML = "Sucursales";
        document.getElementById("branch_stores_button_new").innerHTML = "Nuevo (F2)";
        document.getElementById("branch_stores_refresh").innerHTML = "Refrescar (F4)";
        document.getElementById("branch_stores_branch_store").innerHTML = "Sucursal";
        document.getElementById("branch_stores_name").innerHTML = "Nombre";
        document.getElementById("branch_stores_address").innerHTML = "Dirección";
        document.getElementById("branch_stores_phone").innerHTML = "Teléfono";
        document.getElementById("branch_stores_authorizer").innerHTML = "Authorizador";
        document.getElementById("branch_store_label_name").innerHTML = "Nombre";
        document.getElementById("branch_store_label_address").innerHTML = "Dirección";
        document.getElementById("branch_store_label_phone").innerHTML = "Teléfono";
        document.getElementById("branch_store_label_authorizer").innerHTML = "Authorizador";
        document.getElementById("branch_store_save").innerHTML = "Salvar";
        document.getElementById("branch_store_delete").innerHTML = "Eliminar";
        document.getElementById("branch_store_close").innerHTML = "Cerrar";
    }
}


function translate_users_groups( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("users_groups_menu").innerHTML = "Menú";
        document.getElementById("users_groups_user_groups").innerHTML = "Grupos de Usuarios";
        document.getElementById("users_groups_new").innerHTML = "Nuevo (F2)";
        document.getElementById("users_groups_refresh").innerHTML = "Refrescar (F4)";
        document.getElementById("users_groups_group").innerHTML = "Grupo";
        document.getElementById("users_groups_name").innerHTML = "Nombre";
        document.getElementById("users_groups_permissions").innerHTML = "Permisos";
    }
}

function translate_users( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("users_menu").innerHTML = "Menú";
        document.getElementById("users_system_users").innerHTML = "Usuarios del Sistema";
        document.getElementById("users_new").innerHTML = "Nuevo (F2)";
        document.getElementById("users_refresh").innerHTML = "Refrescar (F4)";
        document.getElementById("users_user").innerHTML = "Usuario";
        document.getElementById("users_name").innerHTML = "Nombre";
        document.getElementById("users_email").innerHTML = "Correo Electrónico";
        document.getElementById("users_phone").innerHTML = "Teléfono";
        document.getElementById("users_group").innerHTML = "Grupo";   
        
        document.getElementById("users_label_id").innerHTML = "Usuario";
        document.getElementById("users_label_name").innerHTML = "Nombre";
        document.getElementById("users_label_email").innerHTML = "Correo Electrónico";
        document.getElementById("users_label_phone").innerHTML = "Teléfono";
        document.getElementById("users_label_user_groups").innerHTML = "Grupos de Usuarios";
        document.getElementById("users_label_user_groups_list").innerHTML = "Grupos de Usuario (Separados por comas)";
        document.getElementById("users_label_organization").innerHTML = "Organización";
        document.getElementById("users_permissions_list").innerHTML = "Lista de Permisos (Separados por comas)";
        document.getElementById("users_label_password").innerHTML = "Contraseña";
        document.getElementById("users_label_password_confirmation").innerHTML = "Confirmación de Contraseña";
        document.getElementById("users_button_save").innerHTML = "Salvar";
        document.getElementById("users_button_close").innerHTML = "Cerrar";
        document.getElementById("users_button_delete").innerHTML = "Eliminar";
    }
}


function translate_tokens( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("tokens_menu").innerHTML = "Menú";
        document.getElementById("tokens_admin_tokens").innerHTML = "Tokens de Acceso";
        document.getElementById("tokens_new").innerHTML = "Nuevo (F2)";
        document.getElementById("tokens_refresh").innerHTML = "Refrescar (F4)";
        document.getElementById("tokens_token").innerHTML = "Token";
        document.getElementById("tokens_user").innerHTML = "Usuario";
        document.getElementById("tokens_date").innerHTML = "Fecha de creación";
        document.getElementById("tokens_expiry_time").innerHTML = "Fecha de expiración";
        document.getElementById("tokens_status").innerHTML = "Estatus";
        document.getElementById("tokens_used_for").innerHTML = "Usado para";
        document.getElementById("tokens_observation").innerHTML = "Observaciones";

    }
}


function translate_element( language, value ) 
{
    const spanish_dictionary = { 
        "Generated Pins": "Pins Generados",
        "Branch Stores": "Sucursales",
        "My authorizations": "Mis autorizaciones",
        "User Groups": "Grupos de Usuarios",
        "System Users": "Usuarios del Sistema",
        "Admin Access Tokens": "Tokens de Acceso",
        "Pin to user": "Pin para usuario",
        "Are you sure you want to remove the User Group?": "¿Está seguro que desea eliminar el Grupo de Usuarios?",
        "Attention": "Atención",
        "Are you sure you want to remove the Branch Store?": "¿Está seguro que desea eliminar la Sucursal",
        "Are you sure you want to remove the User?": "¿Está seguro que desea eliminar el Usuario?",
        "Are you sure you want to remove the Access Token?": "¿Está seguro que desea eliminar el Token de Acceso?",
    };

    if( language == "es") 
    {
        if( spanish_dictionary[value] != null ) 
        {
            return spanish_dictionary[value];
        }
    }

    return value;

}

