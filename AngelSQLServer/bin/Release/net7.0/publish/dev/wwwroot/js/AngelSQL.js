// AngelSQL class API:
class AngelSQL {
    constructor(user, password, url) {
        this.user = user;
        this.password = password;
        this.url = url;
        this.angelQuery = {
            type: 'identification',
            user: user,
            password: password,
            token: '',
            command: '',
        };
    }

    async SetToken(token) {
        this.angelQuery.token = token;
    }

    async Gettoken() {
        return this.angelQuery.token;
    }

    async start() {
        const response = await fetch(this.url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json; charset=UTF-8' },
            body: JSON.stringify(this.angelQuery),
        });

        const angelResponse = await response.json();
        this.angelQuery.token = angelResponse.token;
        return angelResponse.result;

    }

    async prompt(command) {
        this.angelQuery.type = 'query';
        this.angelQuery.command = command;
        const response = await fetch(this.url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(this.angelQuery),
        });
        const angelResponse = await response.json();
        return angelResponse.result;
    }
}
