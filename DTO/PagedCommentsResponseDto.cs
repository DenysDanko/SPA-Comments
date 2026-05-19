namespace SPA_Comments.DTO
{
    public class PagedCommentsResponseDto
    {
        public IEnumerable<CommentResponseDto> RootItems { get; set; } = new List<CommentResponseDto>();
        public IEnumerable<CommentResponseDto> AllItems { get; set; } = new List<CommentResponseDto>();
        public int TotalCount { get; set; }
    }
}
