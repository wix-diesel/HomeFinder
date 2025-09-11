using System;
using System.ComponentModel.DataAnnotations;

namespace HomeFinder.API.Models
{
    public class CategoryDTO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }
}