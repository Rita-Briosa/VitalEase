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
    /// <summary>
    /// Controller responsible for registering new users.
    /// </summary>
    public class RegisterController : Controller
    {
        /// <summary>
        /// Database context used for accessing and manipulating records.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Application configuration that provides access to relevant settings and parameters.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterController"/> class.
        /// </summary>
        /// <param name="context">
        /// The database context (<see cref="VitalEaseServerContext"/>) that enables data storage and access operations.
        /// </param>
        /// <param name="configuration">
        /// The application configuration (<see cref="IConfiguration"/>) used to access settings and configuration parameters.
        /// </param>
        public RegisterController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user by creating the corresponding profile and storing the data in the database.
        /// </summary>
        /// <param name="model">
        /// An instance of <see cref="RegisterViewModel"/> that contains the necessary registration data, including:
        /// email, password, birthdate, username, height, weight, gender, and an indication of cardiac issues.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the registration:
        /// <list type="bullet">
        ///   <item>
        ///     <c>BadRequest</c> if the provided data is invalid, if the user already exists, if the username already exists,
        ///     if the user is under 16 years old, if the password is weak, or if any error occurs during the process.
        ///   </item>
        ///   <item>
        ///     <c>Ok</c> with a success message, the generated JWT token, and the user's data if the registration is successful.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Checks whether the user is at least 16 years old based on the provided birthdate; if not, it logs the attempt and returns an error.
        ///   </item>
        ///   <item>
        ///     Determines if a user with the same email already exists; if so, it logs the attempt and returns an error.
        ///   </item>
        ///   <item>
        ///     Determines if a profile with the same username already exists; if so, it logs the attempt and returns an error.
        ///   </item>
        ///   <item>
        ///     Creates a new profile with the provided data and saves it to the database.
        ///   </item>
        ///   <item>
        ///     Validates the strength of the password; if the password does not meet the required criteria, it logs the attempt and returns an error.
        ///   </item>
        ///   <item>
        ///     Creates a new user by associating the newly created profile, the hashed password, the email, and the default user type,
        ///     while setting the email verification status to false, and stores these details in the database.
        ///   </item>
        ///   <item>
        ///     Generates a JWT token for the new user; if token generation fails, it logs the attempt and returns an error.
        ///   </item>
        ///   <item>
        ///     Constructs an email confirmation link using the generated token and sends a confirmation email to the user;
        ///     if sending the email fails, it returns a 500 error.
        ///   </item>
        ///   <item>
        ///     Logs the successful registration operation and returns an Ok response with a success message, the generated token, and the user's data.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/register")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            if (model.Weight < 30 || model.Weight > 400)
            {
                await LogAction("Register Attempt", "Failed - Invalid Weigth", 0);
                return BadRequest(new { message = "Invalid weight (valid values are between 30 - 400 kg)" });
            }
 
            if (model.Height < 90 || model.Height > 251)
            {
                await LogAction("Register Attempt", "Failed - Invalid Height", 0);
                return BadRequest(new { message = "Invalid height (valid values are between 90 - 251 cm)" });
            }

            // Verifica se a idade é maior ou igual a 16 anos
            if (model.BirthDate > DateTime.Today.AddYears(-16))
            {
                await LogAction("Register Attempt", "Failed - Age restriction", 0);
                return BadRequest(new { message = "You must be at least 16 years old to register." });
            }

            var existingUser = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser != null)
            {
                // Registrar log de erro (e-mail já existente)
                await LogAction("Register Attempt", "Failed - Email already exists", 0);
                return BadRequest(new { message = "Email already exists" }); // Retorna erro 400
            }

            var existingUsername = await _context.Profiles
                                     .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (existingUsername != null)
            {
                // Registrar log de erro (e-mail já existente)
                await LogAction("Register Attempt", "Failed - Username already exists", 0);
                return BadRequest(new { message = "Username already exists" }); // Retorna erro 400
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

            if (!IsPasswordValid(model.Password))
            {
                await LogAction("Password reset attempt", "Failed - Weak password", 0);
                return BadRequest(new { message = "Password does not meet the required criteria." });
            } 

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

            var emailConfirmationLink = $"https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/emailValidation?token={Uri.EscapeDataString(token)}";

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

        /// <summary>
        /// Validates whether the password meets the security criteria.
        /// </summary>
        /// <param name="password">The password to be validated.</param>
        /// <returns>
        /// <c>true</c> if the password is valid; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Sends an email confirmation message to the specified recipient.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="emailConfirmationLink">
        /// The confirmation link to be included in the email body, which the recipient must click to confirm their email address.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> representing the asynchronous operation, returning <c>true</c> if the email was sent successfully,
        /// or <c>false</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// This method retrieves email configuration settings from the application's configuration, including:
        /// <list type="bullet">
        ///   <item>The sender's email address (<c>EmailSettings:FromEmail</c>).</item>
        ///   <item>The SMTP server address (<c>EmailSettings:SmtpServer</c>).</item>
        ///   <item>The SMTP port (<c>EmailSettings:SmtpPort</c>), which is validated to ensure it's a valid number.</item>
        ///   <item>The SMTP username and password for authentication (<c>EmailSettings:SmtpUsername</c> and <c>EmailSettings:SmtpPassword</c>).</item>
        /// </list>
        /// It then constructs an HTML email message with the subject "Register email" and a body containing the confirmation link.
        /// The email is sent using an <see cref="SmtpClient"/> configured with SSL enabled for secure transmission.
        /// If an error occurs during the process, the exception is logged to the console and the method returns <c>false</c>.
        /// </remarks>
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
                    Subject = "Register email",
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

        /// <summary>
        /// Determines whether the specified email address is valid.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>
        /// <c>true</c> if the email address is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to create a new instance of the <see cref="System.Net.Mail.MailAddress"/> class using the provided email.
        /// If the creation is successful and the generated address matches the input, the email is considered valid.
        /// If an exception occurs during this process, the method returns <c>false</c>.
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
        /// Generates a SHA256 hash for the specified password.
        /// </summary>
        /// <param name="password">The plain text password to be hashed.</param>
        /// <returns>
        /// A hexadecimal string representation of the SHA256 hash of the provided password.
        /// </returns>
        /// <remarks>
        /// This method converts the input password to a UTF-8 encoded byte array, computes its SHA256 hash,
        /// and then converts the resulting byte array into a lowercase hexadecimal string.
        /// </remarks>
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

        /// <summary>
        /// Logs an audit action by recording it in the database.
        /// </summary>
        /// <param name="action">The action that was performed.</param>
        /// <param name="status">The status or outcome of the action.</param>
        /// <param name="UserId">The identifier of the user associated with the action.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation of logging the action.
        /// </returns>
        /// <remarks>
        /// This method creates an <see cref="AuditLog"/> object with the current timestamp, the specified action,
        /// status, and user ID. The log is then added to the database context and the changes are saved asynchronously.
        /// </remarks>
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

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The <see cref="User"/> object for which the token is generated. It must contain a valid email address.</param>
        /// <returns>
        /// A string representing the generated JWT token.
        /// </returns>
        /// <remarks>
        /// This method creates a JWT token using a symmetric security key derived from a secret configured in the application.
        /// It sets up a token descriptor that includes the user's email as a claim and specifies an expiration time of one hour from the current UTC time.
        /// The token is then created using the <see cref="JwtSecurityTokenHandler"/> and returned as a string.
        /// </remarks>
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
