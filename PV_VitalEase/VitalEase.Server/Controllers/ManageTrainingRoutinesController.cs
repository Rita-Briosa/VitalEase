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
    /// Controlador responsável pela gestão das rotinas de treino.
    /// </summary>
    /// <remarks>
    /// Este controlador permite gerir as rotinas de treino, facilitando operações de criação, edição, eliminação e consulta de rotinas.
    /// Está baseado no contexto da base de dados <see cref="VitalEaseServerContext"/> para aceder aos registos necessários.
    /// </remarks>
    public class ManageTrainingRoutinesController : ControllerBase
    {
        /// <summary>
        /// Contexto da base de dados utilizado para operações relacionadas com as rotinas de treino.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="ManageTrainingRoutinesController"/>.
        /// </summary>
        /// <param name="context">
        /// O contexto da base de dados (<see cref="VitalEaseServerContext"/>) que permite efetuar operações de acesso e manipulação dos dados.
        /// </param>
        public ManageTrainingRoutinesController(VitalEaseServerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém os exercícios associados a uma determinada rotina de treino.
        /// </summary>
        /// <param name="routineId">
        /// O identificador da rotina de treino (em formato string) para a qual se pretende obter os exercícios.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém, em caso de sucesso, uma lista de exercícios (em formato DTO) associados à rotina especificada.
        /// Caso contrário, retorna uma mensagem de erro adequada.
        /// </returns>
        /// <remarks>
        /// O método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se o parâmetro <c>routineId</c> pode ser convertido para um inteiro. Se não, retorna um BadRequest.
        ///   </item>
        ///   <item>
        ///     Procura na tabela <c>ExerciseRoutines</c> os registos que associam exercícios à rotina especificada.
        ///   </item>
        ///   <item>
        ///     Se não existirem registos na relação, retorna um NotFound com uma mensagem informativa.
        ///   </item>
        ///   <item>
        ///     Recolhe os IDs dos exercícios associados e, com base neles, busca os exercícios completos na tabela <c>Exercises</c>.
        ///   </item>
        ///   <item>
        ///     Mapeia os exercícios para um DTO para evitar expor entidades diretamente, convertendo o nível de dificuldade para string.
        ///   </item>
        ///   <item>
        ///     Retorna os exercícios em formato JSON.
        ///   </item>
        /// </list>
        /// Em caso de exceção, retorna um BadRequest com uma mensagem de erro e os detalhes da exceção.
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

                return Ok(exerciseDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching exercises", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove um exercício de uma rotina de treino específica.
        /// </summary>
        /// <param name="routineId">O identificador da rotina da qual o exercício deve ser removido.</param>
        /// <param name="exerciseId">O identificador do exercício a remover.</param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// - <c>NoContent</c> se a remoção for bem-sucedida;
        /// - <c>NotFound</c> se a relação entre a rotina e o exercício não for encontrada;
        /// - <c>BadRequest</c> em caso de erro.
        /// </returns>
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
        /// Obtém os detalhes de um exercício específico a partir do seu identificador.
        /// </summary>
        /// <param name="exerciseId">
        /// O identificador do exercício, passado como string, que será convertido para inteiro.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> contendo, em caso de sucesso, os detalhes do exercício em formato DTO; 
        /// caso contrário, uma mensagem de erro apropriada.
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Converte o parâmetro <c>exerciseId</c> de string para inteiro. Se a conversão falhar, retorna um <c>BadRequest</c> com uma mensagem de "Invalid exercise ID".
        ///   </item>
        ///   <item>
        ///     Procura o exercício na tabela <c>Exercises</c> com base no ID convertido. Se o exercício não for encontrado, retorna um <c>BadRequest</c> com a mensagem "Exercise don't found".
        ///   </item>
        ///   <item>
        ///     Se o exercício for encontrado, mapeia os seus dados para um objeto DTO, convertendo o nível de dificuldade para string, e retorna esse objeto no formato JSON.
        ///   </item>
        /// </list>
        /// Em caso de exceção, retorna um <c>BadRequest</c> com a mensagem "Error fetching exercise" e os detalhes da exceção.
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
        /// Obtém a lista de mídia associada a um exercício específico.
        /// </summary>
        /// <param name="exerciseId">
        /// O identificador do exercício, passado como string, que será convertido para inteiro.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> contendo:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com a lista de mídia, se forem encontrados registos correspondentes.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>NotFound</c> se não for encontrada nenhuma mídia associada ao exercício.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> com uma mensagem de erro, caso ocorra alguma exceção ou se o ID for inválido.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se o parâmetro <c>exerciseId</c> pode ser convertido para um inteiro. Se não puder, retorna um BadRequest com a mensagem "Invalid exercise ID".
        ///   </item>
        ///   <item>
        ///     Busca na tabela <c>ExerciseMedia</c> os registos que associam mídia ao exercício com base no ID convertido e recolhe os respetivos IDs de mídia.
        ///   </item>
        ///   <item>
        ///     Utiliza os IDs recolhidos para buscar os detalhes completos da mídia na tabela <c>Media</c>.
        ///   </item>
        ///   <item>
        ///     Se a lista de mídia estiver vazia, retorna um resultado <c>NotFound</c> com uma mensagem informativa.
        ///   </item>
        ///   <item>
        ///     Em caso de sucesso, retorna a lista de mídia em formato JSON.
        ///   </item>
        /// </list>
        /// Em caso de exceção, retorna um <c>BadRequest</c> com uma mensagem de erro e os detalhes da exceção.
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
        /// Adiciona uma nova rotina de treino e associa exercícios a ela.
        /// </summary>
        /// <param name="model">
        /// O modelo que contém os dados necessários para criar a nova rotina, incluindo o nome, descrição, tipo, nível, necessidades e a lista de exercícios a associar.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso se a rotina e os exercícios forem adicionados corretamente.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados forem inválidos ou ocorrer algum erro durante o processo.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Valida o modelo recebido. Caso os dados sejam inválidos, retorna um <c>BadRequest</c> com a mensagem "Invalid data".
        ///   </item>
        ///   <item>
        ///     Converte o nível de dificuldade da rotina para o enum <see cref="RoutineLevel"/>. Se a conversão falhar, retorna um <c>BadRequest</c> com a mensagem "Invalid difficulty level".
        ///   </item>
        ///   <item>
        ///     Cria um novo objeto <see cref="Routine"/> com os dados fornecidos e o adiciona à base de dados. A rotina é definida como não customizada (<c>IsCustom = false</c>).
        ///   </item>
        ///   <item>
        ///     Se existir uma lista de exercícios no modelo, o método itera sobre cada ID de exercício:
        ///     <list type="bullet">
        ///       <item>
        ///         Procura o exercício original na base de dados.
        ///       </item>
        ///       <item>
        ///         Cria uma nova associação (<see cref="ExerciseRoutine"/>) entre o exercício e a rotina, definindo valores padrão para <c>Sets</c>, <c>Reps</c> e <c>Duration</c>.
        ///       </item>
        ///       <item>
        ///         Adiciona essa associação à base de dados e guarda as alterações para gerar o ID da associação.
        ///       </item>
        ///       <item>
        ///         Invoca o método <c>AddExerciseIdToRoutine</c> para guardar a associação do exercício à rotina.
        ///       </item>
        ///     </list>
        ///   </item>
        ///   <item>
        ///     Adiciona, em bloco, as associações de mídia relacionadas aos exercícios (se houver) e guarda as alterações na base de dados.
        ///   </item>
        ///   <item>
        ///     Retorna um <c>Ok</c> com uma mensagem de sucesso se todas as operações forem realizadas sem erros.
        ///   </item>
        ///   <item>
        ///     Em caso de exceção, retorna um <c>BadRequest</c> com uma mensagem de erro e os detalhes da exceção.
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
        /// Adiciona uma nova rotina personalizada para um utilizador específico.
        /// </summary>
        /// <param name="userId">
        /// O identificador do utilizador para o qual a rotina personalizada será criada.
        /// </param>
        /// <param name="model">
        /// O modelo que contém os dados necessários para criar a rotina, incluindo nome, descrição, tipo, nível de dificuldade e necessidades.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso e o identificador da nova rotina, se a operação for bem-sucedida.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> com uma mensagem de erro, se os dados forem inválidos ou se ocorrer alguma exceção.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Valida o modelo recebido. Caso o modelo seja inválido, retorna um BadRequest com a mensagem "Invalid data".
        ///   </item>
        ///   <item>
        ///     Converte a string do nível de dificuldade para o enum <see cref="RoutineLevel"/>. Se a conversão falhar, retorna um BadRequest com a mensagem "Invalid difficulty level".
        ///   </item>
        ///   <item>
        ///     Cria um novo objeto <see cref="Routine"/> associando-o ao utilizador identificado pelo parâmetro <c>userId</c>,
        ///     define a rotina como personalizada (<c>IsCustom = true</c>) e preenche os restantes campos com os valores do modelo.
        ///   </item>
        ///   <item>
        ///     Adiciona a nova rotina à base de dados e guarda as alterações para gerar o identificador da rotina.
        ///   </item>
        ///   <item>
        ///     Retorna um <c>Ok</c> com uma mensagem de sucesso e o identificador da nova rotina.
        ///   </item>
        ///   <item>
        ///     Em caso de exceção, retorna um <c>BadRequest</c> com a mensagem "Error adding routine" e os detalhes da exceção.
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
        /// Obtém todas as rotinas globais (não personalizadas) que não estão associadas a nenhum utilizador.
        /// </summary>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com a lista de rotinas em formato JSON, se forem encontradas rotinas globais.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> com uma mensagem informativa se não existirem rotinas ou se ocorrer algum erro.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Recolhe todas as rotinas da base de dados onde <c>IsCustom</c> é falso e <c>UserId</c> é nulo, o que indica que
        ///     são rotinas globais (não associadas a um utilizador específico).
        ///   </item>
        ///   <item>
        ///     Se a lista de rotinas estiver vazia, retorna um <c>BadRequest</c> com uma mensagem a indicar que não existem rotinas.
        ///   </item>
        ///   <item>
        ///     Caso contrário, retorna as rotinas recolhidas num resultado <c>Ok</c>.
        ///   </item>
        ///   <item>
        ///     Em caso de exceção, retorna um <c>BadRequest</c> com a mensagem "Error fetching routines" e os detalhes da exceção.
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
        /// Obtém uma lista de rotinas globais (não personalizadas) filtradas com base nos parâmetros facultativos fornecidos.
        /// </summary>
        /// <param name="name">
        /// Opcional. O nome da rotina a filtrar. A comparação é feita de forma case-insensitive.
        /// </param>
        /// <param name="type">
        /// Opcional. O tipo de rotina a filtrar. A comparação é feita de forma case-insensitive.
        /// </param>
        /// <param name="difficultyLevel">
        /// Opcional. O nível de dificuldade da rotina a filtrar. A comparação é feita convertendo o enum para string em minúsculas.
        /// </param>
        /// <param name="numberOfExercises">
        /// Opcional. O número exato de exercícios associados à rotina. Se fornecido, apenas rotinas com este número de exercícios serão retornadas.
        /// </param>
        /// <param name="equipmentNeeded">
        /// Opcional. O equipamento necessário, filtrando rotinas cujo valor em <c>Needs</c> corresponda (case-insensitive) ao fornecido.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que, em caso de sucesso, contém uma lista de rotinas filtradas com o respetivo número de exercícios associados.
        /// Caso ocorra alguma exceção, retorna um <c>BadRequest</c> com a mensagem de erro e detalhes da exceção.
        /// </returns>
        /// <remarks>
        /// O método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Inicializa uma query para obter as rotinas onde <c>IsCustom</c> é falso.
        ///   </item>
        ///   <item>
        ///     Aplica filtros opcionais para o nome, tipo, nível de dificuldade e necessidades da rotina.
        ///   </item>
        ///   <item>
        ///     Seleciona para cada rotina o respetivo número de exercícios associados (contando os registos em <c>ExerciseRoutines</c>).
        ///   </item>
        ///   <item>
        ///     Se o parâmetro <c>numberOfExercises</c> for fornecido, filtra as rotinas para incluir apenas aquelas com o número exato de exercícios especificado.
        ///   </item>
        ///   <item>
        ///     Mapeia o resultado para um objeto que inclui os detalhes da rotina e o número de exercícios, e retorna essa lista em formato JSON.
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
        /// Obtém as rotinas de treino personalizadas associadas a um utilizador específico.
        /// </summary>
        /// <param name="userId">
        /// O identificador do utilizador para o qual se pretendem obter as rotinas personalizadas.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com a lista de rotinas personalizadas em formato JSON, se forem encontradas.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>NotFound</c> com uma mensagem informativa, se nenhuma rotina personalizada for encontrada para o utilizador.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> em caso de erro durante o processamento.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Filtra a tabela de rotinas (<c>Routines</c>) para obter apenas as rotinas personalizadas (<c>IsCustom == true</c>) que estejam associadas ao utilizador identificado pelo <c>userId</c>.
        ///   </item>
        ///   <item>
        ///     Se a lista resultante estiver vazia, retorna um <c>NotFound</c> com a mensagem "Couldn't find custom routines for the user".
        ///   </item>
        ///   <item>
        ///     Caso contrário, retorna as rotinas encontradas num resultado <c>Ok</c>.
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
        /// Elimina uma rotina de treino com base no seu identificador.
        /// </summary>
        /// <param name="routineId">
        /// O identificador da rotina a eliminar.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que representa:
        /// <list type="bullet">
        ///   <item><c>NoContent</c> se a eliminação for bem-sucedida;</item>
        ///   <item><c>NotFound</c> se a rotina não for encontrada;</item>
        ///   <item><c>BadRequest</c> em caso de erro durante a operação.
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
        /// Obtém a relação entre um exercício e uma rotina específica.
        /// </summary>
        /// <param name="routineId">
        /// O identificador da rotina.
        /// </param>
        /// <param name="exerciseId">
        /// O identificador do exercício.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com os detalhes da relação entre o exercício e a rotina, se encontrada.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> em caso de erro, com uma mensagem e os detalhes da exceção.
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
        /// Adiciona uma relação entre um exercício e uma rotina, associando o objeto <see cref="ExerciseRoutine"/> ao exercício especificado.
        /// </summary>
        /// <param name="exerciseId">
        /// O identificador do exercício ao qual se pretende adicionar a relação.
        /// </param>
        /// <param name="exerciseRoutine">
        /// O objeto que representa a relação a ser adicionada, contendo os dados relativos à rotina e aos parâmetros associados (ex.: sets, reps, duração).
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>NoContent</c> se a relação for adicionada com sucesso;
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se o exercício não for encontrado;
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se ocorrer algum erro durante a operação, retornando os detalhes da exceção.
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
