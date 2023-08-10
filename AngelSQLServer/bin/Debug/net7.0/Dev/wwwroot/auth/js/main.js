async function sendPOST(data) {

  url = window.location.protocol + '//' + window.location.host + "/AngelPOST";

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


async function login(user, password) 
{
    return sendToAngelPOST(user, "tokens/admintokens", "", "GetTokenFromUser", { User: user, Password: password });
}

async function GetGroupsUsingTocken(user, token) 
{
    return sendToAngelPOST(user, "tokens/admintokens", "", "GetGroupsUsingTocken", { TokenToObtainPermission: token });
}

async function GetUser(user, token, userToObtain)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetUser", { User: userToObtain });
}

async function SaveUSer(user, token, userToSave)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "UpsertUser", userToSave);
}

async function GetUsers(user, token)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetUsers",  {});
}


async function sendToAngelPOST(user, api_name, token, OperationType, object_data) {

  account = ""; 

  if (user.includes("@")) {
    account = user.split("@")[1];
  }
  
console.log(api_name);

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

  return await sendPOST(api);

}


function generateButton(href, iconSrc, buttonText, buttonClass) {
  let button = document.createElement("a");
  button.href = href;
  button.className = buttonClass;

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

  document.body.appendChild(button);
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
