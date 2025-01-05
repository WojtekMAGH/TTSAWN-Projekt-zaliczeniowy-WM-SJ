namespace PostService_module.Shared.DTOs
{
    public class PostResponseDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public DateTime UserRegisteredAt { get; set; }
    }

}
