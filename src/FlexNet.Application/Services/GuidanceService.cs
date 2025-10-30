using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Exceptions;  // ← Fixed namespace
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services;

public class GuidanceService : IGuidanceService
{
    private readonly IGuidanceService _innerService;
    private readonly ILogger<GuidanceService> _logger;

    private const int MaxRetries = 3;
    private const int BaseDelaySeconds = 1;
    private const double JitterFactor = 0.5;
    
    public GuidanceService(
        IGuidanceService innerService,
        ILogger<GuidanceService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<string>> GetGuidanceAsync(
        string userMessage, 
        IEnumerable<ConversationMessage> conversationHistory, 
        StudentContext studentContext)
    {
        var attempt = 0;
        
        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                _logger.LogInformation("Attempt {Attempt} of {MaxRetries}", attempt, MaxRetries);
                
                var result = await _innerService.GetGuidanceAsync(
                    userMessage, 
                    conversationHistory, 
                    studentContext);
                
                _logger.LogInformation("Request succeeded on attempt {Attempt}", attempt);
                return result;  
            }
            catch (ServiceException ex) when (ex.CanRetry && attempt < MaxRetries)  
            {
                var delay = CalculateDelay(attempt, ex.RetryAfter);
                
                _logger.LogWarning(
                    ex, 
                    "Attempt {Attempt} failed with retryable error {ErrorCode}. Retrying after {Delay}ms",
                    attempt, ex.ErrorCode, delay.TotalMilliseconds);
                
                await Task.Delay(delay);
            }
            catch (ServiceException ex) when (!ex.CanRetry)  
            {
                _logger.LogError(
                    ex, 
                    "Request failed with non-retryable error {ErrorCode}", 
                    ex.ErrorCode);
                
                return CreateFailureResult(ex);
            }
            catch (ServiceException ex) when (ex.CanRetry && attempt >= MaxRetries)
            {
                _logger.LogError(
                    ex, 
                    "Request failed after {Attempts} attempts with error {ErrorCode}", 
                    attempt, ex.ErrorCode);
                
                return CreateFailureResult(ex);
            }
        }

        var error = new ErrorInfo(
            ErrorCode: "MAX_RETRIES_EXCEEDED",
            CanRetry: true,
            RetryAfter: 60,
            Message: "We're experiencing difficulties. Please try again in a moment.");  
        
        return Result<string>.Failure(error);
    }

    private static TimeSpan CalculateDelay(int attempt, TimeSpan? retryAfter)
    {
        if (retryAfter.HasValue) 
            return retryAfter.Value;
        
        var exponentialDelay = BaseDelaySeconds * Math.Pow(2, attempt - 1);
        var jitterMs = Random.Shared.NextDouble() * JitterFactor * exponentialDelay * 1000;
        var totalMs = (exponentialDelay * 1000) + jitterMs;
        
        return TimeSpan.FromMilliseconds(totalMs);  
    }

    private static Result <string> CreateFailureResult(ServiceException ex)
    {
        var error = new ErrorInfo(
            ErrorCode: ex.ErrorCode,
            CanRetry: ex.CanRetry,
            RetryAfter: ex.RetryAfter.HasValue ? (int)ex.RetryAfter.Value.TotalSeconds : null, 
            Message: ex.UserMessage);
        
        return Result<string>.Failure(error);
    }
}