// UploadModel.cs
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class FileUploadInfo
{
    public string FileName { get; set; }
    public int FileSize { get; set; }
    public string ContentType { get; set; }
    public string FileDirectory { get; set; }
    public string Url { get; set; }
    public bool ProceedToUpload { get; set; }
    public string ErrorMessage { get; set; }

}
