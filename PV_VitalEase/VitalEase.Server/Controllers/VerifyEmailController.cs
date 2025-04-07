using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;

namespace VitalEase.Server.Controllers
{
    /// <summary>
    /// Controller responsible for verifying users' email addresses.
    /// </summary>
    public class VerifyEmailController : Controller
    {
        /// <summary>
        /// VitalEaseServerContext database context, used to access the application's data.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Configuration interface, used to access the application's settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyEmailController"/> controller.
        /// </summary>
        /// <param name="context">
        /// The database context (<see cref="VitalEaseServerContext"/>) that enables data access operations.
        /// </param>
        /// <param name="configuration">
        /// The configuration interface (<see cref="IConfiguration"/>) used to access the application's settings.
        /// </param>
        public VerifyEmailController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        /// <summary>
        /// Validates the email verification token and updates the user's email status if the token is valid.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="VerifyEmailViewModel"/> that contains the email verification token sent in the query string.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     An <c>Ok</c> result with a success message and the verified email, if the token is valid and the email has not yet been verified.
        ///   </item>
        ///   <item>
        ///     A <c>BadRequest</c> result if the token has expired or if the email has already been verified.
        ///   </item>
        /// </list>
        /// </returns>
        [HttpGet("api/ValidateVerifyEmailToken")]
        public IActionResult ValidateVerifyEmailToken([FromQuery] VerifyEmailViewModel model)
        {
            var (isValid, email) = ValidateToken(model.Token);

            if (!isValid)
            {

                return BadRequest(new { message = "Token expired." });
            }

            var user =  _context.Users.FirstOrDefault(u => u.Email == email);

            
            if (user != null && user.IsEmailVerified !=true)
            {
                
                user.IsEmailVerified = true;
            }
            else
            {
                return BadRequest(new { message = "Email já verificado." });
            }

            _context.SaveChanges();
            return Ok(new { message = "Token is valid.", email});
        }

        /// <summary>
        /// Validates the JWT token and extracts the email contained in its claims.
        /// </summary>
        /// <param name="token">
        /// The JWT token to be validated.
        /// </param>
        /// <returns>
        /// A tuple with a boolean indicating whether the token is valid and, if so, the email extracted from the claims.
        /// If the token is invalid or expired, returns (false, "").
        /// </returns>
        private (bool IsValid, string Email) ValidateToken(string token)
        {
            var key = Encoding.UTF8.GetBytes("Chave_secreta_pertencente_a_vital_easee_e_impenetravel");
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validar o token com os parâmetros necessários
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validar a chave de assinatura
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Usar a chave para validação
                    ValidateIssuer = false, // Desabilitar validação de emissor
                    ValidateAudience = false, // Desabilitar validação de público
                    ClockSkew = TimeSpan.FromMinutes(5) // Margem de 5 minutos para expiração
                }, out _);

                // Obter o email do token
                var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return (false, "");
                }

                return (true, email);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "");
            }
            catch (SecurityTokenException)
            {
                // Token inválido
                return (false, "");
            }
            catch (Exception ex)
            {
                // Outros erros
                Console.WriteLine($"Erro ao validar o token: {ex.Message}");
                return (false, "");
            }
        }
    }
}
