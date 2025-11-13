using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services;

public interface IEncryptionKeyProvider
{
    Task<byte[]> GetEncryptionKeyAsync(CancellationToken cancellationToken = default);
}

internal sealed class KeyVaultEncryptionKeyProvider : IEncryptionKeyProvider
{
    private const string CacheKey = "EncryptionKey_Bytes";
    private readonly string _secretName;
    private readonly SecretClient _secretClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<KeyVaultEncryptionKeyProvider> _logger;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromDays(1); // Cache very long
    
    public KeyVaultEncryptionKeyProvider(
        SecretClient secretClient,
        IMemoryCache cache,
        IConfiguration config,
        ILogger<KeyVaultEncryptionKeyProvider> logger)
    {
        _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Read from same config pattern as your API key
        _secretName = config["KeyVault:EncryptionKeySecretName"] ?? "EncryptionKey";
        _logger.LogInformation("Encryption key secret name: {SecretName}", _secretName);
    }
    
    public async Task<byte[]> GetEncryptionKeyAsync(CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cache.TryGetValue(CacheKey, out byte[] cachedKey))
        {
            _logger.LogDebug("Encryption key retrieved from cache");
            return cachedKey;
        }
        
        try
        {
            
            Response<KeyVaultSecret> response = await _secretClient.GetSecretAsync(
                _secretName, 
                cancellationToken: cancellationToken);
            
            var keyBase64 = response.Value?.Value 
                ?? throw new InvalidOperationException("Encryption key not found in Key Vault");
            
            // Normalize (same as your API key provider)
            keyBase64 = keyBase64.Trim().Trim('\uFEFF', '\u200B', '\u200E', '\u200F');
            
            var keyBytes = Convert.FromBase64String(keyBase64);
            
            if (keyBytes.Length != 32)
            {
                throw new InvalidOperationException(
                    $"Encryption key must be 32 bytes (256 bits). Got: {keyBytes.Length} bytes");
            }
            
            // Cache for a long time (encryption key rarely changes)
            _cache.Set(CacheKey, keyBytes, _cacheTtl);
            
            
            return keyBytes;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, 
                "Failed to load encryption key from Key Vault. Name={SecretName} Status={Status}", 
                _secretName, ex.Status);
            throw new InvalidOperationException(
                $"Failed to retrieve encryption key from Key Vault: {_secretName}", ex);
        }
    }
}