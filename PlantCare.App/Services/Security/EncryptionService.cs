using System.Security.Cryptography;
using System.Text;

namespace PlantCare.App.Services.Security;

/// <summary>
/// Implementation of encryption/decryption using AES (Advanced Encryption Standard).
/// This service is used to protect sensitive data such as passwords and personal information.
/// 
/// NOTE: In production, encryption keys should be:
/// 1. Stored in secure platform-specific storage (iOS Keychain, Android Keystore)
/// 2. Rotated periodically
/// 3. Never hardcoded in the application
/// 4. Generated using cryptographically secure random number generators
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService()
    {
        // IMPORTANT: For production use, implement secure key management:
        // 1. Use SecureStorage to retrieve keys from platform-specific secure storage
        // 2. If keys don't exist, generate them securely and store them
        // 3. Implement key rotation policies
        
        // Example using .NET MAUI SecureStorage (recommended):
        // var savedKey = SecureStorage.GetAsync("encryption_key").Result;
        // if (string.IsNullOrEmpty(savedKey))
        // {
        //     savedKey = Convert.ToBase64String(GenerateSecureKey(32));
        //     SecureStorage.SetAsync("encryption_key", savedKey);
        // }
        // _key = Convert.FromBase64String(savedKey);

        // Temporary keys for development - MUST be replaced with secure storage in production
        _key = Encoding.UTF8.GetBytes("your-32-char-key-here-change-this");
        _iv = Encoding.UTF8.GetBytes("your-16-char-iv");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Encryption failed. See inner exception for details.", ex);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Decryption failed. The cipher text may be corrupted or invalid. See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random key.
    /// </summary>
    /// <param name="size">The size of the key in bytes (typically 32 for AES-256).</param>
    /// <returns>A secure random key as byte array.</returns>
    private static byte[] GenerateSecureKey(int size)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] key = new byte[size];
            rng.GetBytes(key);
            return key;
        }
    }
}
