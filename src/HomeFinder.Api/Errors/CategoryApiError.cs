// バックエンド: カテゴリー API エラーレスポンス

using HomeFinder.Core.Errors;

namespace HomeFinder.Api.Errors
{
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
