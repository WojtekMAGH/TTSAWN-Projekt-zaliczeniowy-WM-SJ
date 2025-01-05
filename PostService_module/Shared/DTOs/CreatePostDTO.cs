namespace PostService_module.Shared.DTOs
{
    public class CreatePostDTO
    {
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; } = true;
        public int UserId { get; set; }
    }
}
