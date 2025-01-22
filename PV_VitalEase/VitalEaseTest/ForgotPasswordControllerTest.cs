using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;

namespace VitalEaseTest
{
    public class ForgotPasswordControllerTest : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _mockConfiguration;
        public ForgotPasswordControllerTest(VitalEaseContextFixture fixture)
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
                    Password = "dfssdfsdf", // Use a senha "hasheada" aqui, pois o código de produção verifica o hash
                    Type = UserType.Standard // Altere conforme o tipo de usuário desejado
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }
       
        }

        [Fact]
        public async Task ForgotPassword_InvalidEmailFormat_ReturnsBadRequest()
        {

            var controller = new ForgotPasswordController(_context, _mockConfiguration.Object);

            var invalidEmailModel = new ForgotPasswordViewModel { Email = "invalid-email" };

            var result = await controller.ForgotPassword(invalidEmailModel);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("Invalid email format.", message);

        }

        [Fact]
        public async Task ForgotPassword_UserNotFound_ReturnsNotFound()
        {
            var controller = new ForgotPasswordController(_context, _mockConfiguration.Object);

            var userNotFoundModel = new ForgotPasswordViewModel { Email = "user@gmail.com" };

            var result = await controller.ForgotPassword(userNotFoundModel);

            var notFoundbadRequestResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundbadRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("User not found.", message);

        }

        [Fact]
        public async Task ForgotPassword_EmailSendFails_ReturnsServerError()
        {
            var controller = new ForgotPasswordController(_context, _mockConfiguration.Object);

            var emailSendFailedModel = new ForgotPasswordViewModel { Email = "z.lucio@outlook.com" };

            var result = await controller.ForgotPassword(emailSendFailedModel);

            var emailErrorRequestResult = Assert.IsType<ObjectResult>(result);
            var response = emailErrorRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("Failed to send reset password email.", message);
        }

        [Fact]
        public async Task ForgotPassword_EmailSentSuccessfully_ReturnsOk()
        {
            _mockConfiguration.Setup(c => c["EmailSettings:FromEmail"]).Returns("easevital939@gmail.com");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.gmail.com");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpPort"]).Returns("587");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpUsername"]).Returns("easevital939@gmail.com");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpPassword"]).Returns("lqpd lgct aycu kpnx");

            var controller = new ForgotPasswordController(_context, _mockConfiguration.Object);

            var emailSendedModel = new ForgotPasswordViewModel { Email = "z.lucio@outlook.com" };

            var result = await controller.ForgotPassword(emailSendedModel);

            var emailOkRequestResult = Assert.IsType<OkObjectResult>(result);
            var response = emailOkRequestResult.Value;

            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();


            Assert.NotNull(message);
            Assert.Equal("Password reset instructions have been sent to your email.", message);

            _mockConfiguration.Setup(c => c["EmailSettings:FromEmail"]).Returns("test@example.com");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.example.com");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpPort"]).Returns("587");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpUsername"]).Returns("smtpUser");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpPassword"]).Returns("smtpPassword");
        }
    }
}
