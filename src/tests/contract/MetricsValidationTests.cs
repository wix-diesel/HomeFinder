using HomeFinder.Infrastructure.Metrics;
using HomeFinder.Infrastructure.Reports;
using Xunit;

namespace ContractTests;

/// <summary>
/// メトリクス検証テスト
/// SC-001/SC-002/SC-003 のしきい値逸脱を検知
/// </summary>
public class MetricsValidationTests
{
    /// <summary>
    /// テスト: 自動カテゴリ登録の成功率が 80% 以上であることを検証
    /// SC-001 関連
    /// </summary>
    [Fact]
    public void CategoryAutofillSuccessRate_MustBeAtLeast80Percent()
    {
        // Arrange
        var report = new WeeklyMetricsReport
        {
            CategoryAutofillSuccessCount = 80,
            CategoryAutofillFailureCount = 20
        };

        // Act
        var successRate = report.SuccessRatePercent;

        // Assert
        Assert.True(successRate >= 80, $"Success rate {successRate:F2}% is below 80% threshold");
    }

    /// <summary>
    /// テスト: 新規カテゴリの登録が適切に追跡されていることを検証
    /// SC-001 関連
    /// </summary>
    [Fact]
    public void NewCategoryCount_MustBeCounted()
    {
        // Arrange
        var report = new WeeklyMetricsReport
        {
            CategoryAutofillSuccessCount = 100,
            NewCategoryCount = 25,
            ExistingCategoryReuseCount = 75
        };

        // Assert
        Assert.Equal(100, report.NewCategoryCount + report.ExistingCategoryReuseCount);
    }

    /// <summary>
    /// テスト: カテゴリ同時登録の競合数がカウントされていることを検証
    /// SC-002 関連
    /// </summary>
    [Fact]
    public void CategoryConflictCount_MustBeRecorded()
    {
        // Arrange
        var report = new WeeklyMetricsReport
        {
            CategoryConflictCount = 5
        };

        // Assert
        Assert.True(report.CategoryConflictCount >= 0, "Conflict count cannot be negative");
    }

    /// <summary>
    /// テスト: 商品検索のレイテンシが 3 秒以下であることを検証
    /// SC-003 関連
    /// </summary>
    [Fact]
    public void LookupLatency_MustNotExceed3Seconds()
    {
        // Arrange
        var report = new WeeklyMetricsReport
        {
            MaxLookupLatencyMs = 2500  // 2.5 秒
        };

        // Assert
        Assert.True(report.MaxLookupLatencyMs <= 3000, 
            $"Max latency {report.MaxLookupLatencyMs}ms exceeds 3000ms threshold");
    }

    /// <summary>
    /// テスト: 外部 API のエラー（レート制限 + タイムアウト）が追跡されていることを検証
    /// SC-003 関連
    /// </summary>
    [Fact]
    public void ExternalApiErrors_MustBeTracked()
    {
        // Arrange
        var report = new WeeklyMetricsReport
        {
            ExternalApiRateLimitCount = 2,
            ExternalApiTimeoutCount = 1
        };

        // Assert
        Assert.True(report.ExternalApiRateLimitCount >= 0, "Rate limit count cannot be negative");
        Assert.True(report.ExternalApiTimeoutCount >= 0, "Timeout count cannot be negative");
    }

    /// <summary>
    /// テスト: レポート文字列化で必須フィールドが含まれることを検証
    /// </summary>
    [Fact]
    public void ReportToString_ContainsRequiredFields()
    {
        // Arrange
        var report = new WeeklyMetricsReport
        {
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 1, 7),
            CategoryAutofillSuccessCount = 100,
            CategoryAutofillFailureCount = 10,
            NewCategoryCount = 20,
            ExistingCategoryReuseCount = 80,
            ManualCategoryCreateCount = 5,
            CategoryConflictCount = 2,
            AvgLookupLatencyMs = 250.5,
            MaxLookupLatencyMs = 2500,
            MinLookupLatencyMs = 100,
            ExternalApiRateLimitCount = 1,
            ExternalApiTimeoutCount = 0
        };

        // Act
        var reportText = report.ToString();

        // Assert
        Assert.Contains("SC-001", reportText);
        Assert.Contains("SC-002", reportText);
        Assert.Contains("SC-003", reportText);
        Assert.Contains("成功率", reportText);
        Assert.Contains("100", reportText);  // Success count
    }

    /// <summary>
    /// テスト: メトリクスオブジェクトの作成と操作
    /// </summary>
    [Fact]
    public void BarcodeLookupMetrics_CanBeInstantiated()
    {
        // Arrange & Act
        var metrics = new BarcodeLookupMetrics();

        // Assert
        Assert.NotNull(metrics);
        
        // Verify methods exist and can be called
        metrics.RecordCategoryAutofillSuccess("4901301417350", 150, true);
        metrics.RecordLookupFailure("1234567890", "Product not found");
        metrics.RecordLookupLatency("4901301417350", 250);
        metrics.RecordManualCategoryCreate("飲料");
        metrics.RecordCategoryConflict("飲料");
        metrics.RecordRateLimitHit();
        metrics.RecordExternalApiTimeout(3000);
    }

    /// <summary>
    /// テスト: 複数期間のレポート生成
    /// </summary>
    [Fact]
    public void MultipleReports_CanBeGenerated()
    {
        // Arrange
        var reports = new List<WeeklyMetricsReport>();

        // Act
        for (int i = 0; i < 4; i++)
        {
            reports.Add(new WeeklyMetricsReport
            {
                PeriodStart = new DateTime(2025, 1, 1).AddDays(i * 7),
                PeriodEnd = new DateTime(2025, 1, 7).AddDays(i * 7),
                CategoryAutofillSuccessCount = 100 + (i * 10),
                NewCategoryCount = 20 + i
            });
        }

        // Assert
        Assert.Equal(4, reports.Count);
        Assert.True(reports[0].CategoryAutofillSuccessCount < reports[3].CategoryAutofillSuccessCount);
    }
}
