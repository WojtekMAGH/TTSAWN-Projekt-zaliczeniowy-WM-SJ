using AutoMapper;
using UserService.Core.Entities;
using UserService.Shared.DTOs;

namespace UserService.Core.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(src => src.RegisteredAt));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.RegisteredAt, opt => opt.Ignore());
        }
    }
}