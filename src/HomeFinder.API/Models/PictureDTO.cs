namespace HomeFinderAPI.Models
{
    public class PictureDTO
    {
        /// <summary>
        /// Unique identifier for the picture.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// URL or path to the picture file.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the picture.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
