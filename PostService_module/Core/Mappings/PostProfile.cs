using AutoMapper;
using PostService_module.Core.Entities;
using PostService_module.Shared.DTOs;

namespace PostService_module.Core.Mappings
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            // Map Post <-> PostListDTO
            CreateMap<Post, PostListDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrl) ? null : src.ImageUrl))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.UserRegisteredAt, opt => opt.MapFrom(src => src.UserRegisteredAt));

            // Map Post <-> PostResponseDTO
            CreateMap<Post, PostResponseDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrl) ? null : src.ImageUrl))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.UserRegisteredAt, opt => opt.MapFrom(src => src.UserRegisteredAt));

            CreateMap<PostResponseDTO, Post>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.UserRegisteredAt, opt => opt.MapFrom(src => src.UserRegisteredAt));

            // Map CreatePostDTO -> Post
            CreateMap<CreatePostDTO, Post>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrl) ? null : src.ImageUrl))
                .ForMember(dest => dest.FirstName, opt => opt.Ignore())
                .ForMember(dest => dest.LastName, opt => opt.Ignore())
                .ForMember(dest => dest.UserRegisteredAt, opt => opt.Ignore()); // Add this

            // Map UpdatePostDTO -> Post
            CreateMap<UpdatePostDTO, Post>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrl) ? null : src.ImageUrl))
                .ForMember(dest => dest.FirstName, opt => opt.Ignore())
                .ForMember(dest => dest.LastName, opt => opt.Ignore())
                .ForMember(dest => dest.UserRegisteredAt, opt => opt.Ignore()); // Add this

            // Map PostListDTO <-> PostResponseDTO
            CreateMap<PostListDTO, PostResponseDTO>();
            CreateMap<PostResponseDTO, PostListDTO>();
        }
    }
}