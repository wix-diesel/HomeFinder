using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Core.Entities;

public class Room
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
}
