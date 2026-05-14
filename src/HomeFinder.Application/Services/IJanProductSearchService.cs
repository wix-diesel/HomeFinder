using DotNext;
using HomeFinder.Application.Contracts;

namespace HomeFinder.Application.Services;

/// <summary>
/// JAN コードによる商品検索サービス。
/// </summary>
public interface IJanProductSearchService
{
    Task<Result<JanProductDto>> SearchByJanAsync(string jan, CancellationToken cancellationToken = default);
}
