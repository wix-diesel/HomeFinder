namespace HomeFinder.Api.Errors;

public record ApiError(string Code, string Message, IReadOnlyCollection<ApiErrorDetail>? Details = null)
{
    public static ApiError ValidationError(IReadOnlyCollection<ApiErrorDetail> details) =>
        new("VALIDATION_ERROR", "入力内容に誤りがあります。", details);

    public static ApiError ItemNotFound() =>
        new("ITEM_NOT_FOUND", "指定された物品は存在しません。");

    public static ApiError ItemNameConflict() =>
        new("ITEM_NAME_CONFLICT", "同じ名称の物品がすでに登録されています。");

    public static ApiError ItemDeleteNotFound() =>
        new("ITEM_NOT_FOUND", "削除対象の物品が見つかりません。");

    public static ApiError ItemDeleteConflict() =>
        new("ITEM_DELETE_CONFLICT", "物品の削除に競合が発生しました。");

    public static ApiError Forbidden() =>
        new("FORBIDDEN", "この操作を行う権限がありません。");
}

public record ApiErrorDetail(string Field, string Reason);
