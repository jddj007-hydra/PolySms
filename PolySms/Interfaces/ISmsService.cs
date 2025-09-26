using PolySms.Enums;
using PolySms.Models;

namespace PolySms.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// 使用默认提供商发送短信
    /// </summary>
    Task<SmsResponse> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用指定提供商发送短信（字符串方式）
    /// </summary>
    Task<SmsResponse> SendSmsAsync(SmsRequest request, string providerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用指定提供商发送短信（枚举方式）
    /// </summary>
    Task<SmsResponse> SendSmsAsync(SmsRequest request, SmsProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有可用的提供商列表
    /// </summary>
    IEnumerable<string> GetAvailableProviders();

    /// <summary>
    /// 检查指定提供商是否可用
    /// </summary>
    bool IsProviderAvailable(string providerName);

    /// <summary>
    /// 检查指定提供商是否可用（枚举方式）
    /// </summary>
    bool IsProviderAvailable(SmsProvider provider);
}