async function runExample() {
    // Replace with your actual credentials and URL
    const user = 'db';
    const password = 'db';
    const url = 'https://localhost:7170/AngelSQL';

    const angelSql = new AngelSQL(user, password, url);

    try {
        const startResult = await angelSql.start();
        console.log('Start Result:', startResult);

        var result = await angelSql.prompt('GET ACCOUNTS');
        console.log('Prompt Result:', result);

    } catch (error) {
        console.error('Error:', error);
    }
}

var angelSql = null;

function StartAngelSQL(user, password, url) {
    angelSql = new AngelSQL(user, password, url);

    let result = angelSql.start();

    if (result.startsWith("Error:")) {
        console.log('Start Result:', result);
        return result;
    }

    sessionStorage.setItem('AngelSQLToken', angelSql.GetToken());
    return "Ok.";

}