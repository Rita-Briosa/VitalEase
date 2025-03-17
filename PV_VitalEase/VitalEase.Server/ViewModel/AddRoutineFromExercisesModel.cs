using System.ComponentModel.DataAnnotations;
using VitalEase.Server.Models;

namespace VitalEase.Server.ViewModel
{
    public class AddRoutineFromExercisesModel
    {
        [Required(ErrorMessage = "The Exercise Id is required")]
        public int ExerciseId { get; set; }

        [Required(ErrorMessage = "The Routine Id is required")]
        public int RoutineId { get; set; }
    }
}
