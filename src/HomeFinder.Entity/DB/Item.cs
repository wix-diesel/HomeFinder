
namespace HomeFinder.Entity.DB
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        public string JANCode { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public Picture Picture { get; set; } = new Picture();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}