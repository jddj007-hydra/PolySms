using System.Security.Cryptography;
using System.Text;

namespace PolySms.Providers.Aliyun;

public static class AliyunSignatureHelper
{
    private const string SignatureVersion = "1.0";
    private const string SignatureMethod = "HMAC-SHA1";

    public static (string Url, Dictionary<string, string> Headers) BuildRequest(
        string endpoint,
        string accessKeyId,
        string accessKeySecret,
        Dictionary<string, string> parameters)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var nonce = Guid.NewGuid().ToString();

        var allParameters = new Dictionary<string, string>(parameters)
        {
            ["AccessKeyId"] = accessKeyId,
            ["SignatureVersion"] = SignatureVersion,
            ["SignatureMethod"] = SignatureMethod,
            ["Timestamp"] = timestamp,
            ["SignatureNonce"] = nonce,
            ["Format"] = "JSON",
            ["Version"] = "2017-05-25"
        };

        var signature = CalculateSignature(accessKeySecret, allParameters);
        allParameters["Signature"] = signature;

        var queryString = string.Join("&",
            allParameters.OrderBy(kv => kv.Key)
                        .Select(kv => $"{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}"));

        var url = $"https://{endpoint}/?{queryString}";

        return (url, new Dictionary<string, string>
        {
            ["User-Agent"] = "PolySms/1.0.0"
        });
    }

    private static string CalculateSignature(string accessKeySecret, Dictionary<string, string> parameters)
    {
        // 阿里云签名算法：
        // 1. 排除Signature参数
        // 2. 按Key字典序排序
        // 3. 每个参数都需要进行URL编码
        // 4. 构造规范化查询字符串

        var sortedParams = parameters.Where(kv => kv.Key != "Signature")
                                  .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                                  .Select(kv => $"{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}");

        var canonicalizedQueryString = string.Join("&", sortedParams);

        // 构造待签名字符串: HTTPMethod&URI&CanonicalizedQueryString
        var stringToSign = $"POST&{UrlEncode("/")}&{UrlEncode(canonicalizedQueryString)}";

        var key = Encoding.UTF8.GetBytes(accessKeySecret + "&");
        var data = Encoding.UTF8.GetBytes(stringToSign);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }

    private static string UrlEncode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Uri.EscapeDataString(value)
                  .Replace("+", "%20")
                  .Replace("*", "%2A")
                  .Replace("%7E", "~")
                  .Replace("%21", "!")
                  .Replace("%27", "'")
                  .Replace("%28", "(")
                  .Replace("%29", ")")
                  .Replace("%7E", "~");
    }
}