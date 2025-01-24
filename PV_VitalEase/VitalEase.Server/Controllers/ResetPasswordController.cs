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
    public class ResetPasswordController : Controller
    {
        private readonly VitalEaseServerContext _context;
        private readonly IConfiguration _configuration;

        public ResetPasswordController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

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
