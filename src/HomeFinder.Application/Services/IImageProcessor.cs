namespace HomeFinder.Application.Services;

/// <summary>
/// 画像処理（検証・リサイズ・メタデータ取得）を抽象化するインターフェース（Application 層）
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// 画像の解像度（幅・高さ）を取得する
    /// </summary>
    /// <param name="imageStream">画像データストリーム</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>幅と高さのタプル</returns>
    Task<(int Width, int Height)> GetDimensionsAsync(Stream imageStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// 画像を指定の最大サイズにアスペクト比を保ちながらリサイズする
    /// </summary>
    /// <param name="imageStream">元の画像データストリーム</param>
    /// <param name="maxWidth">最大幅（ピクセル）</param>
    /// <param name="maxHeight">最大高さ（ピクセル）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>リサイズ済みの画像ストリーム</returns>
    Task<Stream> ResizeAsync(Stream imageStream, int maxWidth, int maxHeight, CancellationToken cancellationToken = default);
}
