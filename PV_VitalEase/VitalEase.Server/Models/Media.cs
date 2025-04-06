namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a media item, such as an image or video, which can be associated with exercises.
    /// </summary>
    public class Media
    {
        /// <summary>
        /// Gets or sets the unique identifier for the media.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the media item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL where the media is located.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the type of the media (e.g., image, video).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="ExerciseMedia"/> associations linking this media to exercises.
        /// </summary>
        public ICollection<ExerciseMedia> ExerciseMedia { get; set; }

        /// <summary>
        /// Deletes the media item.
        /// </summary>
        /// <remarks>
        /// The implementation for deleting the media should be added here.
        /// </remarks>
        public void DeleteMedia()
        {
            // Implementation here
        }

        /// <summary>
        /// Retrieves basic information about the media item.
        /// </summary>
        /// <returns>
        /// A string containing the media's name and URL.
        /// </returns>
        public string GetMediaInfo()
        {
            return $"Name: {Name}, URL: {Url}";
        }
    }
}
