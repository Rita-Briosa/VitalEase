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
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace VitalEaseTest
{
    public class ExerciseControllerTest : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;
        private readonly Mock<IConfiguration> _configurationMock;

        public ExerciseControllerTest(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
            _configurationMock = new Mock<IConfiguration>(); // Mock do IConfiguration
        }

        [Fact]
        public async Task GetExercises_ReturnsOk_WhenExercisesExist()
        {
            // Arrange
            _context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Push-Up",
                Description = "Upper body exercise",
                Type = "Muscle-focused",
                DifficultyLevel = RoutineLevel.Intermediate,
                MuscleGroup = "Chest",
                EquipmentNecessary = "N/A"
            });
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetExercises();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(returnedList);
        }

        [Fact]
        public async Task GetExercises_ReturnsBadRequest_WhenNoExercisesExist()
        {
            // Arrange
            _context.Exercises.RemoveRange(_context.Exercises);
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetExercises();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<IEnumerable<Exercise>>(badRequest.Value);
            Assert.Empty(returnedList);
        }

        [Fact]
        public async Task GetMedia_ReturnsOk_WhenMediaExistsForExercise()
        {
            // Arrange
            var media = new Media
            {
                Id = 1,
                Name = "Push-up video",
                Url = "http://example.com/video.mp4",
                Type = "Video"
            };

            var exercise = new Exercise
            {
                Id = 1,
                Name = "Push-Up",
                Description = "Upper body exercise",
                Type = "Muscle-focused",
                DifficultyLevel = RoutineLevel.Intermediate,
                MuscleGroup = "Chest",
                EquipmentNecessary = "N/A"
            };

            var exerciseMedia = new ExerciseMedia
            {
                ExerciseId = exercise.Id,
                MediaId = media.Id
            };

            _context.Media.Add(media);
            _context.Exercises.Add(exercise);
            _context.ExerciseMedia.Add(exerciseMedia);
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetMedia(exercise.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var mediaList = Assert.IsAssignableFrom<IEnumerable<Media>>(okResult.Value);
            Assert.Single(mediaList);
            Assert.Equal(media.Id, mediaList.First().Id);
        }


        [Fact]
        public async Task GetMedia_ReturnsNotFound_WhenNoMediaForExercise()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 2,
                Name = "Pull-Up",
                Description = "Upper body exercise",
                Type = "Muscle-focused",
                DifficultyLevel = RoutineLevel.Advanced,
                MuscleGroup = "Back",
                EquipmentNecessary = "Pull Bar"
            };

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetMedia(exercise.Id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            var response = notFoundResult.Value;
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("No media found for this exercise.", message);
        }

        [Fact]
        public async Task GetFilteredExercises_ReturnsAll_WhenNoFiltersApplied()
        {
            var exercise = new Exercise
            {
                Id = 2,
                Name = "Pull-Up",
                Description = "Upper body exercise",
                Type = "Muscle-focused",
                DifficultyLevel = RoutineLevel.Advanced,
                MuscleGroup = "Back",
                EquipmentNecessary = "Pull Bar"
            };

            var media = new Media
            {
                Id = 2,
                Name = "Pull-up video",
                Url = "http://example.com/video.mp4",
                Type = "Video"
            };

            var exerciseMedia = new ExerciseMedia
            {
                ExerciseId = exercise.Id,
                MediaId = media.Id
            };

            _context.Media.Add(media);
            _context.Exercises.Add(exercise);
            _context.ExerciseMedia.Add(exerciseMedia);
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetFilteredExercises(null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetFilteredExercises_FiltersByType()
        {
            // Arrange
            _context.Exercises.AddRange(
                new Exercise {Id=3, Name = "Plank", Description = "", Type = "Core", DifficultyLevel = RoutineLevel.Intermediate, MuscleGroup = "Abs", EquipmentNecessary = "None" },
                new Exercise { Id= 4, Name = "Squat", Description= "" ,Type = "Legs", DifficultyLevel = RoutineLevel.Beginner, MuscleGroup = "Legs", EquipmentNecessary = "Barbell" }
            );
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetFilteredExercises("Core", null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetFilteredExercises_FiltersByDifficultyLevel()
        {
            // Arrange
            _context.Exercises.Add(new Exercise
            {
                Id = 5,
                Name = "Deadlift",
                Description = "",
                Type = "Muscle-focused",
                DifficultyLevel = RoutineLevel.Advanced,
                MuscleGroup = "Back",
                EquipmentNecessary = "Barbell"
            });
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetFilteredExercises(null, "Advanced", null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetRoutinesOnExercises_ReturnsOk_WhenCustomRoutinesExist()
        {
            // Arrange
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

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetRoutinesOnExercises(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedRoutines = Assert.IsAssignableFrom<IEnumerable<Routine>>(okResult.Value);
            Assert.Single(returnedRoutines);
            Assert.Equal(userId, returnedRoutines.First().UserId);
        }

        [Fact]
        public async Task GetRoutinesOnExercises_ReturnsBadRequest_WhenNoCustomRoutinesExist()
        {
            // Arrange
            var userId = 2;
            _context.Routines.Add(new Routine
            {
                Id = 2,
                UserId = userId,
                Name = "Default Routine",
                Type = "Warm-up",
                Description = "",
                Needs = "",
                IsCustom = false // Not a custom routine
            });
            await _context.SaveChangesAsync();

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.GetRoutinesOnExercises(userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequest.Value;
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Dont exist Custom Routines", message);
        }

        [Fact]
        public async Task AddToRoutine_ReturnsOk_WhenExerciseAddedSuccessfully()
        {
            // Arrange
            var userId = 3;
            _context.Routines.Add(new Routine
            {
                Id = 3,
                UserId = userId,
                Name = "Default Routine",
                Type = "Warm-up",
                Description = "",
                Needs = "",
                IsCustom = false // Not a custom routine
            });
            await _context.SaveChangesAsync();

            var model = new AddRoutineFromExercisesViewModel
            {
                ExerciseId = 1,
                RoutineId = 3,
                duration = 60,
                reps = 10,
                sets = 3
            };

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.AddToRoutine(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Exercise added to routine successfully!", message);
        }

        [Fact]
        public async Task AddToRoutine_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var controller = new ExercisesController(_context);
            controller.ModelState.AddModelError("ExerciseId", "Required");

            var model = new AddRoutineFromExercisesViewModel(); // vazio, inválido

            // Act
            var result = await controller.AddToRoutine(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequest.Value;
            Assert.NotNull(response);
            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Invalid data", message);
        }

        [Fact]
        public async Task AddToRoutine_ReturnsNotFound_WhenExerciseDoesNotExist()
        {

            var model = new AddRoutineFromExercisesViewModel
            {
                ExerciseId = 999, // inexistente
                RoutineId = 2,
                duration = 30,
                reps = 8,
                sets = 3
            };

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.AddToRoutine(model);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFound.Value;
            Assert.NotNull(response);

            var message = response.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);
               
            Assert.Equal("Exercise not found", message);
        }

        [Fact]
        public async Task AddToRoutine_ReturnsNotFound_WhenRoutineDoesNotExist()
        {

            var model = new AddRoutineFromExercisesViewModel
            {
                ExerciseId = 1,
                RoutineId = 999, // inexistente
                duration = 45,
                reps = 15,
                sets = 3
            };

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.AddToRoutine(model);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFound.Value;
            Assert.NotNull(response);
            var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);
            Assert.Equal("Routine not found", message);
        }

        [Fact]
        public async Task AddExercise_ReturnsOk_WhenExerciseIsAddedSuccessfully()
        {
            // Arrange
            var model = new AddExercisesViewModel
            {
                newName = "Push-Up",
                newDescription = "An upper body exercise.",
                newType = "Strength",
                newDifficultyLevel = "Intermediate",  // válido
                newMuscleGroup = "Chest",
                newEquipmentNecessary = "None",
                newMediaName = "PushUpVideo",
                newMediaUrl = "http://example.com/pushup",
                newMediaType = "video",
                newMediaName1 = "PushUpImage",
                newMediaUrl1 = "http://example.com/pushup.jpg",
                newMediaType1 = "Image"
            };

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.AddExercise(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Verifica se o retorno é Ok
            var response = okResult.Value; // Extrai o valor retornado
            Assert.NotNull(response);

            var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Exercise added successfully!", message); // Verifica a mensagem de sucesso
        }

        [Fact]
        public async Task AddExercise_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var controller = new ExercisesController(_context);
            controller.ModelState.AddModelError("newName", "Required");

            var model = new AddExercisesViewModel
            {
                newDescription = "Test Description",
                newType = "Strength",
                newDifficultyLevel = "Intermediate",
                newMuscleGroup = "Chest",
                newEquipmentNecessary = "None"
            };

            // Act
            var result = await controller.AddExercise(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequest.Value;
            Assert.NotNull(response);

            var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Invalid data", message);
        }

        [Fact]
        public async Task AddExercise_ReturnsBadRequest_WhenInvalidDifficultyLevelIsProvided()
        {
            // Arrange
            var model = new AddExercisesViewModel
            {
                newName = "Push-Up",
                newDescription = "An upper body exercise.",
                newType = "Strength",
                newDifficultyLevel = "InvalidLevel",  // inválido
                newMuscleGroup = "Chest",
                newEquipmentNecessary = "None",
                newMediaName = "PushUpVideo",
                newMediaUrl = "http://example.com/pushup",
                newMediaType = "Video",
                newMediaName1 = "PushUpImage",
                newMediaUrl1 = "http://example.com/pushup.jpg",
                newMediaType1 = "Image"
            };

            var controller = new ExercisesController(_context);

            // Act
            var result = await controller.AddExercise(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequest.Value;
            Assert.NotNull(response);

            var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
            Assert.NotNull(message);

            Assert.Equal("Invalid difficulty level", message);
        }

       
    }
}

