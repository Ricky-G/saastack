using AncillaryApplication;
using AncillaryInfrastructure.Api._3rdParties;
using Application.Interfaces;
using Application.Resources.Shared;
using Common;
using Common.Configuration;
using FluentAssertions;
using Infrastructure.Interfaces;
using Infrastructure.Shared.ApplicationServices.External;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Api.Operations.Shared._3rdParties.Mailgun;
using Microsoft.AspNetCore.Http;
using Moq;
using UnitTesting.Common;
using Xunit;

namespace AncillaryInfrastructure.UnitTests.Api._3rdParties;

[Trait("Category", "Unit")]
public class MailgunApiSpec
{
    private readonly MailgunApi _api;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private readonly Mock<IMailgunApplication> _mailgunApplication;
    private readonly MailgunSignature _signature;

    public MailgunApiSpec()
    {
        var recorder = new Mock<IRecorder>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _httpContextAccessor.Setup(hca => hca.HttpContext)
            .Returns(new DefaultHttpContext
            {
                Request = { IsHttps = true }
            });
        var caller = new Mock<ICallerContext>();
        caller.Setup(cc => cc.CallId).Returns("acallid");
        var callerFactory = new Mock<ICallerContextFactory>();
        callerFactory.Setup(cf => cf.Create())
            .Returns(caller.Object);
        _mailgunApplication = new Mock<IMailgunApplication>();
        var settings = new Mock<IConfigurationSettings>();
        settings.Setup(s =>
                s.Platform.GetString(MailgunClient.Constants.WebhookSigningKeySettingName, It.IsAny<string>()))
            .Returns("asecret");
        _signature = new MailgunSignature
        {
            Timestamp = "1",
            Token = "atoken",
            Signature = "bf106940253fa7477ba4b55a027126b70037ce9b00e67aa3bf4f5bab2775d3e1"
        };

        _api = new MailgunApi(recorder.Object, _httpContextAccessor.Object, callerFactory.Object,
            settings.Object, _mailgunApplication.Object);
    }

    [Fact]
    public async Task WhenNotifyWebhookEventAndNotHttps_ThenReturnsError()
    {
        _httpContextAccessor.Setup(hca => hca.HttpContext)
            .Returns(new DefaultHttpContext
            {
                Request = { IsHttps = false }
            });

        var result = await _api.NotifyWebhookEvent(new MailgunNotifyWebhookEventRequest
        {
            Signature = new MailgunSignature()
        }, CancellationToken.None);

        result().Should().BeError(ErrorCode.NotAuthenticated);
        _mailgunApplication.Verify(app => app.NotifyWebhookEvent(It.IsAny<ICallerContext>(),
            It.IsAny<MailgunEventData>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenNotifyWebhookEventAndInvalidSignature_ThenReturnsError()
    {
        var result = await _api.NotifyWebhookEvent(new MailgunNotifyWebhookEventRequest
        {
            Signature = new MailgunSignature()
        }, CancellationToken.None);

        result().Should().BeError(ErrorCode.NotAuthenticated);
        _mailgunApplication.Verify(app => app.NotifyWebhookEvent(It.IsAny<ICallerContext>(),
            It.IsAny<MailgunEventData>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task WhenNotifyWebhookEventAndWithNEvent_ThenReturnsEmptyResponse()
    {
        var result = await _api.NotifyWebhookEvent(new MailgunNotifyWebhookEventRequest
        {
            Signature = _signature,
            EventData = new MailgunEventData
            {
                Message = new MailgunMessage()
            }
        }, CancellationToken.None);

        result().Value.Should().BeOfType<EmptyResponse>();
        _mailgunApplication.Verify(app => app.NotifyWebhookEvent(It.IsAny<ICallerContext>(),
            It.IsAny<MailgunEventData>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenNotifyWebhookEvent_ThenNotifies()
    {
        _mailgunApplication.Setup(app => app.NotifyWebhookEvent(It.IsAny<ICallerContext>(),
                It.IsAny<MailgunEventData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);
        var eventData = new MailgunEventData
        {
            Event = "anevent"
        };

        var result = await _api.NotifyWebhookEvent(new MailgunNotifyWebhookEventRequest
        {
            Signature = _signature,
            EventData = eventData
        }, CancellationToken.None);

        result().Value.Should().BeOfType<EmptyResponse>();
        _mailgunApplication.Verify(app => app.NotifyWebhookEvent(It.Is<ICallerContext>(cc => cc.CallId == "acallid"),
            eventData, It.IsAny<CancellationToken>()));
    }
}