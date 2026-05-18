using SPA_Comments.DTO;

namespace SPA_Comments.Services
{
    public interface ICommentService
    {
        Task<(IEnumerable<CommentResponseDto> Items, int TotalCount)> GetPagedCommentsAsync(int page, string sortBy, bool desc);
        Task<CommentResponseDto> CreateCommentAsync(CommentCreateDto dto);
    }
}
