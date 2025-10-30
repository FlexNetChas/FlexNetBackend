using FlexNet.Application.DTOs.UserDescription.Request;
using FlexNet.Application.DTOs.UserDescription.Response;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Services;
using FlexNet.Domain.Entities;
using Moq;
using Xunit.Abstractions;

namespace FlexNet.Application.Tests
{
    // ITestOutputHelper is used to write output, displayed in test results window.
    // Useful for debugging and logging during tests with it's own window instead of sorting through console logs (dotnet test)
    public class UserDescriptionServiceTests
    {
        private readonly Mock<IUserDescriptionRepo> _repoMock;
        private readonly UserDescriptionService _service;
        private readonly ITestOutputHelper _output;

        public UserDescriptionServiceTests(ITestOutputHelper output)
        {
            _repoMock = new Mock<IUserDescriptionRepo>();
            _service = new UserDescriptionService(_repoMock.Object);
            _output = output;
        }

        /* Theory Tests for practis reason. The function will work fine with Fact
         * Though Theory allows us to run the same test with different inputs 
         * witch is useful in Should_UpdateFields_WhenPatchingUser
         */

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Should_ThrowError_WhenUserNotFound(int userId)
        {
            // Arrange
            _repoMock.Setup(repoMock => repoMock.GetUserDescriptionByUserIdAsync(userId))
                     .ReturnsAsync((UserDescription?)null);

            // Act
            var exception = await Record.ExceptionAsync(
                async () => await _service.GetUserDescriptionByUserIdAsync(userId)
            );
            _output.WriteLine($"Exception cast successfully: {exception?.Message}");

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<KeyNotFoundException>(exception);
        }

        [Theory]
        [InlineData(1, 18, "Male", "Highschool", "Want to become a pilot")]
        [InlineData(2, 30, "Female", "Master's Degree", "Want to change career to programming")]
        public async Task Should_ReturnUserDescription_WhenUserExists
            (int id, int age, string gender, string education, string purpose)
        {
            // Arrange
            var expected = new UserDescriptionResponseDto(
                id, age, gender, education, purpose
            );

            _repoMock.Setup(repoMock => repoMock.GetUserDescriptionByUserIdAsync(id))
                .ReturnsAsync(new UserDescription
                {
                    Id = id,
                    Age = age,
                    Gender = gender,
                    Education = education,
                    Purpose = purpose
                });

            // Act
            var result = await _service.GetUserDescriptionByUserIdAsync(id);
            _output.WriteLine(message: $"Result found successfully: {result}");

            // Assert
            Assert.NotNull(result);
            Assert.Equivalent(expected, result);
        }

        [Theory]
        [InlineData(
           1,  // Patch only Age
                25, "Male", "University", "Want to become a pilot", 
                30, null, null, null)] 
        [InlineData(
           2,  // Patch all fields
                20, "Female", "Highschool", "Want to become a lawyer", 
                40, "Male", "University", "Become a doctor")] 
        public async Task Should_UpdateFields_WhenPatchingUser(
            int existingId, int existingAge, string existingGender, string existingEducation, string existingPurpose,
                            int? patchAge, string? patchGender, string? patchEducation, string? patchPurpose)
        {
            // Arrange
            var existing = new UserDescription
            {
                Id = existingId,
                Age = existingAge,
                Gender = existingGender,
                Education = existingEducation,
                Purpose = existingPurpose
            };

            _output.WriteLine($"Before patching: Id={existing.Id}, Age={existing.Age}, Gender={existing.Gender}, Education={existing.Education}, Purpose={existing.Purpose}");

            var request = new PatchUserDescriptionRequestDto(patchAge, patchGender, patchEducation, patchPurpose);

            _repoMock.Setup(repoMock => repoMock.GetUserDescriptionByUserIdAsync(existing.Id))
                .ReturnsAsync(existing);

            _repoMock.Setup(repoMock => repoMock.UpdateUserDescriptionAsync(It.IsAny<UserDescription>()))
                .ReturnsAsync((UserDescription userDesc) => userDesc);

            var expected = new UserDescriptionResponseDto(
                existing.Id,
                patchAge ?? existing.Age,
                patchGender ?? existing.Gender,
                patchEducation ?? existing.Education,
                patchPurpose ?? existing.Purpose
            );

            // Act
            var result = await _service.PatchUserDescriptionAsync(existing.Id, request);
            _output.WriteLine($"Response after patching: {result}");

            // Assert
            Assert.NotNull(result);
            Assert.Equivalent(expected, result);
                // Verify update called exactly once
                _repoMock.Verify(repo => repo.UpdateUserDescriptionAsync(It.IsAny<UserDescription>()), Times.Once);
        }
    }
}
