using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Api.src.Contracts;

public class UpdateRoomRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
}
