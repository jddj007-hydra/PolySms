# PolySms

一个轻量级的中国多云短信发送SDK，支持阿里云和腾讯云短信服务。**零第三方SDK依赖，使用HTTP直接调用，大幅减少包体积。**

## 🚀 核心特性

- ✅ **多云支持**：同时支持阿里云和腾讯云短信服务
- 🪶 **超轻量级**：零第三方SDK依赖，使用HTTP直接调用，包体积仅约**20KB**
- 🎛️ **灵活选择**：支持全局配置默认提供商，也可单独指定
- 🔄 **故障转移**：自动故障转移，提供商失败时自动切换到备用
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
            return Ok(new { success = true, requestId = response.RequestId });
        }

        return BadRequest(new { success = false, error = response.ErrorMessage });
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

### 智能提供商选择策略

```csharp
public async Task<SmsResponse> SendWithStrategy(SmsRequest request, string scenario)
{
    return scenario switch
    {
        "marketing" => await _smsService.SendSmsAsync(request, "Tencent"), // 营销短信用腾讯云
        "verification" => await _smsService.SendSmsAsync(request, "Aliyun"), // 验证码用阿里云
        "notification" => await _smsService.SendSmsAsync(request), // 通知短信用默认
        _ => await _smsService.SendSmsAsync(request)
    };
}
```

## 📊 技术架构

PolySms采用全新的轻量级架构：

- **HTTP直接调用**：绕过厚重的官方SDK，直接使用HTTP调用云服务API
- **自研签名算法**：内置阿里云RPC签名和腾讯云TC3-HMAC-SHA256签名算法
- **零外部依赖**：除.NET标准库外，无任何第三方依赖
- **统一抽象接口**：为不同云厂商提供统一的调用接口



## 🚀 性能优势

- **超轻量级**：无需加载大型SDK程序集，启动快速
- **低内存占用**：更少的依赖意味着更少的内存开销
- **部署体积小**：显著减少Docker镜像和部署包大小

## 📄 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件。

---

**让短信发送变得简单高效！** 🚀