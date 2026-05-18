using Microsoft.AspNetCore.Mvc;
using SPA_Comments.DTO;
using SPA_Comments.Services;

namespace CommentSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] string sortBy = "date", [FromQuery] bool desc = true)
        {
            var result = await _commentService.GetPagedCommentsAsync(page, sortBy, desc);
            return Ok(new
            {
                totalCount = result.TotalCount,
                page,
                pageSize = 25,
                items = result.Items
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CommentCreateDto dto)
        {
            try
            {
                var result = await _commentService.CreateCommentAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}