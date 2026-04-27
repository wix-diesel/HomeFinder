using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Api.Contracts;

public class CreateCategoryRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Icon { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string Color { get; set; } = string.Empty;
}
