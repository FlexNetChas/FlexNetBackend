using FlexNet.Application.DTOs.AI;
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
        UserContextDto userContextDto)
    {
        var attempt = 0;
        
        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                
                var result = await _innerService.GetGuidanceAsync(
                    userMessage, 
                    conversationHistory, 
                    userContextDto);
                
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
    public async Task<Result<string>> GenerateTitleAsync(
    IEnumerable<ConversationMessage> conversationHistory,
    UserContextDto? userContextDto = null)
{
    var attempt = 0;
    
    while (attempt < MaxRetries)
    {
        attempt++;

        try
        {

            var result = await _innerService.GenerateTitleAsync(
                conversationHistory, 
                userContextDto);
            
            if (result.IsSuccess)
            {
                return result;
            }
            
            // If it failed but error says can retry
            if (result.Error?.CanRetry == true && attempt < MaxRetries)
            {
                var delay = CalculateDelay(attempt, 
                    result.Error.RetryAfter.HasValue 
                        ? TimeSpan.FromSeconds(result.Error.RetryAfter.Value) 
                        : null);
                
                _logger.LogWarning(
                    "Title generation attempt {Attempt} failed with retryable error {ErrorCode}. Retrying after {Delay}ms",
                    attempt, result.Error.ErrorCode, delay.TotalMilliseconds);
                
                await Task.Delay(delay);
                continue;
            }
            
            // Non-retryable error or last attempt - return the error
            _logger.LogWarning(
                "Title generation failed with error {ErrorCode}", 
                result.Error?.ErrorCode);
            
            return result;
        }
        catch (ServiceException ex) when (ex.CanRetry && attempt < MaxRetries)
        {
            var delay = CalculateDelay(attempt, ex.RetryAfter);
            
            _logger.LogWarning(
                ex, 
                "Title generation attempt {Attempt} failed. Retrying after {Delay}ms",
                attempt, delay.TotalMilliseconds);
            
            await Task.Delay(delay);
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Title generation failed with error {ErrorCode}", ex.ErrorCode);
            return CreateFailureResult(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during title generation");
            return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "UNEXPECTED_ERROR",
                Message: $"Unexpected error: {ex.Message}",
                CanRetry: false,
                RetryAfter: null));
        }
    }

    var error = new ErrorInfo(
        ErrorCode: "MAX_RETRIES_EXCEEDED",
        Message: "Failed to generate title after multiple attempts",
        CanRetry: true,
        RetryAfter: 60);
    
    return Result<string>.Failure(error);
}

    public async IAsyncEnumerable<Result<string>> GetGuidanceStreamingAsync(string userMessage, IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        await foreach (var chunk in _innerService.GetGuidanceStreamingAsync(
                           userMessage,
                           conversationHistory,
                           userContextDto))
        {
            yield return chunk;
        }
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