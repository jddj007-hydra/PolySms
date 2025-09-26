using Microsoft.Extensions.Logging;

namespace PolySms.Providers.Http;

public class HttpSmsClient : IHttpSmsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpSmsClient> _logger;

    public HttpSmsClient(HttpClient httpClient, ILogger<HttpSmsClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> headers, string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            foreach (var header in headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    // 特殊处理Authorization header
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TC3-HMAC-SHA256", header.Value.Substring("TC3-HMAC-SHA256 ".Length));
                }
                else if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    // Content-Type通过Content设置，跳过
                    continue;
                }
                else
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(content))
            {
                // 腾讯云需要application/json，阿里云不使用body
                if (headers.ContainsKey("Content-Type") && headers["Content-Type"].Contains("application/json"))
                {
                    request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
                }
                else
                {
                    request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                }
            }

            _logger.LogDebug("Sending HTTP request to {Url}", url);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            _logger.LogDebug("HTTP response status: {StatusCode}", response.StatusCode);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending HTTP request to {Url}", url);
            throw;
        }
    }
}