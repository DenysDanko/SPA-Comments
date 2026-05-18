using Microsoft.Extensions.Caching.Memory;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CommentSystem.Api.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IMemoryCache _cache;
        private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public CaptchaService(IMemoryCache cache) => _cache = cache;

        public (string CaptchaId, byte[] ImageBytes) GenerateCaptcha()
        {
            string? code = new string(Enumerable.Repeat(Chars, 5).Select(s => s[new Random().Next(s.Length)]).ToArray());
            string? captchaId = Guid.NewGuid().ToString();

            _cache.Set(captchaId, code, TimeSpan.FromMinutes(5));

            using var image = new Image<Rgba32>(150, 50);

            Font font;
            try
            {
                font = SystemFonts.CreateFont("Arial", 25, FontStyle.Bold);
            }
            catch
            {
                font = SystemFonts.Collection.Families.First().CreateFont(25, FontStyle.Bold);
            }

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var random = new Random();
                for (int i = 0; i < 6; i++)
                {
                    var p1 = new PointF(random.Next(150), random.Next(50));
                    var p2 = new PointF(random.Next(150), random.Next(50));
                    ctx.DrawLine(Color.Silver, 1.5f, p1, p2);
                }

                ctx.DrawText(code, font, Color.Black, new PointF(20, 10));

                for (int i = 0; i < 100; i++)
                {
                    ctx.Fill(Color.Gray, new EllipsePolygon(random.Next(150), random.Next(50), 1));
                }
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