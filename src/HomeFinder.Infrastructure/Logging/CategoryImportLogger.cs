using HomeFinder.Core.Entities;
using Microsoft.Extensions.Logging;

namespace HomeFinder.Infrastructure.Logging;

/// <summary>
/// 自動カテゴリ登録イベントの監査ログを記録するクラス。
/// バーコード検索による自動登録プロセスを追跡可能にする。
/// </summary>
public class CategoryImportLogger
{
    private readonly ILogger<CategoryImportLogger> _logger;

    public CategoryImportLogger(ILogger<CategoryImportLogger> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 新しいカテゴリが自動登録されたことをログに記録する。
    /// </summary>
    /// <param name="category">登録されたカテゴリ</param>
    /// <param name="source">登録元（"rakuten" など）</param>
    public void LogCategoryCreated(Category category, string source = "rakuten")
    {
        _logger.LogInformation(
            "カテゴリが自動登録されました。CategoryId={CategoryId}, Name={CategoryName}, " +
            "NormalizedName={NormalizedName}, Source={Source}, ExternalId={ExternalId}, CreatedBy={CreatedBy}",
            category.Id, category.Name, category.NormalizedName, source, category.ExternalId, category.CreatedBy);
    }

    /// <summary>
    /// 既存のカテゴリが再利用されたことをログに記録する。
    /// </summary>
    /// <param name="category">再利用されたカテゴリ</param>
    /// <param name="queryKey">検索キー（通常は正規化されたカテゴリ名）</param>
    public void LogCategoryReused(Category category, string queryKey)
    {
        _logger.LogInformation(
            "既存カテゴリが再利用されました。CategoryId={CategoryId}, Name={CategoryName}, QueryKey={QueryKey}",
            category.Id, category.Name, queryKey);
    }

    /// <summary>
    /// カテゴリ自動登録時の競合（UNIQUE 制約違反）をログに記録する。
    /// </summary>
    /// <param name="categoryName">登録しようとしたカテゴリ名</param>
    /// <param name="conflictingCategoryId">競合するカテゴリの ID</param>
    /// <param name="retriedSuccessfully">リトライが成功したか</param>
    public void LogConflictAndRetry(string categoryName, Guid? conflictingCategoryId, bool retriedSuccessfully)
    {
        if (retriedSuccessfully)
        {
            _logger.LogWarning(
                "カテゴリ登録時に UNIQUE 制約違反が発生しましたが、リトライで既存を取得しました。" +
                "CategoryName={CategoryName}, ConflictingCategoryId={ConflictingCategoryId}",
                categoryName, conflictingCategoryId);
        }
        else
        {
            _logger.LogError(
                "カテゴリ登録時に UNIQUE 制約違反が発生し、リトライでも既存を取得できませんでした。" +
                "CategoryName={CategoryName}",
                categoryName);
        }
    }

    /// <summary>
    /// カテゴリ自動登録プロセスの完了をログに記録する。
    /// </summary>
    /// <param name="barcode">処理対象のバーコード（JAN）</param>
    /// <param name="resultCategoryId">結果として確定したカテゴリ ID</param>
    /// <param name="isNewCategory">新規登録されたカテゴリか</param>
    public void LogAutofillCompletion(string barcode, Guid resultCategoryId, bool isNewCategory)
    {
        _logger.LogInformation(
            "バーコード検索によるカテゴリ自動登録処理が完了しました。" +
            "Barcode={Barcode}, ResultCategoryId={ResultCategoryId}, IsNewCategory={IsNewCategory}",
            barcode, resultCategoryId, isNewCategory);
    }

    /// <summary>
    /// カテゴリ自動登録プロセスのエラーをログに記録する。
    /// </summary>
    /// <param name="barcode">処理対象のバーコード（JAN）</param>
    /// <param name="exception">発生した例外</param>
    /// <param name="message">追加メッセージ</param>
    public void LogAutofillError(string barcode, Exception exception, string message = "")
    {
        _logger.LogError(
            exception,
            "バーコード検索によるカテゴリ自動登録処理でエラーが発生しました。" +
            "Barcode={Barcode}, Message={Message}",
            barcode, message);
    }
}
