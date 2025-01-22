using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;

namespace VitalEaseTest
{
    public class AuditLogsControllerTest : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;

        public AuditLogsControllerTest(VitalEaseContextFixture fixture)
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

                _context.Users.Add(user);
                _context.SaveChanges();
            }

        }

        [Fact]
        public async Task GetLogs_ReturnsOkWithLogs()
        {
            var auditLogs = new List<AuditLog>
            {
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-5) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-4) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-3) }
            };

            _context.AuditLogs.AddRange(auditLogs);

            await _context.SaveChangesAsync();

            var controller = new AuditLogsController(_context);

            // Act
            var result = await controller.GetLogs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Verifica se o resultado é Ok (200)
            var returnedLogs = Assert.IsAssignableFrom<IEnumerable<AuditLog>>(okResult.Value); // Verifica se os logs são retornados
            Assert.NotNull(returnedLogs);
            Assert.Equal(3, returnedLogs.Count()); // Verifica a quantidade de logs retornados

            _context.AuditLogs.RemoveRange(auditLogs);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetLogs_ReturnsBadRequestOnLogsEmpty()
        {
            var options = new DbContextOptionsBuilder<VitalEaseServerContext>()
                .UseInMemoryDatabase("VitalEaseTest1")  // Nome do banco em memória
                .Options;

            VitalEaseServerContext VitalEaseTestContext1 = new VitalEaseServerContext(options);

            VitalEaseTestContext1.Database.EnsureCreated();

            var controller = new AuditLogsController(VitalEaseTestContext1);

            var result = await controller.GetLogs();

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result); // Verifica se o resultado é Ok (200)
            var returnedLogs = badResult.Value as IEnumerable<AuditLog>; // Converte o valor para uma lista de AuditLog

            Assert.NotNull(returnedLogs); // Verifica se a lista não é nula
            Assert.Empty(returnedLogs); // Verifica se a lista está vazia
        }
    }
}
