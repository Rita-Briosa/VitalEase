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
    /// Controlador responsável pelas operações relacionadas com as contas de utilizador.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Contexto da base de dados VitalEaseServerContext, utilizado para operações de acesso à base de dados.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Interface de configuração, utilizada para aceder às definições da aplicação.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="AccountController"/>.
        /// </summary>
        /// <param name="context">O contexto da base de dados <see cref="VitalEaseServerContext"/>.</param>
        /// <param name="configuration">A interface de configuração <see cref="IConfiguration"/> da aplicação.</param>
        public AccountController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Processa o pedido de login do utilizador.
        /// </summary>
        /// <param name="model">Modelo de dados de login contendo as credenciais do utilizador e a opção "Remember Me".</param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que pode ser:
        /// - <c>BadRequest</c> se os dados forem inválidos ou se o email não estiver verificado;
        /// - <c>Unauthorized</c> se o email ou a password estiverem incorretos ou se a conta estiver bloqueada;
        /// - <c>Ok</c> com os dados do utilizador e o token JWT, se o login for bem-sucedido.
        /// </returns>
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
        /// Calcula o hash SHA256 da palavra-passe fornecida.
        /// </summary>
        /// <param name="password">A palavra-passe a encriptar.</param>
        /// <returns>Uma string que representa o hash SHA256 da palavra-passe em formato hexadecimal.</returns>
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
        /// Regista uma acção de auditoria, gravando-a na base de dados.
        /// </summary>
        /// <param name="action">A acção que foi realizada.</param>
        /// <param name="status">O estado ou resultado da acção.</param>
        /// <param name="UserId">O identificador do utilizador associado à acção.</param>
        /// <returns>Uma Task que representa a operação assíncrona de gravação do log.</returns>
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
        /// Gera um token JWT para o utilizador especificado.
        /// </summary>
        /// <param name="user">O objeto utilizador para o qual se pretende gerar o token.</param>
        /// <param name="rememberMe">
        /// Indica se a sessão deve ser prolongada (opção "Remember Me").
        /// Se definido como true, o token expira após 30 dias; caso contrário, expira após 15 minutos.
        /// </param>
        /// <returns>Uma string contendo o token JWT gerado.</returns>
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
                    : DateTime.UtcNow.AddMinutes(15), // Standard session: 15 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Valida a sessão do utilizador através do token JWT presente no cabeçalho Authorization.
        /// </summary>
        /// <remarks>
        /// Este método realiza os seguintes passos:
        /// <list type="bullet">
        ///   <item>
        ///     Extrai o token do cabeçalho Authorization da requisição. Se o token não for fornecido, retorna Unauthorized.
        ///   </item>
        ///   <item>
        ///     Verifica na base de dados se existe um utilizador com o token de sessão correspondente. Se não encontrar, retorna Unauthorized.
        ///   </item>
        ///   <item>
        ///     Valida o token utilizando o <see cref="JwtSecurityTokenHandler"/> e parâmetros de validação, sem permitir margem de erro (ClockSkew zero).
        ///   </item>
        ///   <item>
        ///     Se o token for válido, retorna um objeto Ok com os dados do utilizador.
        ///   </item>
        ///   <item>
        ///     Se o token tiver expirado (lançando <see cref="SecurityTokenExpiredException"/>), invalida a sessão na base de dados e retorna Unauthorized.
        ///   </item>
        ///   <item>
        ///     Em caso de qualquer outra exceção, retorna Unauthorized indicando um token inválido.
        ///   </item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica se a sessão é válida ou não, juntamente com os dados do utilizador em caso afirmativo.
        /// </returns>
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
        /// Envia um email informativo ao utilizador, avisando que a sua conta está bloqueada.
        /// </summary>
        /// <param name="toEmail">O endereço de email do destinatário.</param>
        /// <returns>
        /// Um <see cref="Task{bool}"/> que resulta em <c>true</c> se o email for enviado com sucesso; caso contrário, <c>false</c>.
        /// </returns>
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


    }
}