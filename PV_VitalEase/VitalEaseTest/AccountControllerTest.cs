using Microsoft.AspNetCore.Mvc;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;
using Microsoft.Extensions.Configuration;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing.Printing;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;


namespace VitalEaseTest
{
    public class AccountControllerTests : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _configurationMock;

        public AccountControllerTests(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
            _configurationMock = new Mock<IConfiguration>(); // Mock do IConfiguration

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

        // Teste de Carga
        [Fact]
        public async Task Login_LoadTest_SuccessfulLogin_ReturnsOkWithToken()
        {
            // Número de utilizadores virtuais (VUs) para simular no teste de carga
            int vus = 50;  // Ajuste para o número desejado de utilizadores virtuais
            int durationInSeconds = 30;  // Duração do teste de carga em segundos

            var tasks = new List<Task>();

            using (var client = new HttpClient())
            {
                for (int i = 0; i < vus; i++)
                {
                    tasks.Add(SimulateLoginRequest(client));
                }

                // Aguardar todas as tarefas (requisições) serem completadas
                await Task.WhenAll(tasks);
            }

            Console.WriteLine("Teste de carga concluído.");
        }

        // Função para simular uma requisição de login de utilizador
        private async Task SimulateLoginRequest(HttpClient client)
        {
            // Simulando login com email e senha válidos
            var model = new LoginViewModel { Email = "user@example.com", Password = "Password123!" };

            var response = await client.PostAsJsonAsync("https://localhost:7180/api/account/login", model); // Ajuste a URL conforme necessário

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseJson = JObject.Parse(responseBody);
                var message = responseJson["message"].ToString();
                Console.WriteLine($"Login bem-sucedido: {message}");
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Resposta bruta: " + responseBody);

                if (!string.IsNullOrWhiteSpace(responseBody) && responseBody.Trim().StartsWith("{"))
                {
                    try
                    {
                        var responseJson = JObject.Parse(responseBody);
                        var message = responseJson["message"]?.ToString();

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"✅ Login bem-sucedido: {message}");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Falha no login: {message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erro ao fazer parse do JSON: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ A resposta não é um JSON válido ou está vazia.");
                }

            }

            // Espera de 1 segundo entre as requisições
            await Task.Delay(1000);
        }



        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AccountController(_context, _configurationMock.Object);

            var model = new LoginViewModel { Email = "user1234@test.com", Password = "hashed_password" }; // Email inexistente

            // Act
            var result = await controller.Login(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result); // O tipo correto é UnauthorizedObjectResult

            // O valor de UnauthorizedObjectResult é um objeto simples, não um JsonResult
            var response = unauthorizedResult.Value; // A resposta é um dicionário


            Assert.NotNull(response); // Verifique se a resposta não é nula

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();

            // Verifique se o valor da chave "message" é o esperado
            Assert.NotNull(message); // Verifique se o valor da chave "message" não é nulo
            Assert.Equal("Email is incorrect", message); // Compare com a string correta
        }


        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorized()
        {
            VitalEaseContextFixture fixture = new VitalEaseContextFixture();
            var context = fixture.VitalEaseTestContext;
            // Arrange
            var controller = new AccountController(context, _configurationMock.Object);

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
            var controller = new AccountController(_context, _configurationMock.Object);


            var auditLogs = new List<AuditLog>
            {
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-7) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-6) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-5) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-4) },
                new AuditLog { UserId = 1, Action = "Login Attempt", Status = "Failed - Password Incorrect", Timestamp = DateTime.Now.AddMinutes(-3) }
            };

            _context.AuditLogs.AddRange(auditLogs);
            await _context.SaveChangesAsync();

            var model = new LoginViewModel { Email = "z.lucio@outlook.com", Password = "wrong_password" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var unauthorizedResult = Assert.IsType<ObjectResult>(result);
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
            var controller = new AccountController(_context, _configurationMock.Object);

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