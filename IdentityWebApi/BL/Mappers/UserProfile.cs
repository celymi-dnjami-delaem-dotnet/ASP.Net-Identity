using AutoMapper;

using IdentityWebApi.DAL.Entities;
using IdentityWebApi.PL.Models.Action;
using IdentityWebApi.PL.Models.DTO;

using System.Linq;

namespace IdentityWebApi.BL.Mappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<AppUser, UserResultDto>()
            .ForMember(
                dist => dist.Roles,
                ex => ex.MapFrom(en => en.UserRoles.Select(x => x.Role.Name))
            );

        CreateMap<UserActionModel, AppUser>();
        CreateMap<UserRegistrationActionModel, AppUser>();
    }
}
