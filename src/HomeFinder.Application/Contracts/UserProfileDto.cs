namespace HomeFinder.Application.Contracts;

public record UserProfileDto(
    string EntraObjectId,
    string Email,
    string DisplayName,
    string AvatarImagePath);
