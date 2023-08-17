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

async function login(user, password) {
    return sendToAngelPOST(user, "tokens/admintokens", "", "GetTokenFromUser", { User: user, Password: password });
}


