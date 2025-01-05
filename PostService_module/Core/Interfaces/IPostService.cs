using PostService_module.Core.Entities;
using PostService_module.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostService_module.Core.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllPostsAsync(); 
        Task<PostResponseDTO> GetPostByIdAsync(int id);
        Task<IEnumerable<PostListDTO>> GetPostsByUserIdAsync(int userId);
        Task AddPostAsync(CreatePostDTO createPostDto);
        Task UpdatePostAsync(int id, UpdatePostDTO updatePostDto);
        Task DeletePostAsync(int id);
    }
}