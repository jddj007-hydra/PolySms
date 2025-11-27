namespace PolySms.Configuration;

public class TencentSmsOptions
{
    public const string SectionName = "TencentSms";

    public string SecretId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "ap-beijing";
    public string SmsSdkAppId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "sms.tencentcloudapi.com";
}