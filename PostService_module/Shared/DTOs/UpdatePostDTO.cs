namespace PostService_module.Shared.DTOs
{
    public class UpdatePostDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
    }

}

