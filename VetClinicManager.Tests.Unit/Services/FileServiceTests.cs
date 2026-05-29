using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class FileServiceTests
{
    private string _webRootPath = null!;
    private Mock<IWebHostEnvironment> _mockEnvironment = null!;
    private FileService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _webRootPath = Path.Combine(Path.GetTempPath(), "VetClinicTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_webRootPath);

        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(_webRootPath);

        _service = new FileService(_mockEnvironment.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_webRootPath))
            Directory.Delete(_webRootPath, recursive: true);
    }

    [Test]
    public async Task SaveFileAsync_WhenFileIsEmpty_ShouldReturnNull()
    {
        var file = CreateFormFileMock("photo.jpg", length: 0);

        var result = await _service.SaveFileAsync(file.Object, "uploads/animals");

        result.Should().BeNull();
        Directory.Exists(Path.Combine(_webRootPath, "uploads", "animals")).Should().BeFalse();
    }

    [Test]
    public async Task SaveFileAsync_WhenValidFile_ShouldReturnRelativeUrlAndCreateFileOnDisk()
    {
        var file = CreateFormFileMock("photo.jpg", content: [0xFF, 0xD8, 0xFF]);

        var result = await _service.SaveFileAsync(file.Object, "uploads/animals");

        result.Should().NotBeNull();
        result.Should().StartWith("/uploads/animals/");
        result.Should().EndWith(".jpg");

        var physicalPath = Path.Combine(_webRootPath, result!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(physicalPath).Should().BeTrue();
        (await File.ReadAllBytesAsync(physicalPath)).Should().Equal(0xFF, 0xD8, 0xFF);
    }

    [Test]
    public async Task DeleteFile_WhenFileExists_ShouldRemoveFileFromDisk()
    {
        var file = CreateFormFileMock("avatar.png", content: [1, 2, 3]);
        var url = await _service.SaveFileAsync(file.Object, "uploads/animals");
        var physicalPath = Path.Combine(_webRootPath, url!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(physicalPath).Should().BeTrue();

        _service.DeleteFile(url);

        File.Exists(physicalPath).Should().BeFalse();
    }

    [Test]
    public void DeleteFile_WhenUrlIsNullOrEmpty_ShouldNotThrow()
    {
        var actNull = () => _service.DeleteFile(null);
        var actEmpty = () => _service.DeleteFile("");

        actNull.Should().NotThrow();
        actEmpty.Should().NotThrow();
    }

    private static Mock<IFormFile> CreateFormFileMock(string fileName, int length = -1, byte[]? content = null)
    {
        content ??= [];
        if (length < 0)
            length = content.Length;

        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(length);
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((stream, _) => stream.Write(content))
            .Returns(Task.CompletedTask);

        return file;
    }
}