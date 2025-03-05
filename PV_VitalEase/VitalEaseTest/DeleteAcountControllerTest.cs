using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using Xunit;

namespace VitalEaseTest
{
    public class DeleteAccountControllerTests
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<ILogger<DeleteAccountController>> _loggerMock;

        public DeleteAccountControllerTests()
        {
            // Cria base de dados em memória para testes
            var options = new DbContextOptionsBuilder<VitalEaseServerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new VitalEaseServerContext(options);
            _loggerMock = new Mock<ILogger<DeleteAccountController>>();
        }

        private DeleteAccountController CreateControllerWithUser(int? userId)
        {
            var controller = new DeleteAccountController(_context, _loggerMock.Object);

            // Se um ID de utilizador for fornecido, adiciona a reivindicação ao utilizador autenticado
            if (userId.HasValue)
            {
                var claims = new[] { new Claim("nameid", userId.Value.ToString()) };
                var identity = new ClaimsIdentity(claims);
                var user = new ClaimsPrincipal(identity);
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            }

            return controller;
        }

        // Testa se um utilizador não autenticado recebe Unauthorized (401)
        [Fact]
        public async Task DeleteAccount_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Cria um controlador sem utilizador autenticado
            var controller = CreateControllerWithUser(null);

            // Executa o método DeleteAccount
            var result = await controller.DeleteAccount();

            // Verifica se retorna Unauthorized (401)
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // Testa se um ID de um utilizador inválido (não numérico) retorna BadRequest (400)
        [Fact]
        public async Task DeleteAccount_InvalidUserId_ReturnsBadRequest()
        {
            // Cria um controlador com um ID inválido (não numérico)
            var controller = CreateControllerWithUser(-1);

            // Executa o método DeleteAccount
            var result = await controller.DeleteAccount();

            // Verifica se retorna BadRequest (400)
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Testa se tentar apagar um utulizador que nao existe retorna NotFound (404)
        [Fact]
        public async Task DeleteAccount_UserNotFound_ReturnsNotFound()
        {
            // Cria um controlador com um ID de utilizador que nao existe na base de dados
            var controller = CreateControllerWithUser(999);

            // Executa o método DeleteAccount
            var result = await controller.DeleteAccount();

            // Verifica se retorna NotFound (404)
            Assert.IsType<NotFoundObjectResult>(result);
        }

        //Testa o delete de uma conta que nao existe e verifica se o utilizador foi removido corretamente
        [Fact]
        public async Task DeleteAccount_UserExists_DeletesAccountAndReturnsOk()
        {
            // Adiciona um utilizador da base de dados para testar a remoção
            var user = new User { Id = 1, Email = "test@example.com", Password = "hashedpassword" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Cria um controlador com o ID do utilizador recém-adicionado
            var controller = CreateControllerWithUser(user.Id);

            // Executa o método DeleteAccount
            var result = await controller.DeleteAccount();

            // Verifica se retorna Ok (200)
            Assert.IsType<OkObjectResult>(result);

            // Verifica se o utilizador foi realmente removido
            var deletedUser = await _context.Users.FindAsync(user.Id);
            Assert.Null(deletedUser);
        }
    }
}
