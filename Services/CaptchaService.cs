using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace CommentSystem.Api.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IMemoryCache _cache;
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public CaptchaService(IMemoryCache cache) => _cache = cache;

        public (string CaptchaId, byte[] ImageBytes) GenerateCaptcha()
        {
            string? code = new string(Enumerable.Repeat(Chars, 5).Select(s => s[new Random().Next(s.Length)]).ToArray());
            string? captchaId = Guid.NewGuid().ToString();

            _cache.Set(captchaId, code, TimeSpan.FromMinutes(5));

            using var image = new Image<Rgba32>(150, 50);
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);
                for (int i = 0; i < 10; i++)
                {
                    ctx.DrawLines(Color.Silver, 1, new PointF(new Random().Next(150), new Random().Next(50)), new PointF(new Random().Next(150), new Random().Next(50)));
                }
                ctx.DrawText(code, SystemFonts.CreateFont("Arial", 25), Color.Black, new PointF(20, 10));
            });

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return (captchaId, ms.ToArray());
        }

        public bool ValidateCaptcha(string captchaId, string userAnswer)
        {
            if (_cache.TryGetValue(captchaId, out string? correctCode))
            {
                _cache.Remove(captchaId);
                return string.Equals(correctCode, userAnswer, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}