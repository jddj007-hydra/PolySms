using PolySms.Configuration;
using PolySms.Enums;
using PolySms.Extensions;
using PolySms.Interfaces;
using PolySms.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // 添加日志
    services.AddLogging(config =>
    {
        config.AddConsole();
        config.SetMinimumLevel(LogLevel.Information);
    });

    // 配置PolySms（使用全局配置）
    services.AddPolySms(
        sms => {
            sms.DefaultProvider = "Tencent";  // 设置腾讯云为默认提供商
            sms.EnableFailover = true;        // 启用故障转移
            sms.ProviderPriority = new List<string> { "Tencent", "Aliyun" }; // 优先级：腾讯云 > 阿里云
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
});

var host = builder.Build();

// 获取服务
var smsService = host.Services.GetRequiredService<ISmsService>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

// 创建测试请求
var request = new SmsRequest
{
    PhoneNumber = "13800138000",
    TemplateId = "SMS_001",
    SignName = "测试签名",
    TemplateParams = new Dictionary<string, string>
    {
        { "code", "123456" },
        { "name", "张三" }
    }
};

try
{
    logger.LogInformation("=== PolySms 使用示例 ===");

    // 1. 使用默认提供商发送（按配置使用腾讯云）
    logger.LogInformation("1. 使用默认提供商发送短信");
    var response1 = await smsService.SendSmsAsync(request);
    LogResponse(logger, response1, "默认提供商");

    // 2. 指定使用阿里云发送（字符串方式）
    logger.LogInformation("2. 指定使用阿里云发送短信（字符串方式）");
    var response2 = await smsService.SendSmsAsync(request, "Aliyun");
    LogResponse(logger, response2, "阿里云");

    // 3. 指定使用腾讯云发送（枚举方式）
    logger.LogInformation("3. 指定使用腾讯云发送短信（枚举方式）");
    var response3 = await smsService.SendSmsAsync(request, SmsProvider.Tencent);
    LogResponse(logger, response3, "腾讯云");

    // 4. 显示可用的提供商列表
    logger.LogInformation("4. 可用的提供商列表");
    var availableProviders = smsService.GetAvailableProviders();
    logger.LogInformation("可用提供商: {Providers}", string.Join(", ", availableProviders));

    // 5. 检查提供商是否可用
    logger.LogInformation("5. 检查提供商可用性");
    logger.LogInformation("阿里云是否可用: {Available}", smsService.IsProviderAvailable("Aliyun"));
    logger.LogInformation("腾讯云是否可用: {Available}", smsService.IsProviderAvailable(SmsProvider.Tencent));
    logger.LogInformation("不存在的提供商是否可用: {Available}", smsService.IsProviderAvailable("NotExist"));

    // 6. 测试故障转移功能
    logger.LogInformation("6. 测试故障转移功能");
    logger.LogInformation("当默认提供商失败时，会自动尝试其他提供商（如果配置了EnableFailover=true）");

}
catch (Exception ex)
{
    logger.LogError(ex, "发送短信时发生异常");
}

logger.LogInformation("=== 示例程序执行完成 ===");

static void LogResponse(ILogger logger, SmsResponse response, string scenario)
{
    if (response.IsSuccess)
    {
        logger.LogInformation("✅ {Scenario} - 发送成功！Provider: {Provider}, RequestId: {RequestId}",
            scenario, response.Provider, response.RequestId);
    }
    else
    {
        logger.LogWarning("❌ {Scenario} - 发送失败！Provider: {Provider}, Error: {ErrorCode} - {ErrorMessage}",
            scenario, response.Provider, response.ErrorCode, response.ErrorMessage);
    }
}