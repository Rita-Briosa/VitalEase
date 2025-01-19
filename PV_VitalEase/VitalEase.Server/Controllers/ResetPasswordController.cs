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

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            // Validar o modelo recebido
            if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
            {
                await LogAction("Password change Attempt", "Failed - Invalid Data", 0);
                return BadRequest(new { message = "Invalid parameters.Token and Password are required." });
            }

            var (isValid, email, userId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Password change Attempt", "Failed - Invalid or expired token", 0);
                return BadRequest(new { message = "Invalid or expired token." });
            }

            // Procurar o usuário pelo email no banco de dados
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Verificar se a nova senha atende aos critérios de segurança
            if (!IsPasswordValid(model.NewPassword))
            {
                await LogAction("Password change Attempt", "Failed - Password does not meet request criteria", user.Id);
                return BadRequest(new { message = "Password does not meet the required criteria." });
            }


            var hashedPasswordFromInput = HashPassword(model.NewPassword);
            // Verificar se a nova senha é diferente da senha antiga
            if (user.Password == hashedPasswordFromInput)
            {
                await LogAction("Password change Attempt", "Failed - New password cannot be the same as the old one.", user.Id);
                return BadRequest(new { message = "New password cannot be the same as the old one." });
            }

            // Atualizar a senha do usuário
            user.Password = hashedPasswordFromInput;

            try
            {
                // Salvar a alteração no banco de dados
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                await LogAction("Password change Attempt", "Failed - An error occurred while updating the password.", user.Id);
                // Retornar um erro genérico se algo der errado ao salvar
                return StatusCode(500, new { message = "An error occurred while updating the password.", details = ex.Message });
            }

            await LogAction("Password change Attempt", "Success - Password reset successfully.", user.Id);
            // Retornar uma resposta de sucesso
            return Ok(new { message = "Password reset successfully." });
        }


        /// <summary>
        /// Valida o token JWT.
        /// </summary>
        /// <param name="token">Token a ser validado.</param>
        /// <returns>Uma tupla com: se é válido, email e userId.</returns>
        private (bool IsValid, string Email, int UserId) ValidateToken(string token)
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

                // Validar se o email e userId são válidos
                if (string.IsNullOrEmpty(email) || userId == 0)
                {
                    return (false, "", 0);
                }

                // Se chegou até aqui, o token é válido
                return (true, email, userId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", 0);
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", 0);
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", 0);
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
    }
}
