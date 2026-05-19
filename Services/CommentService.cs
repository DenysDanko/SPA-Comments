using AutoMapper;
using CommentSystem.Api.Data;
using CommentSystem.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly ICaptchaService _captchaService;
        private readonly IMemoryCache _memoryCache;
        private const int PageSize = 25;

        public CommentService(ApplicationDbContext context, IWebHostEnvironment env, IMapper mapper, 
            IHubContext<CommentHub> hubContext, ICaptchaService captchaService, IMemoryCache memoryCache)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
            _hubContext = hubContext;
            _captchaService = captchaService;
            _memoryCache = memoryCache;
        }

        public async Task<PagedCommentsResponseDto> GetPagedCommentsAsync(int page, string sortBy, bool desc)
        {
            string cacheKey = $"comments_page_{page}_{sortBy}_{desc}";

            if (!_memoryCache.TryGetValue(cacheKey, out PagedCommentsResponseDto? cachedResponse))
            {
                IQueryable<Comment> rootQuery = _context.Comments
                    .Where(c => c.ParentId == null);

                rootQuery = sortBy.ToLower() switch
                {
                    "username" => desc ? rootQuery.OrderByDescending(c => c.UserName) : rootQuery.OrderBy(c => c.UserName),
                    "email" => desc ? rootQuery.OrderByDescending(c => c.Email) : rootQuery.OrderBy(c => c.Email),
                    _ => desc ? rootQuery.OrderByDescending(c => c.CreatedAt) : rootQuery.OrderBy(c => c.CreatedAt),
                };

                int totalCount = await rootQuery.CountAsync();

                List<Comment>? rootComments = await rootQuery
                    .Skip((page - 1) * 25)
                    .Take(25)
                    .ToListAsync();

                List<Comment>? allComments = await _context.Comments.ToListAsync();

                cachedResponse = new PagedCommentsResponseDto
                {
                    RootItems = _mapper.Map<IEnumerable<CommentResponseDto>>(rootComments),
                    AllItems = _mapper.Map<IEnumerable<CommentResponseDto>>(allComments),
                    TotalCount = totalCount
                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30));

                _memoryCache.Set(cacheKey, cachedResponse, cacheOptions);
            }

            return cachedResponse!;
        }

        public async Task<CommentResponseDto> CreateCommentAsync(CommentCreateDto dto)
        {
            if (!_captchaService.ValidateCaptcha(dto.CaptchaId, dto.CaptchaAnswer))
                throw new ArgumentException("Invalid captcha.");

            Comment? comment = _mapper.Map<Comment>(dto);

            if (dto.File != null)
            {
                comment.FilePath = await ProcessFileAsync(dto.File);
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            _memoryCache.Remove("comments_p1_date_true");

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