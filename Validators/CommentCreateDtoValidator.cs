using FluentValidation;
using SPA_Comments.DTO;
using System.Text.RegularExpressions;

namespace CommentSystem.Api.Validators
{
    public class CommentCreateDtoValidator : AbstractValidator<CommentCreateDto>
    {
        public CommentCreateDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User Name is required.")
                .Matches("^[a-zA-Z0-9]*$").WithMessage("User Name can only contain Latin letters and numbers.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail is required.")
                .EmailAddress().WithMessage("Incorrect E-mail format.");

            RuleFor(x => x.HomePage)
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Incorrect URL format for Home page.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("The message cannot be empty.")
                .Must(IsValidXhtml).WithMessage("The text contains prohibited tags or has unclosed tags.");

            RuleFor(x => x.File)
                .Must(file => file == null || IsValidFile(file))
                .WithMessage("Invalid file type or size (TXT up to 100KB, JPG/GIF/PNG images).");
        }

        private bool IsValidXhtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;

            var allowedTags = new[] { "a", "code", "i", "strong" };

            var tagRegex = new Regex(@"<(/?[a-zA-Z0-9]+)[^>]*>");
            var matches = tagRegex.Matches(text);

            foreach (Match match in matches)
            {
                var tagName = match.Groups[1].Value.Replace("/", "").ToLower();
                if (!allowedTags.Contains(tagName)) return false;
            }

            return IsTagsBalanced(text);
        }

        private bool IsTagsBalanced(string text)
        {
            var stack = new Stack<string>();
            var tagRegex = new Regex(@"<(/?)(a|code|i|strong)(?:\s+[^>]*)?>");
            var matches = tagRegex.Matches(text);

            foreach (Match match in matches)
            {
                var isClosing = match.Groups[1].Value == "/";
                var tagName = match.Groups[2].Value;

                if (!isClosing)
                    stack.Push(tagName);
                else
                {
                    if (stack.Count == 0 || stack.Pop() != tagName)
                        return false;
                }
            }
            return stack.Count == 0;
        }

        private bool IsValidFile(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedImageExt = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (allowedImageExt.Contains(ext)) return true;

            if (ext == ".txt")
                return file.Length <= 100 * 1024;

            return false;
        }
    }
}