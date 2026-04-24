namespace HomeFinder.Api.src.Common.Errors;

public class ItemNotFoundException(Guid id) : Exception($"Item not found: {id}")
{
    public Guid ItemId { get; } = id;
}

public class ItemNameConflictException(string name) : Exception($"Item name conflict: {name}")
{
    public string Name { get; } = name;
}
