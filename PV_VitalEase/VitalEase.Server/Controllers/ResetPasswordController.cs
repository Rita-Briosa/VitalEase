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
        /// Processes the user's password reset request.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ResetPasswordViewModel"/> that contains the reset token and the new password.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the outcome of the password reset operation. It may return:
        /// <list type="bullet">
        ///   <item><c>Ok</c> – if the password is successfully reset;</item>
        ///   <item><c>BadRequest</c> – if the parameters are invalid, if the token is invalid or expired, or if the new password is weak or identical to the old one;</item>
        ///   <item><c>NotFound</c> – if the user is not found;</item>
        ///   <item><c>StatusCode(500)</c> – in case of an error when updating the database.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>Checks whether the model and the required parameters (token and new password) have been provided; if not, logs the action and returns a BadRequest.</item>
        ///   <item>Validates the token using the <c>ValidateToken</c> method, which returns the token's validity, the user's email, the userId, and the tokenId.</item>
        ///   <item>If the token is not valid, logs the action and returns a BadRequest with the message "Invalid or expired token."</item>
        ///   <item>Searches the database for the user based on the email extracted from the token. If the user is not found, returns a NotFound.</item>
        ///   <item>Checks whether the new password meets the complexity criteria via the <c>IsPasswordValid</c> method.</item>
        ///   <item>Generates the hash of the new password and ensures that it differs from the current password hash; if they are identical, logs the action and returns a BadRequest.</item>
        ///   <item>Updates the user's password, records the date of the password change, and invalidates the current session token.</item>
        ///   <item>Marks the reset token as used, if the tokenId is valid.</item>
        ///   <item>Sends a password reset alert notification to the user's email.</item>
        ///   <item>Attempts to save the changes to the database; if successful, logs the action as successful and returns an Ok with a success message. In case of an error, logs the action as failed and returns a StatusCode(500) with the error details.</item>
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
        /// Validates the JWT token.
        /// </summary>
        /// <param name="token">Token to be validated.</param>
        /// <returns>A tuple containing: whether it is valid, the email, and the userId.</returns>
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
        /// Method to validate if the password meets the security criteria.
        /// </summary>
        /// <param name="password">Password to be validated.</param>
        /// <returns>True if the password is valid, false otherwise.</returns>
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
        /// Logs an audit action in the database.
        /// </summary>
        /// <param name="action">The action that was performed.</param>
        /// <param name="status">The state or result of the action.</param>
        /// <param name="UserId">The identifier of the user associated with the action.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation of logging the action.
        /// </returns>
        /// <remarks>
        /// This method creates a new audit log entry with the current date and time, the action, the status, and the user identifier.
        /// The log entry is added to the database context and the changes are saved asynchronously.
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
        /// Sends a password reset confirmation email to the specified address.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that indicates whether the email was successfully sent (true) or not (false).
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves the application's email settings, such as the sender, SMTP server, port, and credentials.
        ///   </item>
        ///   <item>
        ///     Checks whether the sender's email is properly configured and if the SMTP port is a valid number.
        ///   </item>
        ///   <item>
        ///     Creates an HTML email message with the subject "Password Reset Confirmation" and a body that informs the user
        ///     that their password has been successfully updated, including instructions to contact support if the change is unrecognized.
        ///   </item>
        ///   <item>
        ///     Sends the email asynchronously using an <see cref="SmtpClient"/> configured to use SSL for enhanced security.
        ///   </item>
        ///   <item>
        ///     If an error occurs during sending, the error is logged to the console and the method returns false.
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
