using System.ComponentModel.DataAnnotations;
using VitalEase.Server.Models;

namespace VitalEase.Server.ViewModel
{
    public class AddRoutineFromExercisesViewModel
    {
        [Required(ErrorMessage = "The Exercise Id is required")]
        public int ExerciseId { get; set; }

        [Required(ErrorMessage = "The Routine Id is required")]
        public int RoutineId { get; set; }

        public int? reps { get; set; }

        public int? duration { get; set; }

        public int? sets { get; set; }
    }
}
