using PostService_module.Core.Entities;
using PostService_module.Core.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using PostService_module.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace PostService_module.Core.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserClientService _userClientService;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository, IUserClientService userClientService, IMapper mapper)
        {
            _postRepository = postRepository;
            _userClientService = userClientService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            var enrichedPosts = new List<Post>();

            foreach (var post in posts)
            {
                try
                {
                    var userDetails = await _userClientService.GetUserByIdAsync(post.UserId);
                    if (userDetails != null)
                    {
                        post.FirstName = userDetails.FirstName;
                        post.LastName = userDetails.LastName;
                        post.UserName = $"{userDetails.FirstName} {userDetails.LastName}";
                        post.UserRegisteredAt = userDetails.RegisteredAt;
                        enrichedPosts.Add(post);
                    }
                }
                catch (Exception ex)
                {
                    post.FirstName = "Unknown";
                    post.LastName = "User";
                    post.UserName = "Unknown User";
                    post.UserRegisteredAt = DateTime.UtcNow; 
                }
            }

            return enrichedPosts;
        }

        public async Task<PostResponseDTO> GetPostByIdAsync(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                throw new KeyNotFoundException($"Post with ID {id} not found.");
            }

            try
            {
                var userDetails = await _userClientService.GetUserByIdAsync(post.UserId);
                post.FirstName = userDetails.FirstName;
                post.LastName = userDetails.LastName;
                post.UserName = $"{userDetails.FirstName} {userDetails.LastName}";
            }
            catch (Exception ex)
            {
                // Handle error - post will still be returned
                post.FirstName = "Unknown";
                post.LastName = "User";
                post.UserName = "Unknown User";
                Console.Error.WriteLine($"Error fetching user details for post {post.Id}: {ex.Message}");
            }

            return _mapper.Map<PostResponseDTO>(post);
        }

        public async Task<IEnumerable<PostListDTO>> GetPostsByUserIdAsync(int userId)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId);
            var enrichedPosts = new List<Post>();

            try
            {
                var userDetails = await _userClientService.GetUserByIdAsync(userId);
                foreach (var post in posts)
                {
                    post.FirstName = userDetails.FirstName;
                    post.LastName = userDetails.LastName;
                    post.UserName = $"{userDetails.FirstName} {userDetails.LastName}";
                    enrichedPosts.Add(post);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                foreach (var post in posts)
                {
                    post.FirstName = "Unknown";
                    post.LastName = "User";
                    post.UserName = "Unknown User";
                    enrichedPosts.Add(post);
                }
                Console.Error.WriteLine($"Error fetching user details for userId {userId}: {ex.Message}");
            }

            return _mapper.Map<IEnumerable<PostListDTO>>(enrichedPosts);
        }

        public async Task AddPostAsync(CreatePostDTO createPostDto)
        {
            var post = _mapper.Map<Post>(createPostDto);
            await _postRepository.AddPostAsync(post);
        }

        public async Task UpdatePostAsync(int id, UpdatePostDTO updatePostDto)
        {
            var existingPost = await _postRepository.GetPostByIdAsync(id);
            if (existingPost == null)
                throw new KeyNotFoundException($"Post with ID {id} not found.");

            existingPost.Content = updatePostDto.Content;
            existingPost.ImageUrl = updatePostDto.ImageUrl;
            existingPost.IsPublic = updatePostDto.IsPublic;
            await _postRepository.UpdatePostAsync(existingPost);
        }

        public async Task DeletePostAsync(int id)
        {
            await _postRepository.DeletePostAsync(id);
        }
    }
}