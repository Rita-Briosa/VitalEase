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
    /// Controller responsible for managing password recovery requests.
    /// </summary>
    public class ForgotPasswordController : Controller
    {
        /// <summary>
        /// Database context VitalEaseServerContext, used to access the application records.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Configuration interface used to access the application settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the controller <see cref="ForgotPasswordController"/>.
        /// </summary>
        /// <param name="context">
        /// The database context (<see cref="VitalEaseServerContext"/>) that enables data access operations.
        /// </param>
        /// <param name="configuration">
        /// The configuration interface (<see cref="IConfiguration"/>) to access the application settings.
        /// </param>
        public ForgotPasswordController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Initiates the password reset process for a user by validating the provided email, generating a reset token and link,
        /// and sending password reset instructions via email.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ForgotPasswordViewModel"/> containing the email address of the user who wishes to reset their password.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     a 200 OK response with a message if the password reset instructions were sent successfully;
        ///   </item>
        ///   <item>
        ///     a 400 Bad Request response if the email is missing or in an invalid format;
        ///   </item>
        ///   <item>
        ///     a 404 Not Found response if no user with the specified email is found;
        ///   </item>
        ///   <item>
        ///     a 500 Internal Server Error response if the email fails to send.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates that the email provided in the <paramref name="model"/> is not null and is in a valid format using the <see cref="IsValidEmail"/> method.
        ///   </item>
        ///   <item>
        ///     It searches for the user in the database using the email address. If no user is found, a NotFound response is returned.
        ///   </item>
        ///   <item>
        ///     A password reset token is generated using the <see cref="GenerateToken(string, int)"/> method with the user's email and ID.
        ///   </item>
        ///   <item>
        ///     A reset password link is constructed by embedding the token into a predefined URL.
        ///   </item>
        ///   <item>
        ///     The reset password email is sent to the user's email address via the <see cref="SendPasswordResetEmail(string, string)"/> method.
        ///   </item>
        ///   <item>
        ///     If the email is sent successfully, an OK response is returned with a success message; otherwise, an error response is returned.
        ///   </item>
        /// </list>
        /// </remarks>
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
        /// Determines whether the specified email address is valid.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>
        /// <c>true</c> if the email address is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to create an instance of <see cref="System.Net.Mail.MailAddress"/> using the provided email.
        /// If the email address is correctly formatted and no exception is thrown, the method returns <c>true</c>;
        /// otherwise, if an exception is thrown, it returns <c>false</c>.
        /// </remarks>
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
        /// Sends a password reset email to the specified recipient.
        /// </summary>
        /// <param name="toEmail">The email address of the recipient.</param>
        /// <param name="resetLink">The URL link that the recipient must click to reset their password.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> representing the asynchronous operation, which returns <c>true</c>
        /// if the email was sent successfully, or <c>false</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// This method retrieves the necessary email configuration settings (sender's email, SMTP server, port, username, and password)
        /// from the application's configuration. It first validates that the sender's email is correctly configured and that the SMTP port
        /// is a valid number. An HTML email message is then constructed with the subject "Password Reset Request" and a body that includes the
        /// provided reset link. The email is sent securely via an <see cref="SmtpClient"/> with SSL enabled. If an exception occurs during
        /// the sending process, the error is logged to the console, and the method returns <c>false</c>.
        /// </remarks>
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
        /// Generates a JWT token for password reset purposes for a specified user.
        /// </summary>
        /// <param name="email">The email address of the user requesting the token.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A string representing the generated JWT token.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the JWT key is not configured properly in the application settings.
        /// </exception>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     It retrieves the JWT secret key from the configuration and verifies that it is not null or empty.
        ///   </item>
        ///   <item>
        ///     A unique token identifier (tokenId) is generated using <c>Guid.NewGuid()</c>.
        ///   </item>
        ///   <item>
        ///     A symmetric security key is created using the secret key, and signing credentials are set up with the HMAC SHA256 algorithm.
        ///   </item>
        ///   <item>
        ///     A JWT token is then created with the specified issuer, audience, and a set of claims including the user's email, userId, and tokenId. 
        ///     The token is configured to expire 30 minutes from the time of generation.
        ///   </item>
        ///   <item>
        ///     A corresponding <see cref="ResetPasswordTokens"/> record is created with the token details and is saved in the database.
        ///   </item>
        ///   <item>
        ///     Finally, the method returns the JWT token as a string.
        ///   </item>
        /// </list>
        /// Note that the token record is saved synchronously using <c>_context.SaveChanges()</c>.
        /// </remarks>
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
