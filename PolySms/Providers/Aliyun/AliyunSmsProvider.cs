using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolySms.Configuration;
using PolySms.Interfaces;
using PolySms.Models;
using PolySms.Providers.Http;
using System.Text.Json;

namespace PolySms.Providers.Aliyun;

public class AliyunSmsProvider : ISmsProvider
{
    private readonly AliyunSmsOptions _options;
    private readonly ILogger<AliyunSmsProvider> _logger;
    private readonly IHttpSmsClient _httpClient;

    public string ProviderName => "Aliyun";

    public AliyunSmsProvider(IOptions<AliyunSmsOptions> options, ILogger<AliyunSmsProvider> logger, IHttpSmsClient httpClient)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<SmsResponse> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["Action"] = "SendSms",
                ["PhoneNumbers"] = request.PhoneNumber,
                ["SignName"] = request.SignName,
                ["TemplateCode"] = request.TemplateId
            };

            if (request.TemplateParams.Count > 0)
            {
                parameters["TemplateParam"] = JsonSerializer.Serialize(request.TemplateParams);
            }

            var (url, headers) = AliyunSignatureHelper.BuildRequest(
                _options.Endpoint,
                _options.AccessKeyId,
                _options.AccessKeySecret,
                parameters);

            _logger.LogDebug("Sending SMS via Aliyun to {PhoneNumber} with template {TemplateId}",
                request.PhoneNumber, request.TemplateId);

            var httpResponse = await _httpClient.PostAsync(url, headers, string.Empty, cancellationToken);
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var result = new SmsResponse
            {
                IsSuccess = responseJson.GetProperty("Code").GetString() == "OK",
                RequestId = responseJson.TryGetProperty("RequestId", out var requestId) ? requestId.GetString() ?? string.Empty : string.Empty,
                BizId = responseJson.TryGetProperty("BizId", out var bizId) ? bizId.GetString() ?? string.Empty : string.Empty,
                ErrorCode = responseJson.TryGetProperty("Code", out var code) ? code.GetString() ?? string.Empty : string.Empty,
                ErrorMessage = responseJson.TryGetProperty("Message", out var message) ? message.GetString() ?? string.Empty : string.Empty,
                Provider = ProviderName
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Aliyun");
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