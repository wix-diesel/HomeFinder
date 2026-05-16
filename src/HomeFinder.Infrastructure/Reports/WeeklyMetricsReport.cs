namespace HomeFinder.Infrastructure.Reports;

/// <summary>
/// 週次メトリクス集計レポート
/// SC-001/SC-002/SC-003 のシナリオに対応したメトリクスを集計
/// </summary>
public class WeeklyMetricsReport
{
    /// <summary>
    /// レポート生成日
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 集計対象期間（開始）
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// 集計対象期間（終了）
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// SC-001: 自動カテゴリ登録の成功件数
    /// </summary>
    public long CategoryAutofillSuccessCount { get; set; }

    /// <summary>
    /// SC-001: 自動カテゴリ登録の失敗件数
    /// </summary>
    public long CategoryAutofillFailureCount { get; set; }

    /// <summary>
    /// SC-001: 新規登録されたカテゴリ数
    /// </summary>
    public long NewCategoryCount { get; set; }

    /// <summary>
    /// SC-001: 既存カテゴリの再利用数
    /// </summary>
    public long ExistingCategoryReuseCount { get; set; }

    /// <summary>
    /// SC-002: 手動カテゴリ作成の件数
    /// </summary>
    public long ManualCategoryCreateCount { get; set; }

    /// <summary>
    /// SC-002: カテゴリ同時登録による競合件数
    /// </summary>
    public long CategoryConflictCount { get; set; }

    /// <summary>
    /// SC-003: 商品検索の平均レイテンシ（ミリ秒）
    /// </summary>
    public double AvgLookupLatencyMs { get; set; }

    /// <summary>
    /// SC-003: 商品検索の最大レイテンシ（ミリ秒）
    /// </summary>
    public long MaxLookupLatencyMs { get; set; }

    /// <summary>
    /// SC-003: 商品検索の最小レイテンシ（ミリ秒）
    /// </summary>
    public long MinLookupLatencyMs { get; set; }

    /// <summary>
    /// SC-003: 外部 API のレート制限到達回数
    /// </summary>
    public long ExternalApiRateLimitCount { get; set; }

    /// <summary>
    /// SC-003: 外部 API のタイムアウト回数
    /// </summary>
    public long ExternalApiTimeoutCount { get; set; }

    /// <summary>
    /// 成功率（%）
    /// </summary>
    public decimal SuccessRatePercent
    {
        get
        {
            var total = CategoryAutofillSuccessCount + CategoryAutofillFailureCount;
            return total == 0 ? 0 : (CategoryAutofillSuccessCount * 100m / total);
        }
    }

    /// <summary>
    /// レポートをテキスト形式で出力
    /// </summary>
    public override string ToString()
    {
        return $"""
                === 週次メトリクスレポート ===
                生成日時: {GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}
                集計期間: {PeriodStart:yyyy-MM-dd} ～ {PeriodEnd:yyyy-MM-dd}
                
                【SC-001: 自動カテゴリ登録】
                - 成功件数: {CategoryAutofillSuccessCount}
                - 失敗件数: {CategoryAutofillFailureCount}
                - 成功率: {SuccessRatePercent:F2}%
                - 新規カテゴリ: {NewCategoryCount}
                - 再利用: {ExistingCategoryReuseCount}
                
                【SC-002: 管理機能】
                - 手動カテゴリ作成: {ManualCategoryCreateCount}
                - 同時登録競合: {CategoryConflictCount}
                
                【SC-003: パフォーマンス】
                - 平均レイテンシ: {AvgLookupLatencyMs:F2}ms
                - 最大レイテンシ: {MaxLookupLatencyMs}ms
                - 最小レイテンシ: {MinLookupLatencyMs}ms
                - 外部API レート制限: {ExternalApiRateLimitCount}
                - 外部API タイムアウト: {ExternalApiTimeoutCount}
                """;
    }
}

/// <summary>
/// メトリクス集計ジョブのインターフェース
/// </summary>
public interface IMetricsAggregationService
{
    /// <summary>
    /// 指定期間のメトリクスを集計し、レポートを生成
    /// </summary>
    /// <param name="periodStart">集計開始日</param>
    /// <param name="periodEnd">集計終了日</param>
    /// <returns>集計レポート</returns>
    Task<WeeklyMetricsReport> AggregateWeeklyMetricsAsync(DateTime periodStart, DateTime periodEnd);

    /// <summary>
    /// 先週のレポートを生成
    /// </summary>
    Task<WeeklyMetricsReport> AggregateLastWeekAsync();
}
