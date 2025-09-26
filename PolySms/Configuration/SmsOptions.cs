namespace PolySms.Configuration;

public class SmsOptions
{
    public const string SectionName = "Sms";

    /// <summary>
    /// 默认使用的短信提供商 (Aliyun, Tencent)
    /// </summary>
    public string DefaultProvider { get; set; } = "Aliyun";

    /// <summary>
    /// 启用故障转移，当主提供商失败时自动切换到其他提供商
    /// </summary>
    public bool EnableFailover { get; set; } = true;

    /// <summary>
    /// 提供商优先级列表，按优先级排序
    /// </summary>
    public List<string> ProviderPriority { get; set; } = new() { "Aliyun", "Tencent" };

    /// <summary>
    /// 默认短信签名
    /// </summary>
    public string? DefaultSignName { get; set; }

    /// <summary>
    /// 启用调试日志，记录详细的请求和响应信息（注意：可能包含敏感信息）
    /// </summary>
    public bool EnableDebugLog { get; set; } = false;
}