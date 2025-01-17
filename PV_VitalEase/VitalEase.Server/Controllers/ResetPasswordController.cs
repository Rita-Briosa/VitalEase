using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;

namespace VitalEase.Server.Controllers
{
    public class ResetPasswordController : Controller
    {
        private readonly VitalEaseServerContext _context;

        public ResetPasswordController(VitalEaseServerContext context)
        {
            _context = context;
        }

        [HttpPost("resetPassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            // Validar o modelo recebido
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest(new { message = "Invalid parameters. Email and password are required." });
            }

            // Verificar se a nova senha atende aos critérios de segurança
            if (!IsPasswordValid(model.NewPassword))
            {
                return BadRequest(new { message = "Password does not meet the required criteria." });
            }

            // Procurar o usuário pelo email no banco de dados
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Verificar se a nova senha é diferente da senha antiga
            if (user.Password == model.NewPassword)
            {
                return BadRequest(new { message = "New password cannot be the same as the old one." });
            }

            // Atualizar a senha do usuário
            user.Password = model.NewPassword;

            try
            {
                // Salvar a alteração no banco de dados
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Retornar um erro genérico se algo der errado ao salvar
                return StatusCode(500, new { message = "An error occurred while updating the password.", details = ex.Message });
            }

            // Retornar uma resposta de sucesso
            return Ok(new { message = "Password reset successfully." });
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
    }
}
