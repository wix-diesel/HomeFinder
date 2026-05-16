using System.Diagnostics;

namespace HomeFinder.Infrastructure.Metrics;

/// <summary>
/// バーコード検索とカテゴリ管理のメトリクスを計測するクラス。
/// OpenTelemetry との連携を想定した拡張性のある設計。
/// </summary>
public class BarcodeLookupMetrics
{
    private readonly ActivitySource _activitySource;

    public BarcodeLookupMetrics()
    {
        _activitySource = new ActivitySource("HomeFinder.Barcode");
    }

    /// <summary>
    /// 自動カテゴリ登録成功のメトリクスを記録する。
    /// </summary>
    /// <param name="barcode">検索バーコード</param>
    /// <param name="durationMs">処理時間（ミリ秒）</param>
    /// <param name="isNewCategory">新規カテゴリ登録したか</param>
    public void RecordCategoryAutofillSuccess(string barcode, long durationMs, bool isNewCategory)
    {
        using var activity = _activitySource.StartActivity("category_autofill.success");
        activity?.SetTag("category_autofill.barcode", barcode);
        activity?.SetTag("category_autofill.duration_ms", durationMs);
        activity?.SetTag("category_autofill.is_new", isNewCategory);
        
        // OpenTelemetry Meter が設定されている場合の拡張ポイント
        // meter.CreateCounter<long>("category_autofill.success").Add(1);
    }

    /// <summary>
    /// 商品検索失敗のメトリクスを記録する。
    /// </summary>
    /// <param name="barcode">検索バーコード</param>
    /// <param name="reason">失敗理由</param>
    public void RecordLookupFailure(string barcode, string reason)
    {
        using var activity = _activitySource.StartActivity("lookup.failure");
        activity?.SetTag("lookup.barcode", barcode);
        activity?.SetTag("lookup.failure_reason", reason);
        
        // OpenTelemetry Meter が設定されている場合の拡張ポイント
        // meter.CreateCounter<long>("lookup.failure").Add(1, new("reason", reason));
    }

    /// <summary>
    /// 商品検索のレイテンシを記録する。
    /// </summary>
    /// <param name="barcode">検索バーコード</param>
    /// <param name="durationMs">処理時間（ミリ秒）</param>
    public void RecordLookupLatency(string barcode, long durationMs)
    {
        using var activity = _activitySource.StartActivity("lookup.latency");
        activity?.SetTag("lookup.barcode", barcode);
        activity?.SetTag("lookup.latency_ms", durationMs);
        
        // OpenTelemetry Meter が設定されている場合の拡張ポイント
        // meter.CreateHistogram<long>("lookup.latency_ms").Record(durationMs);
    }

    /// <summary>
    /// 手動カテゴリ作成のカウントを記録する。
    /// </summary>
    /// <param name="categoryName">作成されたカテゴリ名</param>
    public void RecordManualCategoryCreate(string categoryName)
    {
        using var activity = _activitySource.StartActivity("category_manual_create");
        activity?.SetTag("category_manual_create.name", categoryName);
        
        // OpenTelemetry Meter が設定されている場合の拡張ポイント
        // meter.CreateCounter<long>("category_manual_create.count").Add(1);
    }

    /// <summary>
    /// 同時登録競合（UNIQUE 制約違反）のメトリクスを記録する。
    /// </summary>
    /// <param name="categoryName">競合したカテゴリ名</param>
    public void RecordCategoryConflict(string categoryName)
    {
        using var activity = _activitySource.StartActivity("category.conflict");
        activity?.SetTag("category.conflict.name", categoryName);
        
        // OpenTelemetry Meter が設定されている場合の拡張ポイント
        // meter.CreateCounter<long>("category.conflict").Add(1);
    }

    /// <summary>
    /// 外部 API のレート制限に達したのを記録する。
    /// </summary>
    public void RecordRateLimitHit()
    {
        using var activity = _activitySource.StartActivity("external_api.rate_limit");
        
        // OpenTelemetry Meter が設定されている場合の拡張ポイント
        // meter.CreateCounter<long>("external_api.rate_limit").Add(1);
    }

    /// <summary>
    /// 外部 API のタイムアウトを記録する。
    /// </summary>
    /// <param name="durationMs">タイムアウトまでの経過時間</param>
    public void RecordExternalApiTimeout(long durationMs)
    {
        using var activity = _activitySource.StartActivity("external_api.timeout");
        activity?.SetTag("external_api.timeout_ms", durationMs);
        
        // OpenTelemetry Meter が設定されている場合の拡張ポイント
        // meter.CreateCounter<long>("external_api.timeout").Add(1);
    }
}
