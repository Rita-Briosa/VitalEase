namespace VitalEase.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using VitalEase.Server.Data;
    using VitalEase.Server.Models;

    [ApiController]
    [Route("api/manage-training-routines")]
    public class ManageTrainingRoutinesController : ControllerBase
    {
        private readonly VitalEaseServerContext _context;

        public ManageTrainingRoutinesController(VitalEaseServerContext context)
        {
            _context = context;
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
