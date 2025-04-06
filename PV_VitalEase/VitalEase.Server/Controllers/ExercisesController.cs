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
    /// Controlador responsável pela gestão dos exercícios.
    /// </summary>
    public class ExercisesController : Controller
        {
        /// <summary>
        /// Contexto da base de dados, utilizado para aceder aos dados da aplicação.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="ExercisesController"/>.
        /// </summary>
        /// <param name="context">
        /// O contexto da base de dados (<see cref="VitalEaseServerContext"/>) que permite efetuar operações de acesso aos dados.
        /// </param>
        public ExercisesController(VitalEaseServerContext context)
            {
                _context = context;
        }

        /// <summary>
        /// Obtém todos os exercícios registados na base de dados e retorna-os num formato JSON.
        /// </summary>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>Recolhe todos os exercícios da base de dados utilizando o método assíncrono <c>ToListAsync()</c>.</item>
        ///   <item>Verifica se a lista de exercícios está vazia e, se estiver, retorna um <c>BadRequest</c> com uma lista vazia.</item>
        ///   <item>Mapeia cada exercício para um objeto anónimo (DTO) que contém as seguintes propriedades:
        ///     <list type="bullet">
        ///       <item><c>Id</c></item>
        ///       <item><c>Name</c></item>
        ///       <item><c>Description</c></item>
        ///       <item><c>Type</c></item>
        ///       <item><c>DifficultyLevel</c> (convertido para string)</item>
        ///       <item><c>MuscleGroup</c></item>
        ///       <item><c>EquipmentNecessary</c></item>
        ///     </list>
        ///   </item>
        ///   <item>Retorna os exercícios mapeados num formato JSON.</item>
        /// </list>
        /// Em caso de erro, o método captura a exceção e retorna um <c>BadRequest</c> com uma mensagem de erro.
        /// </remarks>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém a lista de exercícios em formato JSON ou uma mensagem de erro em caso de falha.
        /// </returns>
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
        /// Obtém a lista de mídia associada a um exercício específico.
        /// </summary>
        /// <param name="exerciseId">
        /// O identificador do exercício para o qual se pretende obter a mídia.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com a lista de mídia, se forem encontrados registos correspondentes.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>NotFound</c> se não for encontrada nenhuma mídia associada ao exercício.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> com uma mensagem de erro, caso ocorra alguma exceção durante o processamento.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Procura na tabela <c>ExerciseMedia</c> os registos que associam a mídia ao exercício, recolhendo os respetivos IDs.
        ///   </item>
        ///   <item>
        ///     Utiliza os IDs recolhidos para buscar os detalhes completos da mídia na tabela <c>Media</c>.
        ///   </item>
        ///   <item>
        ///     Se não for encontrada nenhuma mídia, retorna um resultado <c>NotFound</c> com uma mensagem informativa.
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
        /// Obtém uma lista de exercícios filtrados com base nos parâmetros facultativos fornecidos.
        /// </summary>
        /// <param name="type">
        /// Opcional. O tipo de exercício a filtrar. Se fornecido, serão retornados apenas os exercícios cujo campo <c>Type</c> corresponda ao valor fornecido.
        /// </param>
        /// <param name="difficultyLevel">
        /// Opcional. O nível de dificuldade a filtrar. É convertido para string e comparado com o valor fornecido.
        /// </param>
        /// <param name="muscleGroup">
        /// Opcional. O grupo muscular a filtrar. São retornados os exercícios cujo campo <c>MuscleGroup</c> contenha este valor.
        /// </param>
        /// <param name="equipmentNeeded">
        /// Opcional. O equipamento necessário a filtrar. São retornados os exercícios cujo campo <c>EquipmentNecessary</c> contenha este valor.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> contendo, em caso de sucesso, uma lista de exercícios filtrados em formato JSON.
        /// Caso ocorra algum erro, retorna um <c>BadRequest</c> com uma mensagem de erro e os detalhes da exceção.
        /// </returns>
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
        /// Obtém as rotinas personalizadas associadas a um determinado utilizador.
        /// </summary>
        /// <param name="userId">
        /// O identificador do utilizador para o qual se pretende obter as rotinas personalizadas.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém:
        /// - Um objecto JSON com as rotinas, se forem encontradas;
        /// - Um BadRequest com uma mensagem de erro se não existirem rotinas personalizadas ou se ocorrer um erro.
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Procura na base de dados as rotinas (Routines) associadas ao utilizador cujo campo <c>IsCustom</c> é verdadeiro.
        ///   </item>
        ///   <item>
        ///     Se a lista de rotinas estiver vazia, retorna um BadRequest com a mensagem "Dont exist Custom Routines".
        ///   </item>
        ///   <item>
        ///     Caso contrário, retorna as rotinas encontradas em formato JSON.
        ///   </item>
        ///   <item>
        ///     Em caso de exceção, retorna um BadRequest com a mensagem de erro e os detalhes da exceção.
        ///   </item>
        /// </list>
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
        /// Adiciona um exercício a uma rotina especificada.
        /// </summary>
        /// <param name="model">
        /// O modelo que contém os dados necessários para adicionar o exercício à rotina, incluindo os identificadores do exercício e da rotina, 
        /// bem como os parâmetros de duração, repetições e sets.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que representa a resposta da operação:
        /// - <c>Ok</c> se o exercício for adicionado com sucesso à rotina.
        /// - <c>BadRequest</c> se os dados enviados forem inválidos ou se ocorrer um erro durante a operação.
        /// - <c>NotFound</c> se o exercício ou a rotina especificada não for encontrada.
        /// </returns>
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
        /// Adiciona um novo exercício à base de dados, juntamente com as mídias associadas.
        /// </summary>
        /// <param name="model">
        /// O modelo que contém os detalhes do exercício a ser adicionado, tais como nome, descrição, tipo, nível de dificuldade,
        /// grupo muscular, equipamento necessário, e informações sobre as mídias (nomes, URLs e tipos) associadas ao exercício.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica se o exercício foi adicionado com sucesso ou se ocorreu algum erro.
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Valida o modelo recebido. Se o modelo for inválido, retorna um BadRequest com a mensagem "Invalid data".
        ///   </item>
        ///   <item>
        ///     Converte a string referente ao nível de dificuldade para o enum <see cref="RoutineLevel"/>. Caso a conversão falhe,
        ///     retorna um BadRequest com a mensagem "Invalid difficulty level".
        ///   </item>
        ///   <item>
        ///     Cria um novo objeto <see cref="Exercise"/> com os dados do modelo e o adiciona à base de dados, guardando as alterações.
        ///   </item>
        ///   <item>
        ///     Cria uma lista de objetos <see cref="Media"/> com base nos dados de mídia fornecidos no modelo. Adiciona duas mídias obrigatórias
        ///     e, opcionalmente, uma terceira, se as informações estiverem presentes.
        ///   </item>
        ///   <item>
        ///     Adiciona os objetos de mídia à base de dados e guarda as alterações.
        ///   </item>
        ///   <item>
        ///     Associa as mídias criadas ao exercício, criando objetos <see cref="ExerciseMedia"/> que estabelecem a relação entre o exercício
        ///     e cada mídia, e guarda as alterações na base de dados.
        ///   </item>
        ///   <item>
        ///     Retorna um Ok com uma mensagem de sucesso se todas as operações forem concluídas sem erros.
        ///   </item>
        ///   <item>
        ///     Caso ocorra alguma exceção, o método captura o erro e retorna um BadRequest com a mensagem "Error adding exercise"
        ///     e os detalhes da exceção.
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
