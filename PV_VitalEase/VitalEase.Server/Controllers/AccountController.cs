using Microsoft.AspNetCore.Mvc;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;

namespace VitalEase.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly VitalEaseServerContext _context;

        public AccountController(VitalEaseServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            // Verifica se os dados enviados são válidos
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            // Verifica se o usuário existe e a senha está correta
            if (user == null || user.Password != model.Password)
            {
                return Unauthorized(new { message = "Email or password is incorrect" }); // Retorna erro 401
            }

            // Supondo que você queira retornar um token ou informações do usuário
            var userInfo = new
            {
                userId = user.Id,
                email = user.Email,
                type = user.Type // Tipo de usuário
            };

            return Ok(new
            {
                message = "Login successful",
                user = userInfo
            }); // Retorna 200 com dados do usuário
        }
    }
}