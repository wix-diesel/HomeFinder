using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Application.Contracts;

public class UpdateUserProfileRequest
{
    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(512)]
    public string AvatarImagePath { get; set; } = string.Empty;
}
