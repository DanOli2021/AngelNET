const app_name = "MyBusiness POS Help Desk";

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

        document.getElementById("help_button_search").innerHTML = "Buscar";

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
        document.getElementById("topics_menu").innerHTML = "Menú";
        document.getElementById("topics_users_new").innerHTML = "Nuevo Tema (F2)";
        document.getElementById("topics_refresh").innerHTML = "Refrescar (F4)";
        document.getElementById("topics_topic").innerHTML = "Tema";
        document.getElementById("topics_description").innerHTML = "Descripción";
        document.getElementById("topics_subtopic").innerHTML = "Subtemas";
        document.getElementById("topics_createdby").innerHTML = "Creado por";
        document.getElementById("topics_createdat").innerHTML = "Fecha de Creación";
        document.getElementById("topics_updatedby").innerHTML = "Actualizado por";
        document.getElementById("topics_updatedat").innerHTML = "Fecha de Actualización";

        document.getElementById("topics_label_topic").innerHTML = "Tema";
        document.getElementById("topics_label_description").innerHTML = "Descripción";
        document.getElementById("topics_button_save").innerHTML = "Guardar";
        document.getElementById("topics_button_delete").innerHTML = "Eliminar";
        document.getElementById("topics_button_close").innerHTML = "Cerrar";

    }        

}

function translate_topic( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("topic_menu").innerHTML = "Temas";
        document.getElementById("topic_new").innerHTML = "Nuevo Subtema (F2)";
        document.getElementById("topic_refresh").innerHTML = "Refrescar (F4)";

        document.getElementById("subtopics_subtopic").innerHTML = "Subtema";
        document.getElementById("subtopics_description").innerHTML = "Descripción";
        document.getElementById("subtopics_detail").innerHTML = "Detalle";
        document.getElementById("subtopics_createdby").innerHTML = "Creado por";
        document.getElementById("subtopics_createdat").innerHTML = "Fecha de Creación";
        document.getElementById("subtopics_updatedby").innerHTML = "Actualizado por";
        document.getElementById("subtopics_updatedat").innerHTML = "Fecha de Actualización";

        document.getElementById("subtopics_label_topic").innerHTML = "Subtema";
        document.getElementById("subtopics_label_description").innerHTML = "Descripción";
        document.getElementById("subtopics_button_save").innerHTML = "Guardar";
        document.getElementById("subtopics_button_delete").innerHTML = "Eliminar";
        document.getElementById("subtopics_button_close").innerHTML = "Cerrar";
        document.getElementById("subtopics_label_list").innerHTML = "Tema Asociado";

    }        

}


function translate_content( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("content_topics_menu").innerHTML = "Temas";
        document.getElementById("content_subtopics_menu").innerHTML = "Subtemas";
        document.getElementById("content_new").innerHTML = "Nuevo Contenido (F2)";
        document.getElementById("content_refresh").innerHTML = "Refrescar (F4)";

        document.getElementById("content_label_list").innerHTML = "Subtema Asociado";
        document.getElementById("content_content_title").innerHTML = "Título del Contenido";
        document.getElementById("content_description").innerHTML = "Descripción";
        document.getElementById("content_details").innerHTML = "Detalle";
        document.getElementById("content_version").innerHTML = "Versión";
        document.getElementById("content_is_public").innerHTML = "Es Público";

        document.getElementById("content_createdby").innerHTML = "Creado por";
        document.getElementById("content_createdat").innerHTML = "Fecha de Creación";
        document.getElementById("content_updatedby").innerHTML = "Actualizado por";
        document.getElementById("content_updatedat").innerHTML = "Fecha de Actualización";

        document.getElementById("content_label_title").innerHTML = "Subtema";
        document.getElementById("content_label_description").innerHTML = "Descripción";
        document.getElementById("content_label_version").innerHTML = "Versión";
        document.getElementById("content_label_status").innerHTML = "Estado";
        document.getElementById("content_label_is_public").innerHTML = "Es Público";

        document.getElementById("content_button_save").innerHTML = "Guardar";
        document.getElementById("content_button_delete").innerHTML = "Eliminar";
        document.getElementById("content_button_close").innerHTML = "Cerrar";

    }        

}



function translate_contentdetail( language ) 
{
    if( language == null) 
    {
        return;
    }

    if( language == "es" )
    {
        document.getElementById("contentdetail_new").innerHTML = "Nuevo Detalle de Contenido (F2)";
        document.getElementById("contentdetail_refresh").innerHTML = "Refrescar (F4)";
        document.getElementById("contentdetail_open").innerHTML = "Abrir enlace de contenido";

        document.getElementById("contentdetail_label_file").innerHTML = "Seleccionar Archivo";
        document.getElementById("contentdetail_button_upload_now").innerHTML = "Subir Ahora";
        document.getElementById("contentdetail_content_label").innerHTML = "Contenido";
        document.getElementById("contentdetail_pasteimage_label").innerHTML = "Imagen desde Portapapeles";
        document.getElementById("contentdetail_button_paste").innerHTML = "Pegar imagen";

        document.getElementById("dontentdetail_label_content_type").innerHTML = "Tipo de Contenido";
        document.getElementById("contentdetail_label_content_order").innerHTML = "Orden de Contenido";

        document.getElementById("contentdetail_option_text").innerHTML = "Texto";
        document.getElementById("contentdetail_option_tittle1").innerHTML = "Título 1";
        document.getElementById("contentdetail_option_tittle2").innerHTML = "Título 2";
        document.getElementById("contentdetail_option_tittle3").innerHTML = "Título 3";
        document.getElementById("contentdetail_option_visualbasic").innerHTML = "Código Visual Basic";
        document.getElementById("contentdetail_option_csharp").innerHTML = "Código C#";
        document.getElementById("contentdetail_option_python").innerHTML = "Código Python";
        document.getElementById("contentdetail_option_javascript").innerHTML = "Código JavaScript";
        document.getElementById("contentdetail_option_html").innerHTML = "Código HTML";
        document.getElementById("contentdetail_option_css").innerHTML = "Código CSS";
        document.getElementById("contentdetail_option_sqlserver").innerHTML = "Código SQL Server";
        document.getElementById("contentdetail_option_angelsqlquery").innerHTML = "Código AngelSQL";
        document.getElementById("contentdetail_option_image").innerHTML = "Imagen";
        document.getElementById("contentdetail_option_video").innerHTML = "Video";
        document.getElementById("contentdetail_option_url").innerHTML = "Enlace URL";

        document.getElementById("contentdetail_button_save").innerHTML = "Guardar";
        document.getElementById("contentdetail_button_delete").innerHTML = "Eliminar";
        document.getElementById("contentdetail_button_close").innerHTML = "Cerrar";

    }        

}



function translate_element( language, value ) 
{
    const spanish_dictionary = { 
        "Manage my collaborations": "Administrar mis colaboraciones",
        "Show Topics": "Mostrar Temas",
    };

    if( language == "es") 
    {
        if( spanish_dictionary[value] != null ) 
        {
            return spanish_dictionary[value];
        } 
        else 
        {
            return value;
        }
    } 

    return value;

}

