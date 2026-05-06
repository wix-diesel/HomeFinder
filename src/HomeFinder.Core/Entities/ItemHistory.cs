namespace HomeFinder.Core.Entities;

public class ItemHistory
{
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public ItemHistoryChangeType ChangeType { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public Item? Item { get; set; }
}
