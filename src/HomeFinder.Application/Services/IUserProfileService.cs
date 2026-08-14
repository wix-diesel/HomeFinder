using DotNext;
using HomeFinder.Application.Contracts;

namespace HomeFinder.Application.Services;

public interface IUserProfileService
{
    Task<Result<UserProfileDto>> GetOrCreateProfileAsync(
        string entraObjectId,
        string email,
        CancellationToken cancellationToken = default);

    Task<Result<UserProfileDto>> UpdateProfileAsync(
        string entraObjectId,
        string email,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<UserProfileDto>> UpdateThemePreferenceAsync(
        string entraObjectId,
        string email,
        UpdateThemePreferenceRequest request,
        CancellationToken cancellationToken = default);
}
