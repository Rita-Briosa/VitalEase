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

namespace VitalEase.Server.Controllers
{
    public class MyProfileController : Controller
    {
        private readonly VitalEaseServerContext _context;
        private readonly IConfiguration _configuration;
        public MyProfileController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

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
                    await LogAction("Password change Attempt", "Failed - Password is not valid", 0);
                    return Unauthorized(new { message = "Password is not valid" });
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

                var token = GenerateToken(model.NewEmail, user.Id);

                var changeEmailLink = $"https://localhost:4200/confirmNewEmail?token={Uri.EscapeDataString(token)}";

                var emailSent = await SendEmailResetLink(model.NewEmail, changeEmailLink);
                if (!emailSent)
                {
                    await LogAction("Email change Attempt", "Failed - Failed to send change email to new email", user.Id);
                    return StatusCode(500, new { message = "Failed to send change email to new email." });
                }

                var changeOldEmailLink = $"https://localhost:4200/confirmOldEmail?token={Uri.EscapeDataString(token)}";

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

        [HttpGet("api/ValidateNewEmailToken")]
        public async Task<IActionResult>ConfirmNewEmailToken([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", email });
        }

        [HttpGet("api/ConfirmNewEmailChange")]
        public async Task<IActionResult> ConfirmNewEmailChange([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateToken(model.Token);

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
                user.Email = email;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Email changed successfully.", email });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", email });
        }


        [HttpGet("api/CancelNewEmailChange")]
        public async Task<IActionResult> CancelNewEmailChange([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateToken(model.Token);

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

            await LogAction("Email change Attempt", "Change email successfully canceled.", userId);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Change email successfully canceled.", email });
        }

        [HttpGet("api/ValidateOldEmailToken")]
        public async Task<IActionResult> ConfirmOldEmailToken([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateTokenOnOldEmail(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

          
            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", email });
        }

        [HttpGet("api/ConfirmOldEmailToken")]
        public async Task<IActionResult> ConfirmOldEmailChange([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateTokenOnOldEmail(model.Token);

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
                user.Email = email;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Email changed successfully.", email });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", email });
        }

        [HttpGet("api/CancelOldEmailChange")]
        public async Task<IActionResult> CancelOldEmailChange([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateToken(model.Token);

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

            await LogAction("Email change Attempt", "Change email successfully canceled.", userId);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Change email successfully canceled.", email });
        }

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

        public string GenerateToken(string email, int userId)
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
                    new Claim(ClaimTypes.Email, email),
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
        private (bool IsValid, string Email, int UserId, string idToken) ValidateToken(string token)
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
                var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
                var userIdClaim = claimsPrincipal.FindFirst("userId");
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var tokenIdClaim = claimsPrincipal.FindFirst("tokenId");
                var tokenId = tokenIdClaim != null ? tokenIdClaim.Value : "";
                
                // Validar se o email e userId são válidos
                if (string.IsNullOrEmpty(email) || userId == 0)
                {
                    return (false, "", 0,tokenId);
                }
                var tokenRecord = _context.ResetEmailTokens
              .FirstOrDefault(l => l.TokenId == tokenId);

                if (tokenRecord == null)
                {
                    // Token não encontrado na base de dados
                    return (false, email, userId,tokenId);
                }

                if (tokenRecord.IsUsed)
                {
                    // Se o token já foi usado, retorna falso
                    return (false, email, userId,tokenId);
                }

                // Se chegou até aqui, o token é válido
                return (true, email, userId,tokenId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", 0,"");
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", 0,"");
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", 0,"");
            }
        }

        /// <summary>
        /// Valida o token JWT.
        /// </summary>
        /// <param name="token">Token a ser validado.</param>
        /// <returns>Uma tupla com: se é válido, email e userId.</returns>
        private (bool IsValid, string Email, int UserId, string idToken) ValidateTokenOnOldEmail(string token)
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
                var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
                var userIdClaim = claimsPrincipal.FindFirst("userId");
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var tokenIdClaim = claimsPrincipal.FindFirst("tokenId");
                var tokenId = tokenIdClaim != null ? tokenIdClaim.Value : "";

                // Validar se o email e userId são válidos
                if (string.IsNullOrEmpty(email) || userId == 0)
                {
                    return (false, "", 0, tokenId);
                }
                var tokenRecord = _context.ResetEmailTokens
              .FirstOrDefault(l => l.TokenId == tokenId);

                if (tokenRecord == null)
                {
                    // Token não encontrado na base de dados
                    return (false, email, userId, tokenId);
                }

                if (tokenRecord.IsUsedOnOldEmail)
                {
                    // Se o token já foi usado, retorna falso
                    return (false, email, userId, tokenId);
                }

                // Se chegou até aqui, o token é válido
                return (true, email, userId, tokenId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", 0, "");
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", 0, "");
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", 0, "");
            }
        }
    }
}