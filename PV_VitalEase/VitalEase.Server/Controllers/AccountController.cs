using Microsoft.AspNetCore.Mvc;
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
                await LogAction("Login Attempt", "Failed - Invalid Data", null);

                return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
            }

            // Busca o usuário na base de dados
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email is incorrect" });
            }

            var hashedPasswordFromInput = HashPassword(model.Password);
            var hashedUserPassword = HashPassword(user.Password);

            // Verifica se o usuário existe e a senha está correta
            if ( hashedUserPassword != hashedPasswordFromInput)
            {
                // Registrar log de erro (credenciais incorretas)
                await LogAction("Login Attempt", "Failed - Password Incorrect", user);

                return Unauthorized(new { message = "Password is incorrect" }); // Retorna erro 401
            }

            // Registrar log de sucesso
            await LogAction("Login Attempt", "Success", user);

            // Agora tratamos o comportamento do "Remember Me"
            if (model.RememberMe)
            {
                // Armazenamos o usuário de forma persistente se "Remember Me" estiver selecionado
                // Este será armazenado no localStorage do cliente
                var userInfo = new
                {
                    userId = user.Id,
                    email = user.Email,
                    type = user.Type, // Tipo de usuário
                    rememberMe = true
                };

                // Retorna os dados do usuário com "Remember Me" marcado
                return Ok(new
                {
                    message = "Login successful",
                    user = userInfo,
                    rememberMe = true // Informa ao frontend que a sessão será persistente
                });
            }
            else
            {
                // Se "Remember Me" não estiver selecionado, a sessão será temporária
                var userInfo = new
                {
                    userId = user.Id,
                    email = user.Email,
                    type = user.Type, // Tipo de usuário
                    rememberMe = false
                };

                // Retorna os dados do usuário, mas informa que a sessão será temporária
                return Ok(new
                {
                    message = "Login successful",
                    user = userInfo,
                    rememberMe = false // Informa ao frontend que a sessão é temporária
                });
            }
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
        private async Task LogAction(string action, string status, User UserId)
        {
            var log = new AuditLog
            {
                Timestamp = DateTime.Now,
                Action = action,
                Status = status,
                User = UserId
            };

            // Salvar o log no banco de dados
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

    }
}