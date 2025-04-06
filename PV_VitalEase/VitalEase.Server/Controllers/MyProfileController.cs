using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VitalEase.Server.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace VitalEase.Server.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão do perfil do utilizador.
    /// </summary>
    public class MyProfileController : Controller
    {
        /// <summary>
        /// Contexto da base de dados VitalEaseServerContext, utilizado para aceder aos dados da aplicação.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Interface de configuração, utilizada para aceder às definições da aplicação.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="MyProfileController"/>.
        /// </summary>
        /// <param name="context">
        /// O contexto da base de dados (<see cref="VitalEaseServerContext"/>) que permite efetuar operações de acesso aos dados.
        /// </param>
        /// <param name="configuration">
        /// A interface de configuração (<see cref="IConfiguration"/>) para aceder às definições da aplicação.
        /// </param>
        public MyProfileController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Elimina a conta de um utilizador com base no email fornecido.
        /// </summary>
        /// <param name="email">
        /// O endereço de email do utilizador cuja conta deve ser eliminada.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com uma mensagem de sucesso, se a conta for eliminada com sucesso.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> se o email não for fornecido ou se ocorrer algum erro durante o processo.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>NotFound</c> se não for encontrado nenhum utilizador com o email especificado.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se o email foi fornecido; caso contrário, retorna um <c>BadRequest</c> com uma mensagem apropriada.
        ///   </item>
        ///   <item>
        ///     Utiliza <c>Uri.UnescapeDataString</c> para garantir que o email está corretamente decodificado.
        ///   </item>
        ///   <item>
        ///     Procura na base de dados um utilizador com o email fornecido, incluindo o seu perfil associado.
        ///   </item>
        ///   <item>
        ///     Se o utilizador não for encontrado, retorna um <c>NotFound</c> com a mensagem "User not found".
        ///   </item>
        ///   <item>
        ///     Envia uma notificação por email informando que a conta foi eliminada; se o envio falhar, regista a ação e retorna um erro de servidor.
        ///   </item>
        ///   <item>
        ///     Caso o utilizador possua um perfil associado, este é removido antes da remoção do próprio utilizador.
        ///   </item>
        ///   <item>
        ///     Finalmente, remove o utilizador e guarda as alterações na base de dados, retornando um <c>Ok</c> com uma mensagem de sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpDelete("api/deleteAccount/{email}")]
        public async Task<IActionResult> DeleteAccount(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                email = Uri.UnescapeDataString(email);

                var user = await _context.Users
                    .Include(u => u.Profile) // Garante que o perfil também é carregado
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var deleteAccountNotification = await
                SendEmail(
                 user.Email,
                 "VitalEase - Delete account Notification",
                 "All your data has been successfully deleted. Thank you for using our App."
               );

                if (!deleteAccountNotification)
                {
                    await LogAction("Delete Account Attempt", "Failed - Failed to send delete account notification", user.Id);
                    return StatusCode(500, new { message = "Failed to send delete account notification to email." });
                }

                if (user.Profile != null)
                {
                    _context.Profiles.Remove(user.Profile); // Remove o perfil corretamente
                }

                _context.Users.Remove(user); // Remove o utilizador
                await _context.SaveChangesAsync();

                return Ok(new { message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting account", error = ex.Message });
            }
        }

        /// <summary>
        /// Valida a palavra-passe fornecida para um determinado utilizador.
        /// </summary>
        /// <param name="request">
        /// Um objeto <see cref="PasswordValidationRequest"/> que contém o email e a palavra-passe a validar.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso se a palavra-passe for válida.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se o email ou a palavra-passe não forem fornecidos.
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se o utilizador não for encontrado.
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se a palavra-passe for inválida.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se o email e a palavra-passe foram fornecidos.
        ///   </item>
        ///   <item>
        ///     Procura o utilizador na base de dados com base no email.
        ///   </item>
        ///   <item>
        ///     Valida a palavra-passe utilizando o método <c>VerifyPassword</c> para comparar a palavra-passe fornecida com a armazenada (normalmente, utilizando hash).
        ///   </item>
        ///   <item>
        ///     Retorna um <c>Ok</c> com uma mensagem se a validação for bem-sucedida ou o respectivo erro caso contrário.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/validatePassword")]
        public async Task<IActionResult> ValidatePassword([FromBody] PasswordValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            // Retrieve the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Validate password (you might need to implement password hashing check here)
            var passwordIsValid = VerifyPassword(request.Password, user.Password);
            if (!passwordIsValid)
            {
                return Unauthorized(new { message = "Invalid password." });
            }

            return Ok(new { message = "Password is valid." });
        }

        /// <summary>
        /// Verifica se a palavra-passe fornecida corresponde ao hash da palavra-passe armazenado.
        /// </summary>
        /// <param name="password">A palavra-passe em texto simples a ser verificada.</param>
        /// <param name="hashedPassword">O hash da palavra-passe que se pretende comparar.</param>
        /// <returns>
        /// Retorna <c>true</c> se a palavra-passe, depois de ser hasheada, corresponder ao hash armazenado; caso contrário, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Nesta implementação, a verificação é efetuada comparando o resultado da função <c>HashPassword</c> com o hash armazenado.
        /// Se estiver a utilizar ASP.NET Identity, poderá recorrer à classe <c>PasswordHasher</c> para uma verificação mais robusta.
        /// </remarks>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            // Here you would implement the password hashing verification
            // If you're using ASP.NET Identity, you can use PasswordHasher

            return HashPassword(password) == hashedPassword ? true : false;
        }

        /// <summary>
        /// Obtém as informações do perfil de um utilizador com base no email fornecido.
        /// </summary>
        /// <param name="email">
        /// O endereço de email do utilizador cujo perfil se pretende obter.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com os dados do perfil (username, data de nascimento, peso, altura, género e indicação de problemas cardíacos),
        ///     se o utilizador e o perfil forem encontrados.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> se o email não for fornecido ou se ocorrer algum erro durante o processamento.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>NotFound</c> se o utilizador ou o perfil não forem encontrados.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se o email foi fornecido e o decodifica, se necessário.
        ///   </item>
        ///   <item>
        ///     Procura o utilizador na base de dados pelo email, incluindo o seu perfil associado (utilizando <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     Se o utilizador ou o perfil não forem encontrados, retorna um <c>NotFound</c> com a mensagem apropriada.
        ///   </item>
        ///   <item>
        ///     Se encontrados, mapeia os dados do perfil para um objeto anónimo e retorna-o num resultado <c>Ok</c>.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/getProfileInfo/{email}")]
        public async Task<IActionResult> GetProfileInfo(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                email = Uri.UnescapeDataString(email);

                // Busca o perfil do usuário pelo email e inclui o perfil
                var userProfile = await _context.Users
                    .Include(u => u.Profile) // <- Isso garante que o Profile seja carregado
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (userProfile == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (userProfile.Profile == null)
                {
                    return NotFound(new { message = "Profile not found" });
                }

                var data = new
                {
                    username = userProfile.Profile.Username,
                    birthDate = userProfile.Profile.Birthdate.ToString("yyyy-MM-dd"),
                    weight = userProfile.Profile.Weight,
                    height = userProfile.Profile.Height,
                    gender = userProfile.Profile.Gender,
                    hasHeartProblems = userProfile.Profile.HasHeartProblems,
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Altera a data de nascimento do perfil de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangeBirthDateViewModel"/> que contém o email do utilizador e a nova data de nascimento.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso, se a data de nascimento for alterada com êxito.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados enviados forem inválidos ou se o utilizador tiver menos de 16 anos.
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se o utilizador não for encontrado.
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se o perfil do utilizador não for encontrado.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se os dados enviados são válidos. Caso contrário, regista a tentativa de alteração e retorna um <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Procura o utilizador com base no email fornecido, incluindo o seu perfil (usando <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     Se o utilizador ou o seu perfil não forem encontrados, regista a tentativa e retorna um erro (<c>Unauthorized</c> ou <c>NotFound</c>).
        ///   </item>
        ///   <item>
        ///     Verifica se a nova data de nascimento indica que o utilizador tem pelo menos 16 anos. Se não cumprir, regista a tentativa e retorna um <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Se todas as validações forem satisfeitas, atualiza a data de nascimento no perfil, guarda as alterações e regista a operação com sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeBirthDate")]
        public async Task<IActionResult> ChangeBirthDate([FromBody] ChangeBirthDateViewModel model)
        {
            try {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Birth date change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile) 
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Birth date change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Birth date change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                if (model.BirthDate > DateTime.Today.AddYears(-16))
                {
                    await LogAction("Birth date change Attempt", "Failed - Age restriction", userProfile.Id);
                    return BadRequest(new { message = "You must be at least 16 years old to register." });
                }

                userProfile.Profile.Birthdate = model.BirthDate;
                await _context.SaveChangesAsync();

                await LogAction("Birth Date change Attempt", "Success - Birth Date changed successfully.", 0);
                return Ok(new {message = "Birth date changed successfully!!"});
            }
            catch (Exception ex) {
                return BadRequest(new { message = "Error changing birth date", error = ex.Message });
            }
        }

        /// <summary>
        /// Altera o peso registado no perfil de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangeWeightViewModel"/> que contém o email do utilizador e o novo peso.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item><c>Ok</c> com uma mensagem de sucesso, se o peso for alterado corretamente;</item>
        ///   <item><c>BadRequest</c> se os dados forem inválidos ou se ocorrer um erro durante o processamento;</item>
        ///   <item><c>Unauthorized</c> se nenhum utilizador for encontrado com o email fornecido;</item>
        ///   <item><c>NotFound</c> se o perfil do utilizador não for encontrado.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>Verifica se os dados enviados são válidos. Caso não o sejam, regista a tentativa e retorna um BadRequest.</item>
        ///   <item>Procura o utilizador na base de dados pelo email fornecido, incluindo o perfil associado.</item>
        ///   <item>Se o utilizador ou o seu perfil não forem encontrados, regista a tentativa e retorna a resposta adequada.</item>
        ///   <item>Atualiza o campo de peso no perfil com o valor fornecido e guarda as alterações na base de dados.</item>
        ///   <item>Regista a operação com sucesso e retorna uma resposta Ok com a mensagem de sucesso.</item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeWeight")]
        public async Task<IActionResult> ChangeWeight([FromBody] ChangeWeightViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Weight change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Weight change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Weight change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                userProfile.Profile.Weight = model.Weight;
                await _context.SaveChangesAsync();

                await LogAction("Weight change Attempt", "Success - Weight changed successfully.", 0);
                return Ok(new { message = "Weight changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Weight", error = ex.Message });
            }
        }

        /// <summary>
        /// Altera a altura registada no perfil de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangeHeightViewModel"/> que contém o email do utilizador e a nova altura.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso, se a altura for alterada com êxito;
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados enviados forem inválidos ou se ocorrer um erro durante o processamento;
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se nenhum utilizador for encontrado com o email fornecido;
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se o perfil do utilizador não for encontrado.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se os dados enviados são válidos. Se não forem, regista a tentativa de alteração e retorna um BadRequest.
        ///   </item>
        ///   <item>
        ///     Procura o utilizador na base de dados pelo email, incluindo o seu perfil associado (usando <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     Se o utilizador ou o perfil não forem encontrados, regista a tentativa e retorna a resposta apropriada.
        ///   </item>
        ///   <item>
        ///     Atualiza o campo de altura no perfil com o valor fornecido e guarda as alterações na base de dados.
        ///   </item>
        ///   <item>
        ///     Regista a operação com sucesso e retorna uma resposta <c>Ok</c> com a mensagem de sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeHeight")]
        public async Task<IActionResult> ChangeHeight([FromBody] ChangeHeightViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Height change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Height change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Height change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                userProfile.Profile.Height = model.Height;
                await _context.SaveChangesAsync();

                await LogAction("Height change Attempt", "Success - Height changed successfully.", 0);
                return Ok(new { message = "Height changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Height", error = ex.Message });
            }
        }

        /// <summary>
        /// Altera o género registado no perfil de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangeGenderViewModel"/> que contém o email do utilizador e o novo género.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso se o género for alterado com êxito;
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados enviados forem inválidos ou se ocorrer algum erro durante o processamento;
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se nenhum utilizador for encontrado com o email fornecido;
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se o perfil do utilizador não for encontrado.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Valida os dados enviados. Caso os dados sejam inválidos, regista a tentativa e retorna um <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Procura o utilizador na base de dados pelo email fornecido, incluindo o seu perfil associado.
        ///   </item>
        ///   <item>
        ///     Se o utilizador ou o seu perfil não forem encontrados, regista a tentativa e retorna a resposta adequada.
        ///   </item>
        ///   <item>
        ///     Atualiza o campo de género no perfil com o novo valor e guarda as alterações na base de dados.
        ///   </item>
        ///   <item>
        ///     Regista a operação com sucesso e retorna um <c>Ok</c> com a mensagem de sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeGender")]
        public async Task<IActionResult> ChangeGender([FromBody] ChangeGenderViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Gender change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Gender change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Gender change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }


                userProfile.Profile.Gender = model.Gender;
                await _context.SaveChangesAsync();

                await LogAction("Gender change Attempt", "Success - Gender changed successfully.", 0);
                return Ok(new { message = "Gender changed succesfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Gender", error = ex.Message });
            }
        }

        /// <summary>
        /// Altera o estado de "tem problemas cardíacos" registado no perfil de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangeHasHeartProblemsViewModel"/> que contém o email do utilizador e o novo valor para "HasHeartProblems".
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso se o valor for alterado com êxito;
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados forem inválidos ou se ocorrer algum erro durante o processamento;
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se nenhum utilizador for encontrado com o email fornecido;
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se o perfil do utilizador não for encontrado.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Valida os dados enviados. Se os dados forem inválidos, regista a tentativa e retorna um <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Procura o utilizador na base de dados pelo email fornecido, incluindo o seu perfil associado (usando <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     Se o utilizador ou o seu perfil não forem encontrados, regista a tentativa e retorna a resposta adequada.
        ///   </item>
        ///   <item>
        ///     Atualiza o valor de <c>HasHeartProblems</c> no perfil com o valor fornecido no modelo e guarda as alterações na base de dados.
        ///   </item>
        ///   <item>
        ///     Regista a operação com sucesso e retorna um <c>Ok</c> com a mensagem de sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeHasHeartProblems")]
        public async Task<IActionResult> ChangeHasHeartProblems([FromBody] ChangeHasHeartProblemsViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Has heart problems change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Has heart problems change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Has heart problems Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                userProfile.Profile.HasHeartProblems = model.HasHeartProblems;
                await _context.SaveChangesAsync();

                await LogAction("Has Heart Problems change Attempt", "Success - Has Heart Problems changed successfully.", 0);
                return Ok(new { message = "Has heart problems changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Has heart problems", error = ex.Message });
            }
        }

        /// <summary>
        /// Altera o nome de utilizador (username) registado no perfil de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangeUsernameViewModel"/> que contém o email do utilizador e o novo username desejado.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso se o username for alterado com êxito;
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados enviados forem inválidos ou se ocorrer algum erro durante o processamento;
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se nenhum utilizador for encontrado com o email fornecido;
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> se o perfil do utilizador não for encontrado ou se o novo username já existir.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se os dados enviados são válidos. Caso contrário, regista a tentativa e retorna um <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Procura o utilizador na base de dados pelo email fornecido, incluindo o seu perfil associado (usando <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     Se o utilizador ou o seu perfil não forem encontrados, regista a tentativa e retorna a resposta apropriada.
        ///   </item>
        ///   <item>
        ///     Verifica se o novo username já existe na base de dados. Se existir, regista a tentativa e retorna um <c>NotFound</c> com a mensagem "Username Already Exists".
        ///   </item>
        ///   <item>
        ///     Se o username for único, atualiza o campo <c>Username</c> no perfil com o novo valor, guarda as alterações e regista a operação com sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeUsername")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Username change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Username change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Username change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                var isUnique = await _context.Profiles.FirstOrDefaultAsync(u => u.Username == model.Username);

                if(isUnique != null)
                {
                    await LogAction("Username change Attempt", "Failed - Username Already Exists", userProfile.Id);
                    return NotFound(new { message = "Username Already Exists" });
                }

                userProfile.Profile.Username = model.Username;
                await _context.SaveChangesAsync();

                await LogAction("Username change Attempt", "Success - Username changed successfully.", 0);
                return Ok(new { message = "Username changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Username", error = ex.Message });
            }
        }

        /// <summary>
        /// Altera a palavra-passe de um utilizador após validar a palavra-passe antiga e enviar uma notificação por email.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangePasswordViewModel"/> que contém o email do utilizador, a palavra-passe antiga e a nova palavra-passe.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso se a palavra-passe for alterada corretamente.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados enviados forem inválidos ou se ocorrer um erro durante o processamento.
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se não for encontrado o utilizador com as credenciais fornecidas ou se a nova palavra-passe não for válida.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se o modelo recebido é válido. Caso não o seja, regista a tentativa e retorna um BadRequest.
        ///   </item>
        ///   <item>
        ///     Hasheia a palavra-passe antiga e procura o utilizador com base no email e no hash da palavra-passe antiga.
        ///   </item>
        ///   <item>
        ///     Se o utilizador não for encontrado, regista a tentativa e retorna um Unauthorized.
        ///   </item>
        ///   <item>
        ///     Verifica se a nova palavra-passe cumpre os critérios de validade. Se não for válida, regista a tentativa e retorna um Unauthorized.
        ///   </item>
        ///   <item>
        ///     Envia uma notificação por email ao utilizador informando que a palavra-passe foi alterada. Se o envio falhar, regista a tentativa e retorna um erro de servidor.
        ///   </item>
        ///   <item>
        ///     Gera o hash da nova palavra-passe e verifica se é diferente do hash da palavra-passe antiga. Se forem iguais, regista a tentativa e retorna um Unauthorized.
        ///   </item>
        ///   <item>
        ///     Atualiza a palavra-passe do utilizador com o novo hash, guarda as alterações na base de dados e regista a operação com sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Password change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var hashedOldPassword = HashPassword(model.OldPassword);
                var user = await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedOldPassword);

                if (user == null)
                {
                    await LogAction("Password change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (!IsPasswordValid(model.NewPassword))
                {
                    await LogAction("Password change Attempt", "Failed - Password is not valid", user.Id);
                    return Unauthorized(new { message = "Password is not valid" });
                }

                var passwordChangeNotification = await
                 SendEmail(
                  user.Email,
                  "VitalEase - Email Password Change",
                  "The Password has been updated successfully."
                );

                if (!passwordChangeNotification)
                {
                    await LogAction("Password change Attempt", "Failed - Failed to send password change notification", user.Id);
                    return StatusCode(500, new { message = "Failed to send password change notification to email." });
                }

                var newHashedPassword = HashPassword(model.NewPassword);

                if(hashedOldPassword == newHashedPassword)
                {
                    await LogAction("Password change Attempt", "Failed - Password can not be the same as old one.", 0);
                    return Unauthorized(new { message = "Password can not be the same as old one." });
                }


                user.Password = newHashedPassword;
                await _context.SaveChangesAsync();

               
                await LogAction("Password change Attempt", "Success - Password changed successfully.", 0);
                return Ok(new { message = "Password changed successfully!!" });
            }
            catch (Exception ex)
            {
                await LogAction("Password change Attempt", "Failed - Error changing Password.", 0);
                return BadRequest(new { message = "Error changing Password", error = ex.Message });
            }
        }

        /// <summary>
        /// Processa o pedido de alteração de email de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ChangeEmailViewModel"/> que contém o email atual, a nova email e a palavra-passe do utilizador para verificação.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso se o pedido for processado e os emails de verificação forem enviados com êxito.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se os dados enviados forem inválidos ou se ocorrer um erro durante o processamento.
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> se o utilizador não for encontrado ou se a palavra-passe estiver incorreta, ou se o novo email já existir.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// O método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Verifica se os dados enviados são válidos; se não forem, regista a tentativa de alteração e retorna um BadRequest.
        ///   </item>
        ///   <item>
        ///     Hasheia a palavra-passe fornecida e procura um utilizador na base de dados cujo email e palavra-passe correspondam.
        ///   </item>
        ///   <item>
        ///     Se o utilizador não for encontrado, regista a tentativa e retorna um Unauthorized.
        ///   </item>
        ///   <item>
        ///     Verifica se o novo email já existe na base de dados; se existir, regista a tentativa e retorna um Unauthorized com a mensagem "Email already exists".
        ///   </item>
        ///   <item>
        ///     Gera um token para verificação de alteração de email, usando o email atual, o novo email e o ID do utilizador.
        ///   </item>
        ///   <item>
        ///     Cria dois links de verificação: um para o novo email e outro para o email antigo, escapando o token para integridade do URL.
        ///   </item>
        ///   <item>
        ///     Envia os links de verificação por email para ambos os endereços. Se o envio falhar para qualquer um, regista a tentativa e retorna um erro de servidor.
        ///   </item>
        ///   <item>
        ///     Regista a operação como bem-sucedida e retorna um Ok com uma mensagem de sucesso.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Email change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var hashedPassword = HashPassword(model.Password);

                var user = await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword);


                if (user == null)
                {
                    await LogAction("Email change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                var emailAlreadyExists = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.NewEmail);

                if (emailAlreadyExists != null)
                {
                    await LogAction("Email change Attempt", "Failed - Email already exists", user.Id);
                    return Unauthorized(new { message = "Email already exists" });
                }

                var token = GenerateToken(user.Email, model.NewEmail, user.Id);

                var changeEmailLink = $"https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/confirmNewEmail?token={Uri.EscapeDataString(token)}";

                var emailSent = await SendEmailResetLink(model.NewEmail, changeEmailLink);
                if (!emailSent)
                {
                    await LogAction("Email change Attempt", "Failed - Failed to send change email to new email", user.Id);
                    return StatusCode(500, new { message = "Failed to send change email to new email." });
                }

                var changeOldEmailLink = $"https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/confirmOldEmail?token={Uri.EscapeDataString(token)}";

                var emailSentOldEmail = await SendEmailResetLink(user.Email, changeOldEmailLink);
                if (!emailSentOldEmail)
                {
                    await LogAction("Email change Attempt", "Failed - Failed to send change email to old email", user.Id);
                    return StatusCode(500, new { message = "Failed to send change email to old email." });
                }


                await LogAction("Email change Attempt", "Success - Change emails are sent to old and new email's.", user.Id);
                return Ok(new { message = "Email verification has been sent to your old email and new email"});
            }
            catch (Exception ex)
            {
                await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                return BadRequest(new { message = "Error changing Email", error = ex.Message });
            }
        }

        /// <summary>
        /// Valida o token para confirmação do novo email e, se for válido, confirma a alteração.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ConfirmNewEmailViewModel"/> que contém o token enviado na query string.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> com uma mensagem de sucesso e o novo email, se o token for válido.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> se o token estiver expirado ou for inválido, registar o erro e retornar uma mensagem apropriada.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Este método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Valida o token utilizando o método <c>ValidateToken</c>, que retorna uma tupla contendo a validade, o email antigo, o novo email, o ID do utilizador e o tokenId.
        ///   </item>
        ///   <item>
        ///     Se o token não for válido, regista a tentativa de alteração e retorna um <c>BadRequest</c> com a mensagem "Token expired."
        ///   </item>
        ///   <item>
        ///     Se o token for válido, guarda as alterações (se houver) na base de dados e retorna um <c>Ok</c> com uma mensagem de sucesso e o novo email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ValidateNewEmailToken")]
        public async Task<IActionResult>ConfirmNewEmailToken([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Confirms the email change request by validating the provided token, updating the user's email, and marking the reset token as used.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmNewEmailViewModel"/> containing the token sent in the query string.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the token is valid and the email change is confirmed.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is invalid/expired, if the user is not found, if the reset token is already used, or if the email has already been verified.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the token using the <c>ValidateToken</c> method, which returns a tuple containing the validity status, the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is invalid, it logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the user based on the user ID extracted from the token. If the user is not found, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Checks the reset email token in the database using the token ID to ensure it has not been used yet. If it has been used, it logs the attempt and returns a <c>BadRequest</c> with the message "Email already verified."
        ///   </item>
        ///   <item>
        ///     If the reset token is valid and marked as used on the old email, the method updates the user's email to the new email, logs the successful operation, sends a success notification email, and returns an <c>Ok</c> response.
        ///   </item>
        ///   <item>
        ///     If the conditions for the reset token are not fully met, it still saves any pending changes and returns an <c>Ok</c> with a message indicating that the token is valid along with the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ConfirmNewEmailChange")]
        public async Task<IActionResult> ConfirmNewEmailChange([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);


            if (user == null)
            {
                await LogAction("Email change Attempt", "Failed - User not found.", 0);
                return BadRequest(new { message = "User not found" });
            }

            var resetEmailToken = await _context.ResetEmailTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

            if (resetEmailToken != null && resetEmailToken.IsUsed != true)
            {

                resetEmailToken.IsUsed = true;
            }
            else
            {
                await LogAction("Email change Attempt", "Failed - Email already verified.", userId);
                return BadRequest(new { message = "Email already verified." });
            }

            if (resetEmailToken.IsUsed == true && resetEmailToken.IsUsedOnOldEmail == true)
            {
                await LogAction("Email change Attempt", "Success - Email changed successfully.", userId);
                user.Email = newEmail;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var successNotification = await
                SendEmail(
                    newEmail,
                    "VitalEase - Email Change Success",
                    "The email change has been completed successfully."
                );

                if (!successNotification)
                {
                    return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
                }

                return Ok(new { message = "Email changed successfully.", newEmail });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Cancels the new email change request by removing the associated reset email token 
        /// and sending a cancellation notification to the user's old email address.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmNewEmailViewModel"/> containing the token used for validating 
        /// the email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the cancellation is successful.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is expired or if there is an error sending the cancellation notification.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the token using the <c>ValidateToken</c> method, which returns a tuple containing the validity,
        ///     the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the reset email token from the database using the token ID, and if found, removes it.
        ///   </item>
        ///   <item>
        ///     Sends a cancellation notification email to the old email address.
        ///   </item>
        ///   <item>
        ///     Logs the cancellation of the email change attempt, saves changes to the database,
        ///     and returns an <c>Ok</c> response with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/CancelNewEmailChange")]
        public async Task<IActionResult> CancelNewEmailChange([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var resetEmailToken = await _context.ResetEmailTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

            if (resetEmailToken != null)
            {
                 _context.ResetEmailTokens.Remove(resetEmailToken);
            }


            var cancelNotification = await 
                SendEmail(
                    oldEmail,
                    "VitalEase - Email Change Cancelation",
                    "The email change has been canceled."
                );
            
            if (!cancelNotification)
            {
                return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
            }

            await LogAction("Email change Attempt", "Change email successfully canceled.", userId);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Change email successfully canceled.", newEmail });
        }

        /// <summary>
        /// Validates the token for confirming the old email change and returns a success message if the token is valid.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmOldEmailViewModel"/> containing the token used for validating the old email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the token is valid.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> with the message "Token expired." if the token is invalid or expired.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the provided token using the <c>ValidateTokenOnOldEmail</c> method, which returns a tuple containing the validity status,
        ///     the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, the method logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     If the token is valid, any pending changes are saved to the database and an <c>Ok</c> response is returned with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ValidateOldEmailToken")]
        public async Task<IActionResult> ConfirmOldEmailToken([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail,userId, tokenId) = ValidateTokenOnOldEmail(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

          
            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Confirms the change of the old email as part of the email change process.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmOldEmailViewModel"/> containing the token used to validate the old email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the token is valid, the user is found, and the reset token has been properly marked as used.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is expired, invalid, the user is not found, or if the reset token indicates that the email has already been verified.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This endpoint performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the provided token using the <c>ValidateTokenOnOldEmail</c> method, which returns a tuple containing the validity status, old email, new email, user ID, and token ID.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the user by the user ID extracted from the token. If no user is found, logs the attempt and returns a <c>BadRequest</c> with the message "User not found."
        ///   </item>
        ///   <item>
        ///     Checks the reset email token associated with the token ID. If the token is not marked as used on the old email, marks it as used; otherwise, logs the attempt and returns a <c>BadRequest</c> with the message "Email already verified."
        ///   </item>
        ///   <item>
        ///     If the reset email token is marked as used (both overall and on the old email), updates the user's email to the new email, logs the successful email change, sends a success notification email to the new email, and returns an <c>Ok</c> response.
        ///   </item>
        ///   <item>
        ///     If the above conditions are not fully met, it saves any pending changes and returns an <c>Ok</c> response indicating that the token is valid along with the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ConfirmOldEmailToken")]
        public async Task<IActionResult> ConfirmOldEmailChange([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateTokenOnOldEmail(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                await LogAction("Email change Attempt", "Failed - User not found.", 0);
                return BadRequest(new { message = "User not found." });
            }

            var resetEmailToken = _context.ResetEmailTokens.FirstOrDefault(u => u.TokenId == tokenId);

            if (resetEmailToken != null && resetEmailToken.IsUsedOnOldEmail != true)
            {

                resetEmailToken.IsUsedOnOldEmail = true;
            }
            else
            {
                await LogAction("Email change Attempt", "Failed - Email already verified.", userId);
                return BadRequest(new { message = "Email already verified." });
            }

            if (resetEmailToken.IsUsed == true && resetEmailToken.IsUsedOnOldEmail == true)
            {
                await LogAction("Email change Attempt", "Success - Email changed successfully.", userId);
                user.Email = newEmail;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var successNotification = await
                SendEmail(
                    newEmail,
                    "VitalEase - Email Change Success",
                    "The email change has been completed successfully."
                );

                if (!successNotification)
                {
                    return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
                }

                return Ok(new { message = "Email changed successfully.", newEmail });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Cancels the old email change process by removing the associated reset email token 
        /// and sending a cancellation notification to the user's old email address.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmOldEmailViewModel"/> containing the token used for validating the old email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the cancellation is completed successfully.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is expired or if an error occurs during the process.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This endpoint performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the provided token using the <c>ValidateToken</c> method, which returns a tuple containing 
        ///     the validity status, the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is invalid, logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the reset email token from the database using the token ID and removes it if found.
        ///   </item>
        ///   <item>
        ///     Sends a cancellation notification email to the old email address.
        ///   </item>
        ///   <item>
        ///     Logs the cancellation attempt, saves any pending changes to the database, 
        ///     and returns an <c>Ok</c> response with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/CancelOldEmailChange")]
        public async Task<IActionResult> CancelOldEmailChange([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail,userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var resetEmailToken = await _context.ResetEmailTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

            if (resetEmailToken != null)
            {
                _context.ResetEmailTokens.Remove(resetEmailToken);
            }

            var cancelNotification = await
                SendEmail(
                    oldEmail,
                    "VitalEase - Email Change Cancelation",
                    "The email change has been canceled."
                );

            if (!cancelNotification)
            {
                return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
            }

            await LogAction("Email change Attempt", "Change email successfully canceled.", userId);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Change email successfully canceled.", newEmail });
        }

        /// <summary>
        /// Sends an email with the specified subject and body to the given email address.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The HTML-formatted body of the email.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean value:
        /// <c>true</c> if the email was sent successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves email settings from the configuration, including the sender's email, SMTP server details, and credentials.
        /// It first validates that the sender's email is properly configured and in a valid format, as well as ensuring that the SMTP port is a valid number.
        /// It then constructs a <see cref="MailMessage"/> with the provided subject and body, and attempts to send it using an <see cref="SmtpClient"/> configured for SSL.
        /// Any exceptions encountered during the sending process are caught, logged to the console for debugging purposes, and the method returns <c>false</c>.
        /// </remarks>
        private async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

                // Verificar se o fromEmail está configurado corretamente
                if (string.IsNullOrEmpty(fromEmail) || !IsValidEmail(fromEmail))
                {
                    throw new Exception("From email is not valid or not configured.");
                }

                // Verificar se o smtpPort é um número válido
                if (!int.TryParse(smtpPortString, out var smtpPort))
                {
                    throw new Exception("SMTP port is not a valid number.");
                }

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail), // Definindo o endereço de 'from' corretamente
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail); // Adicionando o destinatário do e-mail

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true; // Garantir que o envio seja seguro via SSL
                    await client.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Para fins de depuração, vamos logar a mensagem de erro
                Console.WriteLine($"Error occurred while sending email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends an email containing a reset link to confirm an email change request.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="emailLink">The URL that the recipient must click to confirm the email change.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning <c>true</c> if the email was sent successfully,
        /// or <c>false</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// This method retrieves the necessary email configuration settings (e.g., sender's email, SMTP server details, port, and credentials)
        /// from the configuration. It first validates that the sender's email is properly configured and is in a valid format,
        /// and that the SMTP port is a valid number. It then constructs an HTML email with the subject "Change email request"
        /// and an HTML anchor tag embedding the provided <paramref name="emailLink"/>. The email is sent using an SMTP client
        /// configured to use SSL for secure transmission. Any exceptions encountered during the email sending process are logged,
        /// and the method returns <c>false</c>.
        /// </remarks>
        private async Task<bool> SendEmailResetLink(string toEmail, string emailLink)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

                // Verificar se o fromEmail está configurado corretamente
                if (string.IsNullOrEmpty(fromEmail) || !IsValidEmail(fromEmail))
                {
                    throw new Exception("From email is not valid or not configured.");
                }

                // Verificar se o smtpPort é um número válido
                if (!int.TryParse(smtpPortString, out var smtpPort))
                {
                    throw new Exception("SMTP port is not a valid number.");
                }

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail), // Definindo o endereço de 'from' corretamente
                    Subject = "Change email request",
                    Body = $"Click the following link to confirm your email change: <a href='{emailLink}'>Change email link</a>",
                    IsBodyHtml = true
                };

                message.To.Add(toEmail); // Adicionando o destinatário do e-mail

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true; // Garantir que o envio seja seguro via SSL
                    await client.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Para fins de depuração, vamos logar a mensagem de erro
                Console.WriteLine($"Error occurred while sending email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified email address is valid.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>
        /// <c>true</c> if the email address is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to create a new instance of <see cref="System.Net.Mail.MailAddress"/> using the provided email.
        /// If the instantiation succeeds and the constructed address matches the input, the email is considered valid.
        /// If an exception occurs, the method catches it and returns <c>false</c>.
        /// </remarks>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Regista uma acção de auditoria, gravando-a na base de dados.
        /// </summary>
        /// <param name="action">A acção que foi realizada.</param>
        /// <param name="status">O estado ou resultado da acção.</param>
        /// <param name="UserId">O identificador do utilizador associado à acção.</param>
        /// <returns>
        /// Uma <see cref="Task"/> que representa a operação assíncrona de registo do log.
        /// </returns>
        /// <remarks>
        /// Este método cria um objeto <see cref="AuditLog"/> com a hora actual, a acção, o estado e o ID do utilizador,
        /// adiciona-o ao contexto da base de dados e guarda as alterações de forma assíncrona.
        /// </remarks>
        private async Task LogAction(string action, string status, int UserId)
        {
            var log = new AuditLog
            {
                Timestamp = DateTime.Now,
                Action = action,
                Status = status,
                UserId = UserId
            };

            // Salvar o log no banco de dados
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates a password based on predefined complexity requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>
        /// <c>true</c> if the password meets all the requirements; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The password is considered valid if it meets all of the following criteria:
        /// <list type="bullet">
        ///   <item>It must be at least 12 characters long.</item>
        ///   <item>It must contain at least one lowercase letter.</item>
        ///   <item>It must contain at least one uppercase letter.</item>
        ///   <item>It must contain at least one special character from the set: !@#$%^&*(),.?":{}|<> (including a space).</item>
        /// </list>
        /// </remarks>
        private bool IsPasswordValid(string password)
        {
            // Verificar se a senha tem pelo menos 12 caracteres
            if (password.Length < 12)
            {
                return false;
            }

            // Verificar se a senha contém pelo menos uma letra minúscula
            if (!password.Any(char.IsLower))
            {
                return false;
            }

            // Verificar se a senha contém pelo menos uma letra maiúscula
            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            // Verificar se a senha contém pelo menos um caractere especial
            var specialChars = "!@#$%^&*(),.?\":{}|<> ";
            if (!password.Any(c => specialChars.Contains(c)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes the SHA256 hash of a given password and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>
        /// A hexadecimal string representation of the SHA256 hash of the provided password.
        /// </returns>
        /// <remarks>
        /// This method converts the input password into a byte array using UTF8 encoding, computes its SHA256 hash,
        /// and then constructs a hexadecimal string from the resulting byte array.
        /// </remarks>
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Converte a senha para um array de bytes e gera o hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Converte o hash para uma string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Gera um token JWT para a alteração de email, incorporando o email antigo, o novo email, o ID do utilizador e um identificador único para o token.
        /// </summary>
        /// <param name="oldEmail">O email atual do utilizador.</param>
        /// <param name="newEmail">O novo email a ser configurado para o utilizador.</param>
        /// <param name="userId">O identificador do utilizador.</param>
        /// <returns>
        /// Uma string que representa o token JWT gerado, com validade de 60 minutos.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// É lançada se a chave JWT não estiver configurada corretamente na aplicação.
        /// </exception>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Obtém a chave JWT a partir das configurações da aplicação e valida a sua existência.
        ///   </item>
        ///   <item>
        ///     Gera um identificador único (tokenId) para o token.
        ///   </item>
        ///   <item>
        ///     Cria um token JWT com os seguintes claims:
        ///     <list type="bullet">
        ///       <item>O email antigo.</item>
        ///       <item>O novo email.</item>
        ///       <item>O identificador do utilizador.</item>
        ///       <item>O tokenId.</item>
        ///     </list>
        ///   </item>
        ///   <item>
        ///     Define a expiração do token para 60 minutos a partir da criação, utilizando uma chave simétrica e o algoritmo HMAC SHA256 para assinatura.
        ///   </item>
        ///   <item>
        ///     Cria um registo de token em <see cref="ResetEmailTokens"/> com o tokenId, data de criação, data de expiração, e flags de utilização inicialmente definidas como <c>false</c>.
        ///   </item>
        ///   <item>
        ///     Adiciona o registo à base de dados, guarda as alterações e retorna o token JWT gerado.
        ///   </item>
        /// </list>
        /// </remarks>
        public string GenerateToken(string oldEmail, string newEmail, int userId)
        {

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }

            var tokenId = Guid.NewGuid().ToString();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.Now.AddMinutes(60);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Email, oldEmail),
                    new Claim(ClaimTypes.Email, newEmail),
                    new Claim("userId", userId.ToString()),
                    new Claim("tokenId", tokenId)
                },
                expires: expiresAt, // Define o tempo de expiração do token (1 hora, por exemplo)
                signingCredentials: creds
            );

            var resetEmailToken = new ResetEmailTokens
            {
                TokenId = tokenId,         // Usando o tokenId gerado
                CreatedAt = DateTime.Now,
                ExpiresAt = expiresAt,
                IsUsed = false,
                IsUsedOnOldEmail = false,
            };

            _context.ResetEmailTokens.Add(resetEmailToken);
            _context.SaveChanges();

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valida o token JWT.
        /// </summary>
        /// <param name="token">Token a ser validado.</param>
        /// <returns>Uma tupla com: se é válido, email e userId.</returns>
        private (bool IsValid, string oldEmail, string newEmail, int UserId, string idToken) ValidateToken(string token)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Tente validar o token com os parâmetros necessários.
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validar a chave de assinatura
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Usar a chave para validação
                    ValidateIssuer = true, // Validar o emissor
                    ValidateAudience = true, // Validar o público
                    ValidIssuer = _configuration["Jwt:Issuer"], // Emissor configurado
                    ValidAudience = _configuration["Jwt:Audience"], // Público configurado
                    ClockSkew = TimeSpan.FromMinutes(5) // Permitir uma margem de 5 minutos para a expiração
                }, out _);

                // Obter o email e userId do token
                var oldEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[0].Value;
                var newEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[1].Value;
                var userIdClaim = claimsPrincipal.FindFirst("userId");
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var tokenIdClaim = claimsPrincipal.FindFirst("tokenId");
                var tokenId = tokenIdClaim != null ? tokenIdClaim.Value : "";
                
                // Validar se o email e userId são válidos
                if (string.IsNullOrEmpty(oldEmail) || string.IsNullOrEmpty(newEmail) || userId == 0)
                {
                    return (false, "", "", 0,tokenId);
                }
                var tokenRecord = _context.ResetEmailTokens
              .FirstOrDefault(l => l.TokenId == tokenId);

                if (tokenRecord == null)
                {
                    // Token não encontrado na base de dados
                    return (false, oldEmail, newEmail, userId,tokenId);
                }

                if (tokenRecord.IsUsed)
                {
                    // Se o token já foi usado, retorna falso
                    return (false, oldEmail, newEmail, userId,tokenId);
                }

                // Se chegou até aqui, o token é válido
                return (true, oldEmail, newEmail, userId,tokenId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", "", 0,"");
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", "", 0, "");
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", "", 0, "");
            }
        }

        /// <summary>
        /// Valida o token JWT.
        /// </summary>
        /// <param name="token">Token a ser validado.</param>
        /// <returns>Uma tupla com: se é válido, email e userId.</returns>
        private (bool IsValid, string oldEmail, string newEmail ,int UserId, string idToken) ValidateTokenOnOldEmail(string token)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Tente validar o token com os parâmetros necessários.
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validar a chave de assinatura
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Usar a chave para validação
                    ValidateIssuer = true, // Validar o emissor
                    ValidateAudience = true, // Validar o público
                    ValidIssuer = _configuration["Jwt:Issuer"], // Emissor configurado
                    ValidAudience = _configuration["Jwt:Audience"], // Público configurado
                    ClockSkew = TimeSpan.FromMinutes(5) // Permitir uma margem de 5 minutos para a expiração
                }, out _);

                // Obter o email e userId do token
                var oldEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[0].Value;
                var newEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[1].Value;
                var userIdClaim = claimsPrincipal.FindFirst("userId");
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var tokenIdClaim = claimsPrincipal.FindFirst("tokenId");
                var tokenId = tokenIdClaim != null ? tokenIdClaim.Value : "";

                // Validar se o email e userId são válidos
                if (string.IsNullOrEmpty(oldEmail) || string.IsNullOrEmpty(newEmail) || userId == 0)
                {
                    return (false, "", "", 0, tokenId);
                }
                var tokenRecord = _context.ResetEmailTokens
              .FirstOrDefault(l => l.TokenId == tokenId);

                if (tokenRecord == null)
                {
                    // Token não encontrado na base de dados
                    return (false, oldEmail, newEmail, userId, tokenId);
                }

                if (tokenRecord.IsUsedOnOldEmail)
                {
                    // Se o token já foi usado, retorna falso
                    return (false, oldEmail, newEmail, userId, tokenId);
                }

                // Se chegou até aqui, o token é válido
                return (true, oldEmail, newEmail, userId, tokenId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", "", 0, "");
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", "", 0, "");
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", "", 0, "");
            }
        }
    }
}