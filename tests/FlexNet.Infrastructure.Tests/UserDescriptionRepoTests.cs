using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using FlexNet.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace FlexNet.Infrastructure.Tests
{
    public class UserDescriptionRepoTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ApplicationDbContext _context;
        private readonly UserDescriptionRepository _repository;
        private readonly Random _random;

        /* Set up a test DB (FlexNetTestDb) in c-tor. TestDb (Inmemory) will be clean up in Dispose
         * Inmemory db will have a faster runtime compared to setting up a real db with migrations
         */
        public UserDescriptionRepoTests(ITestOutputHelper output)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FlexNetTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserDescriptionRepository(_context);
            _output = output;
            _random = new Random();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetUserDescriptionByUserId_Should_ReturnUserDescription()
        {

            // Arrange: Add sample users
            _context.UserDescriptions.AddRange(new List<UserDescription>
            {
                new() { UserId = 1, Age = 25, Gender = "Male", Education = "Highschool", Purpose = "Become a Pilot" },
                new() { UserId = 2, Age = 30, Gender = "Woman", Education = "University", Purpose = "Become a Lawyer" },
                new() { UserId = 3, Age = 22, Gender = "Male", Education = "Master Degree", Purpose = "Become a Teacher" }
            });

            await _context.SaveChangesAsync();

            // Act: Retrieve UserDescription by UserId
            var result = await _repository.GetUserDescriptionByUserIdAsync(2);
            if (result != null)
            {
                _output.WriteLine(
                    $"Retrieved UserId: {result.UserId}, Age: {result.Age}, Gender: {result.Gender}, Education: {result.Education}, Purpose: {result.Purpose}"
                );

                // Assert: Verify the result
                result.Should().NotBeNull();
                result!.UserId.Should().Be(2);
                result.Age.Should().Be(30);
                result.Gender.Should().Be("Woman");
                result.Education.Should().Be("University");
                result.Purpose.Should().Be("Become a Lawyer");
            }
        }
    }
}
