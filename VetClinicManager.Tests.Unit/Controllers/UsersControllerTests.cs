using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using VetClinicManager.Areas.Admin.Controllers;
using VetClinicManager.Areas.Admin.DTOs.Users;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private Mock<IUserService> _mockService = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IUserService>();
        _mockService.Setup(s => s.GetAllAvailableRolesAsync()).ReturnsAsync(new List<string>());
        _controller = new UsersController(_mockService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task CreatePost_WhenModelInvalid_ShouldReturnViewWithModel()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var model = new UserCreateDto();

        var result = await _controller.Create(model);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(model);
        _mockService.Verify(s => s.CreateUserAsync(It.IsAny<UserCreateDto>()), Times.Never);
    }

    [Test]
    public async Task CreatePost_WhenCreationSucceeds_ShouldRedirectToIndex()
    {
        _mockService.Setup(s => s.CreateUserAsync(It.IsAny<UserCreateDto>()))
            .ReturnsAsync((IdentityResult.Success, new User()));

        var result = await _controller.Create(new UserCreateDto());

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
    }

    [Test]
    public async Task CreatePost_WhenCreationFails_ShouldReturnViewWithModelErrors()
    {
        _mockService.Setup(s => s.CreateUserAsync(It.IsAny<UserCreateDto>()))
            .ReturnsAsync((IdentityResult.Failed(new IdentityError { Description = "Bad" }), null));

        var result = await _controller.Create(new UserCreateDto());

        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task EditPost_WhenIdMismatch_ShouldReturnNotFound()
    {
        var result = await _controller.Edit("a", new UserEditDto { Id = "b" });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task EditPost_WhenUpdateSucceeds_ShouldRedirectToIndexWithSuccessMessage()
    {
        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<UserEditDto>())).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.Edit("u1", new UserEditDto { Id = "u1" });

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenUpdateFails_ShouldReturnViewWithModelErrors()
    {
        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<UserEditDto>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Nope" }));

        var result = await _controller.Edit("u1", new UserEditDto { Id = "u1" });

        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Edit_WhenIdIsNull_ShouldReturnNotFound()
    {
        var result = await _controller.Edit((string?)null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteSucceeds_ShouldRedirectToIndex()
    {
        _mockService.Setup(s => s.DeleteUserAsync("u1")).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.DeleteConfirmed("u1");

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteFails_ShouldRedirectToIndexWithErrorMessage()
    {
        _mockService.Setup(s => s.DeleteUserAsync("u1"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Cannot delete" }));

        var result = await _controller.DeleteConfirmed("u1");

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }
}