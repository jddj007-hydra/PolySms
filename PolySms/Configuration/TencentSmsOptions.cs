namespace PolySms.Configuration;

public class TencentSmsOptions
{
    public const string SectionName = "TencentSms";

    public string SecretId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "ap-beijing";
    public string SmsSdkAppId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "sms.tencentcloudapi.com";
    public bool UseHttps { get; set; } = true;
    /// <summary>
    /// 用于签名计算的原始endpoint（可选）
    /// 当使用内网代理时，配置此项为腾讯云官方endpoint，而Endpoint配置为代理地址
    /// 如果不配置，默认使用Endpoint进行签名计算
    /// </summary>
    public string? OriginEndpoint { get; set; }
}
