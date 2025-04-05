using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VitalEase.Server.Models;


namespace VitalEase.Server.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos pedidos de recuperação de palavra-passe.
    /// </summary>
    public class ForgotPasswordController : Controller
    {
        /// <summary>
        /// Contexto da base de dados VitalEaseServerContext, utilizado para aceder aos registos da aplicação.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Interface de configuração utilizada para aceder às definições da aplicação.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="ForgotPasswordController"/>.
        /// </summary>
        /// <param name="context">
        /// O contexto da base de dados (<see cref="VitalEaseServerContext"/>) que permite efetuar operações de acesso aos dados.
        /// </param>
        /// <param name="configuration">
        /// A interface de configuração (<see cref="IConfiguration"/>) para aceder às definições da aplicação.
        /// </param>
        public ForgotPasswordController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Processa o pedido de recuperação de palavra-passe, enviando um email com instruções de redefinição.
        /// </summary>
        /// <param name="model">
        /// O modelo que contém o email do utilizador que pretende recuperar a palavra-passe.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica o sucesso ou a falha do processo, com uma mensagem informativa.
        /// </returns>
        [HttpPost("api/forgotPassword")]
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

            var token = GenerateToken(user.Email, user.Id);

            // Gerar link de redefinição de senha direto, sem token
            var resetLink = $"https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/resetPassword?token={Uri.EscapeDataString(token)}";

            // Enviar o email de redefinição de senha
            var emailSent = await SendPasswordResetEmail(user.Email, resetLink);
            if (!emailSent)
            {
                return StatusCode(500, new { message = "Failed to send reset password email." });
            }

            return Ok(new { message = "Password reset instructions have been sent to your email." });
        }

        /// <summary>
        /// Verifica se o endereço de email fornecido é válido.
        /// </summary>
        /// <param name="email">O endereço de email a ser validado.</param>
        /// <returns>
        /// Retorna <c>true</c> se o email for válido; caso contrário, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Envia um email com o link de redefinição de palavra-passe para o endereço especificado.
        /// </summary>
        /// <param name="toEmail">O endereço de email do destinatário.</param>
        /// <param name="resetLink">O link para redefinir a palavra-passe.</param>
        /// <returns>
        /// Uma <see cref="Task{bool}"/> que resulta em <c>true</c> se o email for enviado com sucesso; caso contrário, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Gera um token JWT para a redefinição de palavra-passe e regista o token gerado na base de dados.
        /// </summary>
        /// <param name="email">O endereço de email do utilizador para o qual o token será gerado.</param>
        /// <param name="userId">O identificador do utilizador.</param>
        /// <returns>
        /// Uma string representando o token JWT gerado, que expira 30 minutos após a sua criação.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// É lançada se a chave JWT ("Jwt:Key") não estiver configurada corretamente na aplicação.
        /// </exception>
        public string GenerateToken(string email, int userId)
        {

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }

            var tokenId = Guid.NewGuid().ToString();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.Now.AddMinutes(30);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim("userId", userId.ToString()),
                    new Claim("tokenId", tokenId)
                },
                expires: expiresAt, // Define o tempo de expiração do token (1 hora, por exemplo)
                signingCredentials: creds
            );

            var resetPasswordToken = new ResetPasswordTokens
            {
                TokenId = tokenId,         // Usando o tokenId gerado
                CreatedAt = DateTime.Now,
                ExpiresAt = expiresAt,
                IsUsed = false
            };

            _context.ResetPasswordTokens.Add(resetPasswordToken);
            _context.SaveChanges();

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
 }
