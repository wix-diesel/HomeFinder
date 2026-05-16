using DotNext;
using HomeFinder.Application.Contracts;

namespace HomeFinder.Application.Services;

/// <summary>
/// バーコード起点で商品・カテゴリ情報を取得するアプリケーションサービス。
/// </summary>
public interface IItemLookupService
{
    Task<Result<ItemLookupResultDto>> LookupByBarcode(string jan, CancellationToken cancellationToken = default);
}
