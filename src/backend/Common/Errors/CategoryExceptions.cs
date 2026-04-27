// バックエンド: カテゴリー用例外

namespace HomeFinder.Api.Common.Errors
{
    /// <summary>
    /// カテゴリーが見つからない例外
    /// </summary>
    public class CategoryNotFoundException(Guid id) : Exception($"Category not found: {id}")
    {
        public Guid CategoryId { get; } = id;

        public override string Message => $"指定されたカテゴリーは存在しません。(ID: {CategoryId})";
    }

    /// <summary>
    /// カテゴリー名が重複している例外
    /// </summary>
    public class CategoryNameDuplicateException(string name) : Exception($"Category name duplicate: {name}")
    {
        public string Name { get; } = name;

        public override string Message => $"同一名称のカテゴリーが既に存在します。(名称: {Name})";
    }

    /// <summary>
    /// 予約カテゴリ保護例外
    /// </summary>
    public class ReservedCategoryProtectedException(Guid id) : Exception($"Reserved category protected: {id}")
    {
        public Guid CategoryId { get; } = id;

        public override string Message => $"予約カテゴリーは編集・削除できません。(ID: {CategoryId})";
    }

    /// <summary>
    /// カテゴリーバリデーションエラー例外
    /// </summary>
    public class CategoryValidationException(string message, Dictionary<string, string> details) : Exception(message)
    {
        public Dictionary<string, string> Details { get; } = details ?? new Dictionary<string, string>();

        public override string Message => $"カテゴリーバリデーションエラー: {base.Message}";
    }

    /// <summary>
    /// API エラーレスポンス
    /// </summary>
    public class CategoryApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string>? Details { get; set; }

        public static CategoryApiError FromException(Exception ex)
        {
            return ex switch
            {
                CategoryNotFoundException cnf => new CategoryApiError
                {
                    Code = "CATEGORY_NOT_FOUND",
                    Message = cnf.Message,
                },
                CategoryNameDuplicateException cnde => new CategoryApiError
                {
                    Code = "CATEGORY_NAME_DUPLICATE",
                    Message = cnde.Message,
                },
                ReservedCategoryProtectedException rcpe => new CategoryApiError
                {
                    Code = "RESERVED_CATEGORY_PROTECTED",
                    Message = rcpe.Message,
                },
                CategoryValidationException cve => new CategoryApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = cve.Message,
                    Details = cve.Details,
                },
                _ => new CategoryApiError
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = "内部エラーが発生しました。",
                }
            };
        }
    }
}
