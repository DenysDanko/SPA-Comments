namespace SPA_Comments.DTO
{
    public class CommentCreateDto
    {
        public int? ParentId { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? HomePage { get; set; }
        public required string Content { get; set; }
        public string? Captcha { get; set; }
        public required string CaptchaId { get; set; }
        public required string CaptchaAnswer { get; set; }
        public IFormFile? File { get; set; }
    }

    public class CommentResponseDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? HomePage { get; set; }
        public required string Content { get; set; }
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentResponseDto> Replies { get; set; } = new();
    }
}
