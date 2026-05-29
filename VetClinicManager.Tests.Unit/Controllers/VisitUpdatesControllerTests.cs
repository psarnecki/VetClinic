using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using VetClinicManager.Controllers;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.DTOs.VisitUpdates;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class VisitUpdatesControllerTests
{
    private Mock<IVisitUpdateService> _mockVisitUpdateService = null!;
    private Mock<IMedicationService> _mockMedicationService = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private VisitUpdatesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockVisitUpdateService = new Mock<IVisitUpdateService>();
        _mockMedicationService = new Mock<IMedicationService>();
        _mockMedicationService.Setup(s => s.GetMedicationsForSelectListAsync())
            .ReturnsAsync(Array.Empty<MedicationBriefDto>());

        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("vet-1");

        _controller = new VisitUpdatesController(
            _mockVisitUpdateService.Object, _mockMedicationService.Object, _mockUserManager.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, "vet-1"), new Claim(ClaimTypes.Role, "Vet") },
                        "TestAuth"))
                }
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task CreateGet_WhenVisitUnknown_ShouldReturnNotFound()
    {
        _mockVisitUpdateService.Setup(s => s.GetForCreateAsync(99)).ReturnsAsync((VisitUpdateCreateDto?)null);

        var result = await _controller.Create(99);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task CreatePost_WhenModelInvalid_ShouldReturnViewWithoutCallingService()
    {
        _controller.ModelState.AddModelError("VisitId", "Required");
        var dto = new VisitUpdateCreateDto { VisitId = 5 };

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dto);
        _mockVisitUpdateService.Verify(
            s => s.CreateVisitUpdateAsync(It.IsAny<VisitUpdateCreateDto>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task CreatePost_WhenModelValid_ShouldRedirectToVisitDetailsWithSuccessMessage()
    {
        var dto = new VisitUpdateCreateDto { VisitId = 5, Notes = "Stable" };
        _mockVisitUpdateService
            .Setup(s => s.CreateVisitUpdateAsync(dto, "vet-1"))
            .ReturnsAsync(5);

        var result = await _controller.Create(dto);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.ControllerName.Should().Be("Visits");
        redirect.RouteValues!["id"].Should().Be(5);
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenIdMismatch_ShouldReturnBadRequest()
    {
        var result = await _controller.Edit(1, new VisitUpdateEditDto { Id = 2 });

        result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task EditPost_WhenModelValid_ShouldRedirectToVisitDetails()
    {
        _mockVisitUpdateService
            .Setup(s => s.UpdateVisitUpdateAsync(It.IsAny<VisitUpdateEditDto>(), "vet-1", false))
            .ReturnsAsync(55);

        var result = await _controller.Edit(10, new VisitUpdateEditDto { Id = 10 });

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.ControllerName.Should().Be("Visits");
        redirect.RouteValues!["id"].Should().Be(55);
    }

    [Test]
    public async Task EditPost_WhenUnauthorized_ShouldReturnForbid()
    {
        _mockVisitUpdateService
            .Setup(s => s.UpdateVisitUpdateAsync(It.IsAny<VisitUpdateEditDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.Edit(10, new VisitUpdateEditDto { Id = 10 });

        result.Should().BeOfType<ForbidResult>();
    }

    [Test]
    public async Task EditPost_WhenKeyNotFound_ShouldReturnNotFound()
    {
        _mockVisitUpdateService
            .Setup(s => s.UpdateVisitUpdateAsync(It.IsAny<VisitUpdateEditDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Edit(10, new VisitUpdateEditDto { Id = 10 });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task DeleteConfirmed_WhenUnauthorized_ShouldReturnForbid()
    {
        _mockVisitUpdateService
            .Setup(s => s.DeleteVisitUpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.DeleteConfirmed(new VisitUpdateDeleteDto { Id = 10 });

        result.Should().BeOfType<ForbidResult>();
    }

    [Test]
    public async Task DeleteConfirmed_WhenValid_ShouldRedirectToVisitDetails()
    {
        _mockVisitUpdateService
            .Setup(s => s.DeleteVisitUpdateAsync(10, "vet-1", false))
            .ReturnsAsync(77);

        var result = await _controller.DeleteConfirmed(new VisitUpdateDeleteDto { Id = 10 });

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("Visits");
        redirect.RouteValues!["id"].Should().Be(77);
    }
}