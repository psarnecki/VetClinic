using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using VetClinicManager.Areas.Admin.DTOs.Users;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Services;

namespace VetClinicManager.Tests.Unit.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<RoleManager<IdentityRole>> _mockRoleManager = null!;
    private UserService _service = null!;

    [SetUp]
    public void Setup()
    {
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null!, null!, null!, null!);

        _service = new UserService(_mockUserManager.Object, _mockRoleManager.Object, new UserMapper());
    }

    [Test]
    public async Task CreateUserAsync_WhenCreationSucceedsWithNoRoles_ShouldReturnUserAndNotAssignRoles()
    {
        var dto = new UserCreateDto
        {
            Email = "new@test.local",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!",
            FirstName = "New",
            LastName = "User",
            SelectedRoles = new List<string>()
        };

        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        var (result, user) = await _service.CreateUserAsync(dto);

        result.Succeeded.Should().BeTrue();
        user.Should().NotBeNull();
        user!.Email.Should().Be("new@test.local");
        _mockUserManager.Verify(
            m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }

    [Test]
    public async Task CreateUserAsync_WhenCreationFails_ShouldReturnFailedResultAndNullUser()
    {
        var dto = new UserCreateDto
        {
            Email = "bad@test.local",
            Password = "weak",
            ConfirmPassword = "weak",
            FirstName = "Bad",
            LastName = "User"
        };

        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Too weak" }));

        var (result, user) = await _service.CreateUserAsync(dto);

        result.Succeeded.Should().BeFalse();
        user.Should().BeNull();
        _mockUserManager.Verify(
            m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_WhenUserNotFound_ShouldReturnFailedResult()
    {
        _mockUserManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((User?)null);

        var result = await _service.UpdateUserAsync(new UserEditDto { Id = "missing" });

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Description == "User not found.");
    }

    [Test]
    public async Task UpdateUserAsync_WhenRoleSelectionChanged_ShouldAddMissingAndRemoveExtraRoles()
    {
        var user = new User
        {
            Id = "u1",
            Email = "same@test.local",
            UserName = "same@test.local",
            FirstName = "F",
            LastName = "L"
        };

        _mockUserManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Vet", "Client" });
        _mockUserManager.Setup(m => m.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new UserEditDto
        {
            Id = "u1",
            Email = "same@test.local",
            FirstName = "F",
            LastName = "L",
            SelectedRoles = new List<string> { "Vet", "Admin" }
        };

        var result = await _service.UpdateUserAsync(dto);

        result.Succeeded.Should().BeTrue();
        _mockUserManager.Verify(
            m => m.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.SequenceEqual(new[] { "Admin" }))),
            Times.Once);
        _mockUserManager.Verify(
            m => m.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.SequenceEqual(new[] { "Client" }))),
            Times.Once);
        _mockUserManager.Verify(m => m.SetEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_WhenEmailChanged_ShouldSyncEmailAndUserName()
    {
        var user = new User
        {
            Id = "u1",
            Email = "old@test.local",
            UserName = "old@test.local",
            FirstName = "F",
            LastName = "L"
        };

        _mockUserManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.SetEmailAsync(user, "new@test.local")).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.SetUserNameAsync(user, "new@test.local")).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Vet" });

        var dto = new UserEditDto
        {
            Id = "u1",
            Email = "new@test.local",
            FirstName = "F",
            LastName = "L",
            SelectedRoles = new List<string> { "Vet" }
        };

        var result = await _service.UpdateUserAsync(dto);

        result.Succeeded.Should().BeTrue();
        _mockUserManager.Verify(m => m.SetEmailAsync(user, "new@test.local"), Times.Once);
        _mockUserManager.Verify(m => m.SetUserNameAsync(user, "new@test.local"), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_WhenNewPasswordProvided_ShouldRemoveThenAddPassword()
    {
        var user = new User
        {
            Id = "u1",
            Email = "same@test.local",
            UserName = "same@test.local",
            FirstName = "F",
            LastName = "L"
        };

        _mockUserManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Vet" });
        _mockUserManager.Setup(m => m.RemovePasswordAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.AddPasswordAsync(user, "Brand0New!")).ReturnsAsync(IdentityResult.Success);

        var dto = new UserEditDto
        {
            Id = "u1",
            Email = "same@test.local",
            FirstName = "F",
            LastName = "L",
            SelectedRoles = new List<string> { "Vet" },
            NewPassword = "Brand0New!"
        };

        var result = await _service.UpdateUserAsync(dto);

        result.Succeeded.Should().BeTrue();
        _mockUserManager.Verify(m => m.RemovePasswordAsync(user), Times.Once);
        _mockUserManager.Verify(m => m.AddPasswordAsync(user, "Brand0New!"), Times.Once);
    }

    [Test]
    public async Task DeleteUserAsync_WhenUserUnknown_ShouldReturnSuccessWithoutDeleting()
    {
        _mockUserManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((User?)null);

        var result = await _service.DeleteUserAsync("missing");

        result.Succeeded.Should().BeTrue();
        _mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Never);
    }
}