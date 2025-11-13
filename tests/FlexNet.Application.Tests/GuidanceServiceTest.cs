using FlexNet.Application.DTOs.AI;
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
           var userMsg = "I'm feeling unsure of what kind of education I should go after Högstadiet";
           var conversationHistory = Enumerable.Empty<ConversationMessage>();
           var context = new UserContextDto(Age: 16, Gender: null, Education: null, Purpose: null);

           var expectedContent = "I understand that it seems unsure but let me help guiding you to your future";
           var successResult = Result<string>.Success(expectedContent);
    
           var mockInnerService = new Mock<IGuidanceService>();
           mockInnerService
               .Setup(s => s.GetGuidanceAsync(userMsg, conversationHistory, context))
               .ReturnsAsync(successResult);  
    
           var mockLogger = new Mock<ILogger<GuidanceService>>();
           var guidanceService = new GuidanceService(mockInnerService.Object, mockLogger.Object);
    
           // ACT
           var result = await guidanceService.GetGuidanceAsync(userMsg, conversationHistory, context);
    
           // ASSERT
           Assert.True(result.IsSuccess);
           Assert.Equal(expectedContent, result.Data);  
           Assert.Null(result.Error);
    
           mockInnerService.Verify(
               s => s.GetGuidanceAsync(userMsg, conversationHistory, context),
               Times.Once);  
       }
       [Fact]
       public async Task GetGuidanceAsync_RetriesOnNetworkError_EventuallySucceeds()
       {
           // ARRANGE
           var userMsg = "I'm feeling unsure of what kind of education I should go after Högstadiet";
           var conversationHistory = Enumerable.Empty<ConversationMessage>();
           var context = new UserContextDto(Age: 16, Gender: null, Education: null, Purpose: null);

           var expectedContent = "Finally worked!";  
    
           var mockInnerService = new Mock<IGuidanceService>();
           mockInnerService
               .SetupSequence(s => s.GetGuidanceAsync(userMsg, conversationHistory, context))
               .Throws(ServiceException.NetworkError("First failure"))
               .Throws(ServiceException.NetworkError("Second failure"))
               .ReturnsAsync(Result<string>.Success(expectedContent));  
           var mockLogger = new Mock<ILogger<GuidanceService>>();
           var guidanceService = new GuidanceService(mockInnerService.Object, mockLogger.Object);
    
           // ACT
           var result = await guidanceService.GetGuidanceAsync(userMsg, conversationHistory, context);
    
           // ASSERT
           Assert.True(result.IsSuccess);
           Assert.Equal(expectedContent, result.Data);  
           Assert.Null(result.Error);
    
           mockInnerService.Verify(
               s => s.GetGuidanceAsync(userMsg, conversationHistory, context),
               Times.Exactly(3));  
       }

       [Fact]
       public async Task GetGuidanceAsync_NonRetryableException_ReturnFailureImmediately()
       {
           // ARRANGE

           var userMsg = "I need help with my future";
           var conversationHistory = Enumerable.Empty<ConversationMessage>();
           var context = new UserContextDto(Age: 16, Gender: null, Education: null, Purpose: null);
           var expectedContent = "Invalid API key";
           
           var mockInnerService = new Mock<IGuidanceService>();
           mockInnerService.Setup(s => s.GetGuidanceAsync(userMsg, conversationHistory, context)).Throws(ServiceException.AuthenticationFailed(expectedContent));
           var mockLogger = new Mock<ILogger<GuidanceService>>();
           var guidanceService = new GuidanceService(mockInnerService.Object, mockLogger.Object);
           
           // ACT
    
           var result = await guidanceService.GetGuidanceAsync(userMsg, conversationHistory, context);
           // ASSERT
           
           Assert.False(result.IsSuccess);
           if (result.Data != null) Assert.Empty(result.Data);
           Assert.NotNull(result.Error);
            Assert.Equal("AUTHENTICATION_ERROR", result.Error.ErrorCode);
            Assert.False(result.Error.CanRetry);
           mockInnerService.Verify(s => s.GetGuidanceAsync(userMsg, conversationHistory, context),
               Times.Once);

       }
    
    }
    
}

