using Microsoft.AspNetCore.Mvc;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing.Printing;
using Newtonsoft.Json.Linq;

namespace VitalEaseTest
{
    public class AccountControllerTests : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;

        public AccountControllerTests(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;

            if (!_context.Users.Any())
            {
                var user = new User
                {
                    Email = "z.lucio@outlook.com",
                    Password = "Password123!", // Use a senha "hasheada" aqui, pois o código de produção verifica o hash
                    Type = UserType.Standard // Altere conforme o tipo de usuário desejado
                };

                var user1 = new User
                {
                    Email = "user@example.com",
                    Password = "13004d8331d779808a2336d46b3553d1594229e2bb696a8e9e14554d82a648da", // Use a senha "hasheada" aqui, pois o código de produção verifica o hash
                    Type = UserType.Admin // Altere conforme o tipo de usuário desejado
                };
               
                _context.Users.Add(user);
                _context.Users.Add(user1);
                _context.SaveChanges();
            }
        }


        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AccountController(_context);

            var model = new LoginViewModel { Email = "user1234@test.com", Password = "hashed_password" }; // Email inexistente

            // Act
            var result = await controller.Login(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result); // O tipo correto é UnauthorizedObjectResult

            // O valor de UnauthorizedObjectResult é um objeto simples, não um JsonResult
            var response = unauthorizedResult.Value; // A resposta é um dicionário

            
            Assert.NotNull(response); // Verifique se a resposta não é nula

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();

            // Verifique se a chave "message" existe e se o valor é o esperado
            Assert.NotNull(message); // Verifique se o valor da chave "message" não é nulo
            Assert.Equal("Email is incorrect", message); // Compare com a string correta
        }

        
        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorized()
        {
            VitalEaseContextFixture fixture = new VitalEaseContextFixture();
            var context = fixture.VitalEaseTestContext;
            // Arrange
            var controller = new AccountController(context);

            var model = new LoginViewModel { Email = "user@example.com", Password = "wrong_password" }; // Senha errada

            // Act
            var result = await controller.Login(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value; // Aqui utilizamos ObjectResult
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();

            Assert.NotNull(message);
            Assert.Equal("Password is incorrect", message);
        }

        
        [Fact]
        public async Task Login_AccountBlocked_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AccountController(_context);


            var auditLogs = new List<AuditLog>
            {
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-5) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-4) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-3) }
            };

            _context.AuditLogs.AddRange(auditLogs);
            await _context.SaveChangesAsync();

            var model = new LoginViewModel { Email = "testuser@example.com", Password = "wrong_password" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value; // Aqui utilizamos ObjectResult
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();

            Assert.NotNull(message);
            Assert.Equal("Account is Blocked. Wait 15 minutes and try again.", message);

            _context.AuditLogs.RemoveRange(auditLogs);
            await _context.SaveChangesAsync();
        }

        
        [Fact]
        public async Task Login_Success_ReturnsOkWithToken()
        {
            // Arrange
            var controller = new AccountController(_context);

            var model = new LoginViewModel { Email = "user@example.com", Password = "Password1234!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value; // Aqui utilizamos ObjectResult
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);
            Assert.Equal("Login successful", message);
            var token = response.GetType().GetProperty("token")?.GetValue(response)?.ToString();
            Assert.NotNull(token);
            var user = response.GetType().GetProperty("user")?.GetValue(response)?.ToString();
            Assert.NotNull(user);
        }
    }
}

