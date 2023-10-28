using System;

public class HelpdeskTopics 
{
    public string Id { get; set; }
    public string Topic { get; set; }
    public string Description { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedAt { get; set; }
}

public class HelpdeskSubTopics
{
    public string Id { get; set; }
    public string Topic_id { get; set; }
    public string Subtopic { get; set; }
    public string Description { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedAt { get; set; }
}


public class HelpdeskContent
{
    public string Id { get; set; }
    public string Subtopic_id { get; set; }
    public string Content_title { get; set; }
    public string Description { get; set; }
    public string IsPublic { get; set; }
    public string Status { get; set; }
    public string Version { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedAt { get; set; }
}



public class HelpdeskContentDetails
{
    public string Id { get; set; }
    public string Content_id { get; set; }
    public string Content_title { get; set; }
    public string Content { get; set; }
    public string Content_type { get; set; }
    public int Content_order { get; set; }
    public string IsPublic { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedAt { get; set; }
}