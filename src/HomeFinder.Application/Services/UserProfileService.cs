using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Repositories;
using HomeFinder.Core.Entities;
using HomeFinder.Core.Errors;

namespace HomeFinder.Application.Services;

public class UserProfileService(IUserProfileRepository userProfileRepository) : IUserProfileService
{
    public const string DefaultAvatarPath = "/images/user-avatar-default.svg";

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
            profile.AvatarImagePath = request.AvatarImagePath.Trim();
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

        var avatarImagePath = request.AvatarImagePath?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(avatarImagePath))
        {
            details["avatarImagePath"] = "アイコン画像は必須です。";
        }
        else if (avatarImagePath.Length > 512)
        {
            details["avatarImagePath"] = "アイコン画像パスが長すぎます。";
        }
        else
        {
            var extension = Path.GetExtension(avatarImagePath);
            if (!AllowedAvatarExtensions.Contains(extension))
            {
                details["avatarImagePath"] = "アイコン画像形式が不正です。";
            }
        }

        return details;
    }

    private static UserProfileDto MapToDto(UserProfile profile) =>
        new(profile.EntraObjectId, profile.Email, profile.DisplayName, profile.AvatarImagePath);
}
