using SPA_Comments.DTO;

namespace SPA_Comments.Services
{
    public interface ICommentService
    {
        Task<PagedCommentsResponseDto> GetPagedCommentsAsync(int page, string sortBy, bool desc);
        Task<CommentResponseDto> CreateCommentAsync(CommentCreateDto dto);
    }
}
