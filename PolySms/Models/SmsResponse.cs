using PolySms.Enums;

namespace PolySms.Models;

public class SmsResponse
{
    /// <summary>
    /// 是否发送成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 业务ID
    /// </summary>
    public string BizId { get; set; } = string.Empty;

    /// <summary>
    /// 原始错误代码（云服务商返回的原始错误码）
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 使用的提供商
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// 标准化错误码
    /// </summary>
    public StandardErrorCode StandardErrorCode { get; set; } = StandardErrorCode.Success;

    /// <summary>
    /// 用户友好的错误消息
    /// </summary>
    public string FriendlyErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 是否可重试
    /// </summary>
    public bool IsRetryable { get; set; } = false;
}