namespace PolySms.Providers.Http;

public interface IHttpSmsClient
{
    Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> headers, string content, CancellationToken cancellationToken = default);
}