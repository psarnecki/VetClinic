using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using VetClinicManager.Areas.Admin.Controllers;
using VetClinicManager.Areas.Admin.DTOs.Medications;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class MedicationsControllerTests
{
    private Mock<IMedicationService> _mockService = null!;
    private MedicationsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IMedicationService>();
        _controller = new MedicationsController(_mockService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Details_WhenIdIsNull_ShouldReturnNotFound()
    {
        var result = await _controller.Details((int?)null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Details_WhenServiceReturnsNull_ShouldReturnNotFound()
    {
        _mockService.Setup(s => s.GetMedicationForDetailsAsync(5)).ReturnsAsync((MedicationDetailsDto?)null);

        var result = await _controller.Details(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task CreatePost_WhenModelInvalid_ShouldReturnViewWithDto()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var dto = new MedicationCreateDto();

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dto);
        _mockService.Verify(s => s.CreateMedicationAsync(It.IsAny<MedicationCreateDto>()), Times.Never);
    }

    [Test]
    public async Task CreatePost_WhenModelValid_ShouldRedirectToIndexWithSuccessMessage()
    {
        _mockService.Setup(s => s.CreateMedicationAsync(It.IsAny<MedicationCreateDto>()))
            .ReturnsAsync(new MedicationListDto());

        var result = await _controller.Create(new MedicationCreateDto());

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenIdMismatch_ShouldReturnBadRequest()
    {
        var result = await _controller.Edit(1, new MedicationEditDto { Id = 2 });

        result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task EditPost_WhenUpdateSucceeds_ShouldRedirectToIndexWithSuccessMessage()
    {
        _mockService.Setup(s => s.UpdateMedicationAsync(It.IsAny<MedicationEditDto>())).ReturnsAsync(true);

        var result = await _controller.Edit(3, new MedicationEditDto { Id = 3 });

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task EditPost_WhenUpdateFails_ShouldReturnViewWithModelError()
    {
        _mockService.Setup(s => s.UpdateMedicationAsync(It.IsAny<MedicationEditDto>())).ReturnsAsync(false);

        var result = await _controller.Edit(3, new MedicationEditDto { Id = 3 });

        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteSucceeds_ShouldRedirectToIndexWithSuccessMessage()
    {
        _mockService.Setup(s => s.DeleteMedicationAsync(8)).ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(8);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task DeleteConfirmed_WhenMedicationInUse_ShouldRedirectBackToDeleteWithErrorMessage()
    {
        _mockService.Setup(s => s.DeleteMedicationAsync(8)).ReturnsAsync(false);

        var result = await _controller.DeleteConfirmed(8);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Delete");
        redirect.RouteValues!["id"].Should().Be(8);
        _controller.TempData["ErrorMessage"].Should().NotBeNull();
    }
}