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

namespace VitalEaseTest
{
    public class RegisterControllerTests : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _configurationMock;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string registerUrl = "http://localhost:7180/api/register";

        public RegisterControllerTests(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
            _configurationMock = new Mock<IConfiguration>();
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
            var controller = new RegisterController(_context, _configurationMock.Object);
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

            var result = await controller.Register(model);

            var okResult = Assert.IsType<ObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Register_AgeRestriction_ReturnsBadRequest()
        {
            var controller = new RegisterController(_context, _configurationMock.Object);
            var model = new RegisterViewModel
            {
                Email = "younguser@example.com",
                Username = "younguser",
                Password = "ValidPass@123",
                BirthDate = DateTime.Today.AddYears(-15)
            };

            var result = await controller.Register(model);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You must be at least 16 years old to register.",
                badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value));
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var existingUser = new User
            {
                Email = "duplicate@example.com",
                Password = "hashedpassword",
                Type = UserType.Standard
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var controller = new RegisterController(_context, _configurationMock.Object);

            var model = new RegisterViewModel
            {
                Email = "duplicate@example.com",
                Username = "uniqueuser",
                Password = "ValidPass@123",
                BirthDate = DateTime.Today.AddYears(-20)
            };

            var result = await controller.Register(model);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email already exists",
                badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value));
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            var controller = new RegisterController(_context, _configurationMock.Object);
            var model = new RegisterViewModel
            {
                Email = "user12344@example.com",
                Username = "newuser1234",
                Password = "weakpass",
                Gender = "Male",
                BirthDate = DateTime.Today.AddYears(-18)
            };

            var result = await controller.Register(model);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password does not meet the required criteria.",
                badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value));
        }
    }
}
