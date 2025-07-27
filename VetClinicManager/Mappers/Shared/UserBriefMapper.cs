using Riok.Mapperly.Abstractions;
using VetClinicManager.DTOs.Shared;
using VetClinicManager.Models;

namespace VetClinicManager.Mappers;

[Mapper]
public partial class UserBriefMapper
{
    // Maps a User entity to its brief DTO representation
    public partial UserBriefDto ToUserBriefDto(User user);
}