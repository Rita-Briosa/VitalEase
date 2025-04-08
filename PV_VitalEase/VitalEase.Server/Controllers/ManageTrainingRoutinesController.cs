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

    /// <summary>
    /// Controller responsible for managing training routines within the application.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for operations related to training routines management,
    /// such as creating, updating, and deleting routines, and interacts with the database through the provided context.
    /// </remarks>
    public class ManageTrainingRoutinesController : ControllerBase
    {
        /// <summary>
        /// Gets the database context used for accessing and managing training routines data.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageTrainingRoutinesController"/> class.
        /// </summary>
        /// <param name="context">
        /// The <see cref="VitalEaseServerContext"/> instance used to interact with the application's database.
        /// </param>
        public ManageTrainingRoutinesController(VitalEaseServerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the list of exercises associated with a specific training routine.
        /// </summary>
        /// <param name="routineId">
        /// A string representing the identifier of the routine. This value will be parsed into an integer.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     a 200 OK response with a JSON array of exercise data transfer objects (DTOs) if exercises are found,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response if the routineId is invalid,
        ///   </item>
        ///   <item>
        ///     a 404 Not Found response if no exercise relations exist for the specified routine,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response with error details if an exception occurs during processing.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method executes the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     It first attempts to parse the provided routineId into an integer. If parsing fails, a Bad Request response is returned.
        ///   </item>
        ///   <item>
        ///     It then queries the <c>ExerciseRoutines</c> table to fetch all records where the RoutineId matches the parsed value.
        ///   </item>
        ///   <item>
        ///     If no matching exercise routine records are found, a Not Found response is returned with an appropriate message.
        ///   </item>
        ///   <item>
        ///     Otherwise, the method collects the ExerciseIds from the retrieved records and queries the <c>Exercises</c> table
        ///     to fetch the full details of those exercises.
        ///   </item>
        ///   <item>
        ///     The exercise details are then mapped into a data transfer object (DTO) format to avoid exposing the entire entity directly.
        ///   </item>
        ///   <item>
        ///     Finally, the method returns an OK response with the list of exercise DTOs in JSON format.
        ///   </item>
        /// </list>
        /// </remarks>
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
                    EquipmentNecessary = e.EquipmentNecessary
                }).ToList();

                return Ok(new { message = "Exercises Fetched successfully!", exerciseDtos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching exercises", error = ex.Message });
            }
        }


        /// <summary>
        /// Retrieves the details of a routine by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the routine to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     a 200 OK response with the routine details if the routine is found,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response if an error occurs during the fetch operation.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method attempts to locate the routine by its identifier from the <c>Routines</c> table. 
        /// If the routine is found, it returns the routine details with a 200 OK response.
        /// If an exception occurs during the process, it catches the error and returns a 400 Bad Request response with a corresponding message.
        /// </remarks>
        [HttpGet("getRoutine/{routineId}")]
        public async Task<IActionResult> GetRoutineById(int routineId)
        {
            try
            {
                var routine = await _context.Routines.FirstOrDefaultAsync(r => r.Id == routineId);
                return Ok(routine);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching routine details", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes the association between a specified exercise and routine.
        /// </summary>
        /// <param name="routineId">The identifier of the routine from which the exercise should be removed.</param>
        /// <param name="exerciseId">The identifier of the exercise to be removed from the routine.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     a 204 No Content response if the association is successfully deleted,
        ///   </item>
        ///   <item>
        ///     a 404 Not Found response if no matching exercise routine association is found,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response if an error occurs during the deletion process.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method attempts to locate the exercise-routine association by filtering the <c>ExerciseRoutines</c> table using the provided
        /// routineId and exerciseId. If the association is not found, it returns a 404 Not Found response with a corresponding message.
        /// If the association is found, it is removed from the database context and the changes are saved. On successful deletion, a 204 No Content response is returned.
        /// If an exception occurs during the process, the method catches it and returns a 400 Bad Request response with an error message.
        /// </remarks>
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
                return BadRequest(new { message = "Error deleting exercise from routine" });
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific exercise based on the provided exercise ID.
        /// </summary>
        /// <param name="exerciseId">
        /// A string representing the exercise identifier. This value is expected to be convertible to an integer.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     a 200 OK response with the exercise details (mapped to a DTO) if the exercise is found,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response with an error message if the exercise ID is invalid or if an error occurs,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response with a message if the exercise is not found.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It first attempts to parse the <paramref name="exerciseId"/> string to an integer. If parsing fails,
        ///     a Bad Request response is returned indicating an "Invalid exercise ID".
        ///   </item>
        ///   <item>
        ///     It then queries the <c>Exercises</c> table in the database to retrieve the exercise that matches the parsed ID.
        ///   </item>
        ///   <item>
        ///     If no exercise is found, a Bad Request response is returned with the message "Exercise don't found".
        ///   </item>
        ///   <item>
        ///     If the exercise is found, its details are mapped to a data transfer object (DTO) to avoid exposing the entire entity.
        ///   </item>
        ///   <item>
        ///     Finally, the method returns an OK response containing the exercise DTO in JSON format.
        ///   </item>
        /// </list>
        /// If any exceptions occur during processing, they are caught and a Bad Request response is returned with the error details.
        /// </remarks>
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
                };

                return Ok(exerciseDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching exercise", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the media items associated with a specific exercise.
        /// </summary>
        /// <param name="exerciseId">
        /// A string representing the identifier of the exercise, which will be parsed into an integer.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     a 200 OK response with a JSON array of media items if the media are successfully retrieved,
        ///   </item>
        ///   <item>
        ///     a 404 Not Found response if no media items are associated with the specified exercise,
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response if the exerciseId is invalid or if an error occurs during processing.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Validates that the provided <paramref name="exerciseId"/> can be parsed into an integer.
        ///     If parsing fails, it returns a Bad Request response indicating an invalid exercise ID.
        ///   </item>
        ///   <item>
        ///     Queries the <c>ExerciseMedia</c> table to retrieve all media IDs associated with the given exercise.
        ///   </item>
        ///   <item>
        ///     Uses the retrieved media IDs to fetch the corresponding media details from the <c>Media</c> table.
        ///   </item>
        ///   <item>
        ///     If no media items are found, returns a Not Found response with a corresponding message.
        ///   </item>
        ///   <item>
        ///     If media items are found, returns an OK response with the list of media items in JSON format.
        ///   </item>
        ///   <item>
        ///     Any exceptions encountered during the process are caught and result in a Bad Request response with error details.
        ///   </item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Adds a new training routine and associates exercises with it.
        /// </summary>
        /// <param name="model">
        /// The model containing the necessary data to create the new routine, including the name, description, type, level, requirements, and the list of exercises to associate.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message if the routine and exercises are added successfully,
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the data is invalid or an error occurs during the process.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the received model. If the data is invalid, it returns a <c>BadRequest</c> with the message "Invalid data".
        ///   </item>
        ///   <item>
        ///     Converts the routine's difficulty level to the <see cref="RoutineLevel"/> enum. If the conversion fails, it returns a <c>BadRequest</c> with the message "Invalid difficulty level".
        ///   </item>
        ///   <item>
        ///     Creates a new <see cref="Routine"/> object with the provided data and adds it to the database. The routine is marked as non-custom (<c>IsCustom = false</c>).
        ///   </item>
        ///   <item>
        ///     If a list of exercises is provided in the model, the method iterates over each exercise ID:
        ///     <list type="bullet">
        ///       <item>
        ///         It retrieves the original exercise from the database.
        ///       </item>
        ///       <item>
        ///         Creates a new association (<see cref="ExerciseRoutine"/>) between the exercise and the routine, setting default values for <c>Sets</c>, <c>Reps</c>, and <c>Duration</c>.
        ///       </item>
        ///       <item>
        ///         Adds this association to the database and saves the changes to generate the association's ID.
        ///       </item>
        ///       <item>
        ///         Invokes the <c>AddExerciseIdToRoutine</c> method to store the association between the exercise and the routine.
        ///       </item>
        ///     </list>
        ///   </item>
        ///   <item>
        ///     Adds, in bulk, any media associations related to the exercises (if present) and saves the changes to the database.
        ///   </item>
        ///   <item>
        ///     Returns an <c>Ok</c> response with a success message if all operations are completed without errors.
        ///   </item>
        ///   <item>
        ///     In case of an exception, returns a <c>BadRequest</c> with an error message and the exception details.
        ///   </item>
        /// </list>
        /// </remarks>
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
                        var exercise = await _context.Exercises.FindAsync(exerciseId);
                        if (exercise == null) continue;

                        // Criar a relaçao
                        var newExerciseInRoutine = new ExerciseRoutine
                        {
                            ExerciseId = exerciseId,
                            RoutineId = newRoutine.Id,
                            Sets = 1,
                            Reps = 0,
                            Duration = 0,
                        };

                        // Guarda a relaçao
                        _context.ExerciseRoutines.Add(newExerciseInRoutine);
                        await _context.SaveChangesAsync(); // Salva para gerar o ID do novo exercício

                        // Guarda a relaçao na lista de relaçoes do exercicio
                        await AddExerciseIdToRoutine(exercise.Id, newExerciseInRoutine);
                    }

                    // Adicionar todas as associações de exercícios de uma vez
                    //_context.ExerciseRoutines.AddRange(exerciseRoutines);
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

        /// <summary>
        /// Adds a new custom training routine for a specific user.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user for whom the custom routine will be created.
        /// </param>
        /// <param name="model">
        /// The model containing the necessary data to create the routine, including name, description, type, difficulty level, and requirements.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the identifier of the new routine if the operation is successful.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> with an error message if the data is invalid or if an exception occurs.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the received model. If the model is invalid, it returns a BadRequest with the message "Invalid data."
        ///   </item>
        ///   <item>
        ///     Converts the difficulty level string to the <see cref="RoutineLevel"/> enum. If the conversion fails, it returns a BadRequest with the message "Invalid difficulty level."
        ///   </item>
        ///   <item>
        ///     Creates a new <see cref="Routine"/> object associated with the user identified by the <paramref name="userId"/>,
        ///     marks the routine as custom (<c>IsCustom = true</c>), and populates the other fields with the values from the model.
        ///   </item>
        ///   <item>
        ///     Adds the new routine to the database and saves the changes to generate the routine's identifier.
        ///   </item>
        ///   <item>
        ///     Returns an Ok response with a success message and the identifier of the new routine.
        ///   </item>
        ///   <item>
        ///     In case of an exception, returns a BadRequest with the message "Error adding routine" and the exception details.
        ///   </item>
        /// </list>
        /// </remarks>
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
                    User = await _context.Users.FindAsync(userId),
                    Description = model.newDescription,
                    Type = model.newType,
                    Level = routineLevel,
                    IsCustom = true,
                    Needs = model.newNeeds,
                };

                _context.Routines.Add(newRoutine);
                await _context.SaveChangesAsync(); // Salva para gerar o ID da rotina


                return Ok(new { message = "Routine and exercises added successfully!", routineId = $"{newRoutine.Id}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding routine", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all global (non-custom) routines that are not associated with any user.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> result with the list of routines in JSON format if global routines are found,
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> result with an informative message if no routines exist or if an error occurs.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves all routines from the database where <c>IsCustom</c> is false and <c>UserId</c> is null, indicating that they are global routines (not associated with a specific user).
        ///   </item>
        ///   <item>
        ///     If the list of routines is empty, it returns a <c>BadRequest</c> with a message indicating that no routines exist.
        ///   </item>
        ///   <item>
        ///     Otherwise, it returns the retrieved routines in an <c>Ok</c> result.
        ///   </item>
        ///   <item>
        ///     In case of an exception, it returns a <c>BadRequest</c> with the message "Error fetching routines" and the exception details.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/getRoutines")]
        public async Task<IActionResult> GetRoutines()
        {

            try
            {
                // Fetch all media from the database
                var routines = await _context.Routines.Where(r => r.IsCustom == false && r.UserId == null).ToListAsync();


                if (routines.IsNullOrEmpty())
                {
                    return BadRequest(new { message = "Dont exist Routines" });
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
        /// Retrieves a list of global (non-custom) routines filtered based on the provided optional parameters.
        /// </summary>
        /// <param name="name">
        /// Optional. The name of the routine to filter by. The comparison is case-insensitive.
        /// </param>
        /// <param name="type">
        /// Optional. The type of routine to filter by. The comparison is case-insensitive.
        /// </param>
        /// <param name="difficultyLevel">
        /// Optional. The difficulty level of the routine to filter by. The comparison is performed by converting the enum to a lowercase string.
        /// </param>
        /// <param name="numberOfExercises">
        /// Optional. The exact number of exercises associated with the routine. If provided, only routines with this exact number of exercises will be returned.
        /// </param>
        /// <param name="equipmentNeeded">
        /// Optional. The required equipment, filtering routines whose <c>Needs</c> value matches the provided string in a case-insensitive manner.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that, if successful, contains a list of filtered routines along with the respective number of associated exercises.
        /// In case of an exception, it returns a <c>BadRequest</c> with an error message and details of the exception.
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It initializes a query to retrieve routines where <c>IsCustom</c> is false.
        ///   </item>
        ///   <item>
        ///     It applies optional filters for the routine's name, type, difficulty level, and equipment requirements.
        ///   </item>
        ///   <item>
        ///     For each routine, it selects the associated number of exercises by counting the records in <c>ExerciseRoutines</c>.
        ///   </item>
        ///   <item>
        ///     If the <c>numberOfExercises</c> parameter is provided, it filters the routines to include only those with the exact specified number of exercises.
        ///   </item>
        ///   <item>
        ///     Finally, it maps the result to an object that includes the routine details and the exercise count, and returns this list in JSON format.
        ///   </item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Retrieves the custom training routines associated with a specific user.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user for whom custom routines are to be retrieved.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     an Ok response with a JSON array of custom routines if any are found,
        ///   </item>
        ///   <item>
        ///     a NotFound response with an informative message if no custom routines are found for the user,
        ///   </item>
        ///   <item>
        ///     a BadRequest response if an error occurs during processing.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It filters the Routines table to retrieve only those routines marked as custom (<c>IsCustom == true</c>)
        ///     that are associated with the specified user (i.e., where <c>UserId</c> equals the provided <paramref name="userId"/>).
        ///   </item>
        ///   <item>
        ///     If the resulting list is empty, it returns a NotFound response with the message "Couldn't find custom routines for the user."
        ///   </item>
        ///   <item>
        ///     Otherwise, it returns an Ok response containing the list of custom routines in JSON format.
        ///   </item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Deletes a training routine based on its identifier.
        /// </summary>
        /// <param name="routineId">
        /// The identifier of the routine to delete.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that represents:
        /// <list type="bullet">
        ///   <item>
        ///     <c>NoContent</c> if the deletion is successful;
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> if the routine is not found;
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> in case of an error during the operation.
        ///   </item>
        /// </list>
        /// </returns>
        [HttpDelete("api/deleteRoutine/{routineId}")]
        public async Task<IActionResult> deleteRoutine(int routineId)
        {
            try
            {
                var routine = await _context.Routines.FindAsync(routineId);

                if (routine == null)
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

        /// <summary>
        /// Retrieves the association between a specific exercise and a training routine.
        /// </summary>
        /// <param name="routineId">
        /// The identifier of the routine.
        /// </param>
        /// <param name="exerciseId">
        /// The identifier of the exercise.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     an Ok response with the details of the association between the exercise and the routine, if found;
        ///   </item>
        ///   <item>
        ///     a BadRequest response in case of an error, including an error message and exception details.
        ///   </item>
        /// </list>
        /// </returns>
        [HttpGet("api/getExerciseRoutine/{routineId}")]
        public async Task<IActionResult> getExerciseRoutine(int routineId, int exerciseId)
        {
            try
            {
                var exerciseRoutines = await _context.ExerciseRoutines.Where(exr => exr.RoutineId == routineId && exr.ExerciseId == exerciseId).FirstOrDefaultAsync();
                return Ok(exerciseRoutines);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching routines", error = ex.Message });
            }
        }

        /// <summary>
        /// Adds an association between an exercise and a routine by linking the specified <see cref="ExerciseRoutine"/> object to the exercise.
        /// </summary>
        /// <param name="exerciseId">
        /// The identifier of the exercise to which the association should be added.
        /// </param>
        /// <param name="exerciseRoutine">
        /// The object representing the association to be added, containing the routine data and related parameters (e.g., sets, reps, duration).
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating:
        /// <list type="bullet">
        ///   <item>
        ///     <c>NoContent</c> if the association is added successfully;
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> if the exercise is not found;
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if an error occurs during the operation, returning the exception details.
        ///   </item>
        /// </list>
        /// </returns>
        [HttpPut]
        private async Task<IActionResult> AddExerciseIdToRoutine(int exerciseId, ExerciseRoutine exerciseRoutine)
        {
            var exercise = await _context.Exercises.Where(e => e.Id == exerciseId).FirstOrDefaultAsync();

            try
            {
                if (exercise == null)
                {
                    return NotFound("Exercise not found");
                }

                exercise.ExerciseRoutine.Add(exerciseRoutine);
                await _context.SaveChangesAsync();

                return NoContent();

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding relation", error = ex.Message });
            }
        }

        /// <summary>
        /// Edita os detalhes da relação entre um exercício e uma rotina específica.
        /// </summary>
        /// <param name="exerciseId">
        /// O identificador do exercício.
        /// </param>
        /// <param name="routineId">
        /// O identificador da rotina.
        /// </param>
        /// <param name="model">
        /// O modelo que contém os novos valores para sets, reps e duração.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>NoContent</c> se a edição for bem-sucedida.
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se a relação entre o exercício e a rotina não for encontrada.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> em caso de erro durante a operação.
        ///   </item>
        /// </list>
        /// </returns>
        [HttpPut("api/editExerciseRoutine/{exerciseId}/{routineId}")]
        public async Task<IActionResult> EditExerciseRoutine(int exerciseId, int routineId, [FromBody] EditExerciseRoutineViewModel model)
        {
            try
            {
                var exerciseRoutine = await _context.ExerciseRoutines.Where(exr => exr.ExerciseId == exerciseId && exr.RoutineId == routineId).FirstOrDefaultAsync();

                if(exerciseRoutine == null)
                {
                    return NotFound();
                }

                exerciseRoutine.Sets = model.sets;
                exerciseRoutine.Reps = model.reps;
                exerciseRoutine.Duration = model.duration;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch
            {
                return BadRequest(new { message = "Error editing Exercise Routine" });
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
