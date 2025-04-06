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

    /// <summary>
    /// Controller responsible for managing exercises.
    /// </summary>
    public class ExercisesController : Controller
        {
        /// <summary>
        /// Database context, used to access the application data.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Initializes a new instance of the controller. <see cref="ExercisesController"/>.
        /// </summary>
        /// <param name="context">
        /// The database context (<see cref="VitalEaseServerContext"/>) that enables data access operations.
        /// </param>
        public ExercisesController(VitalEaseServerContext context)
            {
                _context = context;
        }

        /// <summary>
        /// Retrieves all exercises from the database and returns a list of exercise data transfer objects.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     an HTTP 200 (OK) response with a JSON array of exercise DTOs if exercises are found,
        ///   </item>
        ///   <item>
        ///     an HTTP 400 (Bad Request) response with an empty list if no exercises are found,
        ///     or with an error message if an exception occurs.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves all exercise records from the database using <c>ToListAsync</c>.
        ///   </item>
        ///   <item>
        ///     If no exercises are found (i.e., the retrieved list is empty), it returns a Bad Request with an empty list.
        ///   </item>
        ///   <item>
        ///     Maps each exercise to an anonymous data transfer object (DTO) that includes properties such as Id, Name,
        ///     Description, Type, DifficultyLevel (converted to a string), MuscleGroup, and EquipmentNecessary.
        ///   </item>
        ///   <item>
        ///     Returns the list of DTOs in a JSON response.
        ///   </item>
        /// </list>
        /// If an exception occurs during the process, the method catches it and returns a Bad Request with an appropriate error message.
        /// </remarks>
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

        /// <summary>
        /// Retrieves the media associated with a specific exercise.
        /// </summary>
        /// <param name="exerciseId">
        /// The identifier of the exercise for which to fetch the associated media.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     a 200 OK response with the list of media details in JSON format if media is found,
        ///   </item>
        ///   <item>
        ///     a 404 Not Found response with a message if no media is associated with the exercise,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response with error details if an exception occurs during processing.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     It retrieves the media IDs linked to the given exercise from the <c>ExerciseMedia</c> table.
        ///   </item>
        ///   <item>
        ///     It then queries the <c>Media</c> table using these IDs to obtain the full media details.
        ///   </item>
        ///   <item>
        ///     If the retrieved media list is empty, it returns a NotFound response with an appropriate message.
        ///   </item>
        ///   <item>
        ///     If an exception occurs during the process, it catches the exception and returns a BadRequest response containing the error details.
        ///   </item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Retrieves a list of exercises from the database based on the provided optional filters.
        /// </summary>
        /// <param name="type">
        /// An optional parameter specifying the type of exercise to filter by. 
        /// Only exercises with a matching type will be included.
        /// </param>
        /// <param name="difficultyLevel">
        /// An optional parameter specifying the difficulty level (as a string) to filter by. 
        /// Only exercises whose difficulty level (converted to a string) matches this value will be included.
        /// </param>
        /// <param name="muscleGroup">
        /// An optional parameter specifying a muscle group. 
        /// Only exercises that target a muscle group containing this string will be included.
        /// </param>
        /// <param name="equipmentNeeded">
        /// An optional parameter specifying the required equipment. 
        /// Only exercises whose required equipment contains this string will be included.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     A 200 OK response with the filtered list of exercises in JSON format if the operation succeeds.
        ///   </item>
        ///   <item>
        ///     A 400 Bad Request response with an error message if an exception occurs during processing.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method builds a query on the <c>Exercises</c> table using the provided filters:
        /// <list type="bullet">
        ///   <item>
        ///     If a <c>type</c> is provided, the query includes only exercises with that type.
        ///   </item>
        ///   <item>
        ///     If a <c>difficultyLevel</c> is provided, the query filters exercises by comparing the string representation of their difficulty level.
        ///   </item>
        ///   <item>
        ///     If a <c>muscleGroup</c> is provided, the query includes exercises whose muscle group contains the specified substring.
        ///   </item>
        ///   <item>
        ///     If an <c>equipmentNeeded</c> value is provided, the query includes exercises whose equipment requirements contain the specified substring.
        ///   </item>
        /// </list>
        /// The query result is then projected into an anonymous object (acting as a data transfer object) that includes the exercise's basic properties.
        /// Finally, the method returns the filtered list in JSON format.
        /// </remarks>
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

        /// <summary>
        /// Retrieves the custom training routines associated with a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose custom routines are to be retrieved.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     a 200 OK response with a JSON array of custom routines if they are found,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response with a message if no custom routines exist or if an error occurs.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method queries the <c>Routines</c> table for routines that are marked as custom (<c>IsCustom == true</c>) and belong to the specified user (<c>UserId == userId</c>).
        /// If the resulting list is empty, it returns a Bad Request with a message indicating that custom routines do not exist.
        /// In case an exception occurs during the operation, the exception is caught and a Bad Request with an error message is returned.
        /// </remarks>
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

        /// <summary>
        /// Adds an exercise to a specified routine based on the data provided in the view model.
        /// </summary>
        /// <param name="model">
        /// A <see cref="AddRoutineFromExercisesViewModel"/> containing the identifiers for the exercise and the routine,
        /// along with the exercise parameters such as duration, repetitions, and sets.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     an HTTP 200 (OK) response with a success message if the exercise is successfully added to the routine,
        ///   </item>
        ///   <item>
        ///     an HTTP 404 (Not Found) response if either the exercise or the routine is not found,
        ///   </item>
        ///   <item>
        ///     an HTTP 400 (Bad Request) response with error details if the input data is invalid or if an exception occurs during processing.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the input model; if invalid, it returns a Bad Request response with the error messages.
        ///   </item>
        ///   <item>
        ///     Retrieves the exercise from the database using the provided <c>ExerciseId</c>.
        ///   </item>
        ///   <item>
        ///     Retrieves the routine from the database using the provided <c>RoutineId</c>.
        ///   </item>
        ///   <item>
        ///     If either the exercise or the routine is not found, it returns a Not Found response with an appropriate message.
        ///   </item>
        ///   <item>
        ///     Creates a new <see cref="ExerciseRoutine"/> association that links the exercise to the routine, including details such as duration, repetitions, and sets.
        ///   </item>
        ///   <item>
        ///     Adds the association to the database and saves the changes asynchronously.
        ///   </item>
        ///   <item>
        ///     Returns an OK response with a success message upon successful completion.
        ///   </item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Adds a new exercise along with its associated media to the database.
        /// </summary>
        /// <param name="model">
        /// An instance of <see cref="AddExercisesViewModel"/> containing the details for the new exercise, including:
        /// - Basic exercise information such as name, description, type, difficulty level, muscle group, and necessary equipment.
        /// - Media details, which include one or more media items (name, URL, and type) to be associated with the exercise.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        ///   - A 200 OK response with a success message if the exercise and its media are added successfully.
        ///   - A 400 Bad Request response with an error message if the provided data is invalid or if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// The method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the incoming model. If the model is invalid, it returns a Bad Request response with an appropriate message.
        ///   </item>
        ///   <item>
        ///     It attempts to parse the provided difficulty level into the <see cref="RoutineLevel"/> enum. If parsing fails, a Bad Request is returned.
        ///   </item>
        ///   <item>
        ///     A new <see cref="Exercise"/> instance is created using the basic exercise information from the model, then added to the database.
        ///   </item>
        ///   <item>
        ///     The method constructs a list of <see cref="Media"/> objects based on the media details provided in the model. 
        ///     At least two media items are added, and an optional third is included if its details are provided.
        ///   </item>
        ///   <item>
        ///     These media objects are then added to the database and saved.
        ///   </item>
        ///   <item>
        ///     If media items exist, the method creates associations between the newly created exercise and each media item using the <see cref="ExerciseMedia"/> relationship.
        ///   </item>
        ///   <item>
        ///     After all operations are completed, a success message is returned; otherwise, any exceptions are caught and a Bad Request response is generated with the error details.
        ///   </item>
        /// </list>
        /// </remarks>
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
