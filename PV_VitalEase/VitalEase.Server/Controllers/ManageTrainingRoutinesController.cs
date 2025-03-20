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
    }
}
