async function GetSkuInfo(sku) {
  return sendToAngelPOST("PriceChecker/PriceChecker", "GetSkuInfo", { sku: sku });
}

function showDialog(title, message) {
  document.getElementById('dialogTittle').innerText = title;
  document.getElementById('dialogMessage').innerText = '⚠️' + message;
  document.getElementById('myDialog').showModal();
}

function closeDialog() {
  document.getElementById('myDialog').close();
}


async function sendToAngelPOST(api_name, OperationType, object_data) {
  var api = {
    api: api_name,
    account: "",
    language: "C#",
    message:
    {
      OperationType: OperationType,
      Token: "",
      User: "",
      DataMessage: object_data,
      UserLanguage: "en"
    }
  };

  return await sendPOST(api);

}

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

