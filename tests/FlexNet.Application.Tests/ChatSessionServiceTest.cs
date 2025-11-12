using FlexNet.Application.DTOs.ChatMessage.Response;
using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Services;
using FlexNet.Domain.Entities;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace FlexNet.Application.Tests
{
    public class ChatSessionServiceTest
    {
        private readonly Mock<IChatSessionRepo> _repoMock;
        private readonly Mock<IUserContextService> _userContextServiceMock;

        private readonly ChatSessionService _service;
        private readonly ITestOutputHelper _output;

        public ChatSessionServiceTest(ITestOutputHelper output)
        {
            _repoMock = new Mock<IChatSessionRepo>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _service = new ChatSessionService(_repoMock.Object, _userContextServiceMock.Object);
            _output = output;
        }

        [Fact]
        public async Task Should_ThrowKeyNotFoundException_WhenChatSessionNotFoundById()
        {
            // Arrange
            int sessionId = 1;
            int userId = 123;
            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(sessionId, userId)).ReturnsAsync((ChatSession?)null);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _service.GetByIdAsync(sessionId));

            Assert.NotNull(exception);
            Assert.IsType<KeyNotFoundException>(exception);
            _output.WriteLine($"Exception occurred: {exception?.Message}");
        }

        [Fact]
        public async Task Should_ReturnChatSession_WhenFoundById()
        {
            // Arrange
            int sessionId = 1;
            int userId = 123;
            var expectedSession = new ChatSession
            {
                Id = sessionId,
                UserId = userId,
                Summary = "Test Session",
                StartedTime = DateTime.UtcNow,
                EndedTime = null,
                ChatMessages = new List<ChatMessage>()
            };
            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(sessionId, userId)).ReturnsAsync(expectedSession);

            // Act
            var result = await _service.GetByIdAsync(sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sessionId, result.Id);
            Assert.Equal(userId, result.UserId);
            _output.WriteLine($"Retrieved session: {result?.Summary}");
        }

        [Fact]
        public async Task Should_CreateNewChatSession()
        {
            // Arrange
            int userId = 123;
            var newSessionRequest = new CreateChatSessionRequestDto(
                "New Session",
                DateTime.UtcNow,
                null,
                new List<ChatMessageResponseDto>()
            );

            var newSession = new ChatSession
            {
                UserId = userId,
                Summary = newSessionRequest.Summary,
                StartedTime = newSessionRequest.StartedTime,
                EndedTime = newSessionRequest.EndedTime,
                ChatMessages = newSessionRequest.ChatMessages.Select(m => new ChatMessage
                {
                    MessageText = m.MessageText,
                    TimeStamp = m.TimeStamp,
                    LastUpdated = m.LastUpdated
                }).ToList()
            };

            var createdSession = new ChatSession
            {
                Id = 1,
                UserId = userId,
                Summary = "New Session",
                StartedTime = DateTime.UtcNow,
                EndedTime = null,
                ChatMessages = new List<ChatMessage>()
            };

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<ChatSession>())).ReturnsAsync(createdSession);

            // Act
            var result = await _service.CreateAsync(newSessionRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Session", result.Summary);
            Assert.Equal(userId, result.UserId);
            _output.WriteLine($"Created session: {result?.Summary}");
        }

        [Fact]
        public async Task Should_UpdateChatSession()
        {
            // Arrange
            int sessionId = 1;
            int userId = 123;
            var updateRequest = new UpdateChatSessionsRequestDto(
                sessionId,
                "Updated Summary",
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow,
                new List<ChatMessageResponseDto>()
            );

            var updatedSession = new ChatSession
            {
                Id = sessionId,
                UserId = userId,
                Summary = updateRequest.Summary,
                StartedTime = updateRequest.StartedTime,
                EndedTime = updateRequest.EndedTime,
                ChatMessages = updateRequest.ChatMessages.Select(m => new ChatMessage
                {
                    Id = m.Id ?? 0,
                    MessageText = m.MessageText,
                    TimeStamp = m.TimeStamp,
                    LastUpdated = m.LastUpdated
                }).ToList()
            };

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ChatSession>())).ReturnsAsync(updatedSession);

            // Act
            var result = await _service.UpdateAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Summary", result.Summary);
            _output.WriteLine($"Updated session: {result?.Summary}");
        }

        [Fact]
        public async Task Should_DeleteChatSession()
        {
            // Arrange
            int sessionId = 1;
            int userId = 123;

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.DeleteAsync(sessionId, userId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(sessionId);

            // Assert
            Assert.True(result);
            _output.WriteLine($"Deleted session with ID: {sessionId}");
        }

        [Fact]
        public async Task Should_ThrowKeyNotFoundException_WhenDeletingNonExistentChatSession()
        {
            // Arrange
            int sessionId = 1;
            int userId = 123;

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.DeleteAsync(sessionId, userId)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _service.DeleteAsync(sessionId));

            Assert.NotNull(exception);
            Assert.IsType<KeyNotFoundException>(exception);
            _output.WriteLine($"Exception occurred: {exception?.Message}");
        }

        [Fact]
        public async Task Should_EndChatSession()
        {
            // Arrange
            int sessionId = 1;
            int userId = 123;
            var existingSession = new ChatSession
            {
                Id = sessionId,
                UserId = userId,
                Summary = "Session to end",
                StartedTime = DateTime.UtcNow.AddHours(-1),
                EndedTime = null,
                ChatMessages = new List<ChatMessage>()
            };
            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(sessionId, userId)).ReturnsAsync(existingSession);

            var endedSession = new ChatSession
            {
                Id = sessionId,
                UserId = userId,
                Summary = "Session ended",
                StartedTime = DateTime.UtcNow.AddHours(-1),
                EndedTime = DateTime.UtcNow,
                ChatMessages = new List<ChatMessage>()
            };

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ChatSession>())).ReturnsAsync(endedSession);

            // Act
            var result = await _service.EndSessionAsync(sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.EndedTime);
            _output.WriteLine($"Ended session with ID: {sessionId}");
        }

        [Fact]
        public async Task Should_ThrowInvalidOperationException_WhenEndingAlreadyEndedSession()
        {
            // Arrange
            int sessionId = 1;
            int userId = 123;
            var existingSession = new ChatSession
            {
                Id = sessionId,
                UserId = userId,
                Summary = "Already ended session",
                StartedTime = DateTime.UtcNow.AddHours(-1),
                EndedTime = DateTime.UtcNow.AddHours(-2), // already ended
                ChatMessages = new List<ChatMessage>()
            };
            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(sessionId, userId)).ReturnsAsync(existingSession);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _service.EndSessionAsync(sessionId));

            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
            _output.WriteLine($"Exception occurred: {exception?.Message}");
        }
    }
}
