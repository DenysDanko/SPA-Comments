using AutoMapper;
using CommentSystem.Api.Data;
using CommentSystem.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SPA_Comments.DTO;
using SPA_Comments.Models;
using SPA_Comments.Services;

namespace CommentSystem.Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly IHubContext<CommentHub> _hubContext;
        private const int PageSize = 25;

        public CommentService(ApplicationDbContext context, IWebHostEnvironment env, IMapper mapper, IHubContext<CommentHub> hubContext)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<(IEnumerable<CommentResponseDto> Items, int TotalCount)> GetPagedCommentsAsync(int page, string sortBy, bool desc)
        {
            IQueryable<Comment>? query = _context.Comments
                .Include(c => c.Replies)
                .Where(c => c.ParentId == null);

            query = sortBy.ToLower() switch
            {
                "username" => desc ? query.OrderByDescending(c => c.UserName) : query.OrderBy(c => c.UserName),
                "email" => desc ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
                _ => desc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            };

            int totalCount = await query.CountAsync();
            List<Comment>? items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            IEnumerable<CommentResponseDto> dtos = _mapper.Map<IEnumerable<CommentResponseDto>>(items);
            return (dtos, totalCount);
        }

        public async Task<CommentResponseDto> CreateCommentAsync(CommentCreateDto dto)
        {
            Comment? comment = _mapper.Map<Comment>(dto);

            if (dto.File != null)
            {
                comment.FilePath = await ProcessFileAsync(dto.File);
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            CommentResponseDto? response = _mapper.Map<CommentResponseDto>(comment);

            await _hubContext.Clients.All.SendAsync("ReceiveComment", response);

            return response;
        }

        private async Task<string?> ProcessFileAsync(IFormFile file)
        {
            string? folderName = Path.Combine("wwwroot", "uploads");
            if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);

            string? fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string? fullPath = Path.Combine(folderName, fileName);

            if (file.ContentType.StartsWith("image/"))
            {
                using Image image = await Image.LoadAsync(file.OpenReadStream());
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(320, 240),
                    Mode = ResizeMode.Max
                }));
                await image.SaveAsync(fullPath);
            }
            else if (file.ContentType == "text/plain")
            {
                if (file.Length > 100 * 1024) throw new Exception("The TXT file is too large.");
                using FileStream stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }
    }
}