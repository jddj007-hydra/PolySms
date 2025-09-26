using PolySms.Configuration;
using PolySms.Enums;
using PolySms.Extensions;
using PolySms.Interfaces;
using PolySms.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            sms.DefaultProvider = "Tencent";//"Aliyun";
            sms.EnableFailover = false;       // 关闭故障转移
            sms.DefaultSignName = "宜宾市审计局"; // 设置默认短信签名
            sms.ProviderPriority = new List<string> { "Aliyun", "Tencent" };
        },
        aliyun => {
            aliyun.AccessKeyId = "";
            aliyun.AccessKeySecret = "";
        },
        tencent => {
            tencent.SecretId = "";
            tencent.SecretKey = "";
            tencent.SmsSdkAppId = "";
        });
});


var host = builder.Build();

// 获取服务
var smsService = host.Services.GetRequiredService<ISmsService>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var smsOptions = host.Services.GetRequiredService<IOptions<SmsOptions>>();

// 创建根据提供商生成请求的函数
static SmsRequest CreateRequestForProvider(string providerName, ILogger logger)
{
    if (providerName == "Tencent")
    {
        logger.LogInformation("使用腾讯云模板配置");
        return new SmsRequest
        {
            PhoneNumber = "15755392871",
            TemplateId = "2528873",
            TemplateParams = new Dictionary<string, string>
            {
                { "1", "1237" },
            }
        };
    }
    else
    {
        logger.LogInformation("使用阿里云模板配置");
        return new SmsRequest
        {
            PhoneNumber = "15755392871",
            TemplateId = "SMS_496030165",
            TemplateParams = new Dictionary<string, string>
            {
                { "code", "1236" },
            }
        };
    }
}

// 根据默认提供商创建请求
var defaultProvider = smsOptions.Value.DefaultProvider;
var request = CreateRequestForProvider(defaultProvider, logger);

try
{
    logger.LogInformation("=== PolySms 发送测试 ===");

    // 指定提供商发送短信（避免故障转移时的模板混淆）
    logger.LogInformation("发送短信中...");
    var response = await smsService.SendSmsAsync(request, defaultProvider);
    LogResponse(logger, response, "发送结果");

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