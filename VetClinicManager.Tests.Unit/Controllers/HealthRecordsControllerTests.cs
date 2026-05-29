using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using VetClinicManager.Controllers;
using VetClinicManager.DTOs.HealthRecords;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class HealthRecordsControllerTests
{
    private Mock<IHealthRecordService> _mockService = null!;
    private HealthRecordsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IHealthRecordService>();
        _controller = new HealthRecordsController(_mockService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Details_WhenServiceReturnsNull_ShouldReturnNotFound()
    {
        _mockService.Setup(s => s.GetDetailsAsync(5)).ReturnsAsync((HealthRecordDetailsDto?)null);

        var result = await _controller.Details(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task CreateGet_WhenAnimalHasRecordOrIsMissing_ShouldRedirectToAnimalDetailsWithErrorMessage()
    {
        _mockService.Setup(s => s.PrepareCreateDtoAsync(7)).ReturnsAsync((HealthRecordCreateDto?)null);

        var result = await _controller.Create(7);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.ControllerName.Should().Be("Animals");
        redirect.RouteValues!["id"].Should().Be(7);
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task CreateGet_WhenAnimalHasNoRecord_ShouldReturnViewWithDto()
    {
        var dto = new HealthRecordCreateDto { AnimalId = 7, AnimalName = "Rex" };
        _mockService.Setup(s => s.PrepareCreateDtoAsync(7)).ReturnsAsync(dto);

        var result = await _controller.Create(7);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dto);
    }

    [Test]
    public async Task CreatePost_WhenModelInvalid_ShouldReturnView()
    {
        _controller.ModelState.AddModelError("AnimalId", "Required");
        var dto = new HealthRecordCreateDto { AnimalId = 7 };
        _mockService.Setup(s => s.PrepareCreateDtoAsync(7)).ReturnsAsync(new HealthRecordCreateDto { AnimalName = "Rex" });

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dto);
        _mockService.Verify(s => s.CreateHealthRecordAsync(It.IsAny<HealthRecordCreateDto>()), Times.Never);
    }

    [Test]
    public async Task CreatePost_WhenModelValid_ShouldRedirectToDetailsWithSuccessMessage()
    {
        _mockService.Setup(s => s.CreateHealthRecordAsync(It.IsAny<HealthRecordCreateDto>())).ReturnsAsync(33);

        var result = await _controller.Create(new HealthRecordCreateDto { AnimalId = 7 });

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(33);
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenIdMismatch_ShouldReturnBadRequest()
    {
        var result = await _controller.Edit(1, new HealthRecordEditDto { Id = 2 });

        result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task EditPost_WhenUpdateSucceeds_ShouldRedirectToDetailsWithSuccessMessage()
    {
        _mockService.Setup(s => s.UpdateHealthRecordAsync(It.IsAny<HealthRecordEditDto>())).ReturnsAsync(true);

        var result = await _controller.Edit(4, new HealthRecordEditDto { Id = 4 });

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Details");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenUpdateFails_ShouldReturnViewWithModelError()
    {
        _mockService.Setup(s => s.UpdateHealthRecordAsync(It.IsAny<HealthRecordEditDto>())).ReturnsAsync(false);

        var result = await _controller.Edit(4, new HealthRecordEditDto { Id = 4 });

        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteSucceeds_ShouldRedirectToAnimalsIndexWithSuccessMessage()
    {
        _mockService.Setup(s => s.DeleteHealthRecordAsync(9)).ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(9);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("Animals");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteFails_ShouldRedirectWithErrorMessage()
    {
        _mockService.Setup(s => s.DeleteHealthRecordAsync(9)).ReturnsAsync(false);

        var result = await _controller.DeleteConfirmed(9);

        result.Should().BeOfType<RedirectToActionResult>();
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }
}