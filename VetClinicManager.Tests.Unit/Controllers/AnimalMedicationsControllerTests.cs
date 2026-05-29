using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using VetClinicManager.Controllers;
using VetClinicManager.DTOs.AnimalMedications;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Controllers;

[TestFixture]
public class AnimalMedicationsControllerTests
{
    private Mock<IMedicationService> _mockMedicationService = null!;
    private Mock<IAnimalMedicationService> _mockAnimalMedicationService = null!;
    private AnimalMedicationsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockMedicationService = new Mock<IMedicationService>();
        _mockMedicationService.Setup(s => s.GetMedicationsForSelectListAsync())
            .ReturnsAsync(Array.Empty<MedicationBriefDto>());
        _mockAnimalMedicationService = new Mock<IAnimalMedicationService>();

        _controller = new AnimalMedicationsController(_mockMedicationService.Object, _mockAnimalMedicationService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task CreateGet_WhenAnimalUnknown_ShouldReturnNotFound()
    {
        _mockAnimalMedicationService.Setup(s => s.GetForCreateAsync(99)).ReturnsAsync((AnimalMedicationCreateDto?)null);

        var result = await _controller.Create(99);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task CreatePost_WhenValidWithHealthRecordId_ShouldRedirectToHealthRecordDetails()
    {
        _mockAnimalMedicationService.Setup(s => s.CreateAnimalMedicationAsync(It.IsAny<AnimalMedicationCreateDto>()))
            .ReturnsAsync(1);

        var result = await _controller.Create(new AnimalMedicationCreateDto { AnimalId = 5, HealthRecordId = 12 });

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.ControllerName.Should().Be("HealthRecords");
        redirect.RouteValues!["id"].Should().Be(12);
    }

    [Test]
    public async Task CreatePost_WhenValidWithoutHealthRecordId_ShouldRedirectToAnimalDetails()
    {
        _mockAnimalMedicationService.Setup(s => s.CreateAnimalMedicationAsync(It.IsAny<AnimalMedicationCreateDto>()))
            .ReturnsAsync(1);

        var result = await _controller.Create(new AnimalMedicationCreateDto { AnimalId = 5, HealthRecordId = 0 });

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.ControllerName.Should().Be("Animals");
        redirect.RouteValues!["id"].Should().Be(5);
    }

    [Test]
    public async Task CreatePost_WhenModelInvalid_ShouldReturnView()
    {
        _controller.ModelState.AddModelError("MedicationId", "Required");
        var dto = new AnimalMedicationCreateDto { AnimalId = 5 };

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(dto);
        _mockAnimalMedicationService.Verify(s => s.CreateAnimalMedicationAsync(It.IsAny<AnimalMedicationCreateDto>()), Times.Never);
    }

    [Test]
    public async Task EditPost_WhenIdMismatch_ShouldReturnBadRequest()
    {
        var result = await _controller.Edit(1, new AnimalMedicationEditDto { Id = 2 });

        result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task EditPost_WhenUpdateFails_ShouldReturnNotFound()
    {
        _mockAnimalMedicationService.Setup(s => s.UpdateAnimalMedicationAsync(It.IsAny<AnimalMedicationEditDto>()))
            .ReturnsAsync(false);

        var result = await _controller.Edit(3, new AnimalMedicationEditDto { Id = 3 });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task EditPost_WhenUpdateSucceeds_ShouldRedirectToHealthRecordDetails()
    {
        _mockAnimalMedicationService.Setup(s => s.UpdateAnimalMedicationAsync(It.IsAny<AnimalMedicationEditDto>()))
            .ReturnsAsync(true);

        var result = await _controller.Edit(3, new AnimalMedicationEditDto { Id = 3, HealthRecordId = 15 });

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("HealthRecords");
        redirect.RouteValues!["id"].Should().Be(15);
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Test]
    public async Task DeleteConfirmed_WhenDeleteSucceeds_ShouldRedirectToHealthRecordDetails()
    {
        _mockAnimalMedicationService.Setup(s => s.DeleteAnimalMedicationAsync(8)).ReturnsAsync(true);

        var result = await _controller.DeleteConfirmed(new AnimalMedicationDeleteDto { Id = 8, HealthRecordId = 20 });

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("HealthRecords");
        redirect.RouteValues!["id"].Should().Be(20);
        _mockAnimalMedicationService.Verify(s => s.DeleteAnimalMedicationAsync(8), Times.Once);
    }
}