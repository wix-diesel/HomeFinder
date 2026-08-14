using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HomeFinder.Application.Utils;
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
    [Required(ErrorMessage = "テーマは light または dark を指定してください。")]
    [JsonConverter(typeof(ThemeModeJsonConverter))]
    public ThemeMode? ThemePreference { get; set; }
}
