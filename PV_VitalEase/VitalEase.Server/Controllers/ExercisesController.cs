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
    using static System.Runtime.InteropServices.JavaScript.JSType;


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
                    var exercises = await _context.Exercises.ToListAsync();


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
                // Buscar os IDs da mídia associada ao exercício na tabela ExerciseMedia
                var mediaIds = await _context.ExerciseMedia
                                             .Where(em => em.ExerciseId == exerciseId)
                                             .Select(em => em.MediaId)
                                             .ToListAsync();

                // Buscar os detalhes da mídia correspondente na tabela Media
                var mediaList = await _context.Media
                                              .Where(m => mediaIds.Contains(m.Id))
                                              .ToListAsync();

                if (!mediaList.Any()) // Verifica se a lista está vazia
                {
                    return NotFound(new { message = "No media found for this exercise." });
                }

                return Ok(mediaList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching media", error = ex.Message });
            }
        }

        [HttpGet("api/getFilteredExercises")]
        public async Task<IActionResult> GetFilteredExercises(string? type, string? difficultyLevel, string? muscleGroup, string? equipmentNeeded)
        {
            try
            {
                var query = _context.Exercises.AsQueryable();

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
                    ExerciseRoutine = e.ExerciseRoutine,
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
                var routines = await _context.Routines.Where(r => r.UserId == userId && r.IsCustom == true).ToListAsync();


                if (routines.IsNullOrEmpty())
                {
                    return BadRequest(new { message = "Dont exist Custom Routines" });
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
        public async Task<IActionResult> AddToRoutine([FromBody] AddRoutineFromExercisesViewModel model)
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

                var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == model.ExerciseId);
                var routine = await _context.Routines.FirstOrDefaultAsync(r => r.Id == model.RoutineId);

                if (exercise == null)
                    return NotFound(new { message = "Exercise not found" });

                if (routine == null)
                    return NotFound(new { message = "Routine not found" });

                // Criar relação entre o exercício e a rotina
                var exerciseRoutine = new ExerciseRoutine
                {
                    ExerciseId = exercise.Id,
                    Exercise = exercise,
                    RoutineId = routine.Id,
                    Routine = routine,
                    Duration = model.duration,
                    Reps = model.reps,
                    Sets = model.sets,
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

        [HttpPost("api/addExercise")]
        public async Task<IActionResult> AddExercise([FromBody] AddExercisesViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid data",
                    });
                }

                if (!Enum.TryParse(model.newDifficultyLevel, out RoutineLevel difficultyLevel))
                {
                    return BadRequest(new { message = "Invalid difficulty level" });
                }

                var newExercise = new Exercise
                {
                    Name = model.newName,
                    Description = model.newDescription,
                    Type = model.newType,
                    DifficultyLevel = difficultyLevel,
                    MuscleGroup = model.newMuscleGroup,
                    EquipmentNecessary = model.newEquipmentNecessary,
                };

                _context.Exercises.Add(newExercise);
                await _context.SaveChangesAsync();

                // Criar e adicionar mídias associadas ao exercício
                var mediaList = new List<Media>
        {
            new Media { Name = model.newMediaName, Url = model.newMediaUrl, Type = model.newMediaType },
            new Media { Name = model.newMediaName1, Url = model.newMediaUrl1, Type = model.newMediaType1 }
        };

                if (!string.IsNullOrEmpty(model.newMediaName2) &&
                    !string.IsNullOrEmpty(model.newMediaUrl2) &&
                    !string.IsNullOrEmpty(model.newMediaType2))
                {
                    mediaList.Add(new Media { Name = model.newMediaName2, Url = model.newMediaUrl2, Type = model.newMediaType2});
                }

                _context.Media.AddRange(mediaList);
                await _context.SaveChangesAsync();

                if (mediaList.Any())
                {
                    var newExerciseMediaList = mediaList.Select(m => new ExerciseMedia
                    {
                        ExerciseId = newExercise.Id,
                        MediaId = m.Id // Mantendo a relação correta com a mídia existente
                    }).ToList();

                    _context.ExerciseMedia.AddRange(newExerciseMediaList);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Exercise added successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding exercise", error = ex.Message });
            }
        }
    }
}
