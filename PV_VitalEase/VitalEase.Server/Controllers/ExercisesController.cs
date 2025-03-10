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
    using Microsoft.IdentityModel.Tokens;
    using NuGet.Common;


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
                    // Fetch all logs from the database
                    var exercises = await _context.Exercises.ToListAsync();


                    if (exercises.IsNullOrEmpty())
                    {
                        return BadRequest(new List<Exercise>());
                    }

                    // Return the logs in JSON format
                    return Ok(exercises);
                }
                catch (Exception ex)
                {
                    // Handle error and return a bad request response
                    return BadRequest(new { message = "Error fetching exercises", error = ex.Message });
                }
            }
        }
}
