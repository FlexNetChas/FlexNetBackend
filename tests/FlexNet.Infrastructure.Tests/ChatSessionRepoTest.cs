using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using FlexNet.Infrastructure.Repositories;
using FlexNet.Application.Services;
using FlexNet.Application.DTOs.ChatMessage.Response;
using FlexNet.Application.DTOs.ChatSession.Request;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using FluentAssertions;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;

namespace FlexNet.Infrastructure.Tests
{
    public class ChatSessionRepoTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ApplicationDbContext _context;
        private readonly IChatSessionRepo _repo;
        private readonly IUserContextService _userContextService;
        private readonly ChatSessionService _service;

        public ChatSessionRepoTest(ITestOutputHelper output)
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FlexNetTestDb")  
                .Options;

            _context = new ApplicationDbContext(options);
            _repo = new ChatSessionRepo(_context);
            _userContextService = new MockUserContextService(); // fake userContext for testing
            _service = new ChatSessionService(_repo, _userContextService);
            _output = output;
        }

        // Dispose after tests to clean up the in-memory database
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Test: Retrieve Chat Session by ID
        [Fact]
        public async Task GetChatSessionById_Should_ReturnSession_WhenFound()
        {
            // Arrange
            var userId = 123;
            var session = new ChatSession
            {
                UserId = userId,
                Summary = "Test Session",
                StartedTime = DateTime.UtcNow,
                EndedTime = null
            };
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByIdAsync((int)session.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(session.Id);
            result.UserId.Should().Be(userId);
            result.Summary.Should().Be("Test Session");
            result.EndedTime.Should().BeNull();
            result.StartedTime.Should().BeCloseTo(session.StartedTime, TimeSpan.FromSeconds(5));
            _output.WriteLine($"Retrieved session: {result?.Summary}");
        }

        // Test: Create a new chat session
        [Fact]
        public async Task CreateChatSession_Should_AddNewSession()
        {
            // Arrange
            var userId = 123;
            var createRequest = new CreateChatSessionRequestDto(
                "New Session",
                DateTime.UtcNow,
                null,
                new List<ChatMessageResponseDto>()
            );

            // Act
            var result = await _service.CreateAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Summary.Should().Be("New Session");
            result.EndedTime.Should().BeNull();
            result.StartedTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _output.WriteLine($"Created session: {result?.Summary}");
        }

        // Test: Update an existing chat session
        [Fact]
        public async Task UpdateChatSession_Should_UpdateSession()
        {
            // Arrange
            var userId = 123;
            var session = new ChatSession
            {
                UserId = userId,
                Summary = "Test Session",
                StartedTime = DateTime.UtcNow,
                EndedTime = null
            };
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateChatSessionsRequestDto(
                (int)session.Id,
                "Updated Session",
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow,
                new List<ChatMessageResponseDto>()
            );

            // Act
            var result = await _service.UpdateAsync(updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.Summary.Should().Be("Updated Session");
            _output.WriteLine($"Updated session: {result?.Summary}");
        }

        // Test: Delete a chat session
        [Fact]
        public async Task DeleteChatSession_Should_RemoveSession()
        {
            // Arrange
            var userId = 123;
            var session = new ChatSession
            {
                UserId = userId,
                Summary = "Session to delete",
                StartedTime = DateTime.UtcNow,
                EndedTime = null
            };
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteAsync((int)session.Id);

            // Assert
            result.Should().BeTrue();
            _output.WriteLine($"Deleted session with ID: {session.Id}");
        }
    }

    // Fake UserContext since we just need an ID for our ChatSession testing.
    public class MockUserContextService : IUserContextService
    {
        public int GetCurrentUserId() => 123;
    }
}
