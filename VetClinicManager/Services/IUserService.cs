using Microsoft.AspNetCore.Identity;
using VetClinicManager.Areas.Admin.DTOs.Users;
using VetClinicManager.Models;

namespace VetClinicManager.Services;

public interface IUserService
{
    // For Index GET action
    Task<List<UserListDto>> GetAllUsersWithRolesAsync();

    // For Create GET and Edit GET actions
    Task<List<string>> GetAllAvailableRolesAsync();

    // For Create POST action
    Task<(IdentityResult result, User? user)> CreateUserAsync(UserCreateDto userDto);

    // For Edit GET action
    Task<UserEditDto?> GetUserForEditAsync(string userId);

    // For Edit POST action
    Task<IdentityResult> UpdateUserAsync(UserEditDto userDto);

    // For Delete GET action
    Task<UserDeleteDto?> GetUserForDeleteAsync(string userId);

    // For Delete POST action
    Task<IdentityResult> DeleteUserAsync(string userId);

    // TODO: Can add user password change functionality
}