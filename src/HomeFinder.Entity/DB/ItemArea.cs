namespace HomeFinder.Entity.DB
{
    public class ItemArea
    {
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int AreaId { get; set; }
        public Area Area { get; set; }

        public int Stock { get; set; }
    }
}