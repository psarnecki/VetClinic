using FluentAssertions;
using VetClinicManager.Areas.Admin.DTOs.Users;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Models;

namespace VetClinicManager.Tests.Unit.Mappers;

[TestFixture]
public class UserMapperTests
{
    private readonly UserMapper _mapper = new();

    [Test]
    public void ToUser_ShouldUseEmailAsUserNameAndConfirmEmail()
    {
        var createDto = new UserCreateDto
        {
            Email = "jane.doe@test.local",
            Password = "Secret123",
            ConfirmPassword = "Secret123",
            FirstName = "Jane",
            LastName = "Doe",
            Specialization = "Surgery"
        };

        var user = _mapper.ToUser(createDto);

        user.Email.Should().Be("jane.doe@test.local");
        user.UserName.Should().Be("jane.doe@test.local");
        user.EmailConfirmed.Should().BeTrue();
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Doe");
        user.Specialization.Should().Be("Surgery");
    }

    [Test]
    public void UpdateUserFromDto_ShouldUpdateProfileFieldsAndPreserveEmailAndUserName()
    {
        var user = new User
        {
            Id = "user-1",
            Email = "original@test.local",
            UserName = "original@test.local",
            FirstName = "Old",
            LastName = "Name",
            Specialization = "General"
        };

        var editDto = new UserEditDto
        {
            Id = "user-1",
            Email = "changed@test.local",
            FirstName = "New",
            LastName = "Surname",
            Specialization = "Dentistry"
        };

        _mapper.UpdateUserFromDto(editDto, user);

        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Surname");
        user.Specialization.Should().Be("Dentistry");
        user.Email.Should().Be("original@test.local");
        user.UserName.Should().Be("original@test.local");
    }
}