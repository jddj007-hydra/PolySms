namespace PolySms.Models;

public class SmsRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, string> TemplateParams { get; set; } = new();
    public string SignName { get; set; } = string.Empty;
}