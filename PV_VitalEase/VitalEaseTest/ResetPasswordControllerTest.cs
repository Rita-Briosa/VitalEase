using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;

namespace VitalEaseTest
{
    public class ResetPasswordControllerTest : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _mockConfiguration;
        
        public ResetPasswordControllerTest(VitalEaseContextFixture fixture)
        {
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["EmailSettings:FromEmail"]).Returns("test@example.com");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.example.com");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpPort"]).Returns("587");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpUsername"]).Returns("smtpUser");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpPassword"]).Returns("smtpPassword");
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("SuperSecretJwtKeysupersupersupersupersupersupersupersupersupersupersupersupersuper");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            _context = fixture.VitalEaseTestContext;

            if (!_context.Users.Any())
            {
                var user = new User
                {
                    Email = "z.lucio@outlook.com",
                    Password = "13004d8331d779808a2336d46b3553d1594229e2bb696a8e9e14554d82a648da", // Use a senha "hasheada" aqui, pois o código de produção verifica o hash
                    Type = UserType.Standard // Altere conforme o tipo de usuário desejado
                };

                _context.Users.Add(user);
              
            }

            _context.SaveChanges();
        }

        [Fact]
        public async Task ResetPassword_InvalidModel_ReturnsBadRequest()
        {
            var model = new ResetPasswordViewModel
            {
                Token = "",
                NewPassword = ""
            };

            var controller = new ResetPasswordController(_context, _mockConfiguration.Object);
            var result = await controller.ResetPassword(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("Invalid parameters.Token and Password are required.", message);
        }

        [Fact]
        public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
        {
            var model = new ResetPasswordViewModel
            {
                Token = "invalidToken",
                NewPassword = "ValidPassword123!"
            };

            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("test_super_secret_key");

            var controller = new ResetPasswordController(_context, _mockConfiguration.Object);
            var result = await controller.ResetPassword(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("Invalid or expired token.", message);
        }

        [Fact]
        public async Task ResetPassword_UserNotFound_ReturnsNotFound()
        {

            var userEmail = "test.test@outlook.com";
            var userId = 2;

            var validToken = GenerateToken(userEmail, userId);

            var model = new ResetPasswordViewModel
            {
                Token = validToken,
                NewPassword = "ValidPassword1234!"
            };

            var controller = new ResetPasswordController(_context, _mockConfiguration.Object);

            var result = await controller.ResetPassword(model);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("User not found.", message);
        }

        private string GenerateToken(string email, int userId)
        {
            var jwtKey = _mockConfiguration.Object["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }

            var tokenId = Guid.NewGuid().ToString();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.Now.AddMinutes(30);
            var token = new JwtSecurityToken(
                issuer: _mockConfiguration.Object["Jwt:Issuer"],
                audience: _mockConfiguration.Object["Jwt:Audience"],
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

        [Fact]
        public async Task ResetPassword_InvalidPasswordCriteria_ReturnsBadRequest()
        {
            var userEmail = "z.lucio@outlook.com";
            var userId = _context.Users.First(u => u.Email == userEmail).Id;

            var validToken = GenerateToken(userEmail, userId);

            var model = new ResetPasswordViewModel
            {
                Token = validToken,
                NewPassword = "short"
            };

            var controller = new ResetPasswordController(_context, _mockConfiguration.Object);
            var result = await controller.ResetPassword(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("Password does not meet the required criteria.", message);
        }

        [Fact]
        public async Task ResetPassword_SameAsOldPassword_ReturnsBadRequest()
        {
            var userEmail = "z.lucio@outlook.com";
            var userId = _context.Users.First(u => u.Email == userEmail).Id;

            var validToken = GenerateToken(userEmail, userId);

            var model = new ResetPasswordViewModel
            {
                Token = validToken,
                NewPassword = "Password1234!"
            };

            var controller = new ResetPasswordController(_context, _mockConfiguration.Object);
            var result = await controller.ResetPassword(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("New password cannot be the same as the old one.", message);
        }

        [Fact]
        public async Task ResetPassword_ValidRequest_UpdatesPassword()
        {
            var userEmail = "z.lucio@outlook.com";
            var userId = _context.Users.First(u => u.Email == userEmail).Id;

            var validToken = GenerateToken(userEmail, userId);

            var model = new ResetPasswordViewModel
            {
                Token = validToken,
                NewPassword = "Password123!"
            };

            var controller = new ResetPasswordController(_context, _mockConfiguration.Object);
            var result = await controller.ResetPassword(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("Your password has been reset successfully.", message);
        }
    }
}
