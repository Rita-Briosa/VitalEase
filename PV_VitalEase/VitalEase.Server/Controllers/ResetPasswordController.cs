using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;
using System.Security.Cryptography;
using VitalEase.Server.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
using Microsoft.CodeAnalysis.Scripting;
using Org.BouncyCastle.Crypto.Generators;

namespace VitalEase.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling password reset operations.
    /// </summary>
    public class ResetPasswordController : Controller
    {
        /// <summary>
        /// Gets the database context used to interact with the application's data.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Gets the application configuration that provides access to settings such as JWT secrets, email settings, etc.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetPasswordController"/> class.
        /// </summary>
        /// <param name="context">
        /// The database context (<see cref="VitalEaseServerContext"/>) used for accessing and managing application data.
        /// </param>
        /// <param name="configuration">
        /// The configuration interface (<see cref="IConfiguration"/>) used to retrieve application settings.
        /// </param>
        public ResetPasswordController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Processa o pedido de redefinição de palavra-passe de um utilizador.
        /// </summary>
        /// <param name="model">
        /// Um objeto do tipo <see cref="ResetPasswordViewModel"/> que contém o token de redefinição e a nova palavra-passe.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica o resultado da operação de redefinição de palavra-passe. 
        /// Poderá retornar:
        /// <list type="bullet">
        ///   <item><c>Ok</c> – se a palavra-passe for redefinida com sucesso;</item>
        ///   <item><c>BadRequest</c> – se os parâmetros forem inválidos, se o token for inválido ou expirado, se a nova palavra-passe for fraca ou igual à antiga;</item>
        ///   <item><c>NotFound</c> – se o utilizador não for encontrado;</item>
        ///   <item><c>StatusCode(500)</c> – em caso de erro ao atualizar a base de dados.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Este método executa as seguintes operações:
        /// <list type="bullet">
        ///   <item>Verifica se o modelo e os parâmetros obrigatórios (token e nova palavra-passe) foram fornecidos; caso contrário, regista a acção e retorna um BadRequest.</item>
        ///   <item>Valida o token utilizando o método <c>ValidateToken</c>, que retorna a validade do token, o email do utilizador, o userId e o tokenId.</item>
        ///   <item>Se o token não for válido, regista a acção e retorna um BadRequest com a mensagem "Invalid or expired token."</item>
        ///   <item>Procura o utilizador na base de dados com base no email extraído do token. Se o utilizador não for encontrado, retorna um NotFound.</item>
        ///   <item>Verifica se a nova palavra-passe cumpre os critérios de complexidade através do método <c>IsPasswordValid</c>.</item>
        ///   <item>Gera o hash da nova palavra-passe e assegura que este difere do hash da palavra-passe atual; se forem iguais, regista a acção e retorna um BadRequest.</item>
        ///   <item>Atualiza a palavra-passe do utilizador, regista a data de alteração da palavra-passe e invalida o token de sessão atual.</item>
        ///   <item>Marca o token de redefinição como utilizado, se o tokenId for válido.</item>
        ///   <item>Envia uma notificação de alerta de redefinição de palavra-passe para o email do utilizador.</item>
        ///   <item>Tenta guardar as alterações na base de dados; se a operação for bem-sucedida, regista a acção com sucesso e retorna um Ok com uma mensagem de sucesso.
        ///       Em caso de erro, regista a acção como falhada e retorna um StatusCode(500) com os detalhes do erro.</item>
        /// </list>
        /// </remarks>
        [HttpPost("api/resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            // Validate request model
            if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
            {
                await LogAction("Password reset attempt", "Failed - Invalid data", 0);
                return BadRequest(new { message = "Invalid parameters. Token and Password are required." });
            }

            // Validate token
            var (isValid, email, userId, tokenId) = ValidateToken(model.Token);
            if (!isValid)
            {
                await LogAction("Password reset attempt", "Failed - Invalid or expired token", 0);
                return BadRequest(new { message = "Invalid or expired token." });
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Validate new password
            if (!IsPasswordValid(model.NewPassword))
            {
                await LogAction("Password reset attempt", "Failed - Weak password", user.Id);
                return BadRequest(new { message = "Password does not meet the required criteria." });
            }

            // Hash the new password
            var hashedNewPassword = HashPassword(model.NewPassword);

            // Ensure new password is different
            if (user.Password == hashedNewPassword)
            {
                await LogAction("Password reset attempt", "Failed - Same as old password", user.Id);
                return BadRequest(new { message = "New password cannot be the same as the old one." });
            }

            // Update password and invalidate session
            user.Password = hashedNewPassword;
            user.PasswordLastChanged = DateTime.UtcNow;
            user.SessionToken = null; // Invalidate session token

            // Mark token as used
            if (!string.IsNullOrEmpty(tokenId))
            {
                var tokenRecord = await _context.ResetPasswordTokens.FirstOrDefaultAsync(t => t.TokenId == tokenId);
                if (tokenRecord != null)
                {
                    tokenRecord.IsUsed = true;
                }
            }

            // Save changes
            try
            {
                await SendPasswordResetEmailWarning(email);
                await _context.SaveChangesAsync();
                await LogAction("Password reset attempt", "Success", user.Id);
                return Ok(new { message = "Password reset successfully. Please log in again." });
            }
            catch (Exception ex)
            {
                await LogAction("Password reset attempt", "Failed - Database error", user.Id);
                return StatusCode(500, new { message = "An error occurred while updating the password.", details = ex.Message });
            }
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
                var tokenRecord = _context.ResetPasswordTokens
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
        /// Método para validar se a senha atende aos critérios de segurança.
        /// </summary>
        /// <param name="password">Senha a ser validada.</param>
        /// <returns>True se a senha for válida, false caso contrário.</returns>
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
        /// Generates a SHA256 hash for the specified password.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>
        /// A hexadecimal string representation of the SHA256 hash of the provided password.
        /// </returns>
        /// <remarks>
        /// This method converts the input password into a UTF8-encoded byte array, computes its SHA256 hash,
        /// and then iterates through the resulting byte array to build a lowercase hexadecimal string representation.
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
        /// Regista uma ação de auditoria no banco de dados.
        /// </summary>
        /// <param name="action">A ação que foi realizada.</param>
        /// <param name="status">O estado ou resultado da ação.</param>
        /// <param name="UserId">O identificador do utilizador associado à ação.</param>
        /// <returns>
        /// Uma <see cref="Task"/> que representa a operação assíncrona de registo da ação.
        /// </returns>
        /// <remarks>
        /// Este método cria um novo registo de auditoria com a data e hora atuais, a ação, o status e o identificador do utilizador.
        /// O registo é adicionado ao contexto da base de dados e as alterações são salvas de forma assíncrona.
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
        /// Validates the access token provided in the query string.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ResetPasswordViewModel"/> containing the token to be validated.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a message, the associated email, and the user ID if the token is valid.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> with an appropriate error message if the token is expired or invalid.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method retrieves the token from the provided model and validates it using the <c>ValidateToken</c> method.
        /// If the token is not valid (expired or otherwise), a <c>BadRequest</c> response is returned.
        /// If the token is valid, the method returns an <c>Ok</c> response with a success message along with the user's email and ID.
        /// </remarks>
        [HttpGet("api/validateTokenAtAccess")]
        public IActionResult ValidateTokenAtAccess([FromQuery] ResetPasswordViewModel model)
        {
            var (isValid, email, userId,tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {

                return BadRequest(new { message = "Token expired." });
            }
            else if (!isValid)
            {
                return BadRequest(new { message = "Invalid token." });
            }

            return Ok(new { message = "Token is valid.", email, userId });
        }

        /// <summary>
        /// Envia um email de aviso de confirmação de redefinição de palavra-passe para o endereço especificado.
        /// </summary>
        /// <param name="toEmail">O endereço de email do destinatário.</param>
        /// <returns>
        /// Uma <see cref="Task{Boolean}"/> que indica se o email foi enviado com sucesso (true) ou não (false).
        /// </returns>
        /// <remarks>
        /// Este método realiza as seguintes operações:
        /// <list type="bullet">
        ///   <item>
        ///     Obtém as configurações de email da aplicação, tais como o remetente, o servidor SMTP, a porta, e as credenciais.
        ///   </item>
        ///   <item>
        ///     Verifica se o email do remetente está corretamente configurado e se a porta SMTP é um número válido.
        ///   </item>
        ///   <item>
        ///     Cria uma mensagem de email HTML com o assunto "Password Reset Confirmation" e um corpo de email que informa o utilizador
        ///     que a sua palavra-passe foi atualizada com sucesso, incluindo instruções para contactar o suporte se a alteração não for reconhecida.
        ///   </item>
        ///   <item>
        ///     Envia o email de forma assíncrona utilizando um <see cref="SmtpClient"/> configurado para utilizar SSL para maior segurança.
        ///   </item>
        ///   <item>
        ///     Se ocorrer um erro durante o envio, o erro é registado na consola e o método retorna false.
        ///   </item>
        /// </list>
        /// </remarks>
        private async Task<bool> SendPasswordResetEmailWarning(string toEmail)
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
                    Subject = "Password Reset Confirmation",
                    Body = @"
                             <html>
                                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                                      <h2>Password Reset Confirmation</h2>
                                       <p>Dear User,</p>
                                       <p>Your password has been successfully updated. If it was you who made this change, no further action is required.</p>
                                       <p>If you did not request this change, please contact our support team immediately to secure your account.</p>
                                       <p>Thank you,<br>
                                        The VitalEase Team</p>
                                       </body>
                             </html>",
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
        /// This method attempts to create a new instance of the <see cref="System.Net.Mail.MailAddress"/> class
        /// using the provided email address. If the instance is created successfully and the generated address
        /// matches the input, the email is considered valid. If an exception occurs during this process, the method
        /// returns <c>false</c>.
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


    }
}
