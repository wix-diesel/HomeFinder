using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Repositories;
using HomeFinder.Core.Entities;
using HomeFinder.Core.Errors;

namespace HomeFinder.Application.Services;

public class UserProfileService(IUserProfileRepository userProfileRepository) : IUserProfileService
{
    public const string DefaultAvatarPath = "/images/user-avatar-default.svg";
    public const ThemeMode DefaultThemePreference = ThemeMode.Light;

    private static readonly HashSet<string> AllowedAvatarExtensions =
    [
        ".png",
        ".jpg",
        ".jpeg",
        ".svg",
    ];

    public async Task<Result<UserProfileDto>> GetOrCreateProfileAsync(
        string entraObjectId,
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entraObjectId) || string.IsNullOrWhiteSpace(email))
            {
                return new Result<UserProfileDto>(new ArgumentException("ユーザー識別情報が不足しています。"));
            }

            var existing = await userProfileRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
            if (existing is not null)
            {
                return MapToDto(existing);
            }

            var now = DateTime.UtcNow;
            var created = await userProfileRepository.AddAsync(new UserProfile
            {
                Id = Guid.NewGuid(),
                EntraObjectId = entraObjectId,
                Email = email.Trim(),
                DisplayName = email.Trim(),
                AvatarImagePath = DefaultAvatarPath,
                ThemePreference = DefaultThemePreference,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            }, cancellationToken);

            return MapToDto(created);
        }
        catch (Exception ex)
        {
            return new Result<UserProfileDto>(ex);
        }
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(
        string entraObjectId,
        string email,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entraObjectId) || string.IsNullOrWhiteSpace(email))
            {
                return new Result<UserProfileDto>(new ArgumentException("ユーザー識別情報が不足しています。"));
            }

            var validationErrors = ValidateRequest(request);
            if (validationErrors.Count > 0)
            {
                return new Result<UserProfileDto>(new UserProfileValidationException("入力内容に誤りがあります。", validationErrors));
            }

            var profile = await userProfileRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
            if (profile is null)
            {
                var initial = await GetOrCreateProfileAsync(entraObjectId, email, cancellationToken);
                if (!initial.IsSuccessful)
                {
                    return new Result<UserProfileDto>(initial.Error!);
                }

                profile = await userProfileRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
                if (profile is null)
                {
                    return new Result<UserProfileDto>(new InvalidOperationException("プロフィールの初期化に失敗しました。"));
                }
            }

            profile.DisplayName = request.DisplayName.Trim();
            // AvatarImagePath は POST /api/users/me/profile/avatar で管理するため、ここでは更新しない
            profile.UpdatedAtUtc = DateTime.UtcNow;

            var updated = await userProfileRepository.UpdateAsync(profile, cancellationToken);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            return new Result<UserProfileDto>(ex);
        }
    }

    public async Task<Result<UserProfileDto>> UpdateThemePreferenceAsync(
        string entraObjectId,
        string email,
        UpdateThemePreferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entraObjectId) || string.IsNullOrWhiteSpace(email))
            {
                return new Result<UserProfileDto>(new ArgumentException("ユーザー識別情報が不足しています。"));
            }

            var themePreference = request.ThemePreference;
            if (themePreference is null || !Enum.IsDefined<ThemeMode>(themePreference.Value))
            {
                return new Result<UserProfileDto>(new UserProfileValidationException(
                    "入力内容に誤りがあります。",
                    new Dictionary<string, string>
                    {
                        ["themePreference"] = "テーマは light または dark を指定してください。",
                    }));
            }

            var profile = await userProfileRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
            if (profile is null)
            {
                var initial = await GetOrCreateProfileAsync(entraObjectId, email, cancellationToken);
                if (!initial.IsSuccessful)
                {
                    return new Result<UserProfileDto>(initial.Error!);
                }

                profile = await userProfileRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
                if (profile is null)
                {
                    return new Result<UserProfileDto>(new InvalidOperationException("プロフィールの初期化に失敗しました。"));
                }
            }

            profile.ThemePreference = themePreference.Value;
            profile.UpdatedAtUtc = DateTime.UtcNow;

            var updated = await userProfileRepository.UpdateAsync(profile, cancellationToken);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            return new Result<UserProfileDto>(ex);
        }
    }

    private static Dictionary<string, string> ValidateRequest(UpdateUserProfileRequest request)
    {
        var details = new Dictionary<string, string>();

        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        if (displayName.Length is < 1 or > 30)
        {
            details["displayName"] = "表示名は1〜30文字で入力してください。";
        }

        // AvatarImagePath は POST /api/users/me/profile/avatar で管理するため、ここでは検証しない

        return details;
    }

    private static UserProfileDto MapToDto(UserProfile profile) =>
        new(profile.EntraObjectId, profile.Email, profile.DisplayName, profile.AvatarImagePath, profile.ThemePreference);
}
