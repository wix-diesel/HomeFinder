using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Api.src.Models;

public class Shelf
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public Room? Room { get; set; }
}
