async function enviarAPost(url, data) {
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