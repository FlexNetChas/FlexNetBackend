using FlexNet.Application.Interfaces;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;

namespace FlexNet.Infrastructure.Services.Gemini;

public class GeminiApiClient: IAiClient
{
    private readonly ILogger<GeminiApiClient> _logger;
    private readonly IApiKeyProvider _apiKeyProvider;

    public GeminiApiClient(ILogger<GeminiApiClient> logger, IApiKeyProvider apiKeyProvider)
    {
        _apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<string>> CallAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Calling Gemini API with prompt length: {Length}", prompt.Length);

            var apiKey = await _apiKeyProvider.GetApiKeyAsync();
            var model = new GenerativeModel() { ApiKey = apiKey };
            var response = await model.GenerateContent(prompt);

            if (string.IsNullOrWhiteSpace(response.Text))
            {
                _logger.LogWarning("Gemini returned empty response");
                return Result<string>.Failure(new ErrorInfo(
                    ErrorCode: "EMPTY_RESPONSE",
                    Message: "AI returned an empty response",
                    CanRetry: false,
                    RetryAfter: null));
            }

            _logger.LogDebug("Gemini API call successful, response length: {Length}", response.Text.Length);
            return Result<string>.Success(response.Text.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed");
            return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "GEMINI_API_ERROR",
                Message: $"Failed to generate content: {ex.Message}",
                CanRetry: true,
                RetryAfter: null));
        }

    }

    public async IAsyncEnumerable<Result<string>> CallStreamingAsync(string prompt)
    {
        var apiKey = await GetApiKeyForStreamingAsync();
        if (!apiKey.IsSuccess)
        {
            yield return apiKey;
            yield break;
        }
        var model = new GenerativeModel(){ApiKey = apiKey.ToString()};
        var chunkCount = 0;
        var streamEnumerator = model.GenerateContentStream(prompt).GetAsyncEnumerator();

        try
        {
            while (await streamEnumerator.MoveNextAsync())
            {
                chunkCount++;

                var response = streamEnumerator.Current;
                var chunkText = response.Text;

                if (string.IsNullOrWhiteSpace(chunkText))
                {
                    _logger.LogDebug("Skipping empty chunk {Count}", chunkCount);
                    continue;
                }
                
                _logger.LogDebug("Streaming chunk {Count}, lenght: {Length}", chunkCount, chunkText.Length);
                
                yield return Result<string>.Success(chunkText);
                _logger.LogInformation("Streaming completed, total chunks: {Count}", chunkCount);
            }
        }
        finally
        {
            await streamEnumerator.DisposeAsync();
            _logger.LogDebug("Stream disposed after {Count} chunks", chunkCount);
        } 
    }

    private async Task<Result<string>> GetApiKeyForStreamingAsync()
    {
        try
        {
            var apiKey = await _apiKeyProvider.GetApiKeyAsync();
            return Result<string>.Success(apiKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get API key for streaming");
            return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "API_KEY_ERROR",
                Message: $"Failed to get API key for streaming: {ex.Message}",
                CanRetry: true,
                RetryAfter: null));
        }
    }
}