namespace VitalEase.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::VitalEase.Server.Data;
    using global::VitalEase.Server.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.IdentityModel.Tokens;
    using NuGet.Common;
    using VitalEase.Server.ViewModel;


        public class ExercisesController : Controller
        {
            private readonly VitalEaseServerContext _context;

            public ExercisesController(VitalEaseServerContext context)
            {
                _context = context;
        }

            [HttpGet("api/getExercises")]
            public async Task<IActionResult> GetExercises()
            {

                try
                {
                    // Fetch all exercises from the database
                    var exercises = await _context.Exercises.Where(e => e.Duration == 0 && e.Reps == 0).ToListAsync();


                    if (exercises.IsNullOrEmpty())
                    {
                        return BadRequest(new List<Exercise>());
                    }

                    var exerciseDtos = exercises.Select(e => new 
                     {
                     Id = e.Id,
                     Name = e.Name,
                     Description = e.Description,
                     Type = e.Type,
                     DifficultyLevel = e.DifficultyLevel.ToString(),
                     MuscleGroup = e.MuscleGroup,
                     EquipmentNecessary = e.EquipmentNecessary,
                     Reps = e.Reps,
                     Duration = e.Duration,
                     // Converter Enum para String
                     }).ToList();

                // Return the exercises in JSON format
                return Ok(exerciseDtos);
                }
                catch (Exception ex)
                {
                    // Handle error and return a bad request response
                    return BadRequest(new { message = "Error fetching exercises", error = ex.Message });
                }
            }

        [HttpGet("api/getMedia/{exerciseId}")]
        public async Task<IActionResult> GetMedia(int exerciseId)
        {

            try
            {
                // Fetch all media from the database
                var media = await _context.Media.Where( m => m.ExerciseId == exerciseId).ToListAsync();


                if (media.IsNullOrEmpty())
                {
                    return BadRequest(new List<Media>());
                }

                // Return the logs in JSON format
                return Ok(media);
            }
            catch (Exception ex)
            {
                // Handle error and return a bad request response
                return BadRequest(new { message = "Error fetching media", error = ex.Message });
            }
        }

        [HttpGet("api/getFilteredExercises")]
        public async Task<IActionResult> GetFilteredExercises(string? type, string? difficultyLevel, string? muscleGroup, string? equipmentNeeded)
        {
            try
            {
                var query = _context.Exercises.Where(e => e.Duration == 0 && e.Reps == 0).AsQueryable();

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(e => e.Type == type);
                }

                if (!string.IsNullOrEmpty(difficultyLevel))
                {
                    query = query.Where(e => e.DifficultyLevel.ToString().Equals(difficultyLevel));
                }

                if (!string.IsNullOrEmpty(muscleGroup))
                {
                    query = query.Where(e => e.MuscleGroup.Contains(muscleGroup));
                }

                if (!string.IsNullOrEmpty(equipmentNeeded))
                {
                    query = query.Where(e => e.EquipmentNecessary.Contains(equipmentNeeded));
                }

                // Fetch all exercises
                var exercises = await query.Select(e => new
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Type = e.Type,
                    DifficultyLevel = e.DifficultyLevel.ToString(),
                    MuscleGroup = e.MuscleGroup,
                    EquipmentNecessary = e.EquipmentNecessary,
                    Reps = e.Reps,
                    Duration = e.Duration,
                    // Converter Enum para String
                } ).ToListAsync();

                return Ok(exercises);
            }
            catch (Exception ex)
            {
                //Handle error and return a bad request response
                return BadRequest(new { message = "Error filtering exercises", error = ex.Message });
            }
        }

        [HttpGet("api/getRoutinesOnExercises/{userId}")]
        public async Task<IActionResult> GetRoutinesOnExercises(int userId)
        {

            try
            {
                // Fetch all media from the database
                var routines = await _context.Routines.Where(r => r.UserId == userId).ToListAsync();


                if (routines.IsNullOrEmpty())
                {
                    return BadRequest(new List<Routine>());
                }

                // Return the logs in JSON format
                return Ok(routines);
            }
            catch (Exception ex)
            {
                // Handle error and return a bad request response
                return BadRequest(new { message = "Error fetching routines", error = ex.Message });
            }
        }

        [HttpPost("api/addRoutine")]
        public async Task<IActionResult> AddRoutine([FromBody] AddRoutineFromExercisesModel model)
        {
            try
            {
             
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

               
                var existingExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == model.ExerciseId);
                var routine = await _context.Routines.FirstOrDefaultAsync(r => r.Id == model.RoutineId);

                if (existingExercise == null)
                    return NotFound(new { message = "Exercise not found" });

                if (routine == null)
                    return NotFound(new { message = "Routine not found" });

                
                var newExercise = new Exercise
                {
                    Name = existingExercise.Name,
                    Description = existingExercise.Description,
                    Type = existingExercise.Type,
                    DifficultyLevel = existingExercise.DifficultyLevel,
                    MuscleGroup = existingExercise.MuscleGroup,
                    EquipmentNecessary = existingExercise.EquipmentNecessary,
                    Reps = 1, 
                    Duration = 1 
                };

                
                _context.Exercises.Add(newExercise);
                await _context.SaveChangesAsync(); 

              
                var exerciseRoutine = new ExerciseRoutine
                {
                    ExerciseId = newExercise.Id, 
                    RoutineId = routine.Id
                };

                
                _context.ExerciseRoutines.Add(exerciseRoutine);
                await _context.SaveChangesAsync(); 

                return Ok(new { message = "Exercise added to routine successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding exercise to routine", error = ex.Message });
            }
        }
    }
}
