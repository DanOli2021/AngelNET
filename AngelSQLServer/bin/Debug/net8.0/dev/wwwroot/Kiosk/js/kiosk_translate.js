const app_name = "MyBusiness POS Kiosk";

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
        document.getElementById("index_privacy_policy").innerHTML = "Política de privacidad";
        document.getElementById("index_about_us").innerHTML = "Acerca de nosotros...";
        document.getElementById("index_privacy_policy_url").href = "privacy_es.html";
        document.getElementById("index_about_url").href = "about_es.html";
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
        document.getElementById("index_privacy_policy").innerHTML = "Política de privacidad";
        document.getElementById("index_about_us").innerHTML = "Acerca de nosotros...";
        document.getElementById("index_privacy_policy_url").href = "privacy_es.html";
        document.getElementById("index_about_url").href = "about_es.html";
    }        

}


function translate_topics( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
    }        

}


function translate_element( language, value ) 
{
    const spanish_dictionary = { 
        "Manage my collaborations": "Administrar mis colaboraciones"
    };

    if( language == "es") 
    {
        if( spanish_dictionary[value] != null ) 
        {
            return spanish_dictionary[value];
        } else 
        {
            return value;
        }
    } 

    return value;

}

