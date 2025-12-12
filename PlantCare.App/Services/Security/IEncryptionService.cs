namespace PlantCare.App.Services.Security;

/// <summary>
/// Interface for encryption and decryption operations on sensitive data.
/// Implements AES encryption for protecting user credentials and personal information.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts plain text using AES encryption.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted text encoded in Base64 format.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts cipher text that was encrypted using the Encrypt method.
    /// </summary>
    /// <param name="cipherText">The encrypted text in Base64 format.</param>
    /// <returns>The decrypted plain text.</returns>
    string Decrypt(string cipherText);
}
