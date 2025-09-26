using PolySms.Configuration;
using PolySms.Interfaces;
using PolySms.Services;
using PolySms.Providers.Http;
using PolySms.Providers.Aliyun;
using PolySms.Providers.Tencent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PolySms.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPolySms(this IServiceCollection services)
    {
        services.AddHttpClient<IHttpSmsClient, HttpSmsClient>();
        services.AddScoped<ISmsProvider, AliyunSmsProvider>();
        services.AddScoped<ISmsProvider, TencentSmsProvider>();
        services.AddScoped<ISmsService, SmsService>();
        return services;
    }

    public static IServiceCollection AddPolySms(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));
        services.Configure<AliyunSmsOptions>(configuration.GetSection(AliyunSmsOptions.SectionName));
        services.Configure<TencentSmsOptions>(configuration.GetSection(TencentSmsOptions.SectionName));

        services.AddHttpClient<IHttpSmsClient, HttpSmsClient>();
        services.AddScoped<ISmsProvider, AliyunSmsProvider>();
        services.AddScoped<ISmsProvider, TencentSmsProvider>();
        services.AddScoped<ISmsService, SmsService>();
        return services;
    }

    public static IServiceCollection AddPolySms(this IServiceCollection services,
        Action<SmsOptions> configureSms,
        Action<AliyunSmsOptions> configureAliyun,
        Action<TencentSmsOptions> configureTencent)
    {
        services.Configure(configureSms);
        services.Configure(configureAliyun);
        services.Configure(configureTencent);

        services.AddHttpClient<IHttpSmsClient, HttpSmsClient>();
        services.AddScoped<ISmsProvider, AliyunSmsProvider>();
        services.AddScoped<ISmsProvider, TencentSmsProvider>();
        services.AddScoped<ISmsService, SmsService>();
        return services;
    }

    /// <summary>
    /// 从指定的配置文件加载PolySms配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configFilePath">配置文件路径，默认为 "config/sms.json"</param>
    /// <param name="optional">配置文件是否可选，默认为false</param>
    /// <param name="reloadOnChange">文件变更时是否重新加载，默认为true</param>
    /// <returns></returns>
    public static IServiceCollection AddPolySmsFromConfigFile(
        this IServiceCollection services,
        string configFilePath = "config/sms.json",
        bool optional = false,
        bool reloadOnChange = true)
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configFilePath, optional, reloadOnChange);

        var smsConfiguration = configBuilder.Build();

        services.Configure<SmsOptions>(smsConfiguration.GetSection(SmsOptions.SectionName));
        services.Configure<AliyunSmsOptions>(smsConfiguration.GetSection(AliyunSmsOptions.SectionName));
        services.Configure<TencentSmsOptions>(smsConfiguration.GetSection(TencentSmsOptions.SectionName));

        services.AddHttpClient<IHttpSmsClient, HttpSmsClient>();
        services.AddScoped<ISmsProvider, AliyunSmsProvider>();
        services.AddScoped<ISmsProvider, TencentSmsProvider>();
        services.AddScoped<ISmsService, SmsService>();

        return services;
    }

    /// <summary>
    /// 为HostBuilder添加SMS配置文件支持
    /// </summary>
    /// <param name="hostBuilder">主机构建器</param>
    /// <param name="configFilePath">配置文件路径</param>
    /// <param name="optional">是否可选</param>
    /// <param name="reloadOnChange">是否监听变化</param>
    /// <returns></returns>
    public static IHostBuilder AddSmsConfigFile(
        this IHostBuilder hostBuilder,
        string configFilePath = "config/sms.json",
        bool optional = false,
        bool reloadOnChange = true)
    {
        return hostBuilder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(configFilePath, optional, reloadOnChange);
        });
    }

    /// <summary>
    /// 向后兼容旧版本的API
    /// </summary>
    [Obsolete("请使用AddPolySms方法，此方法将在未来版本中移除")]
    public static IServiceCollection AddMergeSms(this IServiceCollection services, IConfiguration configuration)
    {
        return AddPolySms(services, configuration);
    }
}