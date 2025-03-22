namespace VitalEase.Server.ViewModel
{
    using System.ComponentModel.DataAnnotations;
    using VitalEase.Server.Models;
    public class AddExercisesViewModel
    {

        [Required(ErrorMessage = "The new Name is required")]
        public string newName { get; set; }

        [Required(ErrorMessage = "The new Description is required")]
        public string newDescription { get; set; }

        [Required(ErrorMessage = "The new Type is required")]
        public string newType{ get; set; }

        [Required(ErrorMessage = "The new Difficulty Level is required")]
        public string newDifficultyLevel { get; set; }

        [Required(ErrorMessage = "The new Muscle Group is required")]
        public string newMuscleGroup { get; set; }

        [Required(ErrorMessage = "The new Equipment Necessary is required")]
        public string newEquipmentNecessary { get; set; }

        [Required(ErrorMessage = "The new Media Name is required")]
        public string newMediaName { get; set; }

        [Required(ErrorMessage = "The new Media Type is required")]
        public string newMediaType { get; set; }

        [Required(ErrorMessage = "The new Media Url is required")]
        public string newMediaUrl { get; set; }

        [Required(ErrorMessage = "The new Media Name 1 is required")]
        public string newMediaName1 { get; set; }

        [Required(ErrorMessage = "The new Media Type 1 is required")]
        public string newMediaType1 { get; set; }

        [Required(ErrorMessage = "The new Media Url 1 is required")]
        public string newMediaUrl1 { get; set; }

        public string? newMediaName2 { get; set; }

        public string? newMediaType2 { get; set; }

        public string? newMediaUrl2 { get; set; }

    }
}
