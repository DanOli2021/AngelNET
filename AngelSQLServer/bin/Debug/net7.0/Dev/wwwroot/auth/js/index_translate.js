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
        document.getElementById("supervisor_log_out").innerHTML = "Salir";
        document.getElementById("supervisor_menu").innerHTML = "Supervisor";
    }
}


function translate_buttons( language, value ) 
{
    const spanish_dictionary = { 
        "Generate Pins": "Pins Generados",
        "Branch Stores": "Sucursales",
        "My authorizations": "Mis autorizaciones",
        "User Groups": "Grupos de Usuarios",
        "System Users": "Usuarios del Sistema",
        "Admin Access Tokens": "Tokens de Acceso"
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

