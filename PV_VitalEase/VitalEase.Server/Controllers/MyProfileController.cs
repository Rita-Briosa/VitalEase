using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;

namespace VitalEase.Server.Controllers
{
    public class MyProfileController : Controller
    {
        private readonly VitalEaseServerContext _context;

        public MyProfileController(VitalEaseServerContext context)
        {
            _context = context;
        }

        [HttpGet("api/getProfileInfo/{email}")]
        public async Task<IActionResult> GetProfileInfo(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                email = Uri.UnescapeDataString(email);

                // Busca o perfil do usuário pelo email e inclui o perfil
                var userProfile = await _context.Users
                    .Include(u => u.Profile) // <- Isso garante que o Profile seja carregado
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (userProfile == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (userProfile.Profile == null)
                {
                    return NotFound(new { message = "Profile not found" });
                }

                var data = new
                {
                    username = userProfile.Profile.Username,
                    birthDate = userProfile.Profile.Birthdate.ToString("yyyy-MM-dd"),
                    weight = userProfile.Profile.Weight,
                    height = userProfile.Profile.Height,
                    gender = userProfile.Profile.Gender,
                    hasHeartProblems = userProfile.Profile.HasHeartProblems,
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching profile", error = ex.Message });
            }
        }

        [HttpPost("api/changeBirthDate")]
        public async Task<IActionResult> ChangeBirthDate([FromBody] ChangeBirthDateViewModel model)
        {
            try {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Birth date change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile) 
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Birth date change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Birth date change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                if (model.BirthDate > DateTime.Today.AddYears(-16))
                {
                    await LogAction("Birth date change Attempt", "Failed - Age restriction", userProfile.Id);
                    return BadRequest(new { message = "You must be at least 16 years old to register." });
                }

                userProfile.Profile.Birthdate = model.BirthDate;
                await _context.SaveChangesAsync();


                return Ok(new {message = "Birth date changed sucefully!!"});
            }
            catch (Exception ex) {
                return BadRequest(new { message = "Error changing birth date", error = ex.Message });
            }
        }

        [HttpPost("api/changeWeight")]
        public async Task<IActionResult> ChangeWeight([FromBody] ChangeWeightViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Weight change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Weight change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Weight change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                userProfile.Profile.Weight = model.Weight;
                await _context.SaveChangesAsync();


                return Ok(new { message = "Weight changed sucefully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Weight", error = ex.Message });
            }
        }

        [HttpPost("api/changeHeight")]
        public async Task<IActionResult> ChangeHeight([FromBody] ChangeHeightViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Height change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Height change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Height change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                userProfile.Profile.Height = model.Height;
                await _context.SaveChangesAsync();


                return Ok(new { message = "Height changed sucefully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Height", error = ex.Message });
            }
        }

        [HttpPost("api/changeGender")]
        public async Task<IActionResult> ChangeGender([FromBody] ChangeGenderViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Gender change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Gender change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Gender change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }


                userProfile.Profile.Gender = model.Gender;
                await _context.SaveChangesAsync();


                return Ok(new { message = "Gender changed sucefully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Gender", error = ex.Message });
            }
        }

        [HttpPost("api/changeHasHeartProblems")]
        public async Task<IActionResult> ChangeHasHeartProblems([FromBody] ChangeHasHeartProblemsViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Has heart problems change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Has heart problems change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Has heart problems Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                userProfile.Profile.HasHeartProblems = model.HasHeartProblems;
                await _context.SaveChangesAsync();


                return Ok(new { message = "Has heart problems changed sucefully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Has heart problems", error = ex.Message });
            }
        }

        [HttpPost("api/changeUsername")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Username change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Username change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Username change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                var isUnique = await _context.Profiles.FirstOrDefaultAsync(u => u.Username == model.Username);

                if(isUnique != null)
                {
                    await LogAction("Username change Attempt", "Failed - Username Already Exists", userProfile.Id);
                    return NotFound(new { message = "Username Already Exists" });
                }

                userProfile.Profile.Username = model.Username;
                await _context.SaveChangesAsync();


                return Ok(new { message = "Username changed sucefully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Username", error = ex.Message });
            }
        }

        [HttpPost("api/changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Password change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var hashedOldPassword = HashPassword(model.OldPassword);
                var user = await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedOldPassword);

                if (user == null)
                {
                    await LogAction("Password change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (!IsPasswordValid(model.NewPassword))
                {
                    await LogAction("Password change Attempt", "Failed - Password is not valid", 0);
                    return Unauthorized(new { message = "Password is not valid" });
                }

                var newHashedPassword = HashPassword(model.NewPassword);

                if(hashedOldPassword == newHashedPassword)
                {
                    await LogAction("Password change Attempt", "Failed - Password can not be the same as old one.", 0);
                    return Unauthorized(new { message = "Password can not be the same as old one." });
                }


                user.Password = newHashedPassword;
                await _context.SaveChangesAsync();


                return Ok(new { message = "Password changed sucefully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Password", error = ex.Message });
            }
        }

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
    }
}