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
    public class VerifyEmailController : Controller
    {
        private readonly VitalEaseServerContext _context;
        private readonly IConfiguration _configuration;

        public VerifyEmailController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

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
        /// Valida o token JWT.
        /// </summary>
        /// <param name="token">Token a ser validado.</param>
        /// <returns>Uma tupla com: se é válido, email e userId.</returns>
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
