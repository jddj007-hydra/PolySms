using PolySms.Models;

namespace PolySms.Interfaces;

public interface ISmsProvider
{
    string ProviderName { get; }
    Task<SmsResponse> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default);
}