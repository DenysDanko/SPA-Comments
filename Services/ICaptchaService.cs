namespace CommentSystem.Api.Services
{
    public interface ICaptchaService
    {
        (string CaptchaId, byte[] ImageBytes) GenerateCaptcha();
        bool ValidateCaptcha(string captchaId, string userAnswer);
    }
}