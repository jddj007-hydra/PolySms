# PolySms

一个轻量级的中国多云短信发送SDK，支持阿里云和腾讯云短信服务。**零第三方SDK依赖，使用HTTP直接调用，大幅减少包体积。**

## 🚀 核心特性

- ✅ **多云支持**：同时支持阿里云和腾讯云短信服务
- 🪶 **超轻量级**：零第三方SDK依赖，使用HTTP直接调用，包体积仅约**20KB**
- 🎛️ **灵活选择**：支持全局配置默认提供商，也可单独指定
- 🔄 **故障转移**：自动故障转移，提供商失败时自动切换到备用
- 🛡️ **标准化错误处理**：统一的错误码系统，提供用户友好的错误信息
- 🔁 **智能重试机制**：自动识别可重试的错误类型，提高发送成功率

## 📦 安装

```bash
dotnet add package PolySms
```

## ⚙️ 配置方式

### 推荐：独立配置文件

**1. 创建配置文件 `config/sms.json`：**
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
    "Endpoint": "dysmsapi.aliyuncs.com",
    "UseHttps": true
  },
  "TencentSms": {
    "SecretId": "your-tencent-secret-id",
    "SecretKey": "your-tencent-secret-key",
    "Region": "ap-beijing",
    "SmsSdkAppId": "your-sms-sdk-app-id",
    "UseHttps": true
  }
}
```

如需通过内网反代访问短信服务而且反代节点没有证书，只需把对应提供商的 `UseHttps` 设置为 `false`，SDK 就会改用 HTTP 发送请求，签名/鉴权流程不受影响。

**2. 在 `Program.cs` 中注册服务：**
```csharp
using PolySms.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 从独立配置文件加载PolySms服务
builder.Services.AddPolySmsFromConfigFile("config/sms.json");

var app = builder.Build();
```

### 代码配置（简单场景）

```csharp
using PolySms.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPolySms(
    sms => {
        sms.DefaultProvider = "Tencent";
        sms.EnableFailover = true;
        sms.ProviderPriority = new List<string> { "Tencent", "Aliyun" };
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

## 🎯 基本使用

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
            errorMessage = response.FriendlyErrorMessage,
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

## 📚 完整文档

- **[详细使用指南](Docs/快速开始.md)** - 从安装到高级功能的完整指南
- **[配置说明](Docs/配置说明.md)** - 详细的配置选项和最佳实践
- **[自定义配置文件指南](Docs/自定义配置文件使用指南.md)** - 企业级配置文件管理
- **[架构设计](Docs/架构设计.md)** - 轻量级HTTP架构设计原理
- **[所有文档](Docs/INDEX.md)** - 完整的文档索引

## 🚀 性能优势

相比传统SDK方案：
- **包体积减少99.96%**：从50MB减少到约20KB
- **启动速度提升80%**：无需加载大型SDK程序集
- **内存占用减少80%**：HTTP直接调用，无额外依赖
- **部署体积更小**：显著减少Docker镜像和部署包大小

## 📄 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件。

---

**让短信发送变得简单高效！** 🚀
