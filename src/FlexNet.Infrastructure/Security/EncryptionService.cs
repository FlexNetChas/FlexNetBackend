// Infrastructure/Services/Security/EncryptionService.cs

using System.Security.Cryptography;
using FlexNet.Infrastructure.Interfaces;
using FlexNet.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private byte[] _key;
    private readonly ILogger<EncryptionService> _logger;
    
    // Constructor that pre-loads the key synchronously
    private EncryptionService(byte[] key, ILogger<EncryptionService> logger)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // Factory method for async initialization
    public static async Task<EncryptionService> CreateAsync(
        IEncryptionKeyProvider keyProvider,
        ILogger<EncryptionService> logger)
    {
        var key = await keyProvider.GetEncryptionKeyAsync();
        return new EncryptionService(key, logger);
    }
    
    // Now these are synchronous (key already loaded)
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;
        
        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;  // ← Already loaded, synchronous access
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var msEncrypt = new MemoryStream();
            
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);
            
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encryption failed");
            throw new InvalidOperationException("Failed to encrypt data", ex);
        }
    }
    
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;
        
        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            aes.Key = _key;  // ← Synchronous access
            
            var iv = new byte[aes.IV.Length];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decryption failed");
            throw new InvalidOperationException("Failed to decrypt data", ex);
        }
    }
}