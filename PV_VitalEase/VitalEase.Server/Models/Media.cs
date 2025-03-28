﻿namespace VitalEase.Server.Models
{
    public class Media
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }

        public ICollection<ExerciseMedia> ExerciseMedia { get; set; }

        public void DeleteMedia()
        {
            // Implementation here
        }

        public string GetMediaInfo()
        {
            return $"Name: {Name}, URL: {Url}";
        }
    }
}
