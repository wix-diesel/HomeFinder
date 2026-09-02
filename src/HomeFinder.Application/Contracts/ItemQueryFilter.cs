using System;

namespace HomeFinder.Application.Contracts;

/// <summary>
/// 物品一覧の検索・絞り込み条件
///
/// UI の検索ボックス／カテゴリ選択／在庫ありのみチェックに対応し、
/// ページング前の全件に対してデータベース側で適用される。
/// </summary>
public record ItemQueryFilter(
    string? Keyword = null,
    Guid? CategoryId = null,
    bool UnclassifiedOnly = false,
    bool StockOnly = false);
