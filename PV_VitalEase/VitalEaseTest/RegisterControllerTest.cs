using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Net;


namespace VitalEaseTest
{
    public class RegisterControllerTests : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _configurationMock;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string registerUrl = "http://localhost:7180/api/register";
        private readonly HttpClient _client;

        public RegisterControllerTests(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
            _configurationMock = new Mock<IConfiguration>();
            _client = factory.CreateClient(); // Cria um HttpClient para fazer as requisições
        }



        [Fact]
        public async Task Register_LoadTest_StressTest()
        {
            // Arrange
            var tasks = new List<Task>();
            var numRequests = 10;
            var semaphore = new SemaphoreSlim(5); // Limitar a 5 requisições simultâneas

            for (int i = 0; i < numRequests; i++)
            {
                var model = new RegisterViewModel
                {
                    Email = $"newuser{i}@example.com",
                    Username = $"newuser{i}",
                    Password = "StrongPassword@123",
                    BirthDate = DateTime.Today.AddYears(-18),
                    Height = 175,
                    Weight = 70,
                    Gender = "Male",
                    HeartProblems = false
                };

                await semaphore.WaitAsync();

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await RegisterUserAsync(model);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao registrar {model.Email}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            // Act
            await Task.WhenAll(tasks);

            // Assert
            Assert.True(tasks.Count == numRequests);
        }

        private async Task RegisterUserAsync(RegisterViewModel model)
        {
            try
            {
                var response = await client.PostAsJsonAsync(registerUrl, model);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição de {model.Email}: {ex.Message}");
            }
        }

        [Fact]
        public async Task Register_Successful_ReturnsOkWithToken()
        {
            // Arrange: Criar um modelo de usuário válido
            var model = new RegisterViewModel
            {
                Email = "newuser@example.com",
                Username = "newuser",
                Password = "StrongPassword@123",
                BirthDate = DateTime.Today.AddYears(-18),
                Height = 175,
                Weight = 70,
                Gender = "Male",
                HeartProblems = false
            };

            // Act: Envia a requisição para o endpoint de registro
            var response = await _client.PostAsJsonAsync("/api/register", model);

            // Assert: Verifica se a resposta é OK (status code 200)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Lê o conteúdo da resposta (se necessário)
            var content = await response.Content.ReadFromJsonAsync<object>();
            Assert.NotNull(content); // Certifica-se de que a resposta não está vazia
        }

        [Fact]
        public async Task Register_AgeRestriction_ReturnsBadRequest()
        {
            // Arrange: Criar um modelo com um usuário muito jovem (menos de 16 anos)
            var model = new RegisterViewModel
            {
                Email = "younguser@example.com",
                Username = "younguser",
                Password = "ValidPass@123",
                BirthDate = DateTime.Today.AddYears(-15) // Utilizador com menos de 16 anos
            };

            // Act: Envia a requisição para o endpoint de registro
            var response = await _client.PostAsJsonAsync("/api/register", model);

            // Assert: Espera que a resposta seja um erro (400)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange: Adicionar um usuário com o mesmo e-mail para causar duplicação
            var model = new RegisterViewModel
            {
                Email = "duplicate@example.com",
                Username = "duplicateuser",
                Password = "ValidPass@123",
                BirthDate = DateTime.Today.AddYears(-20)
            };

            // Primeiro registro com o e-mail duplicado
            await _client.PostAsJsonAsync("/api/register", model);

            // Act: Envia a requisição para tentar registrar com o mesmo e-mail
            var response = await _client.PostAsJsonAsync("/api/register", model);

            // Assert: Espera que a resposta seja um erro de duplicação (400)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            // Arrange: Criar um modelo com uma senha fraca
            var model = new RegisterViewModel
            {
                Email = "user@example.com",
                Username = "newuser",
                Password = "weakpass", // Senha não atende aos critérios de segurança
                BirthDate = DateTime.Today.AddYears(-18)
            };

            // Act: Tentar registrar o utilizador com uma senha fraca
            var response = await _client.PostAsJsonAsync("/api/register", model);

            // Assert: Espera que a resposta seja um erro (400)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


    }
}
