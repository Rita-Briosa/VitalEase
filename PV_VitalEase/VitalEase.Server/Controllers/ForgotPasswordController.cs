using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;
using Microsoft.CodeAnalysis.Scripting;
using System.Text.RegularExpressions;

namespace VitalEase.Server.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly VitalEaseServerContext _context;
        private readonly IConfiguration _configuration;

        public ForgotPasswordController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            // Verificar se o email enviado está no formato correto
            if (string.IsNullOrEmpty(model.Email) || !IsValidEmail(model.Email))
            {
                return BadRequest(new { message = "Invalid email format." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Gerar link de redefinição de senha direto, sem token
            var resetLink = $"https://localhost:4200/resetPassword?id={user.Id}&email={Uri.EscapeDataString(user.Email)}";

            // Enviar o email de redefinição de senha
            var emailSent = await SendPasswordResetEmail(user.Email, resetLink);
            if (!emailSent)
            {
                return StatusCode(500, new { message = "Failed to send reset password email." });
            }

            return Ok(new { message = "Password reset instructions have been sent to your email." });
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

        private async Task<bool> SendPasswordResetEmail(string toEmail, string resetLink)
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
                    Subject = "Password Reset Request",
                    Body = $"Click the following link to reset your password: <a href='{resetLink}'>Reset Password</a>",
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

        [HttpPost("resetPassword")]
        public IActionResult ResetPassword([FromBody] string email, [FromBody] string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest(new { message = "Invalid parameters." });
            }

            if (!IsPasswordValid(newPassword))
            {
                return BadRequest(new { message = "Password does not meet the required criteria." });
            }

            // Encontre o usuário pelo email
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            // Verificar se a senha já é a mesma
            if (user.Password == newPassword)
            {
                return BadRequest(new { message = "New password cannot be the same as the old one." });
            }

            // Atualizar a senha do usuário
            user.Password = newPassword;

            try
            {
                // Salvar no banco de dados
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Capturar qualquer exceção e retornar um erro genérico
                return StatusCode(500, new { message = "An error occurred while updating the password." });
            }

            // Retornar uma resposta JSON com sucesso
            return Ok(new { message = "Password reset successfully." });
        }


        private bool IsPasswordValid(string password)
        {
            // Verificar se a senha tem pelo menos 12 caracteres
            if (password.Length < 12)
            {
                return false;
            }

            // Verificar se a senha contém pelo menos uma letra minúscula
            if (!password.Any(c => Char.IsLower(c)))
            {
                return false;
            }

            // Verificar se a senha contém pelo menos uma letra maiúscula
            if (!password.Any(c => Char.IsUpper(c)))
            {
                return false;
            }

            var specialChars = "!@#$%^&*(),.?\":{ }|<> ";
            if (!password.Any(c => specialChars.Contains(c)))
            {
                return false;
            }

            return true;
        }
    }
 }
