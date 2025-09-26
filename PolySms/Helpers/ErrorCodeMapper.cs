using PolySms.Enums;

namespace PolySms.Helpers;

/// <summary>
/// 错误代码映射器，将云服务商特有的错误码转换为统一的错误码
/// </summary>
public static class ErrorCodeMapper
{
    /// <summary>
    /// 阿里云错误码映射
    /// </summary>
    private static readonly Dictionary<string, StandardErrorCode> AliyunErrorMapping = new()
    {
        ["OK"] = StandardErrorCode.Success,
        ["InvalidParameter"] = StandardErrorCode.InvalidParameter,
        ["SignatureDoesNotMatch"] = StandardErrorCode.AuthenticationFailed,
        ["InvalidAccessKeyId.NotFound"] = StandardErrorCode.AuthenticationFailed,
        ["InvalidTimeStamp.Expired"] = StandardErrorCode.AuthenticationFailed,
        ["Forbidden.AccessKeyDisabled"] = StandardErrorCode.InsufficientPermissions,
        ["InsufficientBalance"] = StandardErrorCode.InsufficientBalance,
        ["Throttling.User"] = StandardErrorCode.RateLimitExceeded,
        ["InvalidTemplateCode.MalFormed"] = StandardErrorCode.TemplateNotFound,
        ["InvalidSignName.MalFormed"] = StandardErrorCode.SignatureNotFound,
        ["InvalidRecNum.MalFormed"] = StandardErrorCode.InvalidPhoneNumber,
        ["InternalError"] = StandardErrorCode.ProviderInternalError
    };

    /// <summary>
    /// 腾讯云错误码映射
    /// </summary>
    private static readonly Dictionary<string, StandardErrorCode> TencentErrorMapping = new()
    {
        ["Ok"] = StandardErrorCode.Success,
        ["InvalidParameter"] = StandardErrorCode.InvalidParameter,
        ["AuthFailure.SignatureFailure"] = StandardErrorCode.AuthenticationFailed,
        ["AuthFailure.SecretIdNotFound"] = StandardErrorCode.AuthenticationFailed,
        ["AuthFailure.TokenFailure"] = StandardErrorCode.AuthenticationFailed,
        ["UnauthorizedOperation"] = StandardErrorCode.InsufficientPermissions,
        ["RequestLimitExceeded"] = StandardErrorCode.RateLimitExceeded,
        ["InvalidParameterValue.TemplateIDInvalid"] = StandardErrorCode.TemplateNotFound,
        ["InvalidParameterValue.SignNameInvalid"] = StandardErrorCode.SignatureNotFound,
        ["InvalidParameterValue.PhoneNumberInvalid"] = StandardErrorCode.InvalidPhoneNumber,
        ["LimitExceeded.PhoneNumberDailyLimit"] = StandardErrorCode.RateLimitExceeded,
        ["InternalError"] = StandardErrorCode.ProviderInternalError
    };

    /// <summary>
    /// 统一错误码到用户友好消息的映射
    /// </summary>
    private static readonly Dictionary<StandardErrorCode, string> ErrorMessages = new()
    {
        [StandardErrorCode.Success] = "发送成功",
        [StandardErrorCode.InvalidParameter] = "参数错误",
        [StandardErrorCode.AuthenticationFailed] = "认证失败，请检查访问密钥",
        [StandardErrorCode.InsufficientPermissions] = "权限不足",
        [StandardErrorCode.InsufficientBalance] = "账户余额不足",
        [StandardErrorCode.RateLimitExceeded] = "发送频率超限，请稍后重试",
        [StandardErrorCode.TemplateNotFound] = "短信模板不存在",
        [StandardErrorCode.SignatureNotFound] = "短信签名不存在",
        [StandardErrorCode.InvalidPhoneNumber] = "手机号格式错误",
        [StandardErrorCode.NetworkError] = "网络连接错误",
        [StandardErrorCode.ProviderInternalError] = "服务商内部错误",
        [StandardErrorCode.Unknown] = "未知错误"
    };

    /// <summary>
    /// 将阿里云错误码映射为标准错误码
    /// </summary>
    public static StandardErrorCode MapAliyunError(string aliyunErrorCode)
    {
        return AliyunErrorMapping.TryGetValue(aliyunErrorCode, out var standardCode)
            ? standardCode
            : StandardErrorCode.Unknown;
    }

    /// <summary>
    /// 将腾讯云错误码映射为标准错误码
    /// </summary>
    public static StandardErrorCode MapTencentError(string tencentErrorCode)
    {
        return TencentErrorMapping.TryGetValue(tencentErrorCode, out var standardCode)
            ? standardCode
            : StandardErrorCode.Unknown;
    }

    /// <summary>
    /// 获取标准错误码对应的用户友好消息
    /// </summary>
    public static string GetErrorMessage(StandardErrorCode errorCode)
    {
        return ErrorMessages.TryGetValue(errorCode, out var message)
            ? message
            : ErrorMessages[StandardErrorCode.Unknown];
    }

    /// <summary>
    /// 判断错误是否可重试
    /// </summary>
    public static bool IsRetryableError(StandardErrorCode errorCode)
    {
        return errorCode switch
        {
            StandardErrorCode.NetworkError => true,
            StandardErrorCode.ProviderInternalError => true,
            StandardErrorCode.RateLimitExceeded => true,
            _ => false
        };
    }
}