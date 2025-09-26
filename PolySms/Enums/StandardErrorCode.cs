namespace PolySms.Enums;

/// <summary>
/// 统一的错误代码
/// </summary>
public enum StandardErrorCode
{
    /// <summary>
    /// 成功
    /// </summary>
    Success,

    /// <summary>
    /// 参数错误
    /// </summary>
    InvalidParameter,

    /// <summary>
    /// 认证失败
    /// </summary>
    AuthenticationFailed,

    /// <summary>
    /// 权限不足
    /// </summary>
    InsufficientPermissions,

    /// <summary>
    /// 余额不足
    /// </summary>
    InsufficientBalance,

    /// <summary>
    /// 频率限制
    /// </summary>
    RateLimitExceeded,

    /// <summary>
    /// 模板不存在
    /// </summary>
    TemplateNotFound,

    /// <summary>
    /// 签名不存在
    /// </summary>
    SignatureNotFound,

    /// <summary>
    /// 手机号格式错误
    /// </summary>
    InvalidPhoneNumber,

    /// <summary>
    /// 网络错误
    /// </summary>
    NetworkError,

    /// <summary>
    /// 服务商内部错误
    /// </summary>
    ProviderInternalError,

    /// <summary>
    /// 未知错误
    /// </summary>
    Unknown
}