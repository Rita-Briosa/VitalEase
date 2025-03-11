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
                    // Fetch all exercises from the database
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
    }
}
