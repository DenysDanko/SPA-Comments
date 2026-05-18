using CommentSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _captchaService;

    public CaptchaController(ICaptchaService captchaService)
    {
        _captchaService = captchaService;
    }

    [HttpGet]
    public IActionResult GetCaptcha()
    {
        var (id, bytes) = _captchaService.GenerateCaptcha();
        return Ok(new { CaptchaId = id, CaptchaImage = Convert.ToBase64String(bytes) });
    }
}