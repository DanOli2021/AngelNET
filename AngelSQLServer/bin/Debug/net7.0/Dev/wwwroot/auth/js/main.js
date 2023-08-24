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

async function SaveUser(user, token, userToSave)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "UpsertUser", userToSave);
}

async function GetUsers(user, token, Where = {})
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetUsers",  Where);
}

async function GetGroups(user, token)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetGroups",  {});
}

async function DeleteUser(user, token, UserToDelete)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "DeleteUser",  { UserToDelete: UserToDelete });
}

async function GetGroup(user, token, id)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetGroups", { Where: "id = '" + id + "'"});
}

async function SaveGroup(user, token, GroupToSave)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "UpsertGroup", GroupToSave);
}

async function DeleteGroup(user, token, GroupToDelete)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "DeleteGroup",  { UserGroupToDelete: GroupToDelete });
}

async function DeleteBranchStore(user, token, BranchStoreToDelete)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "DeleteBranchStore",  { BranchStoreToDelete: BranchStoreToDelete });
}

async function GetBranchStore(user, token, BranchStoreId)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetBranchStore", { BranchStoreId: BranchStoreId });
}

async function SaveBranchStore(user, token, BranchStoreToSave)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "UpsertBranchStore", BranchStoreToSave);
}

async function GetBranchStores(user, token)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetBranchStores", {});
}

async function GetBranchStoresByUser(user, token)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetBranchStoresByUser", {});
}

async function CreatePermission(user, token, Branchstore_id, Permission_id, PinType = null, AuthorizerMessage = null)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "CreatePermission", { Branchstore_id: Branchstore_id, Permission_id: Permission_id, User: user, PinType: PinType, AuthorizerMessage: AuthorizerMessage });
}

async function GetPins(user, token, InitialDate, FinalDate)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetPins", { InitialDate: InitialDate, FinalDate: FinalDate });
}

async function DeleteToken(user, token, TokenToDelete)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "DeleteToken", { TokenToDelete: TokenToDelete });
}

async function GetTokens(user, token)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetTokens", {});
}

async function GetToken(user, token, TokenId)
{
    return sendToAngelPOST(user, "tokens/admintokens", token, "GetToken", { TokenId: TokenId });
}

async function SaveToken(user, token, Token)
{
    return sendToAngelPOST( user, "tokens/admintokens", token, "SaveToken", Token );
}

async function SendPinToEmail(email) {
  return sendToAngelPOST( "", "tokens/admintokens", "", "SendPinToEmail", { Email: email } );
}

async function CreateAccount(register_info) {
  return sendToAngelPOST( "", "tokens/createaccount", "", "CreateAccount", register_info );
}

async function RecoverMasterPassword(email) {
    return sendToAngelPOST("", "tokens/admintokens", "", "RecoverMasterPassword", { Email: email } );
}


async function sendToAngelPOST(user, api_name, token, OperationType, object_data) {

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

  return await sendPOST(api);

}


function generateButton(href, iconSrc, buttonText, buttonClass, onclick = {}) {
  let button = document.createElement("a");
  button.href = href;
  button.className = buttonClass;
  button.style.paddingRight = "120px";

  let iconSpan = document.createElement("span");
  iconSpan.className = "material-symbols-outlined";
  iconSpan.style.float = "left";  

  let iconImg = document.createElement("img");
  iconImg.src = "images/icons/" + iconSrc;
  iconImg.style.width = "96px";

  let text = document.createElement("h2");
  text.innerText = translate_buttons("es", buttonText);

  iconSpan.appendChild(iconImg);
  button.appendChild(iconSpan);
  button.appendChild(text);

  button.onclick = onclick;

  let div = document.getElementById("buttonszone");
  div.appendChild(button);

}

function generateParagraph(element, text, classstring, stylestring  ) {
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
  return new Date(dateParts[0], dateParts[1]-1, dateParts[2], timeParts[0], timeParts[1], timeParts[2]);
}
