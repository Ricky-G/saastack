using Common.Extensions;
using FluentAssertions;
using Infrastructure.Web.Api.Common.Extensions;
using Infrastructure.Web.Api.Interfaces;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Infrastructure.Web.Api.Common.UnitTests.Extensions;

[Trait("Category", "Unit")]
public class RequestExtensionsSpec
{
    [Fact]
    public void WhenGetRequestInfoAndNoAttribute_ThenThrows()
    {
        var request = new NoRouteRequest();

        request.Invoking(x => x.GetRequestInfo())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(
                Resources.RequestExtensions_MissingRouteAttribute.Format(nameof(NoRouteRequest),
                    nameof(RouteAttribute)));
    }

    [Fact]
    public void WhenGetRequestInfoAndRequestHasNoFields_ThenReturnsInfo()
    {
        var request = new HasNoPropertiesRequest();

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute/{unknown}");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateContainsNoPlaceholdersWithNoDataForGet_ThenReturnsInfo()
    {
        var request = new HasNoPlaceholdersGetRequest();

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateContainsNoPlaceholdersWithDataForGet_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasNoPlaceholdersGetRequest
        {
            Id = "anid",
            ANumberProperty = 999,
            ADateTimeProperty = datum,
            AStringProperty = "avalue"
        };

        var result = request.GetRequestInfo();

        result.Route.Should()
            .Be(
                "/aroute?adatetimeproperty=2023-10-29T12%3a30%3a15Z&anumberproperty=999&astringproperty=avalue&id=anid");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateContainsNoPlaceholdersWithDataForPost_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasNoPlaceholdersPostRequest
        {
            Id = "anid",
            ANumberProperty = 999,
            ADateTimeProperty = datum,
            AStringProperty = "avalue"
        };

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute");
        result.Method.Should().Be(OperationMethod.Post);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasUnknownPlaceholderWithNoDataForGet_ThenReturnsInfo()
    {
        var request = new HasUnknownPlaceholderGetRequest();

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute/{unknown}");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasUnknownPlaceholderWithDataForGet_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasUnknownPlaceholderGetRequest
        {
            Id = "anid",
            ANumberProperty = 999,
            ADateTimeProperty = datum,
            AStringProperty = "avalue"
        };

        var result = request.GetRequestInfo();

        result.Route.Should()
            .Be(
                "/aroute/{unknown}?adatetimeproperty=2023-10-29T12%3a30%3a15Z&anumberproperty=999&astringproperty=avalue&id=anid");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasUnknownPlaceholderWithDataForPost_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasUnknownPlaceholderPostRequest
        {
            Id = "anid",
            ANumberProperty = 999,
            ADateTimeProperty = datum,
            AStringProperty = "avalue"
        };

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute/{unknown}");
        result.Method.Should().Be(OperationMethod.Post);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasPlaceholdersWithNullDataValuesForGet_ThenReturnsInfo()
    {
        var request = new HasPlaceholdersGetRequest
        {
            Id = null,
            AStringProperty1 = null,
            AStringProperty2 = null,
            AStringProperty3 = null,
            ANumberProperty = null,
            ADateTimeProperty = null
        };

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute/apath1/xxxyyy/apath2/apath3");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasPlaceholdersWithDataValuesForGet_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasPlaceholdersGetRequest
        {
            Id = "anid",
            AStringProperty1 = "avalue1",
            AStringProperty2 = "avalue2",
            AStringProperty3 = "avalue3",
            ANumberProperty = 999,
            ADateTimeProperty = datum
        };

        var result = request.GetRequestInfo();

        result.Route.Should()
            .Be(
                "/aroute/anid/apath1/xxx999yyy/apath2/avalue1/avalue2/apath3?adatetimeproperty=2023-10-29T12%3a30%3a15Z&astringproperty3=avalue3");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasPlaceholdersWithSomeDataValuesForGet_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasPlaceholdersGetRequest
        {
            Id = "anid",
            AStringProperty1 = "avalue1",
            AStringProperty2 = null,
            AStringProperty3 = null,
            ANumberProperty = null,
            ADateTimeProperty = datum
        };

        var result = request.GetRequestInfo();

        result.Route.Should()
            .Be("/aroute/anid/apath1/xxxyyy/apath2/avalue1/apath3?adatetimeproperty=2023-10-29T12%3a30%3a15Z");
        result.Method.Should().Be(OperationMethod.Get);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasPlaceholdersWithNullDataValuesForPost_ThenReturnsInfo()
    {
        var request = new HasPlaceholdersPostRequest
        {
            Id = null,
            AStringProperty1 = null,
            AStringProperty2 = null,
            AStringProperty3 = null,
            ANumberProperty = null,
            ADateTimeProperty = null
        };

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute/apath1/xxxyyy/apath2/apath3");
        result.Method.Should().Be(OperationMethod.Post);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasPlaceholdersWithDataValuesForPost_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasPlaceholdersPostRequest
        {
            Id = "anid",
            AStringProperty1 = "avalue1",
            AStringProperty2 = "avalue2",
            AStringProperty3 = "avalue3",
            ANumberProperty = 999,
            ADateTimeProperty = datum
        };

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute/anid/apath1/xxx999yyy/apath2/avalue1/avalue2/apath3");
        result.Method.Should().Be(OperationMethod.Post);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenGetRequestInfoAndRouteTemplateHasPlaceholdersWithSomeDataValuesForPost_ThenReturnsInfo()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasPlaceholdersPostRequest
        {
            Id = "anid",
            AStringProperty1 = "avalue1",
            AStringProperty2 = null,
            AStringProperty3 = null,
            ANumberProperty = null,
            ADateTimeProperty = datum
        };

        var result = request.GetRequestInfo();

        result.Route.Should().Be("/aroute/anid/apath1/xxxyyy/apath2/avalue1/apath3");
        result.Method.Should().Be(OperationMethod.Post);
        result.IsTestingOnly.Should().BeFalse();
    }

    [Fact]
    public void WhenToUrl_ThenReturnsUrl()
    {
        var datum = new DateTime(2023, 10, 29, 12, 30, 15, DateTimeKind.Utc).ToNearestSecond();
        var request = new HasPlaceholdersPostRequest
        {
            Id = "anid",
            AStringProperty1 = "avalue1",
            AStringProperty2 = null,
            AStringProperty3 = null,
            ANumberProperty = null,
            ADateTimeProperty = datum
        };

        var result = request.ToUrl();

        result.Should().Be("/aroute/anid/apath1/xxxyyy/apath2/avalue1/apath3");
    }

    [Fact]
    public void WhenGetRouteTemplatePlaceholdersAndNoAttribute_ThenReturnsEmpty()
    {
        var result = typeof(NoRouteRequest).GetRouteTemplatePlaceholders();

        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenGetRouteTemplatePlaceholdersAndRequestHasNoFields_ThenReturnsEmpty()
    {
        var result = typeof(HasNoPropertiesRequest).GetRouteTemplatePlaceholders();

        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenGetRouteTemplatePlaceholdersAndRouteTemplateContainsNoPlaceholders_ThenReturnsEmpty()
    {
        var result = typeof(HasNoPlaceholdersPostRequest).GetRouteTemplatePlaceholders();

        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenGetRouteTemplatePlaceholdersAndRouteTemplateHasUnknownPlaceholder_ThenReturnsEmpty()
    {
        var result = typeof(HasUnknownPlaceholderGetRequest).GetRouteTemplatePlaceholders();

        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenGetRouteTemplatePlaceholdersAndRouteTemplateHasPlaceholdersForPost_ThenReturns()
    {
        var result = typeof(HasPlaceholdersPostRequest).GetRouteTemplatePlaceholders();

        result.Should().NotBeEmpty();
        result.Count.Should().Be(4);
        result[nameof(HasPlaceholdersPostRequest.Id)].Should().Be(typeof(string));
        result[nameof(HasPlaceholdersPostRequest.ANumberProperty)].Should().Be(typeof(int?));
        result[nameof(HasPlaceholdersPostRequest.AStringProperty1)].Should().Be(typeof(string));
        result[nameof(HasPlaceholdersPostRequest.AStringProperty2)].Should().Be(typeof(string));
    }

    private class NoRouteRequest : IWebRequest<TestResponse>;

    [Route("/aroute/{unknown}", OperationMethod.Get)]
    private class HasNoPropertiesRequest : IWebRequest<TestResponse>;

    [Route("/aroute", OperationMethod.Get)]
    private class HasNoPlaceholdersGetRequest : IWebRequest<TestResponse>
    {
        public DateTime? ADateTimeProperty { get; set; }

        public int? ANumberProperty { get; set; }

        public string? AStringProperty { get; set; }

        public string? Id { get; set; }
    }

    [Route("/aroute", OperationMethod.Post)]
    private class HasNoPlaceholdersPostRequest : IWebRequest<TestResponse>
    {
        public DateTime? ADateTimeProperty { get; set; }

        public int? ANumberProperty { get; set; }

        public string? AStringProperty { get; set; }

        public string? Id { get; set; }
    }

    [Route("/aroute/{unknown}", OperationMethod.Get)]
    private class HasUnknownPlaceholderGetRequest : IWebRequest<TestResponse>
    {
        public DateTime? ADateTimeProperty { get; set; }

        public int? ANumberProperty { get; set; }

        public string? AStringProperty { get; set; }

        public string? Id { get; set; }
    }

    [Route("/aroute/{unknown}", OperationMethod.Post)]
    private class HasUnknownPlaceholderPostRequest : IWebRequest<TestResponse>
    {
        public DateTime? ADateTimeProperty { get; set; }

        public int? ANumberProperty { get; set; }

        public string? AStringProperty { get; set; }

        public string? Id { get; set; }
    }

    [Route("/aroute/{id}/apath1/xxx{anumberproperty}yyy/apath2/{astringproperty1}/{astringproperty2}/apath3",
        OperationMethod.Get)]
    private class HasPlaceholdersGetRequest : IWebRequest<TestResponse>
    {
        public DateTime? ADateTimeProperty { get; set; }

        public int? ANumberProperty { get; set; }

        public string? AStringProperty1 { get; set; }

        public string? AStringProperty2 { get; set; }

        public string? AStringProperty3 { get; set; }

        public string? Id { get; set; }
    }

    [Route("/aroute/{id}/apath1/xxx{anumberproperty}yyy/apath2/{astringproperty1}/{astringproperty2}/apath3",
        OperationMethod.Post)]
    private class HasPlaceholdersPostRequest : IWebRequest<TestResponse>
    {
        public DateTime? ADateTimeProperty { get; set; }

        public int? ANumberProperty { get; set; }

        public string? AStringProperty1 { get; set; }

        public string? AStringProperty2 { get; set; }

        public string? AStringProperty3 { get; set; }

        public string? Id { get; set; }
    }
}