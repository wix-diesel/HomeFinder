namespace HomeFinder.Core.Errors;

public class UserProfileValidationException(string message, Dictionary<string, string> details) : Exception(message)
{
    public Dictionary<string, string> Details { get; } = details;
}

public class UserProfileForbiddenException() : Exception("プロフィール更新権限がありません。")
{
}

/// <summary>
/// 指定されたユーザープロファイルが見つからない場合にスローする。
/// </summary>
public class UserProfileNotFoundException(Guid userId) : Exception($"User profile not found: {userId}")
{
    public Guid UserId { get; } = userId;
}

/// <summary>
/// 指定されたユーザープロファイルが見つからない場合にスローする。
/// </summary>
public class EntraIdNotFoundException(string entraId) : Exception($"User profile not found: {entraId}")
{
    public string EntraId { get; } = entraId;
}