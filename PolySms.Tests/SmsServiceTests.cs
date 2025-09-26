using PolySms.Core.Configuration;
using PolySms.Core.Enums;
using PolySms.Core.Interfaces;
using PolySms.Core.Models;
using PolySms.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace PolySms.Tests;

public class SmsServiceTests
{
    private readonly Mock<ILogger<SmsService>> _loggerMock;
    private readonly Mock<ISmsProvider> _aliyunProviderMock;
    private readonly Mock<ISmsProvider> _tencentProviderMock;
    private readonly Mock<IOptions<SmsOptions>> _optionsMock;
    private readonly SmsOptions _smsOptions;

    public SmsServiceTests()
    {
        _loggerMock = new Mock<ILogger<SmsService>>();
        _aliyunProviderMock = new Mock<ISmsProvider>();
        _tencentProviderMock = new Mock<ISmsProvider>();
        _optionsMock = new Mock<IOptions<SmsOptions>>();

        _smsOptions = new SmsOptions
        {
            DefaultProvider = "Aliyun",
            EnableFailover = true,
            ProviderPriority = new List<string> { "Aliyun", "Tencent" }
        };

        _optionsMock.Setup(x => x.Value).Returns(_smsOptions);

        _aliyunProviderMock.Setup(x => x.ProviderName).Returns("Aliyun");
        _tencentProviderMock.Setup(x => x.ProviderName).Returns("Tencent");
    }

    [Fact]
    public async Task SendSmsAsync_WithDefaultProvider_ShouldUseConfiguredDefault()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object, _tencentProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        var request = new SmsRequest
        {
            PhoneNumber = "13800138000",
            TemplateId = "SMS_001",
            SignName = "测试签名",
            TemplateParams = new Dictionary<string, string> { { "code", "123456" } }
        };

        var expectedResponse = new SmsResponse
        {
            IsSuccess = true,
            RequestId = "req-123",
            Provider = "Aliyun"
        };

        _aliyunProviderMock.Setup(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.SendSmsAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Aliyun", result.Provider);
        _aliyunProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _tencentProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendSmsAsync_WithFailoverEnabled_ShouldTrySecondProvider()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object, _tencentProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        var request = new SmsRequest { PhoneNumber = "13800138000", TemplateId = "SMS_001" };

        var failedResponse = new SmsResponse { IsSuccess = false, ErrorCode = "FAILED", Provider = "Aliyun" };
        var successResponse = new SmsResponse { IsSuccess = true, RequestId = "req-456", Provider = "Tencent" };

        _aliyunProviderMock.Setup(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(failedResponse);
        _tencentProviderMock.Setup(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(successResponse);

        // Act
        var result = await service.SendSmsAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Tencent", result.Provider);
        _aliyunProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _tencentProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendSmsAsync_WithEnumProvider_ShouldUseCorrectProvider()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object, _tencentProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        var request = new SmsRequest { PhoneNumber = "13800138000", TemplateId = "SMS_001" };
        var expectedResponse = new SmsResponse { IsSuccess = true, Provider = "Tencent" };

        _tencentProviderMock.Setup(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.SendSmsAsync(request, SmsProvider.Tencent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Tencent", result.Provider);
        _tencentProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _aliyunProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void GetAvailableProviders_ShouldReturnAllProviderNames()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object, _tencentProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = service.GetAvailableProviders();

        // Assert
        Assert.Contains("Aliyun", result);
        Assert.Contains("Tencent", result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void IsProviderAvailable_WithValidProvider_ShouldReturnTrue()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object, _tencentProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        // Act & Assert
        Assert.True(service.IsProviderAvailable("Aliyun"));
        Assert.True(service.IsProviderAvailable("Tencent"));
        Assert.True(service.IsProviderAvailable(SmsProvider.Aliyun));
        Assert.True(service.IsProviderAvailable(SmsProvider.Tencent));
    }

    [Fact]
    public void IsProviderAvailable_WithInvalidProvider_ShouldReturnFalse()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        // Act & Assert
        Assert.False(service.IsProviderAvailable("NotExist"));
    }

    [Fact]
    public async Task SendSmsAsync_WithSpecificProvider_ShouldUseCorrectProvider()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object, _tencentProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        var request = new SmsRequest { PhoneNumber = "13800138000", TemplateId = "SMS_001" };
        var expectedResponse = new SmsResponse { IsSuccess = true, Provider = "Tencent" };

        _tencentProviderMock.Setup(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.SendSmsAsync(request, "Tencent");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Tencent", result.Provider);
        _tencentProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _aliyunProviderMock.Verify(x => x.SendSmsAsync(It.IsAny<SmsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendSmsAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendSmsAsync(null!, "Aliyun"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendSmsAsync(null!, SmsProvider.Aliyun));
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendSmsAsync(null!));
    }

    [Fact]
    public async Task SendSmsAsync_WithProviderNotFound_ShouldReturnError()
    {
        // Arrange
        var providers = new List<ISmsProvider> { _aliyunProviderMock.Object };
        var service = new SmsService(providers, _loggerMock.Object, _optionsMock.Object);

        var request = new SmsRequest { PhoneNumber = "13800138000", TemplateId = "SMS_001" };

        // Act
        var result = await service.SendSmsAsync(request, "NotExist");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("PROVIDER_NOT_FOUND", result.ErrorCode);
        Assert.Equal("Provider NotExist not found", result.ErrorMessage);
    }
}