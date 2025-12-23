using Riok.Mapperly.Abstractions;
using VetClinicManager.Areas.Admin.DTOs.Users;
using VetClinicManager.Models;

namespace VetClinicManager.Areas.Admin.Mappers;

[Mapper]
public partial class UserMapper 
{
    // --- Mappings to DTOs ---
    
    // Mapping from User to UserListDto (Index)
    public partial UserListDto ToUserListDtoFromUser(User user);
    
    // Mapping from User to UserEditDto (Edit GET)
    public partial UserEditDto ToUserEditDto(User user);
    
    // Mapping from User to UserDeleteDto (Delete GET)
    public partial UserDeleteDto ToUserDeleteDto(User user);

    // --- Mappings from DTOs to Entity ---
    
    // Mapping from UserCreateDto to User (Create POST)
    [MapProperty(nameof(UserCreateDto.Email), nameof(User.UserName))]
    [MapValue(nameof(User.EmailConfirmed), true)]
    [MapperIgnoreSource(nameof(UserCreateDto.Password))]
    [MapperIgnoreSource(nameof(UserCreateDto.ConfirmPassword))]
    public partial User ToUser(UserCreateDto userDto);

    // Mapping from UserEditDto to existing User (Edit POST)
    [MapperIgnoreSource(nameof(UserEditDto.Id))]
    [MapperIgnoreSource(nameof(UserEditDto.Email))]
    [MapperIgnoreTarget(nameof(User.UserName))]
    [MapperIgnoreSource(nameof(UserEditDto.AvailableRoles))]
    [MapperIgnoreSource(nameof(UserEditDto.SelectedRoles))]
    public partial void UpdateUserFromDto(UserEditDto userDto, User user);
}
