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

    public static ApiError ItemUpdateNotFound() =>
        new("ITEM_NOT_FOUND", "更新対象の物品が見つかりません。");

    public static ApiError ItemDeleteConflict() =>
        new("ITEM_DELETE_CONFLICT", "物品の削除に競合が発生しました。");

    public static ApiError Forbidden() =>
        new("FORBIDDEN", "この操作を行う権限がありません。");

    public static ApiError ImageNotFound() =>
        new("IMAGE_NOT_FOUND", "指定されたアイテムの画像が見つかりません。");

    public static ApiError ImageNotFoundForItem(Guid itemId) =>
        new("IMAGE_NOT_FOUND", $"アイテム（ID: {itemId}）に紐付く画像は存在しません。");

    public static ApiError ImageBlobStorageUnavailable() =>
        new("BLOB_STORAGE_UNAVAILABLE", "画像ストレージに一時的に接続できませんでした。しばらく時間を置いてから再度お試しください。");

    public static ApiError ImageDeletePartialSuccess() =>
        new("IMAGE_DELETE_PARTIAL", "データ上の画像は削除されましたが、ストレージからの物理削除に失敗しました。システム管理者に連絡してください。");

    public static ApiError ImageValidationError(string code, string message) =>
        new(code, message);
}

public record ApiErrorDetail(string Field, string Reason);
