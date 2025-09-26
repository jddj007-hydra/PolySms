# PolySms

一个轻量级的中国多云短信发送SDK，支持阿里云和腾讯云短信服务。**零第三方SDK依赖，使用HTTP直接调用，大幅减少包体积。**

## 🚀 核心特性

- ✅ **多云支持**：同时支持阿里云和腾讯云短信服务
- 🪶 **超轻量级**：零第三方SDK依赖，使用HTTP直接调用，包体积仅约**20KB**
- 🎛️ **灵活选择**：支持全局配置默认提供商，也可单独指定
- 🔄 **故障转移**：自动故障转移，提供商失败时自动切换到备用
- 🛡️ **标准化错误处理**：统一的错误码系统，提供用户友好的错误信息
- 🔁 **智能重试机制**：自动识别可重试的错误类型，提高发送成功率
- 📊 **完善监控**：详细的日志记录和错误处理
- 🧪 **测试完备**：完整的单元测试覆盖
- 📦 **易于集成**：标准的.NET依赖注入支持

## 📦 安装

```bash
# 只需安装一个包，包含所有功能
dotnet add package PolySms
```


## ⚙️ 配置方式

### 1. 代码配置（推荐用于简单场景）

```csharp
using PolySms.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 配置PolySms
builder.Services.AddPolySms(
    sms => {
        sms.DefaultProvider = "Tencent";        // 设置默认提供商
        sms.EnableFailover = true;             // 启用故障转移
        sms.ProviderPriority = new List<string> { "Tencent", "Aliyun" }; // 优先级
    },
    aliyun => {
        aliyun.AccessKeyId = "your-aliyun-access-key-id";
        aliyun.AccessKeySecret = "your-aliyun-access-key-secret";
    },
    tencent => {
        tencent.SecretId = "your-tencent-secret-id";
        tencent.SecretKey = "your-tencent-secret-key";
        tencent.SmsSdkAppId = "your-sms-sdk-app-id";
    });

var app = builder.Build();
```

### 2. 配置文件方式（推荐用于生产环境）

**appsettings.json**：
```json
{
  "Sms": {
    "DefaultProvider": "Aliyun",
    "EnableFailover": true,
    "ProviderPriority": ["Aliyun", "Tencent"]
  },
  "AliyunSms": {
    "AccessKeyId": "your-aliyun-access-key-id",
    "AccessKeySecret": "your-aliyun-access-key-secret",
    "Endpoint": "dysmsapi.aliyuncs.com"
  },
  "TencentSms": {
    "SecretId": "your-tencent-secret-id",
    "SecretKey": "your-tencent-secret-key",
    "Region": "ap-beijing",
    "SmsSdkAppId": "your-sms-sdk-app-id"
  }
}
```

**Program.cs**：
```csharp
// 从 appsettings.json 读取
builder.Services.AddPolySms(builder.Configuration);
```

## 🎯 使用方式

### 基本用法

```csharp
public class SmsController : ControllerBase
{
    private readonly ISmsService _smsService;

    public SmsController(ISmsService smsService)
    {
        _smsService = smsService;
    }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendCodeRequest request)
    {
        var smsRequest = new SmsRequest
        {
            PhoneNumber = request.PhoneNumber,
            TemplateId = "SMS_VERIFICATION",
            SignName = "您的应用",
            TemplateParams = new Dictionary<string, string>
            {
                { "code", GenerateCode() },
                { "expire", "5" }
            }
        };

        // 使用配置的默认提供商发送
        var response = await _smsService.SendSmsAsync(smsRequest);

        if (response.IsSuccess)
        {
            return Ok(new {
                success = true,
                requestId = response.RequestId,
                provider = response.Provider
            });
        }

        return BadRequest(new {
            success = false,
            errorCode = response.ErrorCode,
            errorMessage = response.ErrorMessage,
            friendlyMessage = response.FriendlyErrorMessage,
            isRetryable = response.IsRetryable,
            provider = response.Provider
        });
    }
}
```

### 指定提供商发送

```csharp
// 字符串方式指定
var response1 = await _smsService.SendSmsAsync(request, "Aliyun");
var response2 = await _smsService.SendSmsAsync(request, "Tencent");

// 枚举方式指定（类型安全）
var response3 = await _smsService.SendSmsAsync(request, SmsProvider.Aliyun);
var response4 = await _smsService.SendSmsAsync(request, SmsProvider.Tencent);
```

## 🔧 高级功能

### 故障转移机制

当启用 `EnableFailover = true` 时，如果默认提供商发送失败，系统会自动按照 `ProviderPriority` 的顺序尝试其他提供商。

### 智能错误处理和重试策略

```csharp
public class SmartSmsService
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SmartSmsService> _logger;

    public SmartSmsService(ISmsService smsService, ILogger<SmartSmsService> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<SmsResponse> SendWithRetry(SmsRequest request, int maxRetries = 3)
    {
        var response = await _smsService.SendSmsAsync(request);

        int retryCount = 0;
        while (!response.IsSuccess && response.IsRetryable && retryCount < maxRetries)
        {
            retryCount++;
            _logger.LogWarning("发送失败但可重试，第 {RetryCount} 次重试: {ErrorMessage}",
                retryCount, response.FriendlyErrorMessage);

            // 根据错误类型使用不同的重试延迟
            var delay = response.StandardErrorCode switch
            {
                StandardErrorCode.RateLimitExceeded => TimeSpan.FromSeconds(60), // 频率限制等待1分钟
                StandardErrorCode.NetworkError => TimeSpan.FromSeconds(5),       // 网络错误等待5秒
                _ => TimeSpan.FromSeconds(10)                                     // 其他错误等待10秒
            };

            await Task.Delay(delay);
            response = await _smsService.SendSmsAsync(request);
        }

        return response;
    }
}
```

## 📊 技术架构

PolySms采用全新的轻量级架构：

- **HTTP直接调用**：绕过厚重的官方SDK，直接使用HTTP调用云服务API
- **自研签名算法**：内置阿里云RPC签名和腾讯云TC3-HMAC-SHA256签名算法
- **零外部依赖**：除.NET标准库外，无任何第三方依赖
- **统一抽象接口**：为不同云厂商提供统一的调用接口
- **标准化错误处理**：统一错误码映射，提供一致的错误处理体验
- **智能重试机制**：自动识别可重试错误，提高发送成功率

## 🛡️ 错误处理系统

### 标准化错误码

PolySms提供统一的标准化错误码系统，将不同云服务商的错误码映射为统一的`StandardErrorCode`枚举：

```csharp
public enum StandardErrorCode
{
    Success,                    // 成功
    InvalidParameter,           // 参数错误
    AuthenticationFailed,       // 认证失败
    InsufficientPermissions,    // 权限不足
    InsufficientBalance,        // 余额不足
    RateLimitExceeded,          // 频率限制
    TemplateNotFound,           // 模板不存在
    SignatureNotFound,          // 签名不存在
    InvalidPhoneNumber,         // 手机号格式错误
    NetworkError,               // 网络错误
    ProviderInternalError,      // 服务商内部错误
    Unknown                     // 未知错误
}
```

### 增强的响应模型

```csharp
public class SmsResponse
{
    public bool IsSuccess { get; set; }                    // 是否成功
    public string RequestId { get; set; }                  // 请求ID
    public string BizId { get; set; }                      // 业务ID
    public string ErrorCode { get; set; }                  // 原始错误码
    public string ErrorMessage { get; set; }               // 原始错误信息
    public string Provider { get; set; }                   // 使用的提供商
    public StandardErrorCode StandardErrorCode { get; set; } // 标准化错误码
    public string FriendlyErrorMessage { get; set; }       // 用户友好的错误信息
    public bool IsRetryable { get; set; }                  // 是否可重试
}
```

### 使用示例

```csharp
var response = await _smsService.SendSmsAsync(request);

if (!response.IsSuccess)
{
    switch (response.StandardErrorCode)
    {
        case StandardErrorCode.RateLimitExceeded:
            // 频率限制，稍后重试
            _logger.LogWarning("发送频率过高: {Message}", response.FriendlyErrorMessage);
            break;
        case StandardErrorCode.InsufficientBalance:
            // 余额不足，需要充值
            _logger.LogError("账户余额不足: {Message}", response.FriendlyErrorMessage);
            break;
        case StandardErrorCode.TemplateNotFound:
            // 模板不存在，需要检查模板ID
            _logger.LogError("短信模板未找到: {Message}", response.FriendlyErrorMessage);
            break;
        default:
            if (response.IsRetryable)
            {
                // 可重试的错误
                _logger.LogWarning("发送失败但可重试: {Message}", response.FriendlyErrorMessage);
            }
            else
            {
                // 不可重试的错误
                _logger.LogError("发送失败: {Message}", response.FriendlyErrorMessage);
            }
            break;
    }
}
```



## 🚀 性能优势

- **超轻量级**：无需加载大型SDK程序集，启动快速
- **低内存占用**：更少的依赖意味着更少的内存开销
- **部署体积小**：显著减少Docker镜像和部署包大小

## 📄 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件。

---

**让短信发送变得简单高效！** 🚀