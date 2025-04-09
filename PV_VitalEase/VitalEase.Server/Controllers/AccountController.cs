using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;
using System.Configuration;

namespace VitalEase.Server.Controllers
{

    /// <summary>
    /// Controller responsible for operations related to user accounts.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Database context VitalEaseServerContext, used for database access operations.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Configuration interface, used to access the application settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of <see cref="AccountController"/>.
        /// </summary>
        /// <param name="context">The database context <see cref="VitalEaseServerContext"/>.</param>
        /// <param name="configuration">The configuration interface <see cref="IConfiguration"/> da aplicação.</param>
        public AccountController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Processes a login request by validating the user credentials, checking account status, and generating a JWT token for a successful login.
        /// </summary>
        /// <param name="model">
        /// A <see cref="LoginViewModel"/> containing the user's email, password, and a flag indicating whether to remember the session.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     An HTTP 200 OK response with a success message, the generated JWT token, and basic user information if the login is successful.
        ///   </item>
        ///   <item>
        ///     An HTTP 400 Bad Request response if the input data is invalid or if the user does not meet certain conditions (e.g., email not verified).
        ///   </item>
        ///   <item>
        ///     An HTTP 401 Unauthorized response if the email or password is incorrect or if the account is blocked due to multiple failed attempts.
        ///   </item>
        ///   <item>
        ///     An HTTP 500 Internal Server Error if there is an issue sending necessary email notifications.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The login process involves the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the incoming model; if invalid, logs the attempt and returns a Bad Request.
        ///   </item>
        ///   <item>
        ///     Searches for the user in the database by email; if not found, returns Unauthorized with a message indicating an incorrect email.
        ///   </item>
        ///   <item>
        ///     Checks if the user's email has been verified; if not, logs the attempt and returns a Bad Request asking for email verification.
        ///   </item>
        ///   <item>
        ///     Retrieves the user's login attempts from the audit logs and checks if the last five attempts have failed due to an incorrect password within the last 15 minutes.
        ///     If so, it sends a blocked account notification email, logs the blocked attempt, and returns Unauthorized with a block message.
        ///   </item>
        ///   <item>
        ///     Hashes the provided password and compares it with the stored password; if they do not match, logs the attempt and returns Unauthorized with a password incorrect message.
        ///   </item>
        ///   <item>
        ///     Upon successful credential validation, logs the success, generates a JWT token (with an optional "Remember Me" feature), and updates the user's session token and creation timestamp.
        ///   </item>
        ///   <item>
        ///     Constructs an anonymous object containing the user's ID, email, and type, then returns an OK response with a success message, the token, and the user information.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // Verifica se os dados enviados são válidos
            if (!ModelState.IsValid)
            {
                // Registrar log de erro (dados inválidos)
                await LogAction("Login Attempt", "Failed - Invalid Data", 0);

                return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
            }

            // Busca o usuário na base de dados
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email is incorrect" });
            }
            
            if(user.IsEmailVerified == false)
            {
                await LogAction("Login Attempt", "Failed - Email is not verified", 0);

                return BadRequest(new { message = "Please, verify your email." }); // Retorna erro 400
            }

            var loginAttempts = _context.AuditLogs.Select(l => l).Where(l => l.UserId == user.Id && l.Action == "Login Attempt" && l.Status != "Failed - Account Blocked").ToList();
            loginAttempts.Reverse();
            List<AuditLog> lastAttempts = new List<AuditLog>();
            if(loginAttempts != null)
            {
                if(loginAttempts.Count >= 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                            lastAttempts.Add(loginAttempts[i]);
                    }
                }

                bool areAllLastFailed = lastAttempts.Select(a => a).Where(a => a.Status == "Failed - Password Incorrect").Count() == 5 ? true : false;


                if (lastAttempts.Count >= 5 && lastAttempts[0].Timestamp >= DateTime.Now.AddMinutes(-15) && areAllLastFailed)
                {
                    // Enviar o email de redefinição de senha
                    var emailSent = await SendBlockedInformationEmail(user.Email);

                    if (!emailSent)
                    {
                        return StatusCode(500, new { message = "Failed to send email confirmation." });
                    }

                    await LogAction("Login Attempt", "Failed - Account Blocked", user.Id);

                    return Unauthorized(new { message = "Account is Blocked. Wait 15 minutes and try again." });
                }

            }

            var hashedPasswordFromInput = HashPassword(model.Password);

            // Verifica se o usuário existe e a senha está correta
            if ( user.Password != hashedPasswordFromInput)
            {
                // Registrar log de erro (credenciais incorretas)
                await LogAction("Login Attempt", "Failed - Password Incorrect", user.Id);

                return Unauthorized(new { message = "Password is incorrect" }); // Retorna erro 401
            }

            
            // Registrar log de sucesso
            await LogAction("Login Attempt", "Success", user.Id);
 
            var token = GenerateJwtToken(user, model.RememberMe);
            user.SessionToken = token;
            user.SessionTokenCreatedAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Armazenamos o usuário de forma persistente se "Remember Me" estiver selecionado

            var userInfo = new
                {
                    userId = user.Id,
                    email = user.Email,
                    type = user.Type, // Tipo de usuário
                };

                // Retorna os dados do usuário com "Remember Me" marcado
                return Ok(new
                {
                    message = "Login successful",
                    token,
                    user = userInfo
                    
                });
        }

        /// <summary>
        /// Generates a SHA256 hash for the specified password.
        /// </summary>
        /// <param name="password">The plain text password to be hashed.</param>
        /// <returns>
        /// A hexadecimal string representation of the SHA256 hash of the provided password.
        /// </returns>
        /// <remarks>
        /// This method converts the input password into a UTF-8 encoded byte array, computes its SHA256 hash,
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
        /// Logs an action to the database by creating a new audit log entry.
        /// </summary>
        /// <param name="action">The description of the action performed.</param>
        /// <param name="status">The status or outcome of the action.</param>
        /// <param name="UserId">The identifier of the user associated with the action.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation of saving the log entry to the database.
        /// </returns>
        /// <remarks>
        /// This method creates an instance of <see cref="AuditLog"/> with the current timestamp, action description,
        /// status, and user identifier. It then adds this log entry to the database context and saves the changes asynchronously.
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
        /// <param name="user">The <see cref="User"/> object for which the token is generated.</param>
        /// <param name="rememberMe">
        /// A boolean value indicating whether a "Remember Me" session should be created. 
        /// If <c>true</c>, the token will expire in 30 days; otherwise, it will expire in 15 minutes.
        /// </param>
        /// <returns>
        /// A string representing the generated JWT token.
        /// </returns>
        /// <remarks>
        /// This method constructs a token descriptor with the following claims:
        /// <list type="bullet">
        ///   <item>
        ///     <c>NameIdentifier</c>: The user's unique identifier.
        ///   </item>
        ///   <item>
        ///     <c>Email</c>: The user's email address.
        ///   </item>
        ///   <item>
        ///     <c>userType</c>: The type of the user as defined in <see cref="UserType"/>.
        ///   </item>
        ///   <item>
        ///     <c>PasswordLastChanged</c>: The tick count of the last password change or "0" if not set.
        ///   </item>
        /// </list>
        /// The token is signed using a symmetric security key derived from a secret, and its expiration is determined by the value of the <paramref name="rememberMe"/> parameter.
        /// </remarks>
        private string GenerateJwtToken(User user, bool rememberMe)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("Chave_secreta_pertencente_a_vital_ease");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("userType", user.Type.ToString()),
            new Claim("PasswordLastChanged",
            user.PasswordLastChanged.HasValue
                ? user.PasswordLastChanged.Value.Ticks.ToString()
                : "0")
        }),
                Expires = rememberMe
                    ? DateTime.UtcNow.AddDays(30) // "Remember Me" session: 30 days
                    : DateTime.UtcNow.AddHours(2), // Standard session: 2 hours
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validates the current user's session by verifying the JWT token provided in the Authorization header.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and user details if the token is valid.
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> with an error message if the token is missing, invalid, or expired.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Extracts the token from the Authorization header of the incoming HTTP request.
        ///   </item>
        ///   <item>
        ///     Returns an Unauthorized response if no token is provided.
        ///   </item>
        ///   <item>
        ///     Searches for a user in the database whose <c>SessionToken</c> matches the extracted token.
        ///   </item>
        ///   <item>
        ///     Uses a <see cref="JwtSecurityTokenHandler"/> with a symmetric security key to validate the token.
        ///   </item>
        ///   <item>
        ///     If the token is expired, clears the user's session token and its creation timestamp, updates the database,
        ///     and returns an Unauthorized response with a "Token expired" message.
        ///   </item>
        ///   <item>
        ///     If the token is valid, returns an Ok response with a success message and the user's basic details (Id, Email, and Type).
        ///   </item>
        ///   <item>
        ///     If any other exception occurs during token validation, returns an Unauthorized response with a generic "Invalid token" message.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/login/validate-session")]
        public IActionResult ValidateSession()
        {
            // Extract token from Authorization header
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Unauthorized: No token provided." });
            }

            // Validate token in the database
            var user = _context.Users.FirstOrDefault(u => u.SessionToken == token);
            if (user == null)
            {
                return Unauthorized(new { message = "Unauthorized: Invalid token." });
            }

            // Decode the token to check expiration
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("Chave_secreta_pertencente_a_vital_ease");

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // Disable clock skew
                }, out SecurityToken validatedToken);

                // If token is valid, return success response
                return Ok(new
                {
                    message = "Session is valid.",
                    user = new
                    {
                        user.Id,
                        user.Email,
                        user.Type
                    }
                });
            }
            catch (SecurityTokenExpiredException)
            {
                // Token has expired, update database
                user.SessionToken = null; // Invalidate the session
                user.SessionTokenCreatedAt = null; // Clear the creation timestamp
                _context.Users.Update(user);
                _context.SaveChanges();

                return Unauthorized(new { message = "Unauthorized: Token expired." });
            }
            catch (Exception)
            {
                return Unauthorized(new { message = "Unauthorized: Invalid token." });
            }

        }

        /// <summary>
        /// Sends an email notification to inform the recipient that their account is blocked.
        /// </summary>
        /// <param name="toEmail">The email address of the recipient.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that represents the asynchronous operation. 
        /// Returns <c>true</c> if the email was sent successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves email settings (such as the sender's email, SMTP server, port, and credentials) from the configuration.
        /// It validates that the sender's email is correctly configured and that the SMTP port is a valid number.
        /// An email message is then constructed with the subject "Account Blocked" and a body notifying the recipient
        /// that their account is blocked for 15 minutes. The message is sent securely using SSL via an <see cref="SmtpClient"/>.
        /// Any exceptions during the process are caught, the error is logged to the console, and the method returns <c>false</c>.
        /// </remarks>
        private async Task<bool> SendBlockedInformationEmail(string toEmail)
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
                    Subject = "Account Blocked",
                    Body = "Your account is blocked! Wait 15 minutes for it to be unblocked automatically",
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
        /// This method attempts to create a new instance of the <see cref="System.Net.Mail.MailAddress"/> class
        /// using the provided email address. If the instance is created successfully and the generated address
        /// matches the input, the email is considered valid. If an exception occurs during this process,
        /// the method returns <c>false</c>.
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


    }
}