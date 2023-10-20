// GLOBALS
// These lines of code go in each script
#load "Globals.csx"
// END GLOBALS

// Process to send messages to user
// Daniel() Oliver Rojas
// 2023-05-19

#load "translations.csx"
#load "HelpdeskTopics.csx"

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Xml.Linq;
using System.Text;

public class AngelApiOperation
{
    public string OperationType { get; set; }
    public string Token { get; set; }
    public string User { get; set; }
    public string UserLanguage { get; set; }
    public dynamic DataMessage { get; set; }
    public string File { get; set; }
    public long FileSize { get; set; }
    public string FileType { get; set; }
}


public class FileUploadInfo
{
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string FileDirectory { get; set; }
    public string Url { get; set; }
    public bool ProceedToUpload { get; set; }
    public string ErrorMessage { get; set; }

}

private AngelApiOperation api = JsonConvert.DeserializeObject<AngelApiOperation>(message);

//Server parameters
private Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(Environment.GetEnvironmentVariable("ANGELSQL_PARAMETERS"));
private Translations translation = new();
translation.SpanishValues();
CreateTables(db);

// This is the main function that will be called by the API
return api.OperationType switch
{
    "UpsertTopic" => UpsertTopic(api, translation),
    "GetTopicsFromUser" => GetTopicsFromUser(api, translation),
    "GetTopic" => GetTopic(api, translation),
    "UpsertSubTopic" => UpsertSubTopic(api, translation),
    "GetSubTopicsFromTopic" => GetSubTopicsFromTopic(api, translation),
    "GetSubTopic" => GetSubTopic(api, translation),
    "GetContentFromSubTopic" => GetContentFromSubTopic(api, translation),
    "UpsertContent" => UpsertContent(api, translation),
    "GetContent" => GetContent(api, translation),
    "DeleteContent" => DeleteContent(api, translation),
    "GetContentDetail" => GetContentDetail(api, translation),
    "GetTitles" => GetTitles(api, translation),
    "UpsertContentDetail" => UpsertContentDetail(api, translation),
    "GetContentDetailItem" => GetContentDetailItem(api, translation),
    "DeleteContentDetail" => DeleteContentDetail(api, translation),
    "UploadFile" => UploadFile(api, translation),
    "SearchInfo" => SearchInfo(api, translation),
    "GetContentTitles" => GetContentTitles(api, translation),
    "GetPublicContent" => GetPublicContent(api, translation),
    _ => $"Error: No service found {api.OperationType}",
};

string UpsertTopic(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get("Id is required", language);
    }

    if (d.Topic == null)
    {
        return "Error: " + translation.Get("Topic is required", language);
    }

    if (string.IsNullOrEmpty(d.Topic.ToString()))
    {
        return "Error: " + translation.Get("Topic is required", language);
    }

    if (d.Description == null)
    {
        return "Error: " + translation.Get("Description is required", language);
    }

    if (string.IsNullOrEmpty(d.Description.ToString()))
    {
        return "Error: " + translation.Get("Description is required", language);
    }

    if (d.Id == "new" || string.IsNullOrEmpty(d.Id.ToString()))
    {
        d.Id = Guid.NewGuid().ToString();
    }

    if (string.IsNullOrEmpty(d.Topic.ToString()))
    {
        return "Error: " + translation.Get("Topic is required", language);
    }

    result = db.Prompt("SELECT * FROM HelpdeskTopics WHERE id = '" + d.Id + "'");

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    HelpdeskTopics topic = new()
    {
        Id = d.Id,
        Topic = d.Topic,
        Description = d.Description
    };

    if (result == "[]")
    {
        topic.CreatedBy = api.User;
        topic.CreatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }
    else
    {
        DataRow rTopic = db.GetDataRow(result);
        topic.CreatedAt = rTopic["createdat"].ToString();
        topic.CreatedBy = rTopic["createdby"].ToString();
        topic.UpdatedBy = api.User;
        topic.UpdatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }

    result = db.UpsertInto("HelpdeskTopics", topic);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.Prompt( "SELECT id FROM HelpdeskSubTopics WHERE topic_id = '{d.Id}'", true );

    DataTable dtSubTopics = db.GetDataTable(result);

    foreach (DataRow r in dtSubTopics.Rows)
    {
        result = db.Prompt($"UPDATE HelpdeskContentDetails PARTITON KEY {r["id"]} SET Topic_description = '{ d.Description }' WHERE topic_id = '{d.Id}'", true);
    }

    return result;

}


string UpsertSubTopic(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get("Id is required", language);
    }

    if (d.Topic_id == null)
    {
        return "Error: " + translation.Get("Topic_id is required", language);
    }

    if (d.Subtopic == null)
    {
        return "Error: " + translation.Get("Subtopic is required", language);
    }

    if (d.Description == null)
    {
        return "Error: " + translation.Get("Description is required", language);
    }

    if (d.Id == "new" || string.IsNullOrEmpty(d.Id.ToString()))
    {
        d.Id = Guid.NewGuid().ToString();
    }

    if (string.IsNullOrEmpty(d.Topic_id.ToString()))
    {
        return "Error: " + translation.Get("Topic_id is required", language);
    }

    if (string.IsNullOrEmpty(d.Subtopic.ToString()))
    {
        return "Error: " + translation.Get("Subtopic is required", language);
    }

    if (string.IsNullOrEmpty(d.Description.ToString()))
    {
        return "Error: " + translation.Get("Description is required", language);
    }

    result = db.Prompt("SELECT * FROM HelpdeskTopics WHERE id = '" + d.Topic_id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("Topic_id does not exist", language) + " " + d.Topic_id;
    }

    result = db.Prompt("SELECT * FROM HelpdeskSubTopics WHERE id = '" + d.Id + "'", true);

    HelpdeskSubTopics subtopic = new()
    {
        Id = d.Id,
        Topic_id = d.Topic_id,
        Subtopic = d.Subtopic,
        Description = d.Description,
    };

    if (result == "[]")
    {
        subtopic.CreatedBy = api.User;
        subtopic.CreatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }
    else
    {
        DataRow rTopic = db.GetDataRow(result);
        subtopic.CreatedAt = rTopic["createdat"].ToString();
        subtopic.CreatedBy = rTopic["createdby"].ToString();
        subtopic.UpdatedBy = api.User;
        subtopic.UpdatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }

    result = db.UpsertInto("HelpdeskSubTopics", subtopic);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    return result;

}


string UpsertContent(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get("Id is required", language);
    }

    if (d.Subtopic_id == null)
    {
        return "Error: " + translation.Get("Subtopic_id is required", language);
    }

    if (d.Content_title == null)
    {
        return "Error: " + translation.Get("Content_title is required", language);
    }

    if (d.Description == null)
    {
        return "Error: " + translation.Get("Description is required", language);
    }

    if (d.Status == null)
    {
        return "Error: " + translation.Get("Status is required", language);
    }

    if (d.Version == null)
    {
        return "Error: " + translation.Get("Version is required", language);
    }

    if (d.IsPublic == null)
    {
        return "Error: " + translation.Get("IsPublic is required", language);
    }

    if (d.Id == "new" || string.IsNullOrEmpty(d.Id.ToString()))
    {
        d.Id = Guid.NewGuid().ToString();
    }

    if (string.IsNullOrEmpty(d.Subtopic_id.ToString()))
    {
        return "Error: " + translation.Get("Subtopic_id is required", language);
    }

    result = db.Prompt("SELECT * FROM HelpdeskSubTopics WHERE id = '" + d.Subtopic_id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("Subtopic_id does not exist", language) + " " + d.Subtopic_id;
    }

    result = db.Prompt("SELECT * FROM helpdeskcontent WHERE id = '" + d.Id + "'", true);

    HelpdeskContent content = new()
    {
        Id = d.Id,
        Subtopic_id = d.Subtopic_id,
        Content_title = d.Content_title,
        Description = d.Description,
        Status = d.Status,
        Version = d.Version,
        IsPublic = d.IsPublic
    };

    if (result == "[]")
    {
        content.CreatedBy = api.User;
        content.CreatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }
    else
    {
        DataRow rTopic = db.GetDataRow(result);
        content.CreatedAt = rTopic["createdat"].ToString();
        content.CreatedBy = rTopic["createdby"].ToString();
        content.UpdatedBy = api.User;
        content.UpdatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }

    result = db.UpsertInto("HelpdeskContent", content);
    return result;

}


string UpsertContentDetail(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get("Id is required", language);
    }

    if (d.Content_id == null)
    {
        return "Error: " + translation.Get("Content_id is required", language);
    }

    if (d.Content == null)
    {
        return "Error: " + translation.Get("Content is required", language);
    }

    if (d.Content_type == null)
    {
        return "Error: " + translation.Get("Content_type is required", language);
    }

    if (d.Content_order == null)
    {
        return "Error: " + translation.Get("Content_order is required", language);
    }

    if (string.IsNullOrEmpty(d.Content.ToString()))
    {
        return "Error: " + translation.Get("Content is required", language);
    }

    if (string.IsNullOrEmpty(d.Content_type.ToString()))
    {
        return "Error: " + translation.Get("Content_type is required", language);
    }


    if (string.IsNullOrEmpty(d.Content_order.ToString()) || d.Content_order == 0)
    {
        result = db.Prompt("SELECT MAX(Content_order) AS Content_order FROM HelpdeskContentDetails WHERE Content_id = '" + d.Content_id + "'", true);

        if (result == "[]")
        {
            d.Content_order = 1;
        }
        else
        {
            DataRow rContent = db.GetDataRow(result);

            if (rContent["Content_order"] == DBNull.Value)
            {
                d.Content_order = 1;
            }
            else
            {
                d.Content_order = Convert.ToInt32(rContent["Content_order"].ToString()) + 1;
            }
        }

    }

    if (d.Id == "new" || string.IsNullOrEmpty(d.Id.ToString()))
    {
        d.Id = Guid.NewGuid().ToString();
    }

    result = db.Prompt("SELECT * FROM HelpdeskContent WHERE id = '" + d.Content_id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("Content_id does not exist", language) + " " + d.Content_id;
    }

    DataTable dtContent = db.GetDataTable(result);
    string content_title = dtContent.Rows[0]["Content_title"].ToString(); 

    result = db.Prompt("SELECT * FROM HelpdeskSubTopics WHERE id = '" + dtContent.Rows[0]["Subtopic_id"] + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("Subtopic_id does not exist", language) + " " + dtContent.Rows[0]["Subtopic_id"];
    }

    DataTable dtSubTopic = db.GetDataTable(result);
    string subtopic = dtSubTopic.Rows[0]["id"].ToString();
    string subtopic_description = dtSubTopic.Rows[0]["Description"].ToString();

    result = db.Prompt("SELECT * FROM HelpdeskTopics WHERE id = '" + dtSubTopic.Rows[0]["Topic_id"] + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("Topic_id does not exist", language) + " " + dtSubTopic.Rows[0]["Topic_id"];
    }


    if( d.IsPublic == null )
    {
        d.IsPublic = "true";
    }

    DataTable dtTopic = db.GetDataTable(result);
    string topic = dtTopic.Rows[0]["id"].ToString();
    string topic_description = dtTopic.Rows[0]["Description"].ToString();

    HelpdeskContentDetails contentDetail = new()
    {
        Id = d.Id,
        Content = d.Content,
        Content_id = d.Content_id,
        Content_type = d.Content_type,
        Content_order = d.Content_order,
        Topic_id = topic,  
        Topic_description = topic_description,
        Subtopic_id = subtopic,
        Subtopic_description = subtopic_description,
        Content_title = content_title,
        IsPublic = d.IsPublic
    };

    if (result == "[]")
    {
        contentDetail.CreatedBy = api.User;
        contentDetail.CreatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }
    else
    {
        DataRow rTopic = db.GetDataRow(result);
        contentDetail.CreatedAt = rTopic["createdat"].ToString();
        contentDetail.CreatedBy = rTopic["createdby"].ToString();
        contentDetail.UpdatedBy = api.User;
        contentDetail.UpdatedAt = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    }

    result = db.UpsertInto("HelpdeskContentDetails", contentDetail, subtopic);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    result = db.UpsertInto("HelpdeskContentDetails_search", contentDetail, subtopic);

    return result;

}



string GetTopicsFromUser(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    result = db.Prompt("SELECT * FROM HelpdeskTopics WHERE createdby = '" + api.User + "' ORDER BY topic ASC", true);
    return result;

}


string GetSubTopicsFromTopic(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;

    if (d.Topic_id == null)
    {
        return "Error: " + translation.Get("Topic_id is required", api.UserLanguage);
    }

    result = db.Prompt("SELECT * FROM HelpdeskSubTopics WHERE topic_id = '" + d.Topic_id + "' ORDER BY subtopic ASC", true);

    return result;

}



string GetContentFromSubTopic(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;

    if (d.Subtopic_id == null)
    {
        return "Error: " + translation.Get("Subtopic_id is required", api.UserLanguage);
    }

    result = db.Prompt("SELECT * FROM HelpdeskContent WHERE Subtopic_id = '" + d.Subtopic_id + "' ORDER BY description ASC", true);

    return result;

}



string GetSubTopic(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get("Id is required", language);
    }

    result = db.Prompt("SELECT * FROM HelpdeskSubTopics WHERE id = '" + d.Id + "'", true);

    if (result == "[]")
    {
        return result;
    }

    DataRow rTopic = db.GetDataRow(result);

    HelpdeskSubTopics topic = new()
    {
        Id = rTopic["id"].ToString(),
        Topic_id = rTopic["topic_id"].ToString(),
        Subtopic = rTopic["subtopic"].ToString(),
        Description = rTopic["description"].ToString(),
        CreatedBy = rTopic["createdby"].ToString(),
        CreatedAt = rTopic["createdat"].ToString(),
        UpdatedBy = rTopic["updatedby"].ToString(),
        UpdatedAt = rTopic["updatedat"].ToString()
    };

    return db.GetJson(topic);

}


string GetContent(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get("Id is required", language);
    }

    result = db.Prompt("SELECT * FROM HelpdeskContent WHERE id = '" + d.Id + "'", true);

    if (result == "[]")
    {
        return result;
    }

    DataRow rTopic = db.GetDataRow(result);

    HelpdeskContent topic = new()
    {
        Id = rTopic["id"].ToString(),
        Subtopic_id = rTopic["Subtopic_id"].ToString(),
        Content_title = rTopic["Content_title"].ToString(),
        Description = rTopic["description"].ToString(),
        Version = rTopic["version"].ToString(),
        Status = rTopic["status"].ToString(),
        IsPublic = rTopic["IsPublic"].ToString(),
        CreatedBy = rTopic["createdby"].ToString(),
        CreatedAt = rTopic["createdat"].ToString(),
        UpdatedBy = rTopic["updatedby"].ToString(),
        UpdatedAt = rTopic["updatedat"].ToString()
    };

    return db.GetJson(topic);

}


string GetTopic(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get(language, "Id is required");
    }

    result = db.Prompt("SELECT * FROM HelpdeskTopics WHERE id = '" + d.Id + "'", true);

    DataRow rTopic = db.GetDataRow(result);

    HelpdeskTopics topic = new()
    {
        Id = rTopic["id"].ToString(),
        Topic = rTopic["topic"].ToString(),
        Description = rTopic["description"].ToString(),
        CreatedBy = rTopic["createdby"].ToString(),
        CreatedAt = rTopic["createdat"].ToString(),
        UpdatedBy = rTopic["updatedby"].ToString(),
        UpdatedAt = rTopic["updatedat"].ToString()
    };

    return db.GetJson(topic);

}

string DeleteContent(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Content_id == null)
    {
        return "Error: " + translation.Get(language, "Content_id is required");
    }

    result = db.Prompt("SELECT * FROM HelpdeskContentDetails WHERE Content_id = '" + d.Content_id + "'", true);

    if (result != "[]")
    {
        return "Error: " + translation.Get("You first need to delete the content details in order to delete this item", language);
    }

    db.Prompt("DELETE FROM HelpdeskContent PARTITION KEY main WHERE id = '" + d.Content_id + "'", true);
    return "Ok.";

}


string DeleteContentDetail(AngelApiOperation api, Translations translation)
{
    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get(language, "Id is required");
    }

    if( d.Content_id == null )
    {
        return "Error: " + translation.Get(language, "Content_id is required");
    }   

    result = db.Prompt("SELECT * FROM helpdeskcontentdetails WHERE id = '" + d.Id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("Content detail id does not exist ", language) + d.Id;
    }

    result = db.Prompt("SELECT * FROM HelpdeskContent WHERE id = '" + d.Content_id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("Content_id does not exist ", language) + d.Content_id;
    }

    DataRow rContent = db.GetDataRow(result);

    db.Prompt($"DELETE FROM helpdeskcontentdetails PARTITION KEY {rContent["Subtopic_id"]} WHERE id = '" + d.Id + "'", true);
    db.Prompt($"DELETE FROM helpdeskcontentdetails_search PARTITION KEY {rContent["Subtopic_id"]} WHERE id = '" + d.Id + "'", true);
    return "Ok.";

}



private string GetContentDetail(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Content_id == null)
    {
        return "Error: " + translation.Get("Content_id is required", language);
    }

    return db.Prompt("SELECT * FROM HelpdeskContentDetails WHERE Content_id = '" + d.Content_id + "' ORDER BY Content_order", true);

}



private string GetPublicContent(AngelApiOperation api, Translations translation)
{

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Content_id == null)
    {
        return "Error: " + translation.Get("Content_id is required", language);
    }

    return db.Prompt("SELECT * FROM HelpdeskContentDetails WHERE Content_id = '" + d.Content_id + "' AND IsPublic = 'true' ORDER BY Content_order", true);

}




private string GetTitles(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Content_id == null)
    {
        return "Error: " + translation.Get("Content_id is required", language);
    }

    result = db.Prompt("SELECT * FROM HelpdeskContent WHERE id = '" + d.Content_id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("No content found for Content_id:", language) + " " + d.Content_id;
    }

    DataRow rContent = db.GetDataRow(result);
    result = db.Prompt("SELECT * FROM HelpdeskSubTopics WHERE id = '" + rContent["Subtopic_id"] + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("No Subtopic found for Subtopic_id:", language) + " " + rContent["Subtopic_id"];
    }

    DataRow rSubTopic = db.GetDataRow(result);
    result = db.Prompt("SELECT * FROM HelpdeskTopics WHERE id = '" + rSubTopic["Topic_id"] + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("No Topic found for Topic_id:", language) + " " + rSubTopic["Topic_id"];
    }

    DataRow rTopic = db.GetDataRow(result);

    var dTitles = new
    {
        Topic_id = rTopic["id"].ToString(),
        Topic = rTopic["Topic"].ToString(),
        Subtopic = rSubTopic["Subtopic"].ToString(),
        Content = rContent["Content_title"].ToString(),
        Topic_Description = rTopic["Description"].ToString(),
        Subtopic_Description = rSubTopic["Description"].ToString(),
        Content_Description = rContent["Description"].ToString()
    };

    return db.GetJson(dTitles);

}


private string GetContentTitles(AngelApiOperation api, Translations translation)
{

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Content_id == null)
    {
        return "Error: " + translation.Get("Content_id is required", language);
    }

    string result = db.Prompt("SELECT * FROM HelpdeskContent WHERE id = '" + d.Content_id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("No content found for Content_id:", language) + " " + d.Content_id;
    }

    DataRow rContent = db.GetDataRow(result);
    result = db.Prompt("SELECT * FROM HelpdeskSubTopics WHERE id = '" + rContent["Subtopic_id"] + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("No Subtopic found for Subtopic_id:", language) + " " + rContent["Subtopic_id"];
    }

    DataRow rSubTopic = db.GetDataRow(result);
    result = db.Prompt("SELECT * FROM HelpdeskTopics WHERE id = '" + rSubTopic["Topic_id"] + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("No Topic found for Topic_id:", language) + " " + rSubTopic["Topic_id"];
    }

    DataRow rTopic = db.GetDataRow(result);

    var dTitles = new
    {
        Topic_id = rTopic["id"].ToString(),
        Topic = rTopic["Topic"].ToString(),
        Subtopic = rSubTopic["Subtopic"].ToString(),
        Content = rContent["Content_title"].ToString(),
        Topic_Description = rTopic["Description"].ToString(),
        Subtopic_Description = rSubTopic["Description"].ToString(),
        Content_Description = rContent["Description"].ToString()
    };

    return db.GetJson(dTitles);

}



private string GetContentDetailItem(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Id == null)
    {
        return "Error: " + translation.Get("Id is required", language);
    }

    HelpdeskContentDetails contentDetail = new()
    {
        Id = d.Id
    };

    result = db.Prompt("SELECT * FROM HelpdeskContentDetails WHERE id = '" + d.Id + "'", true);

    if (result == "[]")
    {
        return "Error: " + translation.Get("No content found for Id:", language) + " " + d.Id;
    }

    DataRow rContentDetail = db.GetDataRow(result);

    contentDetail.Content = rContentDetail["Content"].ToString();
    contentDetail.Content_id = rContentDetail["Content_id"].ToString();
    contentDetail.Content_order = Convert.ToInt32(rContentDetail["Content_order"].ToString());
    contentDetail.Content_type = rContentDetail["Content_type"].ToString();

    return db.GetJson(contentDetail);

}


private string SearchInfo(AngelApiOperation api, Translations translation) 
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    dynamic d = api.DataMessage;

    if (d.Search == null)
    {
        return "Error: " + translation.Get("Search is required", language);
    }

    if (string.IsNullOrEmpty(d.Search.ToString()))
    {
        return "Error: " + translation.Get("Search is required", language);
    }

    return db.Prompt("SELECT Id, Content_id, substr( Content, 1, 100 ) As 'Content' FROM HelpdeskContentDetails_search WHERE HelpdeskContentDetails_search MATCH '" + d.Search + "' LIMIT 20", true);

}



string IsUserValid(AngelApiOperation api, Translations translation, string group = "HELPEDITOR")
{
    string result = GetGroupsUsingTocken(api.Token, api.User, api.UserLanguage);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic user_data = JsonConvert.DeserializeObject<dynamic>(result);

    if (user_data.groups == null)
    {
        return translation.Get(api.UserLanguage, "No groups found");
    }

    if (!user_data.groups.ToString().Contains(group))
    {
        return translation.Get(api.UserLanguage, "User does not have permission to edit topics");
    }

    return "Ok.";

}


string UploadFile(AngelApiOperation api, Translations translation)
{

    string result = IsUserValid(api, translation);

    if (result.StartsWith("Error:"))
    {
        return result;
    }

    dynamic d = api.DataMessage;
    string language = "en";

    if (api.UserLanguage != null)
    {
        language = api.UserLanguage;
    }

    if (d.Content_id == null)
    {
        return "Error: " + translation.Get("Content_id is required", language);
    }

    string directory = db.Prompt("VAR server_wwwroot", true) + "/helpdesk/" + d.Content_id + "/";

    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    if( api.File == null ) 
    {
        return "Error: " + translation.Get("File is required", language);
    }

    FileUploadInfo file = new()
    {
        FileName = api.File,
        FileSize = api.FileSize,
        ContentType = api.FileType,
        FileDirectory = directory,
        Url = directory + api.File,
        ProceedToUpload = true,
        ErrorMessage = ""
    };

    return db.GetJson(file);

}



string CreateTables(AngelDB.DB db)
{
    string result;

    HelpdeskTopics topic = new();
    result = db.CreateTable(topic);

    if (result.StartsWith("Error"))
    {
        return result;
    }

    HelpdeskSubTopics subtopic = new();
    result = db.CreateTable(subtopic);

    if (result.StartsWith("Error"))
    {
        return result;
    }

    HelpdeskContent content = new();
    result = db.CreateTable(content);

    if (result.StartsWith("Error"))
    {
        return result;
    }

    HelpdeskContentDetails content_details = new();
    result = db.CreateTable(content_details);

    if (result.StartsWith("Error"))
    {
        return result;
    }

    result = db.CreateTable(content_details, "HelpdeskContentDetails_search", true);
    return result;
}

private string GetGroupsUsingTocken(string token, string user, string language)
{

    var d = new
    {
        TokenToObtainPermission = token
    };

    string result = SendToAngelPOST("tokens/admintokens", user, token, "GetGroupsUsingTocken", language, d);

    if (result.StartsWith("Error:"))
    {
        return $"Error: {result}";
    }

    return result;

}



private string SendToAngelPOST(string api_name, string user, string token, string OPerationType, string Language, dynamic object_data)
{

    string db_account = user.Split("@")[1];

    var d = new
    {
        api = api_name,
        account = db_account,
        language = "C#",
        message = new
        {
            OperationType = OPerationType,
            Token = token,
            UserLanguage = Language,
            DataMessage = object_data
        }
    };

    string result = db.Prompt($"POST {server_db.Prompt("VAR server_tokens_url")} MESSAGE {JsonConvert.SerializeObject(d, Formatting.Indented)}", true);
    AngelDB.AngelResponce responce = JsonConvert.DeserializeObject<AngelDB.AngelResponce>(result);
    return responce.result;

}

