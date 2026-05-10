using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Core.Entities;

public class UserProfile
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntraObjectId { get; set; } = string.Empty;

    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string AvatarImagePath { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
