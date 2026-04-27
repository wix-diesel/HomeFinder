namespace HomeFinder.Api.Errors;

public record ApiError(string Code, string Message, IReadOnlyCollection<ApiErrorDetail>? Details = null)
{
    public static ApiError ValidationError(IReadOnlyCollection<ApiErrorDetail> details) =>
        new("VALIDATION_ERROR", "入力内容に誤りがあります。", details);

    public static ApiError ItemNotFound() =>
        new("ITEM_NOT_FOUND", "指定された物品は存在しません。");

    public static ApiError ItemNameConflict() =>
        new("ITEM_NAME_CONFLICT", "同じ名称の物品がすでに登録されています。");
}

public record ApiErrorDetail(string Field, string Reason);
