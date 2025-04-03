namespace VitalEase.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using VitalEase.Server.Data;
    using VitalEase.Server.Models;
    using VitalEase.Server.ViewModel;

    public class ManageTrainingRoutinesController : ControllerBase
    {
        private readonly VitalEaseServerContext _context;

        public ManageTrainingRoutinesController(VitalEaseServerContext context)
        {
            _context = context;
        }

        [HttpGet("api/getExercisesFromRoutine/{routineId}")]
        public async Task<IActionResult> GetExercisesFromRoutine(string routineId)
        {
            try
            {
                if (!int.TryParse(routineId, out int routineInteger))
                {
                    return BadRequest(new { message = "Invalid routine ID" });
                }

                // Buscar os registros da relação entre Rotina e Exercícios
                var exerciseRoutineRelations = await _context.ExerciseRoutines
                    .Where(er => er.RoutineId == routineInteger)
                    .ToListAsync();

                // Verificar se há registros na relação
                if (!exerciseRoutineRelations.Any())
                {
                    return NotFound(new { message = "No exercises found for this routine." });
                }

                // Coletar os IDs dos exercícios relacionados
                var exerciseIds = exerciseRoutineRelations.Select(er => er.ExerciseId).ToList();

                // Buscar os exercícios completos na tabela Exercises
                var exercises = await _context.Exercises
                    .Where(e => exerciseIds.Contains(e.Id))
                    .ToListAsync();

                // Mapear para DTO para evitar expor entidades diretamente
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
                    Duration = e.Duration
                }).ToList();

                return Ok(exerciseDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching exercises", error = ex.Message });
            }
        }

        [HttpDelete("api/deleteExerciseFromRoutine/{routineId}/{exerciseId}")]
        public async Task<IActionResult> deleteExerciseFromRoutine(int routineId, int exerciseId)
        {
            try
            {
                var exercise = await _context.ExerciseRoutines.Where(exRout => exRout.RoutineId == routineId && exRout.ExerciseId == exerciseId).FirstOrDefaultAsync();

                if (exercise == null)
                {
                    return NotFound("Exercise not Found");
                }

                _context.Remove(exercise);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch
            {
                return BadRequest(new {message = "Error deleting exercise from routine"});
            }
        }

        [HttpGet("api/getExerciseDetailsFromRoutine/{exerciseId}")]
        public async Task<IActionResult> GetExerciseDetailsFromRoutine(string exerciseId)
        {
            try
            {
                if (!int.TryParse(exerciseId, out int exerciseInteger))
                {
                    return BadRequest(new { message = "Invalid exercise ID" });
                }



                // Buscar os exercícios completos na tabela Exercises
                var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseInteger);

                if (exercise == null)
                {
                    return BadRequest(new { message = "Exercise don't found" });
                }

                // Mapear para DTO para evitar expor entidades diretamente
                var exerciseDto = new
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Description = exercise.Description,
                    Type = exercise.Type,
                    DifficultyLevel = exercise.DifficultyLevel.ToString(),
                    MuscleGroup = exercise.MuscleGroup,
                    EquipmentNecessary = exercise.EquipmentNecessary,
                    Reps = exercise.Reps,
                    Duration = exercise.Duration
                };

                return Ok(exerciseDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching exercise", error = ex.Message });
            }
        }

        [HttpGet("api/getExerciseMediaFromRoutine/{exerciseId}")]
        public async Task<IActionResult> GetExerciseMedia(string exerciseId)
        {

            try
            {

                if (!int.TryParse(exerciseId, out int exerciseInteger))
                {
                    return BadRequest(new { message = "Invalid exercise ID" });
                }

                var mediaIds = await _context.ExerciseMedia
                                             .Where(em => em.ExerciseId == exerciseInteger)
                                             .Select(em => em.MediaId)
                                             .ToListAsync();

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
                // Handle error and return a bad request response
                return BadRequest(new { message = "Error fetching media", error = ex.Message });
            }
        }

        [HttpPost("api/addNewRoutine")]
        public async Task<IActionResult> AddRoutine([FromBody] AddRoutineViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid data" });
                }

                if (!Enum.TryParse(model.newRoutineLevel, out RoutineLevel routineLevel))
                {
                    return BadRequest(new { message = "Invalid difficulty level" });
                }

                // Criar nova rotina
                var newRoutine = new Routine
                {
                    Name = model.newName,
                    Description = model.newDescription,
                    Type = model.newType,
                    Level = routineLevel,
                    IsCustom = false,
                    Needs = model.newNeeds,
                };

                _context.Routines.Add(newRoutine);
                await _context.SaveChangesAsync(); // Salva para gerar o ID da rotina

                // Lista para armazenar associações de exercícios
                var exerciseRoutines = new List<ExerciseRoutine>();
                var exerciseMediaLinks = new List<ExerciseMedia>();

                if (model.Exercises != null && model.Exercises.Any())
                {
                    foreach (var exerciseId in model.Exercises)
                    {
                        // Buscar exercício original
                        var originalExercise = await _context.Exercises.FindAsync(exerciseId);
                        if (originalExercise == null) continue;

                        // Criar novo exercício baseado no original
                        var newExercise = new Exercise
                        {
                            Name = originalExercise.Name,
                            Description = originalExercise.Description,
                            Type = originalExercise.Type,
                            DifficultyLevel = originalExercise.DifficultyLevel,
                            MuscleGroup = originalExercise.MuscleGroup,
                            EquipmentNecessary = originalExercise.EquipmentNecessary,
                            Reps = 12,
                            Duration = originalExercise.Duration
                        };

                        _context.Exercises.Add(newExercise);
                        await _context.SaveChangesAsync(); // Salva para gerar o ID do novo exercício

                        // Criar associação na tabela ExerciseRoutine
                        exerciseRoutines.Add(new ExerciseRoutine
                        {
                            RoutineId = newRoutine.Id,
                            ExerciseId = newExercise.Id // Usando o novo exercício criado
                        });

                        // Reutilizar os mesmos IDs de mídia já existentes
                        var mediaList = await _context.ExerciseMedia.Where(m => m.ExerciseId == exerciseId).ToListAsync();
                        foreach (var media in mediaList)
                        {
                            exerciseMediaLinks.Add(new ExerciseMedia
                            {
                                ExerciseId = newExercise.Id, // Associa ao novo exercício
                                MediaId = media.MediaId      // Usa o mesmo mediaId do exercício original
                            });
                        }
                    }

                    // Adicionar todas as associações de exercícios de uma vez
                    _context.ExerciseRoutines.AddRange(exerciseRoutines);
                    _context.ExerciseMedia.AddRange(exerciseMediaLinks);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Routine and exercises added successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding routine", error = ex.Message });
            }
        }

        [HttpPost("api/addNewCustomRoutine/{userId}")]
        public async Task<IActionResult> AddCustomRoutine(int userId, [FromBody] AddRoutineViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid data" });
                }

                if (!Enum.TryParse(model.newRoutineLevel, out RoutineLevel routineLevel))
                {
                    return BadRequest(new { message = "Invalid difficulty level" });
                }

                // Criar nova rotina
                var newRoutine = new Routine
                {
                    Name = model.newName,
                    UserId = userId,
                    User =  await _context.Users.FindAsync(userId),
                    Description = model.newDescription,
                    Type = model.newType,
                    Level = routineLevel,
                    IsCustom = true,
                    Needs = model.newNeeds,
                };

                _context.Routines.Add(newRoutine);
                await _context.SaveChangesAsync(); // Salva para gerar o ID da rotina


                return Ok(new { message = "Routine and exercises added successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding routine", error = ex.Message });
            }
        }

        [HttpGet("api/getRoutines")]
        public async Task<IActionResult> GetRoutines()
        {

            try
            {
                // Fetch all media from the database
                var routines = await _context.Routines.Where(r => r.IsCustom == false && r.UserId == null).ToListAsync();


                if (routines.IsNullOrEmpty())
                {
                    return BadRequest( new { message = "Dont exist Routines"});
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

        [HttpGet("api/getFilteredRoutines")]
        public async Task<IActionResult> GetFilteredRoutines(string? name, string? type, string? difficultyLevel, int? numberOfExercises, string? equipmentNeeded)
        {
            try
            {
                var query = _context.Routines.Where(r => r.IsCustom == false).AsQueryable();

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(r => r.Name.ToLower() == name.ToLower());
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(r => r.Type.ToLower() == type.ToLower());
                }

                if (!string.IsNullOrEmpty(difficultyLevel))
                {
                    query = query.Where(r => r.Level.ToString().ToLower().Equals(difficultyLevel.ToLower()));
                }

                if (!string.IsNullOrEmpty(equipmentNeeded))
                {
                    query = query.Where(r => r.Needs.ToString().ToLower().Equals(equipmentNeeded.ToLower()));
                }

                var routinesWithExerciseCount = await query
                    .Select(r => new
                    {
                        Routine = r,
                        ExerciseCount = _context.ExerciseRoutines.Count(er => er.RoutineId == r.Id)
                    })
                    .Where(r => !numberOfExercises.HasValue || r.ExerciseCount == numberOfExercises) // Apply filtering on count
                    .Select(r => new
                    {
                        r.Routine.Id,
                        r.Routine.Name,
                        r.Routine.Type,
                        r.Routine.Level,
                        r.Routine.Needs,
                        r.Routine.Description,
                        r.Routine.IsCustom,
                        ExerciseCount = r.ExerciseCount // Include exercise count in response
                    })
                    .ToListAsync();



                // Fetch all exercises
                //var exercises = await query.Where(r => r.IsCustom == false).ToListAsync();

                return Ok(routinesWithExerciseCount);
            }
            catch (Exception ex)
            {
                //Handle error and return a bad request response
                return BadRequest(new { message = "Error filtering exercises", error = ex.Message });
            }
        }

        [HttpGet("api/getCustomTrainingRoutines")]
        public async Task<IActionResult> getCustomTrainingRoutines(int userId)
        {
            try
            {
                var routines = await _context.Routines.Where(r => r.IsCustom == true && r.UserId == userId).ToListAsync();

                if (routines.IsNullOrEmpty())
                {
                    return NotFound("Couldn't find custom routines for the user");
                }

                return Ok(routines);
            }
            catch
            {
                return BadRequest(new { message = "Error fetching custom training routines" });
            }
        }

        [HttpDelete("api/deleteRoutine/{routineId}")]
        public async Task<IActionResult> deleteRoutine(int routineId)
        {
            try
            {
                var routine = await _context.Routines.FindAsync(routineId);

                if(routine == null)
                {
                    return NotFound("Routine not Found");
                }

                _context.Routines.Remove(routine);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch
            {
                return BadRequest(new { message = "Error deleting Training Routine" });
            }
        }

        /* 
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllRoutines()
        {
            try
            {
                var routines = await _context.ManageTrainingRoutines.Include(tr => tr.Exercises).ToListAsync();
                return Ok(routines);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching routines", error = ex.Message });
            }
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetRoutineById(int id)
        {
            try
            {
                var routine = await _context.ManageTrainingRoutines.Include(tr => tr.Exercises)
                    .FirstOrDefaultAsync(tr => tr.Id == id);
                if (routine == null) return NotFound(new { message = "Routine not found" });
                return Ok(routine);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching routine details", error = ex.Message });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredRoutines(string? type, bool? isCustom, string? needs)
        {
            try
            {
                var query = _context.ManageTrainingRoutines.Include(tr => tr.Exercises).AsQueryable();

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(tr => tr.Type == type);
                }
                if (isCustom.HasValue)
                {
                    query = query.Where(tr => tr.IsCustom == isCustom.Value);
                }
                if (!string.IsNullOrEmpty(needs))
                {
                    query = query.Where(tr => tr.Needs.Contains(needs));
                }

                var routines = await query.ToListAsync();
                return Ok(routines);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error filtering routines", error = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddRoutine([FromBody] ManageTrainingRoutines routine)
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

                _context.ManageTrainingRoutines.Add(routine);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Routine added successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding routine", error = ex.Message });
            }
        }
        */
    }
}
