using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class AddRoutineViewModel
    {
        [Required(ErrorMessage = "The new Name is required")]
        public string newName { get; set; }

        [Required(ErrorMessage = "The new Description is required")]
        public string newDescription { get; set; }

        [Required(ErrorMessage = "The new Type is required")]
        public string newType { get; set; }

        [Required(ErrorMessage = "The new Routine Level is required")]
        public string newRoutineLevel { get; set; }

        [Required(ErrorMessage = "The new needs is required")]
        public string newNeeds { get; set; }

        [Required(ErrorMessage = "The exercises is required")]
        public List<int> Exercises { get; set; } // IDs dos exercícios selecionados

    }
}
