using PostService_module.Core.Entities;

namespace PostService_module.Core.Interfaces
{
    public interface IPostRepository
    {
        Task<Post?> GetPostByIdAsync(int id);  // Retrieve a post by its ID
        Task<IEnumerable<Post>> GetAllPostsAsync();  // Retrieve all posts
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);  // Retrieve posts by a specific user
        Task AddPostAsync(Post post);  // Create a new post
        Task UpdatePostAsync(Post post);  // Update an existing post
        Task DeletePostAsync(int id);  // Delete a post by its ID
    }
}
