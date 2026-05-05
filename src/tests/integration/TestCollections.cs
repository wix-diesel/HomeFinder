namespace IntegrationTests;

/// <summary>
/// 画像系テストは static なブロブストレージ状態を共有するため、
/// 並列実行を防ぐために同一コレクションに集約する。
/// </summary>
[CollectionDefinition("ImageTests")]
public class ImageTestsCollection { }
