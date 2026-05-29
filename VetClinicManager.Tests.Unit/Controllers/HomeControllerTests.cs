using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using VetClinicManager.Controllers;
using VetClinicManager.DTOs.Home;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class HomeControllerTests
{
    private Mock<IDashboardService> _mockDashboardService = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<IUserStore<User>> _mockUserStore = null!;
    private HomeController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockDashboardService = new Mock<IDashboardService>();
        
        _mockUserStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(_mockUserStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _controller = new HomeController(_mockDashboardService.Object, _mockUserManager.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task Index_WhenUserIsVetAndNotAdminOrReceptionist_ShouldReturnIndexStaffView()
    {
        var vetUserId = "vet-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "testVet"),
            new Claim(ClaimTypes.Role, "Vet"),
            new Claim(ClaimTypes.NameIdentifier, vetUserId)
        }, "TestAuthType"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(vetUserId);

        var expectedDashboardData = new DashboardDto();
        _mockDashboardService.Setup(ds => ds.GetDashboardDataForStaffAsync(vetUserId))
            .ReturnsAsync(expectedDashboardData);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>()
            .Which.ViewName.Should().Be("IndexStaff");

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(expectedDashboardData);
    }

    [Test]
    public async Task Index_WhenUserIsClient_ShouldReturnIndexClientView()
    {
        var clientUserId = "client-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "testClient"),
            new Claim(ClaimTypes.Role, "Client"),
            new Claim(ClaimTypes.NameIdentifier, clientUserId)
        }, "TestAuthType"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(clientUserId);

        var expectedDashboardData = new DashboardDto();
        _mockDashboardService.Setup(ds => ds.GetDashboardDataForClientAsync(clientUserId))
            .ReturnsAsync(expectedDashboardData);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>()
            .Which.ViewName.Should().Be("IndexClient");

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(expectedDashboardData);
    }

    [Test]
    public async Task Index_WhenUserHasClientAndVetRoles_ShouldReturnIndexClientView()
    {
        var userId = "dual-role-user";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Role, "Client"),
            new Claim(ClaimTypes.Role, "Vet"),
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthType"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        var expected = new DashboardDto();
        _mockDashboardService.Setup(ds => ds.GetDashboardDataForClientAsync(userId)).ReturnsAsync(expected);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("IndexClient");
        _mockDashboardService.Verify(ds => ds.GetDashboardDataForClientAsync(userId), Times.Once);
        _mockDashboardService.Verify(ds => ds.GetDashboardDataForStaffAsync(It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public async Task Index_WhenUserIsNotAuthenticated_ShouldReturnIndexAnonymousView()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>()
            .Which.ViewName.Should().Be("IndexAnonymous");

        _mockDashboardService.Verify(
            ds => ds.GetDashboardDataForClientAsync(It.IsAny<string>()),
            Times.Never);
        _mockDashboardService.Verify(
            ds => ds.GetDashboardDataForStaffAsync(It.IsAny<string?>()),
            Times.Never);
    }

    [Test]
    public async Task Index_WhenUserIsAdmin_ShouldReturnIndexStaffViewWithUnscopedDashboard()
    {
        var adminId = "admin-1";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminId),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuthType"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var expected = new DashboardDto();
        _mockDashboardService.Setup(ds => ds.GetDashboardDataForStaffAsync(null)).ReturnsAsync(expected);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("IndexStaff");
        (result as ViewResult)!.Model.Should().Be(expected);
        _mockDashboardService.Verify(ds => ds.GetDashboardDataForStaffAsync(null), Times.Once);
        _mockDashboardService.Verify(ds => ds.GetDashboardDataForClientAsync(It.IsAny<string>()), Times.Never);
    }
}
