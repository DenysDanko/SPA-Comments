using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPA_Comments.Models
{

    public class Comment
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public Comment? Parent { get; set; }

        [Required]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Url]
        public string? HomePage { get; set; }

        [Required]
        public required string Content { get; set; }

        public string? FilePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Comment> Replies { get; set; } = [];
    }
}
