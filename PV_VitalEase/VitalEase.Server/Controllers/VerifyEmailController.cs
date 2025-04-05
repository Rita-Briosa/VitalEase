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
    /// Controlador responsável pela verificação de email dos utilizadores.
    /// </summary>
    public class VerifyEmailController : Controller
    {
        /// <summary>
        /// Contexto da base de dados VitalEaseServerContext, utilizado para aceder aos dados da aplicação.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Interface de configuração, utilizada para aceder às definições da aplicação.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="VerifyEmailController"/>.
        /// </summary>
        /// <param name="context">
        /// O contexto da base de dados (<see cref="VitalEaseServerContext"/>) que permite efetuar operações de acesso aos dados.
        /// </param>
        /// <param name="configuration">
        /// A interface de configuração (<see cref="IConfiguration"/>) para aceder às definições da aplicação.
        /// </param>
        public VerifyEmailController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Valida o token de verificação de email e atualiza o estado do email do utilizador se o token for válido.
        /// </summary>
        /// <param name="model">
        /// Um objeto <see cref="VerifyEmailViewModel"/> que contém o token de verificação de email enviado na query string.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém:
        /// <list type="bullet">
        ///   <item>
        ///     Um resultado <c>Ok</c> com uma mensagem de sucesso e o email verificado, se o token for válido e o email ainda não tiver sido verificado.
        ///   </item>
        ///   <item>
        ///     Um resultado <c>BadRequest</c> se o token estiver expirado ou se o email já tiver sido verificado.
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
        /// Valida o token JWT e extrai o email contido nos seus claims.
        /// </summary>
        /// <param name="token">
        /// O token JWT a ser validado.
        /// </param>
        /// <returns>
        /// Uma tupla com um booleano indicando se o token é válido e, em caso afirmativo, o email extraído dos claims.
        /// Se o token for inválido ou expirado, retorna (false, "").
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
