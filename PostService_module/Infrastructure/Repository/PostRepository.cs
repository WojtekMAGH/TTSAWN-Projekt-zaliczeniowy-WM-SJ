using Microsoft.EntityFrameworkCore;
using PostService_module.Core.Entities;
using PostService_module.Core.Interfaces;
using PostService_module.Infrastructure.Data;

namespace PostService_module.Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly PostDbContext _context;

        public PostRepository(PostDbContext context)
        {
            _context = context;
        }

        // New Post in database
        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
    
        }

        // Retrieves a post by their ID
        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _context.Posts.FirstOrDefaultAsync(u => u.Id == id);
        }

        // Deletes an existing post from the database
        public async Task DeletePostAsync(int id)
        {
            Post post = await GetPostByIdAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        // Retrieves all posts from the database
        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts.ToListAsync();
        }

        // Retrieves posts by user id
        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId)  // Filter posts by userId
                .ToListAsync(); 
        }
        public async Task UpdatePostAsync(Post user)
        {
            _context.Posts.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
