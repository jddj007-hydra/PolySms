using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PolySms.Providers.Tencent;

public static class TencentSignatureHelper
{
    private const string Algorithm = "TC3-HMAC-SHA256";
    private const string Service = "sms";
    private const string Version = "2021-01-11";

    public static (string Url, Dictionary<string, string> Headers, string Body) BuildRequest(
        string endpoint,
        string region,
        string secretId,
        string secretKey,
        object requestData)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToString("yyyy-MM-dd");
        var requestBody = JsonSerializer.Serialize(requestData);

        var canonicalRequest = BuildCanonicalRequest("POST", "/", "", GetCanonicalHeaders(endpoint, timestamp), "content-type;host", Sha256Hex(requestBody));
        var credentialScope = $"{date}/{Service}/tc3_request";
        var stringToSign = $"{Algorithm}\n{timestamp}\n{credentialScope}\n{Sha256Hex(canonicalRequest)}";
        var signature = Sign(secretKey, date, stringToSign);

        var authorization = $"{Algorithm} Credential={secretId}/{credentialScope}, SignedHeaders=content-type;host, Signature={signature}";

        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = authorization,
            ["Content-Type"] = "application/json; charset=utf-8",
            ["Host"] = endpoint,
            ["X-TC-Action"] = "SendSms",
            ["X-TC-Timestamp"] = timestamp.ToString(),
            ["X-TC-Version"] = Version,
            ["X-TC-Region"] = region
        };

        return ($"https://{endpoint}/", headers, requestBody);
    }

    private static string BuildCanonicalRequest(string httpMethod, string canonicalUri, string canonicalQueryString,
        string canonicalHeaders, string signedHeaders, string hashedPayload)
    {
        return $"{httpMethod}\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{hashedPayload}";
    }

    private static string GetCanonicalHeaders(string endpoint, long timestamp)
    {
        return $"content-type:application/json; charset=utf-8\nhost:{endpoint}\n";
    }

    private static string Sign(string secretKey, string date, string stringToSign)
    {
        var kDate = HmacSha256(Encoding.UTF8.GetBytes($"TC3{secretKey}"), date);
        var kService = HmacSha256(kDate, Service);
        var kSigning = HmacSha256(kService, "tc3_request");
        var signature = HmacSha256(kSigning, stringToSign);
        return BitConverter.ToString(signature).Replace("-", "").ToLowerInvariant();
    }

    private static byte[] HmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string Sha256Hex(string data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}