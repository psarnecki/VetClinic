using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VetClinicManager.Filters;

namespace VetClinicManager.Tests.Unit.Filters;

[TestFixture]
public class GlobalExceptionFilterTests
{
    private Mock<ILogger<GlobalExceptionFilter>> _mockLogger = null!;
    private Mock<IWebHostEnvironment> _mockEnvironment = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionFilter>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
    }

    [Test]
    public void OnException_WhenDevelopment_ShouldNotHandleException()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var filter = new GlobalExceptionFilter(_mockLogger.Object, _mockEnvironment.Object);
        var context = CreateExceptionContext();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeFalse();
        context.Result.Should().BeNull();
    }

    [Test]
    public void OnException_WhenProduction_ShouldRedirectToHomeErrorAndMarkHandled()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var filter = new GlobalExceptionFilter(_mockLogger.Object, _mockEnvironment.Object);
        var context = CreateExceptionContext();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        var redirect = context.Result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Error");
        redirect.ControllerName.Should().Be("Home");
    }

    private static ExceptionContext CreateExceptionContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/test";

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new ExceptionContext(actionContext, [])
        {
            Exception = new InvalidOperationException("test failure")
        };
    }
}