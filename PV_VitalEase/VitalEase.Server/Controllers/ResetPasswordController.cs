using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;
using System.Security.Cryptography;

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

            var hashedPasswordFromInput = HashPassword(model.NewPassword);
            // Verificar se a nova senha é diferente da senha antiga
            if (user.Password == hashedPasswordFromInput)
            {
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
    }
}
