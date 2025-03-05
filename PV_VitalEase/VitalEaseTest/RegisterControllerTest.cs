using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace VitalEaseTest
{
    // Classe de testes para o RegisterController
    public class RegisterControllerTests : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _configurationMock;

        // Construtor que recebe um fixture para reutilizar o contexto da base de dados em memória
        public RegisterControllerTests(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
            _configurationMock = new Mock<IConfiguration>(); // Mock do IConfiguration
        }

        [Fact]
        public async Task Register_Successful_ReturnsOkWithToken()
        {
            // Arrange: Criar o controlador e um modelo de utilizador válido
            var controller = new RegisterController(_context, _configurationMock.Object);
            var model = new RegisterViewModel
            {
                Email = "newuser@example.com",
                Username = "newuser",
                Password = "StrongPassword@123",
                BirthDate = DateTime.Today.AddYears(-18), // utilizador tem idade válida
                Height = 175,
                Weight = 70,
                Gender = "Male",
                HeartProblems = false
            };

            // Act: Chamar o método Register
            var result = await controller.Register(model);

            // Assert: Verificar se o retorno é um código 200 (Ok)
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;

            Assert.NotNull(response); // O resultado não deve ser nulo
        }

        [Fact]
        public async Task Register_AgeRestriction_ReturnsBadRequest()
        {
            // Arrange: Criar o controlador e um modelo com idade inválida (< 16 anos)
            var controller = new RegisterController(_context, _configurationMock.Object);
            var model = new RegisterViewModel
            {
                Email = "younguser@example.com",
                Username = "younguser",
                Password = "ValidPass@123",
                BirthDate = DateTime.Today.AddYears(-15) // Utilizador tem menos de 16 anos
            };

            // Act: Chamar o método Register
            var result = await controller.Register(model);

            // Assert: Deve retornar erro 400 (BadRequest)
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You must be at least 16 years old to register.",
                badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value));
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange: Criar um utilizador existente na base de dados
            var existingUser = new User
            {
                Email = "duplicate@example.com",
                Password = "hashedpassword",
                Type = UserType.Standard
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var controller = new RegisterController(_context, _configurationMock.Object);

            // Criar um novo utilizador com o mesmo email
            var model = new RegisterViewModel
            {
                Email = "duplicate@example.com", // Mesmo email já com login
                Username = "uniqueuser",
                Password = "ValidPass@123",
                BirthDate = DateTime.Today.AddYears(-20)
            };

            // Act: Tentar registrar um utilizador com email duplicado
            var result = await controller.Register(model);

            // Assert: Deve retornar erro 400 (BadRequest) com a mensagem apropriada
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email already exists",
                badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value));
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            // Arrange: Criar o controlador e um modelo com uma senha fraca
            var controller = new RegisterController(_context, _configurationMock.Object);
            var model = new RegisterViewModel
            {
                Email = "user@example.com",
                Username = "newuser",
                Password = "weakpass", // Senha não atende aos critérios de segurança
                BirthDate = DateTime.Today.AddYears(-18)
            };

            // Act: Tentar registrar o utilizador com uma senha fraca
            var result = await controller.Register(model);

            // Assert: Deve retornar erro 400 (BadRequest) com a mensagem apropriada
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password does not meet the required criteria.",
                badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value));
        }
    }
}
