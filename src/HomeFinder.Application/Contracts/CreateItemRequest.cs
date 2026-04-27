using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Application.Contracts;

public class CreateItemRequest
{
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
