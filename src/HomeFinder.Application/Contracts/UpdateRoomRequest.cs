using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Application.Contracts;

public class UpdateRoomRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
}
