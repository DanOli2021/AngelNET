
function GetContentData(Account, Content_id) {

    var datos = GetContentTitles(Account, Content_id);

    datos.then(function (query) {

        let responce_query = JSON.parse(query);

        if (responce_query.result.startsWith("Error:")) {
            console.log(responce_query.result);
            return;
        }

        var titles = JSON.parse(responce_query.result);
        document.getElementById('PageTitle').innerHTML = titles.Content + " - " + titles.Content_Description;

        var contentdetail_topics = document.getElementById('topic');
        contentdetail_topics.innerHTML = titles.Topic + " - " + titles.Topic_Description;

        var subtopics_menu = document.getElementById('subtopic');
        subtopics_menu.innerHTML = titles.Subtopic + " - " + titles.Subtopic_Description;

        var content_title = document.getElementById('content_title');
        content_title.innerHTML = titles.Content + " - " + titles.Content_Description;

    });

    var ShowContent = document.getElementById('ShowContent');
    ShowContent.innerHTML = "";

    var datos = GetPublicContent(Account, Content_id);

    datos.then(function (query) {

        let responce_query = JSON.parse(query);

        if (responce_query.result.startsWith("Error:")) {
            console.log(responce_query.result);
            return;
        }

        var ContentDetail = JSON.parse(responce_query.result);

        ContentDetail.forEach(element => {
            addContentDetail(element.id, element.Content, element.Content_type, "ShowContent");
        });

    });

}

function formatText(text) {
    let formattedText = text.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    return formattedText;
}

function addHorizontalRule(text) {
    let lines = text.split('\n');
    for (let i = 0; i < lines.length; i++) {
        if (lines[i].trim() === '---') {
            lines[i] = '<hr>';
        }
    }
    return lines.join('\n');
}


function identifyLinks(text) {
    // Expresión regular básica para identificar URLs
    const urlRegex = /https?:\/\/[^\s]+/g;
    let links = text.match(urlRegex);
    return links;
}

function createAnchors(text, links) {
    let anchoredText = text;

    if( links == null || links == undefined || links.length == 0)
        return anchoredText;

    links.forEach(link => {
        const anchor = `<a href="${link}" target="_blank">${link}</a>`;
        anchoredText = anchoredText.replace(link, anchor);
    });
    return anchoredText;
}


async function addContentDetail(Id, Content, Content_type, element) {
    // Crear el elemento h1

    var html_block;

    let formattedContent = formatText(Content);
    formattedContent = addHorizontalRule(formattedContent);
    const links = identifyLinks(formattedContent);
    formattedContent = createAnchors(formattedContent, links);

    if (Content_type == "Text") {
        html_block = document.createElement('p');
        html_block.innerHTML = formattedContent;
    }
    else if (Content_type == "Title1") {
        html_block = document.createElement('h1');
        html_block.textContent = Content;
    }
    else if (Content_type == "Title2") {
        html_block = document.createElement('h2');
        html_block.textContent = formattedContent;
    }
    else if (Content_type == "Title3") {
        html_block = document.createElement('h3');
        html_block.textContent = formattedContent;
    }
    else if (Content_type == "VisualBasic") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-vbnet';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-vbnet';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "C#") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-csharp';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-csharp';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "SQLServerQuery") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-sql';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-sql';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "AngelSQLQuery") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-sql';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-sql';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "JavaScript") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-javascript';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-javascript';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "HTML") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-html';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-html';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "CSS") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-css';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-css';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "Python") {
        html_block = document.createElement('pre');
        html_block.className = 'line-numbers language-python';
        var code_block = document.createElement('code');
        code_block.className = 'line-numbers language-python';
        code_block.textContent = Content;  // Utiliza textContent para evitar la interpretación de HTML
        html_block.appendChild(code_block);
    }
    else if (Content_type == "Url") {
        html_block = document.createElement('div');
        var urlBlock = document.createElement('a');
        urlBlock.textContent = Content;
        urlBlock.href = Content;
        urlBlock.style = "margin-bottom:20px; margin-top:20px; font-size: 20px; font-weight: bold;";
        urlBlock.target = "_blank";
        urlBlock.className = "btn btn-link form-control";
        html_block.appendChild(urlBlock);
}
    else if (Content_type == "Video") {
        var videoId = Content.split('v=')[1];  // Suponiendo que Content contiene la URL completa del video de YouTube
        var ampersandPosition = videoId.indexOf('&');
        if (ampersandPosition != -1) {
            videoId = videoId.substring(0, ampersandPosition);
        }

        html_block = document.createElement('div');
        html_block.className = "row justify-center";
        var videoBlock = document.createElement('iframe');
        videoBlock.className = "embed-responsive embed-responsive-16by9";
        videoBlock.style = "margin-bottom:20px; margin-top:20px;";
        videoBlock.width = "560";  // Ancho del video, puedes ajustarlo según tus necesidades
        videoBlock.height = "315";  // Altura del video, puedes ajustarlo según tus necesidades
        videoBlock.src = "https://www.youtube.com/embed/" + videoId;
        videoBlock.title = "YouTube video player";
        videoBlock.frameborder = "0";
        videoBlock.allow = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture";
        videoBlock.allowFullscreen = true;
        html_block.appendChild(videoBlock);
    }
    else if (Content_type == "Image") {
        var lines = Content.split('\n');
        if (lines.length >= 2) {
            var html_block = document.createElement('div');
            html_block.className = "row justify-ContentDetail-center";
            var imageBlock = document.createElement('img');
            imageBlock.className = "img-fluid mx-auto d-block";
            imageBlock.src = lines[0];  // Primera línea para la fuente de la imagen
            imageBlock.style = lines[1];  // Segunda línea para el estilo
            html_block.appendChild(imageBlock);
        }
        else {
            var html_block = document.createElement('div');
            html_block.className = "row justify-ContentDetail-center";
            var imageBlock = document.createElement('img');
            imageBlock.className = "img-fluid mx-auto d-block";
            html_block.src = Content;  // Primera línea para la fuente de la imagen
            html_block.appendChild(imageBlock);
        }
    }
    else {
        html_block = document.createElement('p');
        html_block.textContent = Content;
    }

    var mydiv = document.getElementById(element);
    mydiv.appendChild(html_block);
    Prism.highlightAll();

}



async function GetPublicContent(account, Content_id) {
    return sendToAngelPOST("helpdesk/helpdesk", "GetPublicContent", { Content_id: Content_id }, account);
}

async function GetContentTitles(account, Content_id) {
    return sendToAngelPOST("helpdesk/helpdesk", "GetContentTitles", { Content_id: Content_id }, account);
}

async function sendToAngelPOST(api_name, OperationType, object_data, account) {

    var api = {
        api: api_name,
        account: account,
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


