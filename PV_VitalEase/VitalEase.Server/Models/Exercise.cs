namespace VitalEase.Server.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public string DifficultyLevel { get; set; }

        public string MuscleGroup { get; set; }

        public string EquipmentNecessary { get; set; }

        public int Reps { get; set; }
        public int Duration { get; set; } // in seconds
        public List<Media> Media { get; set; } = new();

        public List<Routine> Routines { get; set; } = new();

        public void AddMedia(Media media)
        {
            Media.Add(media);
        }

        public void RemoveMedia(Media media)
        {
            Media.Remove(media);
        }

        public string GetExerciseInfo()
        {
            return $"Name: {Name}, Type: {Type}, Reps: {Reps}, Duration: {Duration}";
        }
    }

}
