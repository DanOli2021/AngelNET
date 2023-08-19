async function sendPOST(data, fetch_url = "") {

    url = window.location.protocol + '//' + window.location.host + "/AngelPOST";

    if (fetch_url != "")
    {
        url = fetch_url;
    }    

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });

    if (!response.ok) {
        throw new Error(`Error HTTP: ${response.status}`);
    }

    const result = await response.text();
    return result;
}


async function sendToAngelPOST(user, api_name, token, OperationType, object_data, fetch_url = "") {

    account = "";

    if (user.includes("@")) {
        account = user.split("@")[1];
    }

    var api = {
        api: api_name,
        account: account,
        language: "C#",
        message:
        {
            OperationType: OperationType,
            Token: token,
            DataMessage: object_data
        }
    };

    return await sendPOST(api, fetch_url);

}


async function login(user, password) {
    var url = window.location.protocol + '//' + window.location.host + "/AngelPOST";
    return sendToAngelPOST(user, "tokens/admintokens", "", "GetTokenFromUser", { User: user, Password: password }, url);
}

async function GetGroupsUsingTocken(user, token) {
    var url = window.location.protocol + '//' + window.location.host + "/AngelPOST";
    return sendToAngelPOST(user, "tokens/admintokens", "", "GetGroupsUsingTocken", { TokenToObtainPermission: token }, url);
}

async function SearchSkus(user, token, where) {
    return sendToAngelPOST(user, "kiosk/adminskus", token, "SearchSkus", { Where: where });
}





function generateButton(href, iconSrc, buttonText, buttonClass, onclick = {}) {
    let button = document.createElement("a");
    button.href = href;
    button.className = buttonClass;
    button.style.paddingLeft = "10px"
    button.style.paddingRight = "100px"
    

    let iconSpan = document.createElement("span");
    iconSpan.className = "material-symbols-outlined";
    iconSpan.style.float = "left";

    let iconImg = document.createElement("img");
    iconImg.src = "images/icons/" + iconSrc;
    iconImg.style.width = "96px";

    let text = document.createElement("h2");
    text.innerText = buttonText;

    iconSpan.appendChild(iconImg);
    button.appendChild(iconSpan);
    button.appendChild(text);

    button.onclick = onclick;

    let div = document.getElementById("buttonszone");
    div.appendChild(button);

}

function generateParagraph(element, text, classstring, stylestring) {
    let p = document.createElement(element);
    p.innerText = text;
    p.style.textAlign = stylestring;
    p.className = classstring;
    let div = document.getElementById("buttonszone");
    div.appendChild(p);
}


// Función para verificar si la URL es una imagen válida
function isValidImageUrl(url) {
    // Expresión regular para verificar la extensión de la imagen
    var image_extensions = /\.(jpeg|jpg|gif|png)$/i;

    // Verifica si la URL tiene una extensión de imagen válida
    if (image_extensions.test(url)) {
        return true;
    }

    return false;
}


function isValidDateFormat(dateStr) {
    const regex = /^\d{4}-\d{2}-\d{2}$/;

    // Verifica si cumple con el formato
    if (!regex.test(dateStr)) {
        return false;
    }

    // Trata de crear un objeto Date con la fecha
    const dateObj = new Date(dateStr);

    // Verifica que sea una fecha válida (esto evita fechas como 2023-02-30, por ejemplo)
    if (dateObj.toISOString().slice(0, 10) !== dateStr) {
        return false;
    }

    return true;
}

function parseDate(input) {
    var parts = input.split(' ');
    var dateParts = parts[0].split('-');
    var timeParts = parts[1].split(':');
    // new Date(year, month [, day [, hours[, minutes[, seconds[, ms]]]]])
    return new Date(dateParts[0], dateParts[1] - 1, dateParts[2], timeParts[0], timeParts[1], timeParts[2]);
}
