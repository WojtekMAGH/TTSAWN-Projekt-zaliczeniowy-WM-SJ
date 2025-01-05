using Microsoft.AspNetCore.Mvc;
using PostService_module.Core.Entities;
using PostService_module.Core.Interfaces;
using AutoMapper;
using PostService_module.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PostService_module.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;

        public PostController(IPostService postService, IUploadService uploadService, IMapper mapper)
        {
            _postService = postService;
            _uploadService = uploadService;
            _mapper = mapper;
        }

        // GET: api/post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostListDTO>>> GetAllPosts()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out var parsedUserId))
            {
                return Unauthorized("User ID not found or invalid in token");
            }

            var allPosts = await _postService.GetAllPostsAsync();

            // Filter posts ->
            // 1. Show all public posts
            // 2. Show private posts only if they belong to the current user
            var filteredPosts = allPosts.Where(post =>
                post.IsPublic || post.UserId == parsedUserId
            );

            var postDtos = _mapper.Map<IEnumerable<PostListDTO>>(filteredPosts);
            return Ok(postDtos);
        }

        // GET: api/post/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponseDTO>> GetPostById(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out var parsedUserId))
                {
                    return Unauthorized("User ID not found or invalid in token");
                }

                var post = await _postService.GetPostByIdAsync(id);

                // Can User view this post?
                if (!post.IsPublic && post.UserId != parsedUserId)
                {
                    return Forbid();
                }

                var postResponseDto = _mapper.Map<PostResponseDTO>(post);
                return Ok(postResponseDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/post
        [HttpPost]
        public async Task<ActionResult<PostResponseDTO>> CreatePost([FromBody] CreatePostDTO createPostDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized("User ID not found or invalid in token");
            }

            try
            {
                createPostDto.UserId = parsedUserId;
                await _postService.AddPostAsync(createPostDto);

                var posts = await _postService.GetPostsByUserIdAsync(parsedUserId);
                var lastPost = posts.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                if (lastPost == null)
                    return StatusCode(500, "Failed to create post");

                var postResponseDto = _mapper.Map<PostResponseDTO>(lastPost);
                return CreatedAtAction(nameof(GetPostById), new { id = lastPost.Id }, postResponseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while creating the post");
            }
        }

        // DELETE: api/post/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized("User ID not found or invalid in token");
            }

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != parsedUserId)
                return Forbid();

            // IMAGE
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                _uploadService.DeleteImage(post.ImageUrl);
            }

            await _postService.DeletePostAsync(id);
            return NoContent();
        }

        // PUT: api/post/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDTO updatePostDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
                {
                    return Unauthorized("User ID not found or invalid in token");
                }

                var existingPost = await _postService.GetPostByIdAsync(id);
                if (existingPost == null)
                    return NotFound();

                if (existingPost.UserId != parsedUserId)
                    return Forbid();

                // IMAGE URL CHANGE
                if (!string.IsNullOrEmpty(existingPost.ImageUrl) &&
                    existingPost.ImageUrl != updatePostDto.ImageUrl)
                {
                    _uploadService.DeleteImage(existingPost.ImageUrl);
                }

                await _postService.UpdatePostAsync(id, updatePostDto);

                // Updated post
                var updatedPost = await _postService.GetPostByIdAsync(id);
                return Ok(_mapper.Map<PostResponseDTO>(updatedPost));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the post");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                Console.WriteLine($"Received file upload request. Filename: {file.FileName}, Content-Type: {file.ContentType}");

                var imageUrl = await _uploadService.UploadImageAsync(file);
                return Ok(new { url = imageUrl });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Validation error during upload: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during upload: {ex.Message}");
                return StatusCode(500, "An error occurred while uploading the file");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostListDTO>>> GetPostsByUserId(int userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out var parsedUserId))
            {
                return Unauthorized("User ID not found or invalid in token");
            }

            var posts = await _postService.GetPostsByUserIdAsync(userId);

            // If viewing own profile -> show all posts
            // If viewing other's profile -> show only public posts
            var filteredPosts = parsedUserId == userId
                ? posts
                : posts.Where(p => p.IsPublic);

            var postDtos = _mapper.Map<IEnumerable<PostListDTO>>(filteredPosts);
            return Ok(postDtos);
        }

    }
}
