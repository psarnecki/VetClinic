using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using VetClinicManager.Controllers;
using VetClinicManager.DTOs.Animals;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class AnimalsControllerTests
{
    private Mock<IAnimalService> _mockAnimalService = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private AnimalsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockAnimalService = new Mock<IAnimalService>();
        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _controller = new AnimalsController(_mockUserManager.Object, _mockAnimalService.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    private void SetUser(string? userId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (userId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
        _controller.TempData = new TempDataDictionary(
            _controller.ControllerContext.HttpContext,
            Mock.Of<ITempDataProvider>());
    }

    [Test]
    public async Task Index_WhenUserIsClient_ShouldReturnIndexUserViewScopedToOwner()
    {
        SetUser("client-1", "Client");
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("client-1");
        var expected = Array.Empty<AnimalListUserDto>();
        _mockAnimalService.Setup(s => s.GetAnimalsForOwnerAsync("client-1", "name_desc")).ReturnsAsync(expected);

        var result = await _controller.Index("name_desc");

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("IndexUser");
        view.Model.Should().BeSameAs(expected);
    }

    [Test]
    public async Task Index_WhenUserIdIsNull_ShouldReturnUnauthorized()
    {
        SetUser(null, "Client");
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

        var result = await _controller.Index(null!);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Test]
    public async Task Index_WhenUserIsStaff_ShouldReturnIndexVetRecView()
    {
        SetUser("rec-1", "Receptionist");
        var expected = Array.Empty<AnimalListVetRecDto>();
        _mockAnimalService.Setup(s => s.GetAnimalsForStaffAsync(null)).ReturnsAsync(expected);

        var result = await _controller.Index(null!);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("IndexVetRec");
        view.Model.Should().BeSameAs(expected);
    }

    [Test]
    public async Task Details_WhenUserIsClientAndServiceReturnsNull_ShouldReturnNotFound()
    {
        SetUser("client-1", "Client");
        _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("client-1");
        _mockAnimalService.Setup(s => s.GetAnimalDetailsForOwnerAsync(5, "client-1"))
            .ReturnsAsync((AnimalDetailsUserDto?)null);

        var result = await _controller.Details(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Details_WhenUserIsStaffAndServiceReturnsNull_ShouldReturnNotFound()
    {
        SetUser("rec-1", "Receptionist");
        _mockAnimalService.Setup(s => s.GetAnimalDetailsForStaffAsync(5))
            .ReturnsAsync((AnimalDetailsVetRecDto?)null);

        var result = await _controller.Details(5);

        result.Should().BeOfType<NotFoundResult>();
        _mockAnimalService.Verify(s => s.GetAnimalDetailsForOwnerAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreatePost_WhenModelInvalid_ShouldReturnViewWithDto()
    {
        SetUser("admin-1", "Admin");
        _controller.ModelState.AddModelError("Name", "Required");
        _mockAnimalService.Setup(s => s.GetOwnersForSelectListAsync())
            .ReturnsAsync(Array.Empty<UserBriefDto>());

        var dto = new AnimalCreateDto();
        var result = await _controller.Create(dto);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().BeSameAs(dto);
        _mockAnimalService.Verify(s => s.CreateAnimalAsync(It.IsAny<AnimalCreateDto>()), Times.Never);
    }

    [Test]
    public async Task CreatePost_WhenModelValid_ShouldRedirectToDetailsWithSuccessMessage()
    {
        SetUser("admin-1", "Admin");
        _mockAnimalService.Setup(s => s.CreateAnimalAsync(It.IsAny<AnimalCreateDto>())).ReturnsAsync(42);

        var result = await _controller.Create(new AnimalCreateDto());

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(42);
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenIdMismatch_ShouldReturnBadRequest()
    {
        SetUser("admin-1", "Admin");

        var result = await _controller.Edit(1, new AnimalEditDto { Id = 2 });

        result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task EditPost_WhenUpdateSucceeds_ShouldRedirectToIndexWithSuccessMessage()
    {
        SetUser("admin-1", "Admin");
        _mockAnimalService.Setup(s => s.GetOwnersForSelectListAsync()).ReturnsAsync(Array.Empty<UserBriefDto>());
        _mockAnimalService.Setup(s => s.UpdateAnimalAsync(It.IsAny<AnimalEditDto>())).ReturnsAsync(true);

        var result = await _controller.Edit(7, new AnimalEditDto { Id = 7 });

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenUpdateFails_ShouldReturnViewWithModelError()
    {
        SetUser("admin-1", "Admin");
        _mockAnimalService.Setup(s => s.GetOwnersForSelectListAsync()).ReturnsAsync(Array.Empty<UserBriefDto>());
        _mockAnimalService.Setup(s => s.UpdateAnimalAsync(It.IsAny<AnimalEditDto>())).ReturnsAsync(false);

        var result = await _controller.Edit(7, new AnimalEditDto { Id = 7 });

        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Delete_WhenIdIsNull_ShouldReturnNotFound()
    {
        SetUser("admin-1", "Admin");

        var result = await _controller.Delete((int?)null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteSucceeds_ShouldRedirectToIndexWithSuccessMessage()
    {
        SetUser("admin-1", "Admin");
        _mockAnimalService.Setup(s => s.DeleteAnimalAsync(3)).ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(3);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteFails_ShouldRedirectToIndexWithErrorMessage()
    {
        SetUser("admin-1", "Admin");
        _mockAnimalService.Setup(s => s.DeleteAnimalAsync(3)).ReturnsAsync(false);

        var result = await _controller.DeleteConfirmed(3);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task RemoveImage_WhenSuccessful_ShouldRedirectToEditWithSuccessMessage()
    {
        SetUser("vet-1", "Vet");
        _mockAnimalService.Setup(s => s.RemoveAnimalImageAsync(4)).ReturnsAsync(true);

        var result = await _controller.RemoveImage(4);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Edit");
        redirect.RouteValues!["id"].Should().Be(4);
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task RemoveImage_WhenFails_ShouldRedirectToEditWithErrorMessage()
    {
        SetUser("vet-1", "Vet");
        _mockAnimalService.Setup(s => s.RemoveAnimalImageAsync(4)).ReturnsAsync(false);

        var result = await _controller.RemoveImage(4);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Edit");
        redirect.RouteValues!["id"].Should().Be(4);
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }
}