using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Users.UserBriefs;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class UserBriefMapper
{
    // Maps a User entity to its brief DTO representation
    public partial UserBriefDto ToBriefDto(User user);
}