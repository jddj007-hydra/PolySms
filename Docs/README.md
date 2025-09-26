# PolySms 文档中心

欢迎来到PolySms文档中心！这里包含了使用PolySms SDK所需的所有文档和指南。

## 🎆 PolySms特性

**PolySms** 是一个轻量级的中国多云短信发送SDK：

- 📦 **超轻量级**：仅约20KB，零第三方SDK依赖
- ⚡ **HTTP直接调用**：使用HTTP直接调用云服务API
- 🚀 **高性能**：快速的应用启动和运行性能
- 🔄 **多云支持**：同时支持阿里云和腾讯云短信服务
- 🛡️ **标准化错误处理**：统一错误码系统和友好错误信息
- 🔁 **智能重试机制**：自动识别可重试错误类型

## 📚 文档导航

### 🚀 快速开始
- **[快速开始指南](快速开始.md)** - 从安装到第一次发送短信的完整指南
- **[配置说明](配置说明.md)** - 详细的配置选项和最佳实践
- **[自定义配置指南](自定义配置文件使用指南.md)** - 企业级配置文件管理指南

### 🏠️ 架构与设计
- **[架构设计](架构设计.md)** - 轻量级HTTP架构设计原理和实现详解
- **[API差异处理](API差异处理.md)** - HTTP直接调用如何处理不同云服务商的API差异

### 📚 特殊配置
- **[腾讯云密钥配置说明](腾讯云密钥配置说明.md)** - 腾讯云特殊配置和注意事项

## 🎯 适用场景

PolySms SDK适用于以下场景：

### 🔐 身份验证
- 用户注册验证码
- 登录验证码
- 找回密码验证码
- 双因素认证(2FA)

### 📢 通知消息
- 订单状态通知
- 系统维护通知
- 重要提醒消息
- 安全告警

### 📱 营销推广
- 产品促销信息
- 活动邀请短信
- 会员服务通知
- 节日祝福

## 🔧 核心特性

### ✅ 多云支持
- 🌟 **阿里云短信服务** - 通过HTTP直接调用，零SDK依赖
- 🌟 **腾讯云短信服务** - 自研TC3签名算法，高性能直连
- 🔄 **统一API接口** - 一套代码，多个提供商，无需学习不同SDK

### 🎛️ 灵活配置
- 📄 **配置文件支持** - 支持appsettings.json配置
- 💻 **代码配置** - 支持代码中直接配置
- 🔒 **环境变量** - 支持环境变量和密钥管理
- 🏢 **多环境配置** - 开发、测试、生产环境隔离

### 🛡️ 企业级特性
- 📊 **详细日志** - 完整的结构化日志记录，支持请求追踪
- 🔍 **智能错误处理** - 标准化错误码系统和用户友好的错误信息
- ⚡ **高性能异步** - 基于HttpClient的高性能异步调用
- 🔒 **安全首位** - 自研签名算法，无第三方安全风险
- 🧪 **单元测试** - 完整的测试覆盖和连续集成
- 🔄 **智能重试** - 自动识别可重试错误，提高发送成功率

### 🔄 轻量级扩展
- 💫 **无依赖架构** - 无需第三方SDK，添加新提供商只需HTTP实现
- 🏠 **标准DI支持** - 完美集成.NET依赖注入系统
- 📦 **单包设计** - 只需安装一个包，包含所有功能
- ⚙️ **可配置架构** - 灵活的配置系统，支持多种配置方式

## 🏃‍♂️ 快速体验

### 1. 安装包
```bash
# 只需安装一个包，包含所有功能！
dotnet add package PolySms
```

### 2. 配置服务
```csharp
// 简单统一的注册方式
builder.Services.AddPolySms(builder.Configuration);
```

### 3. 发送短信
```csharp
// 使用示例
var request = new SmsRequest
{
    PhoneNumber = "13800138000",
    TemplateId = "SMS_001",
    SignName = "您的应用",
    TemplateParams = new Dictionary<string, string> { { "code", "123456" } }
};

// 使用默认提供商发送
var response = await smsService.SendSmsAsync(request);

if (response.IsSuccess)
{
    Console.WriteLine($"发送成功！RequestId: {response.RequestId}");
}
else
{
    Console.WriteLine($"发送失败: {response.FriendlyErrorMessage}");
    if (response.IsRetryable)
    {
        Console.WriteLine("该错误可以重试");
    }
}
```

## ⚡ 技术架构

**PolySms采用全新的轻量级架构**：
- 📞 **HTTP直接调用**：绕过厚重的官方SDK，直接使用HTTP调用云服务API
- 🔐 **自研签名算法**：内置阿里云RPC签名和腾讯云TC3-HMAC-SHA256签名算法
- ⚡ **零外部依赖**：除.NET标准库外，无任何第三方依赖
- 🎨 **统一抽象接口**：为不同云厂商提供统一的调用接口
- 🛡️ **标准化错误处理**：统一错误码映射系统，提供一致的错误处理体验

## 📋 API参考

### 核心接口

#### ISmsService
主要的短信发送服务接口，提供统一的短信发送能力。支持多提供商自动切换和故障转移。

```csharp
public interface ISmsService
{
    Task<SmsResponse> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default);
    Task<SmsResponse> SendSmsAsync(SmsRequest request, string providerName, CancellationToken cancellationToken = default);
    Task<SmsResponse> SendSmsAsync(SmsRequest request, SmsProvider provider, CancellationToken cancellationToken = default);
    IEnumerable<string> GetAvailableProviders();
    bool IsProviderAvailable(string providerName);
    bool IsProviderAvailable(SmsProvider provider);
}
```

#### SmsRequest
统一的短信发送请求模型。

```csharp
public class SmsRequest
{
    public string PhoneNumber { get; set; }           // 手机号码
    public string TemplateId { get; set; }            // 模板ID
    public string SignName { get; set; }              // 短信签名
    public Dictionary<string, string> TemplateParams { get; set; } // 模板参数
}
```

#### SmsResponse
统一的短信发送响应模型。包含详细的错误信息和请求追踪信息。

```csharp
public class SmsResponse
{
    public bool IsSuccess { get; set; }                    // 是否发送成功
    public string RequestId { get; set; }                  // 请求ID
    public string BizId { get; set; }                      // 业务ID
    public string ErrorCode { get; set; }                  // 原始错误代码
    public string ErrorMessage { get; set; }               // 原始错误消息
    public string Provider { get; set; }                   // 使用的提供商
    public StandardErrorCode StandardErrorCode { get; set; } // 标准化错误码
    public string FriendlyErrorMessage { get; set; }       // 用户友好的错误消息
    public bool IsRetryable { get; set; }                  // 是否可重试
}
```

#### StandardErrorCode
标准化错误码枚举，将不同云服务商的错误码统一为标准格式。

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

## 🛡️ 错误处理最佳实践

### 基本错误处理
```csharp
var response = await smsService.SendSmsAsync(request);

if (!response.IsSuccess)
{
    // 使用标准化错误码进行分类处理
    switch (response.StandardErrorCode)
    {
        case StandardErrorCode.RateLimitExceeded:
            _logger.LogWarning("发送频率过高，请稍后重试: {Message}", response.FriendlyErrorMessage);
            break;
        case StandardErrorCode.InsufficientBalance:
            _logger.LogError("账户余额不足: {Message}", response.FriendlyErrorMessage);
            // 发送余额不足通知
            break;
        case StandardErrorCode.TemplateNotFound:
            _logger.LogError("短信模板不存在: {Message}", response.FriendlyErrorMessage);
            // 检查模板配置
            break;
        default:
            if (response.IsRetryable)
            {
                _logger.LogWarning("发送失败但可重试: {Message}", response.FriendlyErrorMessage);
                // 实现重试逻辑
            }
            break;
    }
}
```

### 智能重试实现
```csharp
public class RetryableSmsService
{
    private readonly ISmsService _smsService;

    public async Task<SmsResponse> SendWithRetry(SmsRequest request, int maxRetries = 3)
    {
        var response = await _smsService.SendSmsAsync(request);

        int attempts = 1;
        while (!response.IsSuccess && response.IsRetryable && attempts < maxRetries)
        {
            var delay = response.StandardErrorCode switch
            {
                StandardErrorCode.RateLimitExceeded => TimeSpan.FromMinutes(1),
                StandardErrorCode.NetworkError => TimeSpan.FromSeconds(5),
                _ => TimeSpan.FromSeconds(10)
            };

            await Task.Delay(delay);
            attempts++;
            response = await _smsService.SendSmsAsync(request);
        }

        return response;
    }
}
```

## 🎓 学习路径

### 新手入门 (⭐⭐⭐)
1. 阅读 [快速开始指南](快速开始.md) - 了解PolySms的核心优势
2. 体验 5分钟快速集成
3. 对比传统SDK方案的性能差异

### 进阶使用 (⭐⭐⭐⭐)
1. 深入理解 [配置说明](配置说明.md) - 多种配置方式对比
2. 学习 [自定义配置指南](自定义配置文件使用指南.md) - 企业级配置管理
3. 掌握故障转移和智能重试策略

### 高级定制 (⭐⭐⭐⭐⭐)
1. 研究 [架构设计](架构设计.md) - HTTP直连架构原理
2. 理解 [API差异处理](API差异处理.md) - 签名算法实现原理
3. 扩展新的云服务提供商支持

## 🔗 相关链接

### 官方资源
- 🏠 [项目主页](https://github.com/yourname/PolySms)
- 📖 [API文档](https://docs.your-domain.com/polysms)
- 💬 [问题反馈](https://github.com/yourname/PolySms/issues)

### 云服务商文档
- 📘 [阿里云短信服务文档](https://help.aliyun.com/product/44282.html)
- 📗 [腾讯云短信文档](https://cloud.tencent.com/document/product/382)

### 示例和教程
- 💡 [示例项目](../Example/Program.cs)
- 📝 [最佳实践](../README.md#智能错误处理和重试策略)

## ❓ 常见问题

### Q: PolySms有什么优势？
**A:** PolySms的主要优势：
- 📦 **包体积极小**：仅约20KB，轻量级设计
- ⚡ **高性能**：快速启动，低内存占用
- 🔒 **更安全**：零第三方SDK依赖，无潜在安全风险
- 🔄 **多云支持**：统一接口支持多个云服务提供商
- 🛡️ **标准化错误处理**：统一的错误码系统和友好的错误信息

### Q: 如何选择短信服务提供商？
**A:** PolySms支持智能选择策略：
- 🔄 **自动故障转移**：主提供商失败时自动切换
- 🎯 **智能路由**：根据业务场景选择最优提供商
- 📊 **性能监控**：实时监控各提供商的性能表现
- ⚙️ **灵活配置**：支持多种配置方式和策略

### Q: 支持国际短信吗？
**A:** 支持程度取决于您选择的云服务提供商。阿里云和腾讯云都支持国际短信，但可能需要额外的认证和配置。

### Q: 如何处理发送失败的情况？
**A:** SDK提供了完善的错误处理机制：
1. 检查 `SmsResponse.IsSuccess` 判断是否成功
2. 通过 `StandardErrorCode` 获取标准化错误类型
3. 使用 `FriendlyErrorMessage` 获取用户友好的错误信息
4. 根据 `IsRetryable` 判断是否可以重试
5. 实现智能重试机制或切换到备用提供商

### Q: 可以同时使用多个提供商吗？
**A:** 当然可以！PolySms提供强大的多提供商支持：
1. ⚙️ **统一注册**：只需一行代码注册所有提供商
2. 🎯 **精准选择**：`SendSmsAsync(request, "Aliyun")` 或 `SmsProvider.Tencent`
3. 🔄 **自动转移**：配置优先级和故障转移策略
4. 📊 **负载均衡**：根据性能指标智能分配流量

### Q: 错误重试机制是如何工作的？
**A:** PolySms的智能重试机制：
1. 🔍 **自动识别**：系统自动识别可重试的错误类型
2. ⏱️ **智能延迟**：根据错误类型使用不同的重试间隔
3. 🎯 **精准重试**：只对网络错误、服务商内部错误等可重试错误进行重试
4. 🛡️ **防止滥用**：避免对配置错误、余额不足等不可重试错误进行重试

## 🤝 贡献指南

我们欢迎社区贡献！您可以通过以下方式参与：

### 💻 代码贡献
- Fork项目仓库
- 创建功能分支
- 提交Pull Request

### 📖 文档改进
- 发现文档错误或不清楚的地方
- 提供更好的示例代码
- 翻译文档到其他语言

### 🐛 问题反馈
- 报告Bug
- 提出功能建议
- 分享使用经验

## 📄 许可证

本项目基于 [MIT License](../LICENSE) 开源，您可以自由使用、修改和分发。

---

**选择PolySms，体验轻量级架构带来的高效短信服务！** 🚀

如有任何问题，请查看相关文档或在[GitHub Issues](https://github.com/yourname/PolySms/issues)中提问。