using PolySms.Configuration;
using PolySms.Enums;
using PolySms.Interfaces;
using PolySms.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PolySms.Services;

public class SmsService : ISmsService
{
    private readonly IEnumerable<ISmsProvider> _providers;
    private readonly ILogger<SmsService> _logger;
    private readonly SmsOptions _smsOptions;

    public SmsService(
        IEnumerable<ISmsProvider> providers,
        ILogger<SmsService> logger,
        IOptions<SmsOptions> smsOptions)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _smsOptions = smsOptions?.Value ?? new SmsOptions();
    }

    public async Task<SmsResponse> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var defaultProvider = _smsOptions.DefaultProvider;
        _logger.LogInformation("Using default provider: {Provider}", defaultProvider);

        var response = await SendSmsAsync(request, defaultProvider, cancellationToken);

        if (!response.IsSuccess && _smsOptions.EnableFailover)
        {
            _logger.LogWarning("Default provider {Provider} failed, trying failover providers", defaultProvider);

            foreach (var providerName in _smsOptions.ProviderPriority.Where(p => p != defaultProvider))
            {
                if (IsProviderAvailable(providerName))
                {
                    _logger.LogInformation("Trying failover provider: {Provider}", providerName);
                    response = await SendSmsAsync(request, providerName, cancellationToken);

                    if (response.IsSuccess)
                    {
                        _logger.LogInformation("Failover successful with provider: {Provider}", providerName);
                        break;
                    }
                }
            }
        }

        return response;
    }

    public async Task<SmsResponse> SendSmsAsync(SmsRequest request, string providerName, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(providerName))
        {
            _logger.LogError("Provider name cannot be null or empty");
            return new SmsResponse
            {
                IsSuccess = false,
                ErrorCode = "INVALID_PROVIDER_NAME",
                ErrorMessage = "Provider name cannot be null or empty"
            };
        }

        var provider = _providers.FirstOrDefault(p =>
            string.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            _logger.LogError("Provider {ProviderName} not found", providerName);
            return new SmsResponse
            {
                IsSuccess = false,
                ErrorCode = "PROVIDER_NOT_FOUND",
                ErrorMessage = $"Provider {providerName} not found",
                Provider = providerName
            };
        }

        try
        {
            _logger.LogInformation("Sending SMS via {Provider} to {PhoneNumber}",
                provider.ProviderName, request.PhoneNumber);

            var response = await provider.SendSmsAsync(request, cancellationToken);
            response.Provider = provider.ProviderName;

            if (response.IsSuccess)
            {
                _logger.LogInformation("SMS sent successfully via {Provider}, RequestId: {RequestId}",
                    provider.ProviderName, response.RequestId);
            }
            else
            {
                _logger.LogWarning("SMS failed via {Provider}, Error: {ErrorCode} - {ErrorMessage}",
                    provider.ProviderName, response.ErrorCode, response.ErrorMessage);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS via {Provider}", provider.ProviderName);
            return new SmsResponse
            {
                IsSuccess = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message,
                Provider = provider.ProviderName
            };
        }
    }

    public async Task<SmsResponse> SendSmsAsync(SmsRequest request, SmsProvider provider, CancellationToken cancellationToken = default)
    {
        var providerName = provider switch
        {
            SmsProvider.Aliyun => "Aliyun",
            SmsProvider.Tencent => "Tencent",
            SmsProvider.Auto => _smsOptions.DefaultProvider,
            _ => _smsOptions.DefaultProvider
        };

        return await SendSmsAsync(request, providerName, cancellationToken);
    }

    public IEnumerable<string> GetAvailableProviders()
    {
        return _providers.Select(p => p.ProviderName).ToList();
    }

    public bool IsProviderAvailable(string providerName)
    {
        return _providers.Any(p =>
            string.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsProviderAvailable(SmsProvider provider)
    {
        var providerName = provider switch
        {
            SmsProvider.Aliyun => "Aliyun",
            SmsProvider.Tencent => "Tencent",
            SmsProvider.Auto => _smsOptions.DefaultProvider,
            _ => string.Empty
        };

        return !string.IsNullOrEmpty(providerName) && IsProviderAvailable(providerName);
    }
}