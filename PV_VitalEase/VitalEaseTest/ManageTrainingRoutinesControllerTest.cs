using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using VitalEase.Server.Controllers;
using VitalEase.Server.Data;
using VitalEase.Server.Models;
using VitalEase.Server.ViewModel;
using Microsoft.Extensions.Configuration;

using Xunit;
using Moq;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;


namespace VitalEaseTest
{
    public class ManageTrainingRoutinesControllerTest : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _configurationMock;

        public ManageTrainingRoutinesControllerTest(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
            _configurationMock = new Mock<IConfiguration>(); // Mock do IConfiguration
        }

        [Fact]
        public async Task GetExercisesFromRoutine_ReturnsBadRequest_WhenRoutineIdIsInvalid()
        {
            // Arrange
            var controller = new ManageTrainingRoutinesController(_context);
            var invalidRoutineId = "invalid_id"; // ID inválido

            // Act
            var result = await controller.GetExercisesFromRoutine(invalidRoutineId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequest.Value;
            Assert.NotNull(response);

            var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Invalid routine ID", message);
        }

        [Fact]
        public async Task GetExercisesFromRoutine_ReturnsNotFound_WhenNoExercisesFoundForRoutine()
        {
            // Arrange
            var routineId = 1; // Um ID de rotina existente, mas sem exercícios associados

            // Simula a ausência de exercícios para a rotina
            _context.ExerciseRoutines.RemoveRange(_context.ExerciseRoutines);
            await _context.SaveChangesAsync();

            var controller = new ManageTrainingRoutinesController(_context);

            // Act
            var result = await controller.GetExercisesFromRoutine(routineId.ToString());

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value;
            Assert.NotNull(response);

            var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("No exercises found for this routine.", message);
        }

        [Fact]
        public async Task GetExercisesFromRoutine_ReturnsOk_WhenExercisesFoundForRoutine()
        {
            var userId = 1;
            var routine = new Routine
            {
                Id = 1,
                UserId = userId,
                Name = "Custom Routine",
                Type = "Muscle-focused",
                Description = "",
                Needs = "",
                IsCustom = true
            };

            _context.Routines.Add(routine);
            await _context.SaveChangesAsync();

            // Simula a presença de exercícios associados à rotina
            var exercise = new Exercise
            {
                Id = 1,
                Name = "Push-Up",
                Description = "Upper body exercise",
                Type = "Strength",
                DifficultyLevel = RoutineLevel.Intermediate,
                MuscleGroup = "Chest",
                EquipmentNecessary = "None"
            };

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            var exerciseRoutine = new ExerciseRoutine
            {
                RoutineId = routine.Id,
                ExerciseId = exercise.Id,
                Reps = 12,
                Sets = 3,
            };

            _context.ExerciseRoutines.Add(exerciseRoutine);
            await _context.SaveChangesAsync();

            var controller = new ManageTrainingRoutinesController(_context);

            // Act
            var result = await controller.GetExercisesFromRoutine(routine.Id.ToString());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Exercises Fetched successfully!", message);

        }
    }
}
