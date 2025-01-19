using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;

namespace VitalEase.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly VitalEaseServerContext _context;

        public AccountController(VitalEaseServerContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // Verifica se os dados enviados são válidos
            if (!ModelState.IsValid)
            {
                // Registrar log de erro (dados inválidos)
                await LogAction("Login Attempt", "Failed - Invalid Data", 0);

                return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
            }

            // Busca o usuário na base de dados
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email is incorrect" });
            }

            var loginAttempts = _context.AuditLogs.Select(l => l).Where(l => l.UserId == user.Id && l.Status != "Failed - Account Blocked").ToList();
            loginAttempts.Reverse();
            List<AuditLog> lastAttempts = new List<AuditLog>();
            if(loginAttempts != null)
            {
                if(loginAttempts.Count >= 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                            lastAttempts.Add(loginAttempts[i]);
                    }
                }

                bool areAllLastFailed = lastAttempts.Select(a => a).Where(a => a.Status == "Failed - Password Incorrect").Count() == 3 ? true : false;


                if (lastAttempts.Count >= 3 && lastAttempts[0].Timestamp >= DateTime.Now.AddMinutes(-15) && areAllLastFailed)
                {

                    await LogAction("Login Attempt", "Failed - Account Blocked", user.Id);

                    return Unauthorized(new { message = "Account is Blocked. Wait 15 minutes and try again." });
                }

            }

            var hashedPasswordFromInput = HashPassword(model.Password);

            // Verifica se o usuário existe e a senha está correta
            if ( user.Password != hashedPasswordFromInput)
            {
                // Registrar log de erro (credenciais incorretas)
                await LogAction("Login Attempt", "Failed - Password Incorrect", user.Id);

                return Unauthorized(new { message = "Password is incorrect" }); // Retorna erro 401
            }

            // Registrar log de sucesso
            await LogAction("Login Attempt", "Success", user.Id);

            var token = GenerateJwtToken(user, model.RememberMe);

                // Armazenamos o usuário de forma persistente se "Remember Me" estiver selecionado
                // Este será armazenado no localStorage do cliente
                var userInfo = new
                {
                    userId = user.Id,
                    email = user.Email,
                    type = user.Type, // Tipo de usuário
                };

                // Retorna os dados do usuário com "Remember Me" marcado
                return Ok(new
                {
                    message = "Login successful",
                    token,
                    user = userInfo
                    
                });
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

        // Método para registrar as ações no log de auditoria
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

        private string GenerateJwtToken(User user, bool rememberMe)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("Chave_secreta_pertencente_a_vital_ease");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("userType", user.Type.ToString())
        }),
                Expires = rememberMe
                    ? DateTime.UtcNow.AddDays(30) // "Remember Me" session: 30 days
                    : DateTime.UtcNow.AddMinutes(15), // Standard session: 15 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}