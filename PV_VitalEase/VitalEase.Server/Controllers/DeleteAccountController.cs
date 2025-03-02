using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace VitalEase.Server.Controllers
{
    [Authorize] // Garante que apenas users autenticados possam aceder
    [ApiController]
    [Route("api/delete-account")] // Define a rota base para o controlador
    public class DeleteAccountController : ControllerBase
    {
        private readonly VitalEaseServerContext _context;
        private readonly ILogger<DeleteAccountController> _logger;

        public DeleteAccountController(VitalEaseServerContext context, ILogger<DeleteAccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpDelete] // Define o método HTTP DELETE 
        public async Task<IActionResult> DeleteAccount()
        {
            // Obtém o ID do user a partir do token de autenticação
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            // Converte o ID do user de string para inteiro
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID." });
            }

            // Vai procurar o user na bd
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            _logger.LogInformation("User {UserId} has requested account deletion.", user.Id);

            // Remove o user da bd
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Registra a ação no log 
            await LogAction("Account Deletion", "Success", user.Id);

            return Ok(new { message = "Account successfully deleted." });
        }

        // Método para registrar ações no log
        private async Task LogAction(string action, string status, int userId)
        {
            var log = new AuditLog
            {
                Timestamp = DateTime.Now,
                Action = action,
                Status = status,
                UserId = userId
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
