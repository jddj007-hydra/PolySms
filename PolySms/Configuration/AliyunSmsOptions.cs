namespace PolySms.Configuration;

public class AliyunSmsOptions
{
    public const string SectionName = "AliyunSms";

    public string AccessKeyId { get; set; } = string.Empty;
    public string AccessKeySecret { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "dysmsapi.aliyuncs.com";
}