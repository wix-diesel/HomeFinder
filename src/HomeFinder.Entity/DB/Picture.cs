using System;

namespace HomeFinder.Entity.DB
{
    /// <summary>
    /// Represents a picture associated with an item.
    /// </summary>
    public class Picture
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

        /// <summary>
        /// Date and time when the picture was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; }
    }
}