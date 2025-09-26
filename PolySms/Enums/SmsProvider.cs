namespace PolySms.Enums;

/// <summary>
/// 短信服务提供商枚举
/// </summary>
public enum SmsProvider
{
    /// <summary>
    /// 自动选择（使用默认配置）
    /// </summary>
    Auto = 0,

    /// <summary>
    /// 阿里云短信服务
    /// </summary>
    Aliyun = 1,

    /// <summary>
    /// 腾讯云短信服务
    /// </summary>
    Tencent = 2
}