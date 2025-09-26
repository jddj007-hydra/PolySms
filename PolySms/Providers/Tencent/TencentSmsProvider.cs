using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolySms.Configuration;
using PolySms.Interfaces;
using PolySms.Models;
using PolySms.Providers.Http;
using System.Text.Json;

namespace PolySms.Providers.Tencent;

public class TencentSmsProvider : ISmsProvider
{
    private readonly TencentSmsOptions _options;
    private readonly ILogger<TencentSmsProvider> _logger;
    private readonly IHttpSmsClient _httpClient;

    public string ProviderName => "Tencent";

    public TencentSmsProvider(IOptions<TencentSmsOptions> options, ILogger<TencentSmsProvider> logger, IHttpSmsClient httpClient)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<SmsResponse> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestData = new
            {
                PhoneNumberSet = new[] { request.PhoneNumber },
                SmsSdkAppId = _options.SmsSdkAppId,
                SignName = request.SignName,
                TemplateId = request.TemplateId,
                TemplateParamSet = request.TemplateParams.Values.ToArray()
            };

            var (url, headers, body) = TencentSignatureHelper.BuildRequest(
                _options.Region,
                _options.SecretId,
                _options.SecretKey,
                requestData);

            _logger.LogDebug("Sending SMS via Tencent to {PhoneNumber} with template {TemplateId}",
                request.PhoneNumber, request.TemplateId);

            var httpResponse = await _httpClient.PostAsync(url, headers, body, cancellationToken);
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (responseJson.TryGetProperty("Response", out var response))
            {
                var sendStatusSet = response.TryGetProperty("SendStatusSet", out var statusSet) && statusSet.GetArrayLength() > 0
                    ? statusSet[0] : (JsonElement?)null;

                var result = new SmsResponse
                {
                    IsSuccess = sendStatusSet?.TryGetProperty("Code", out var code) == true && code.GetString() == "Ok",
                    RequestId = response.TryGetProperty("RequestId", out var requestId) ? requestId.GetString() ?? string.Empty : string.Empty,
                    BizId = sendStatusSet?.TryGetProperty("SerialNo", out var serialNo) == true ? serialNo.GetString() ?? string.Empty : string.Empty,
                    ErrorCode = sendStatusSet?.TryGetProperty("Code", out var errorCode) == true ? errorCode.GetString() ?? string.Empty : string.Empty,
                    ErrorMessage = sendStatusSet?.TryGetProperty("Message", out var message) == true ? message.GetString() ?? string.Empty : string.Empty,
                    Provider = ProviderName
                };

                return result;
            }

            return new SmsResponse
            {
                IsSuccess = false,
                ErrorCode = "INVALID_RESPONSE",
                ErrorMessage = "Invalid response format",
                Provider = ProviderName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Tencent");
            return new SmsResponse
            {
                IsSuccess = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message,
                Provider = ProviderName
            };
        }
    }
}