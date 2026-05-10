namespace HomeFinder.Core.Errors;

public class UserProfileValidationException(string message, Dictionary<string, string> details) : Exception(message)
{
    public Dictionary<string, string> Details { get; } = details;
}

public class UserProfileForbiddenException() : Exception("プロフィール更新権限がありません。")
{
}
