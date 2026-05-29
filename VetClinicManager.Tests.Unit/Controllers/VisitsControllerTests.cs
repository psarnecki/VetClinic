using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using VetClinicManager.Controllers;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.DTOs.Visits;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class VisitsControllerTests
{
    private Mock<IVisitService> _mockVisitService = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<IUserStore<User>> _mockUserStore = null!;
    private VisitsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockVisitService = new Mock<IVisitService>();
        _mockUserStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(_mockUserStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _controller = new VisitsController(_mockVisitService.Object, _mockUserManager.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Index_WhenUserIsClient_ShouldReturnIndexUserView()
    {
        const string clientId = "client-1";
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(clientId);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, clientId),
                new Claim(ClaimTypes.Role, "Client")
            },
            "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var expected = Array.Empty<VisitListUserDto>();
        _mockVisitService
            .Setup(s => s.GetVisitsForOwnerAsync(clientId, "scheduled"))
            .ReturnsAsync(expected);

        var result = await _controller.Index("scheduled");

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("IndexUser");
        view.Model.Should().BeSameAs(expected);
        _mockVisitService.Verify(s => s.GetVisitsForStaffAsync(It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public async Task Index_WhenUserIsVetAndNotAdmin_ShouldReturnIndexVetViewFilteredByVet()
    {
        const string vetId = "vet-99";
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(vetId);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, vetId),
                new Claim(ClaimTypes.Role, "Vet")
            },
            "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var expected = Array.Empty<VisitListVetRecDto>();
        _mockVisitService
            .Setup(s => s.GetVisitsForStaffAsync(vetId, null))
            .ReturnsAsync(expected);

        var result = await _controller.Index(null!);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("IndexVet");
        view.Model.Should().BeSameAs(expected);
        _mockVisitService.Verify(s => s.GetVisitsForStaffAsync(vetId, null), Times.Once);
    }

    [Test]
    public async Task Index_WhenUserIsReceptionist_ShouldReturnIndexReceptionistViewWithAllVisits()
    {
        const string receptionistId = "rec-1";
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(receptionistId);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, receptionistId),
                new Claim(ClaimTypes.Role, "Receptionist")
            },
            "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var expected = Array.Empty<VisitListVetRecDto>();
        _mockVisitService
            .Setup(s => s.GetVisitsForStaffAsync(null, "title"))
            .ReturnsAsync(expected);

        var result = await _controller.Index("title");

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("IndexReceptionist");
        view.Model.Should().BeSameAs(expected);
        _mockVisitService.Verify(s => s.GetVisitsForStaffAsync(null, "title"), Times.Once);
    }

    [Test]
    public async Task Index_WhenUserIdIsNull_ShouldReturnUnauthorized()
    {
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Role, "Client") },
                    "TestAuth"))
            }
        };

        var result = await _controller.Index(null!);

        result.Should().BeOfType<UnauthorizedResult>();
        _mockVisitService.Verify(s => s.GetVisitsForOwnerAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public async Task Details_WhenOwnerServiceReturnsNull_ShouldReturnNotFound()
    {
        const string clientId = "client-404";
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(clientId);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, clientId),
                new Claim(ClaimTypes.Role, "Client")
            },
            "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockVisitService
            .Setup(s => s.GetDetailsForOwnerAsync(42, clientId))
            .ReturnsAsync((VisitDetailsUserDto?)null);

        var result = await _controller.Details(42);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Details_WhenVetNotAssignedToVisit_ShouldRedirectToIndexWithErrorMessage()
    {
        const string vetId = "vet-1";
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(vetId);
        SetUserWithTempData(vetId, "Vet");

        _mockVisitService.Setup(s => s.GetDetailsForStaffAsync(5))
            .ReturnsAsync(new VisitDetailsVetRecDto { AssignedVet = new UserBriefDto { Id = "another-vet" } });

        var result = await _controller.Details(5);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task Edit_WhenVetNotAssigned_ShouldRedirectToIndexWithErrorMessage()
    {
        const string vetId = "vet-1";
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(vetId);
        SetUserWithTempData(vetId, "Vet");

        _mockVisitService.Setup(s => s.GetForEditAsync(5, vetId, true)).ReturnsAsync((VisitEditDto?)null);

        var result = await _controller.Edit(5);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenIdMismatch_ShouldReturnNotFound()
    {
        SetUserWithTempData("admin-1", "Admin");

        var result = await _controller.Edit(1, new VisitEditDto { Id = 2 });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task EditPost_WhenVetNotAssignedToVisit_ShouldRedirectToIndexWithErrorMessage()
    {
        const string vetId = "vet-1";
        SetUserWithTempData(vetId, "Vet");
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(vetId);

        var dto = new VisitEditDto
        {
            Id = 5,
            Title = "Checkup",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal
        };

        _mockVisitService
            .Setup(s => s.UpdateVisitAsync(5, dto, vetId, true))
            .ReturnsAsync(false);

        var result = await _controller.Edit(5, dto);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenUpdateSucceeds_ShouldRedirectToDetailsWithSuccessMessage()
    {
        const string adminId = "admin-1";
        SetUserWithTempData(adminId, "Admin");
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(adminId);

        var dto = new VisitEditDto
        {
            Id = 5,
            Title = "Checkup",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal
        };

        _mockVisitService
            .Setup(s => s.UpdateVisitAsync(5, dto, adminId, false))
            .ReturnsAsync(true);

        var result = await _controller.Edit(5, dto);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(5);
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenUpdateFailsForStaff_ShouldReturnViewWithErrorMessage()
    {
        const string recId = "rec-1";
        SetUserWithTempData(recId, "Receptionist");
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(recId);

        var dto = new VisitEditDto
        {
            Id = 5,
            Title = "Checkup",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal
        };

        _mockVisitService
            .Setup(s => s.UpdateVisitAsync(5, dto, recId, false))
            .ReturnsAsync(false);

        var result = await _controller.Edit(5, dto);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dto);
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenModelInvalid_ShouldReturnViewWithRepopulatedSelectLists()
    {
        const string recId = "rec-1";
        SetUserWithTempData(recId, "Receptionist");
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(recId);
        _controller.ModelState.AddModelError("Title", "Required");

        var vets = new[] { new UserBriefDto { Id = "v1", FirstName = "Dr", LastName = "House" } };
        _mockVisitService.Setup(s => s.GetVetsForSelectListAsync()).ReturnsAsync(vets);

        var original = new VisitEditDto
        {
            Id = 5,
            Animal = new AnimalBriefDto { Id = 1, Name = "Dog", Species = "Canine" }
        };
        _mockVisitService.Setup(s => s.GetForEditAsync(5, recId, false)).ReturnsAsync(original);

        var dto = new VisitEditDto { Id = 5 };
        var result = await _controller.Edit(5, dto);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<VisitEditDto>().Subject;
        model.Vets.Should().NotBeNull();
        model.Statuses.Should().NotBeNull();
        model.Priorities.Should().NotBeNull();
        model.Animal.Should().Be(original.Animal);
        _mockVisitService.Verify(s => s.UpdateVisitAsync(It.IsAny<int>(), It.IsAny<VisitEditDto>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _mockVisitService.Verify(s => s.GetVetsForSelectListAsync(), Times.Once);
    }

    [Test]
    public async Task CreatePost_WhenModelInvalid_ShouldReturnViewWithoutCallingService()
    {
        SetUserWithTempData("rec-1", "Receptionist");
        _controller.ModelState.AddModelError("Title", "Required");

        _mockVisitService.Setup(s => s.GetAnimalsForSelectListAsync())
            .ReturnsAsync(Array.Empty<AnimalBriefDto>());
        _mockVisitService.Setup(s => s.GetVetsForSelectListAsync())
            .ReturnsAsync(Array.Empty<UserBriefDto>());

        var dto = new VisitCreateDto();
        var result = await _controller.Create(dto);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dto);
        _mockVisitService.Verify(s => s.CreateVisitAsync(It.IsAny<VisitCreateDto>()), Times.Never);
    }

    [Test]
    public async Task CreatePost_WhenModelValid_ShouldRedirectToDetailsWithSuccessMessage()
    {
        SetUserWithTempData("rec-1", "Receptionist");

        var dto = new VisitCreateDto
        {
            Title = "Vaccination",
            ScheduledAt = DateTime.UtcNow,
            Status = VisitStatus.Scheduled,
            Priority = VisitPriority.Normal,
            AnimalId = 3
        };

        _mockVisitService.Setup(s => s.CreateVisitAsync(dto)).ReturnsAsync(42);

        var result = await _controller.Create(dto);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(42);
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteSucceeds_ShouldRedirectToIndexWithSuccessMessage()
    {
        SetUserWithTempData("rec-1", "Receptionist");
        _mockVisitService.Setup(s => s.DeleteVisitAsync(8)).ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(8);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteFails_ShouldRedirectToIndexWithErrorMessage()
    {
        SetUserWithTempData("rec-1", "Receptionist");
        _mockVisitService.Setup(s => s.DeleteVisitAsync(8)).ReturnsAsync(false);

        var result = await _controller.DeleteConfirmed(8);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task GenerateVisitReport_WhenUserIsNull_ShouldReturnUnauthorized()
    {
        SetUserWithTempData("u1", "Vet");
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((User?)null);

        var result = await _controller.GenerateVisitReport(5);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Test]
    public async Task GenerateVisitReport_WhenServiceReturnsNull_ShouldReturnNotFound()
    {
        SetUserWithTempData("u1", "Vet");
        var user = new User { Id = "u1" };
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Vet" });
        _mockVisitService.Setup(s => s.GeneratePdfReportAsync(5, "u1", It.IsAny<IList<string>>()))
            .ReturnsAsync(((byte[], string)?)null);

        var result = await _controller.GenerateVisitReport(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GenerateVisitReport_WhenAuthorized_ShouldReturnPdfFile()
    {
        SetUserWithTempData("u1", "Vet");
        var user = new User { Id = "u1" };
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Vet" });
        _mockVisitService.Setup(s => s.GeneratePdfReportAsync(5, "u1", It.IsAny<IList<string>>()))
            .ReturnsAsync((new byte[] { 1, 2, 3 }, "report.pdf"));

        var result = await _controller.GenerateVisitReport(5);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/pdf");
        file.FileDownloadName.Should().Be("report.pdf");
    }

    [Test]
    public async Task GenerateVisitReport_WhenUnauthorized_ShouldReturnForbid()
    {
        SetUserWithTempData("u1", "Vet");
        var user = new User { Id = "u1" };
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Vet" });
        _mockVisitService.Setup(s => s.GeneratePdfReportAsync(5, "u1", It.IsAny<IList<string>>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.GenerateVisitReport(5);

        result.Should().BeOfType<ForbidResult>();
    }

    private void SetUserWithTempData(string userId, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
        _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
    }
}