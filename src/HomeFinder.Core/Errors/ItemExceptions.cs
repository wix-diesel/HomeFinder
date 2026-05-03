namespace HomeFinder.Core.Errors;

public class ItemNotFoundException(Guid id) : Exception($"Item not found: {id}")
{
    public Guid ItemId { get; } = id;
}

public class ItemNameConflictException(string name) : Exception($"Item name conflict: {name}")
{
    public string Name { get; } = name;
}

/// <summary>
/// 削除対象アイテムの操作が競合した場合（楽観的同時実行制御違反など）にスローする。
/// </summary>
public class ItemDeleteConflictException(Guid id) : Exception($"Item delete conflict: {id}")
{
    public Guid ItemId { get; } = id;
}

/// <summary>
/// 削除操作に必要な権限がない場合にスローする（将来の認証・認可実装で利用）。
/// </summary>
public class ItemDeleteForbiddenException(Guid id) : Exception($"Item delete forbidden: {id}")
{
    public Guid ItemId { get; } = id;
}
