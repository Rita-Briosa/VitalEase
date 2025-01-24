using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace VitalEase.Server.Controllers
{
    public class RegisterController : Controller
    {

        private readonly VitalEaseServerContext _context;
        private readonly IConfiguration _configuration;

        public RegisterController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("api/register")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
             // Verifica se os dados enviados são válidos
            if (!ModelState.IsValid)
            {
                // Registrar log de erro (dados inválidos)
                await LogAction("Register Attempt", "Failed - Invalid Data", 0);

                return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
            }

            var existingUser = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser != null)
            {
                // Registrar log de erro (e-mail já existente)
                await LogAction("Register Attempt", "Failed - Email already exists", 0);
                return BadRequest(new { message = "Email already exists" }); // Retorna erro 400
            }

            var profile = new Profile
            {
                Username = model.Username,
                Birthdate = model.BirthDate, // Corrigido para combinar com o Angular
                Height = model.Height,
                Weight = model.Weight,
                Gender = model.Gender,
                HasHeartProblems = model.HeartProblems
            };


            if (profile == null)
            {
                await LogAction("Register Attempt", "Failed - Error at profile creation", 0);

                return BadRequest(new { message = "Error at profile creation" }); // Retorna erro 400
            }

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Email = model.Email,
                Profile = profile,
                Password = HashPassword(model.Password),
                Type = UserType.Standard,
                IsEmailVerified = false
            };

            if (user == null)
            {
                await LogAction("Register Attempt", "Failed - Error at User creation", 0);

                return BadRequest(new { message = "Error at user creation" }); // Retorna erro 400
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            if (token == null)
            {
                await LogAction("Register Attempt", "Failed - Error at token generation", 0);

                return BadRequest(new { message = "Error at token generation" }); // Retorna erro 400
            }

            var emailConfirmationLink = $"https://vitalease2025.3utilities.com/emailValidation?token={Uri.EscapeDataString(token)}";

            // Enviar o email de redefinição de senha
            var emailSent = await SendEmailConfirmation(user.Email, emailConfirmationLink);

            if (!emailSent)
            {
                return StatusCode(500, new { message = "Failed to send email confirmation." });
            }

            await LogAction("Register Attempt", "Success", user.Id);

            return Ok(new
            {
                message = "Register successful",
                token,
                user = user

            });

        }

        private async Task<bool> SendEmailConfirmation(string toEmail, string emailConfirmationLink)
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
                    Body = $"Click the following link to confirm your Email Address: <a href='{emailConfirmationLink}'>Email Confirmation</a>",
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

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("Chave_secreta_pertencente_a_vital_easee_e_impenetravel");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
       
            new Claim(ClaimTypes.Email, user.Email),
           
        }),
                Expires = DateTime.UtcNow.AddHours(1), // Standard session: 15 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
