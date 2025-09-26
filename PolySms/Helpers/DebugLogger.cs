using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PolySms.Helpers;

/// <summary>
/// 调试日志辅助类，用于脱敏记录敏感信息
/// </summary>
public static class DebugLogger
{
    /// <summary>
    /// 记录HTTP请求详情（脱敏）
    /// </summary>
    public static void LogRequest(ILogger logger, bool enableDebugLog, string provider, string url,
        Dictionary<string, string> headers, string body)
    {
        if (!enableDebugLog || !logger.IsEnabled(LogLevel.Debug)) return;

        var sanitizedHeaders = SanitizeHeaders(headers);
        var sanitizedBody = SanitizeRequestBody(body);

        logger.LogDebug("[{Provider}] HTTP Request - URL: {Url}", provider, SanitizeUrl(url));
        logger.LogDebug("[{Provider}] HTTP Request - Headers: {Headers}", provider, JsonSerializer.Serialize(sanitizedHeaders));

        if (!string.IsNullOrEmpty(sanitizedBody))
        {
            logger.LogDebug("[{Provider}] HTTP Request - Body: {Body}", provider, sanitizedBody);
        }
    }

    /// <summary>
    /// 记录HTTP响应详情（脱敏）
    /// </summary>
    public static void LogResponse(ILogger logger, bool enableDebugLog, string provider,
        int statusCode, string responseContent)
    {
        if (!enableDebugLog || !logger.IsEnabled(LogLevel.Debug)) return;

        logger.LogDebug("[{Provider}] HTTP Response - Status: {StatusCode}", provider, statusCode);
        logger.LogDebug("[{Provider}] HTTP Response - Content: {Content}", provider,
            SanitizeResponseContent(responseContent));
    }

    /// <summary>
    /// 脱敏URL中的敏感参数
    /// </summary>
    private static string SanitizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return url;

        // 脱敏URL中的签名和密钥信息
        var sensitiveParams = new[] { "Signature", "AccessKeyId", "SignatureNonce" };

        foreach (var param in sensitiveParams)
        {
            var pattern = $"{param}=[^&]*";
            url = System.Text.RegularExpressions.Regex.Replace(url, pattern, $"{param}=***");
        }

        return url;
    }

    /// <summary>
    /// 脱敏HTTP头信息
    /// </summary>
    private static Dictionary<string, string> SanitizeHeaders(Dictionary<string, string> headers)
    {
        var sanitized = new Dictionary<string, string>();
        var sensitiveHeaders = new[] { "Authorization", "X-TC-Token" };

        foreach (var kvp in headers)
        {
            if (sensitiveHeaders.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
            {
                sanitized[kvp.Key] = "***";
            }
            else
            {
                sanitized[kvp.Key] = kvp.Value;
            }
        }

        return sanitized;
    }

    /// <summary>
    /// 脱敏请求体中的敏感信息
    /// </summary>
    private static string SanitizeRequestBody(string body)
    {
        if (string.IsNullOrEmpty(body)) return body;

        try
        {
            var jsonDoc = JsonDocument.Parse(body);
            var sensitizedJson = SanitizeJsonElement(jsonDoc.RootElement);
            return JsonSerializer.Serialize(sensitizedJson, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            // 如果不是JSON格式，直接返回（可能需要进一步处理）
            return body;
        }
    }

    /// <summary>
    /// 脱敏响应内容
    /// </summary>
    private static string SanitizeResponseContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;

        try
        {
            var jsonDoc = JsonDocument.Parse(content);
            var sensitizedJson = SanitizeJsonElement(jsonDoc.RootElement);
            return JsonSerializer.Serialize(sensitizedJson, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return content;
        }
    }

    /// <summary>
    /// 递归脱敏JSON元素
    /// </summary>
    private static object? SanitizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => SanitizeJsonObject(element),
            JsonValueKind.Array => element.EnumerateArray().Select(SanitizeJsonElement).ToArray(),
            JsonValueKind.String => SanitizeJsonString(element.GetString() ?? ""),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    /// <summary>
    /// 脱敏JSON对象
    /// </summary>
    private static Dictionary<string, object?> SanitizeJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object?>();
        var sensitiveFields = new[] { "SecretKey", "SecretId", "AccessKeySecret", "AccessKeyId", "TemplateParamSet" };

        foreach (var property in element.EnumerateObject())
        {
            if (sensitiveFields.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
            {
                result[property.Name] = "***";
            }
            else if (string.Equals(property.Name, "TemplateParamSet", StringComparison.OrdinalIgnoreCase))
            {
                // 对模板参数进行部分脱敏
                result[property.Name] = "*** (参数已脱敏)";
            }
            else
            {
                result[property.Name] = SanitizeJsonElement(property.Value);
            }
        }

        return result;
    }

    /// <summary>
    /// 脱敏字符串中的手机号等敏感信息
    /// </summary>
    private static string SanitizeJsonString(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // 脱敏手机号：保留前3位和后4位
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^1[3-9]\d{9}$"))
        {
            return value[..3] + "****" + value[^4..];
        }

        return value;
    }
}