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
    var data = {OperationType: "GetTokenFromUser", User: user, Password: password};
    var api = {
               api: "pos/admintokens", 
               message: JSON.stringify(data),
               language: "C#"
              };

    return await sendPOST(api);;
}


async function getPermissions(token) 
{
    var data = {OperationType: "GetPermisionsUsingTocken", Token: token};
    var api = {
               api: "pos/admintokens", 
               message: JSON.stringify(data),
               language: "C#"
              };

    return await sendPOST(api);;
}