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

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Retorna a view com mensagens de erro
            }

            // Verificar credenciais
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
                return View(model); // Exibe mensagem de erro
            }

            // Ir para página principal
            return RedirectToAction("Index", "Account");
        }
    }
}