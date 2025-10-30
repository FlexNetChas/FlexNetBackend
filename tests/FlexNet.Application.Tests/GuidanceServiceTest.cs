using FlexNet.Application.Services;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlexNet.Application.Tests
{
    
    public class GuidanceServiceTest
    {
       [Fact]
       public async Task GetGuidanceAsync_SuccessOnFirstAttempt_ReturnSuccess()
       {
           // ARRANGE
           var userMessage = "I'm feeling unsure of what kind of education I should go after Högstadiet";
           var conversationHistory = Enumerable.Empty<ConversationMessage>();
           var studentContext = new StudentContext(Age: 16, Gender: null, Education: null, Purpose: null);

           var expectedContent = "I understand that it seems unsure but let me help guiding you to your future";
           var successResult = Result<string>.Success(expectedContent);
    
           var mockInnerService = new Mock<IGuidanceService>();
           mockInnerService
               .Setup(s => s.GetGuidanceAsync(userMessage, conversationHistory, studentContext))
               .ReturnsAsync(successResult);  
    
           var mockLogger = new Mock<ILogger<GuidanceService>>();
           var guidanceService = new GuidanceService(mockInnerService.Object, mockLogger.Object);
    
           // ACT
           var result = await guidanceService.GetGuidanceAsync(userMessage, conversationHistory, studentContext);
    
           // ASSERT
           Assert.True(result.IsSuccess);
           Assert.Equal(expectedContent, result.Data);  
           Assert.Null(result.Error);
    
           mockInnerService.Verify(
               s => s.GetGuidanceAsync(userMessage, conversationHistory, studentContext),
               Times.Once);  
       }
       [Fact]
       public async Task GetGuidanceAsync_RetriesOnNetworkError_EventuallySucceeds()
       {
           // ARRANGE
           var userMessage = "I'm feeling unsure of what kind of education I should go after Högstadiet";
           var conversationHistory = Enumerable.Empty<ConversationMessage>();
           var studentContext = new StudentContext(Age: 16, Gender: null, Education: null, Purpose: null);

           var expectedContent = "Finally worked!";  
    
           var mockInnerService = new Mock<IGuidanceService>();
           mockInnerService
               .SetupSequence(s => s.GetGuidanceAsync(userMessage, conversationHistory, studentContext))
               .Throws(ServiceException.NetworkError("First failure"))
               .Throws(ServiceException.NetworkError("Second failure"))
               .ReturnsAsync(Result<string>.Success(expectedContent));  
           var mockLogger = new Mock<ILogger<GuidanceService>>();
           var guidanceService = new GuidanceService(mockInnerService.Object, mockLogger.Object);
    
           // ACT
           var result = await guidanceService.GetGuidanceAsync(userMessage, conversationHistory, studentContext);
    
           // ASSERT
           Assert.True(result.IsSuccess);
           Assert.Equal(expectedContent, result.Data);  
           Assert.Null(result.Error);
    
           mockInnerService.Verify(
               s => s.GetGuidanceAsync(userMessage, conversationHistory, studentContext),
               Times.Exactly(3));  
       }

       [Fact]
       public async Task GetGuidanaceAsync_NonRetryableException_ReturnFailureImmediately()
       {
           // ARRANGE

           var userMessage = "I need help with my future";
           var conversationHistory = Enumerable.Empty<ConversationMessage>();
           var studentContext = new StudentContext(Age: 16, Gender: null, Education: null, Purpose: null);
           var expectedContent = "Invalid API key";
           
           var mockInnerService = new Mock<IGuidanceService>();
           mockInnerService.Setup(s => s.GetGuidanceAsync(userMessage, conversationHistory, studentContext)).Throws(ServiceException.AuthenticationFailed(expectedContent));
           var mockLogger = new Mock<ILogger<GuidanceService>>();
           var guidanceService = new GuidanceService(mockInnerService.Object, mockLogger.Object);
           
           // ACT
    
           var result = await guidanceService.GetGuidanceAsync(userMessage, conversationHistory, studentContext);
           // ASSERT
           
           Assert.False(result.IsSuccess);
            Assert.Empty(result.Data); 
           Assert.NotNull(result.Error);
            Assert.Equal("AUTHENTICATION_ERROR", result.Error.ErrorCode);
            Assert.False(result.Error.CanRetry);
           mockInnerService.Verify(s => s.GetGuidanceAsync(userMessage, conversationHistory, studentContext),
               Times.Once);

       }
    
    }
    
}

