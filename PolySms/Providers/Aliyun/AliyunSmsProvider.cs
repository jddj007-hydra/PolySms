using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolySms.Configuration;
using PolySms.Interfaces;
using PolySms.Models;
using PolySms.Providers.Http;
using PolySms.Helpers;
using PolySms.Enums;
using System.Text.Json;

namespace PolySms.Providers.Aliyun;

public class AliyunSmsProvider : ISmsProvider
{
    private readonly AliyunSmsOptions _options;
    private readonly SmsOptions _smsOptions;
    private readonly ILogger<AliyunSmsProvider> _logger;
    private readonly IHttpSmsClient _httpClient;

    public string ProviderName => "Aliyun";

    public AliyunSmsProvider(IOptions<AliyunSmsOptions> options, IOptions<SmsOptions> smsOptions, ILogger<AliyunSmsProvider> logger, IHttpSmsClient httpClient)
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

            var parameters = new Dictionary<string, string>
            {
                ["Action"] = "SendSms",
                ["PhoneNumbers"] = request.PhoneNumber,
                ["SignName"] = signName,
                ["TemplateCode"] = request.TemplateId
            };

            if (request.TemplateParams.Count > 0)
            {
                parameters["TemplateParam"] = JsonSerializer.Serialize(request.TemplateParams);
            }

            var (url, headers) = AliyunSignatureHelper.BuildRequest(
                _options.Endpoint,
                _options.UseHttps,
                _options.AccessKeyId,
                _options.AccessKeySecret,
                parameters);

            _logger.LogDebug("Sending SMS via Aliyun to {PhoneNumber} with template {TemplateId}",
                request.PhoneNumber, request.TemplateId);

            // 记录调试日志
            DebugLogger.LogRequest(_logger, _smsOptions.EnableDebugLog, ProviderName, url, headers, string.Empty);

            var httpResponse = await _httpClient.PostAsync(url, headers, string.Empty, cancellationToken);
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            // 记录响应日志
            DebugLogger.LogResponse(_logger, _smsOptions.EnableDebugLog, ProviderName,
                (int)httpResponse.StatusCode, responseContent);

            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var errorCode = responseJson.TryGetProperty("Code", out var code) ? code.GetString() ?? string.Empty : string.Empty;
            var isSuccess = errorCode == "OK";

            // 映射错误码
            var standardErrorCode = ErrorCodeMapper.MapAliyunError(errorCode);
            var friendlyMessage = ErrorCodeMapper.GetErrorMessage(standardErrorCode);
            var isRetryable = ErrorCodeMapper.IsRetryableError(standardErrorCode);

            var result = new SmsResponse
            {
                IsSuccess = isSuccess,
                RequestId = responseJson.TryGetProperty("RequestId", out var requestId) ? requestId.GetString() ?? string.Empty : string.Empty,
                BizId = responseJson.TryGetProperty("BizId", out var bizId) ? bizId.GetString() ?? string.Empty : string.Empty,
                ErrorCode = errorCode,
                ErrorMessage = responseJson.TryGetProperty("Message", out var message) ? message.GetString() ?? string.Empty : string.Empty,
                Provider = ProviderName,
                StandardErrorCode = standardErrorCode,
                FriendlyErrorMessage = friendlyMessage,
                IsRetryable = isRetryable
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Aliyun");

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
