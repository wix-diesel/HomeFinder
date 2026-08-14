using System.ComponentModel.DataAnnotations;
using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Contracts;

public class UpdateUserProfileRequest
{
    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string DisplayName { get; set; } = string.Empty;
}

public class AvatarResponse
{
    public string AvatarImagePath { get; set; } = string.Empty;
}

public class UpdateThemePreferenceRequest
{
    [Required]
    [EnumDataType(typeof(ThemeMode), ErrorMessage = "テーマは Light または Dark を指定してください。")]
    public ThemeMode? ThemePreference { get; set; }
}
