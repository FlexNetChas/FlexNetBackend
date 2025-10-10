﻿using Azure;
using Azure.Security.KeyVault.Secrets;
using FlexNet.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services;

internal sealed class KeyVaultApiKeyProvider : IApiKeyProvider
{
    private readonly string? _secretName;
    private readonly SecretClient _secretClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<KeyVaultApiKeyProvider> _logger;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(10);

    public KeyVaultApiKeyProvider(
        SecretClient secretClient,
        IMemoryCache cache,
        IConfiguration config,
        ILogger<KeyVaultApiKeyProvider> logger)
    {
        _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _secretName = config["KeyVault:SecretName"];
        _logger.LogInformation("Resolved KEY_VAULT_SECRET_NAME: {SecretName}", _secretName ?? "<null>");
    }

    public async Task<string> GetApiKeyAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_secretName))
            throw new InvalidOperationException("Configured secret name is null or empty.");

        if (_cache.TryGetValue(_secretName, out string cachedValue))
            return cachedValue;

        try
        {
            Response<KeyVaultSecret> response = await _secretClient.GetSecretAsync(_secretName, cancellationToken: cancellationToken);
            var raw = response.Value?.Value ?? string.Empty;
            var normalized = NormalizeSecret(raw);

            _logger.LogInformation("Secret name={SecretName} rawLen={RawLen} normalizedLen={NormLen}", _secretName, raw.Length, normalized.Length);

            if (string.IsNullOrWhiteSpace(normalized))
                throw new InvalidOperationException("API key retrieved from Key Vault is empty after normalization.");

            _cache.Set(_secretName, normalized, _cacheTtl);
            return normalized;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "KeyVault GetSecret failed. Name={SecretName} Status={Status} Code={Code}", _secretName, ex.Status, ex.ErrorCode);
            throw new InvalidOperationException($"Failed to retrieve API key from Key Vault: {_secretName}", ex);
        }
    }

    private static string NormalizeSecret(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Trim whitespace and common invisible characters (CR, LF, BOM)
        var trimmed = value.Trim().Trim('\uFEFF', '\u200B', '\u200E', '\u200F');

        // If there are predictable wrappers we don't want (e.g., "Role ... None"), strip them here explicitly.
        // Example (uncomment and adjust if needed):
        // if (trimmed.StartsWith("Role ", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(" None", StringComparison.OrdinalIgnoreCase))
        // {
        //     // custom parsing to extract inner API key
        // }

        return trimmed;
    }
}