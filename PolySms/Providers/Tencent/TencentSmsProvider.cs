using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolySms.Configuration;
using PolySms.Interfaces;
using PolySms.Models;
using PolySms.Providers.Http;
using PolySms.Helpers;
using PolySms.Enums;
using System.Text.Json;

namespace PolySms.Providers.Tencent;

public class TencentSmsProvider : ISmsProvider
{
    private readonly TencentSmsOptions _options;
    private readonly SmsOptions _smsOptions;
    private readonly ILogger<TencentSmsProvider> _logger;
    private readonly IHttpSmsClient _httpClient;

    public string ProviderName => "Tencent";

    public TencentSmsProvider(IOptions<TencentSmsOptions> options, IOptions<SmsOptions> smsOptions, ILogger<TencentSmsProvider> logger, IHttpSmsClient httpClient)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _smsOptions = smsOptions.Value ?? throw new ArgumentNullException(nameof(smsOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<SmsResponse> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取签名：优先使用请求中的签名，否则使用全局默认签名
            var signName = request.SignName ?? _smsOptions.DefaultSignName
                ?? throw new ArgumentException("SignName is required. Please set SignName in request or configure DefaultSignName in SmsOptions.");

            var requestData = new
            {
                PhoneNumberSet = new[] { request.PhoneNumber },
                SmsSdkAppId = _options.SmsSdkAppId,
                SignName = signName,
                TemplateId = request.TemplateId,
                TemplateParamSet = request.TemplateParams.Values.ToArray()
            };

            var (url, headers, body) = TencentSignatureHelper.BuildRequest(
                _options.Endpoint,
                _options.Region,
                _options.SecretId,
                _options.SecretKey,
                requestData);

            _logger.LogDebug("Sending SMS via Tencent to {PhoneNumber} with template {TemplateId}",
                request.PhoneNumber, request.TemplateId);

            // 记录调试日志
            DebugLogger.LogRequest(_logger, _smsOptions.EnableDebugLog, ProviderName, url, headers, body);

            var httpResponse = await _httpClient.PostAsync(url, headers, body, cancellationToken);
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            // 记录响应日志
            DebugLogger.LogResponse(_logger, _smsOptions.EnableDebugLog, ProviderName,
                (int)httpResponse.StatusCode, responseContent);

            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (responseJson.TryGetProperty("Response", out var response))
            {
                // 检查是否有错误
                if (response.TryGetProperty("Error", out var error))
                {
                    var errorCode = error.TryGetProperty("Code", out var errorCodeElement) ? errorCodeElement.GetString() ?? string.Empty : string.Empty;
                    var standardErrorCode = ErrorCodeMapper.MapTencentError(errorCode);
                    var friendlyMessage = ErrorCodeMapper.GetErrorMessage(standardErrorCode);
                    var isRetryable = ErrorCodeMapper.IsRetryableError(standardErrorCode);

                    return new SmsResponse
                    {
                        IsSuccess = false,
                        RequestId = response.TryGetProperty("RequestId", out var requestId) ? requestId.GetString() ?? string.Empty : string.Empty,
                        ErrorCode = errorCode,
                        ErrorMessage = error.TryGetProperty("Message", out var errorMessage) ? errorMessage.GetString() ?? string.Empty : string.Empty,
                        Provider = ProviderName,
                        StandardErrorCode = standardErrorCode,
                        FriendlyErrorMessage = friendlyMessage,
                        IsRetryable = isRetryable
                    };
                }

                // 处理成功响应
                var sendStatusSet = response.TryGetProperty("SendStatusSet", out var statusSet) && statusSet.GetArrayLength() > 0
                    ? statusSet[0] : (JsonElement?)null;

                var statusCode = sendStatusSet?.TryGetProperty("Code", out var code) == true ? code.GetString() ?? string.Empty : string.Empty;
                var isSuccess = statusCode == "Ok";
                var standardErrorCodeSuccess = ErrorCodeMapper.MapTencentError(statusCode);
                var friendlyMessageSuccess = ErrorCodeMapper.GetErrorMessage(standardErrorCodeSuccess);
                var isRetryableSuccess = ErrorCodeMapper.IsRetryableError(standardErrorCodeSuccess);

                var result = new SmsResponse
                {
                    IsSuccess = isSuccess,
                    RequestId = response.TryGetProperty("RequestId", out var requestId2) ? requestId2.GetString() ?? string.Empty : string.Empty,
                    BizId = sendStatusSet?.TryGetProperty("SerialNo", out var serialNo) == true ? serialNo.GetString() ?? string.Empty : string.Empty,
                    ErrorCode = statusCode,
                    ErrorMessage = sendStatusSet?.TryGetProperty("Message", out var statusMessage) == true ? statusMessage.GetString() ?? string.Empty : string.Empty,
                    Provider = ProviderName,
                    StandardErrorCode = standardErrorCodeSuccess,
                    FriendlyErrorMessage = friendlyMessageSuccess,
                    IsRetryable = isRetryableSuccess
                };

                return result;
            }

            var invalidResponseStandardCode = StandardErrorCode.ProviderInternalError;
            var invalidResponseFriendlyMessage = ErrorCodeMapper.GetErrorMessage(invalidResponseStandardCode);
            var invalidResponseIsRetryable = ErrorCodeMapper.IsRetryableError(invalidResponseStandardCode);

            return new SmsResponse
            {
                IsSuccess = false,
                ErrorCode = "INVALID_RESPONSE",
                ErrorMessage = "Invalid response format",
                Provider = ProviderName,
                StandardErrorCode = invalidResponseStandardCode,
                FriendlyErrorMessage = invalidResponseFriendlyMessage,
                IsRetryable = invalidResponseIsRetryable
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Tencent");

            var standardErrorCode = ex is HttpRequestException ? StandardErrorCode.NetworkError : StandardErrorCode.Unknown;
            var friendlyMessage = ErrorCodeMapper.GetErrorMessage(standardErrorCode);
            var isRetryable = ErrorCodeMapper.IsRetryableError(standardErrorCode);

            return new SmsResponse
            {
                IsSuccess = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message,
                Provider = ProviderName,
                StandardErrorCode = standardErrorCode,
                FriendlyErrorMessage = friendlyMessage,
                IsRetryable = isRetryable
            };
        }
    }
}