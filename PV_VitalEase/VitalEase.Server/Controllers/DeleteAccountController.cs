using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace VitalEase.Server.Controllers
{
    /// <summary>
    /// Controlador responsável pela exclusão de contas de utilizador.
    /// Este controlador está protegido, pelo que apenas utilizadores autenticados podem aceder aos seus endpoints.
    /// </summary>
    [Authorize] // Garante que apenas users autenticados possam aceder
    [ApiController]
    [Route("api/delete-account")] // Define a rota base para o controlador
    public class DeleteAccountController : ControllerBase
    {
        /// <summary>
        /// Contexto da base de dados VitalEaseServerContext, utilizado para operações de acesso e gestão dos dados.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Instância do logger para registar informações, advertências e erros.
        /// </summary>
        private readonly ILogger<DeleteAccountController> _logger;

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="DeleteAccountController"/>.
        /// </summary>
        /// <param name="context">O contexto da base de dados a ser utilizado.</param>
        /// <param name="logger">O logger para registo de eventos e erros.</param>
        public DeleteAccountController(VitalEaseServerContext context, ILogger<DeleteAccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Elimina a conta do utilizador autenticado.
        /// </summary>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica o sucesso ou falha da operação de eliminação da conta.
        /// </returns>
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

        /// <summary>
        /// Regista uma acção de auditoria, gravando-a na base de dados.
        /// </summary>
        /// <param name="action">A acção que foi realizada.</param>
        /// <param name="status">O estado ou resultado da acção.</param>
        /// <param name="userId">O identificador do utilizador associado à acção.</param>
        /// <returns>Uma <see cref="Task"/> que representa a operação assíncrona de registo do log.</returns>
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
