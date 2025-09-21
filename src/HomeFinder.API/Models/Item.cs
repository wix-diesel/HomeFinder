
using HomeFinder.Entity.DB;
using HomeFinderAPI.Models;

namespace HomeFinder.API.Models
{
    public class ItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;

        public string JANCode { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public PictureDTO? Picture { get; set; }

        public CategoryDTO? Category { get; set; }
    }
}
