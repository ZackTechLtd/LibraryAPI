using System;
using Common.Util;

namespace LibraryAPIApp.Tests.Unit;

public class RijndaelCryptTests
{
    [Fact]
    public void Encrypt_DefaultPassword_MatchesLegacyCompatibilityVector()
    {
        // Vector computed independently with `openssl enc -aes-128-cbc` (MD5-of-password key,
        // fixed IV, PKCS7) so byte-compatibility with pre-upgrade encrypted data is pinned.
        using var crypt = new RijndaelCrypt();

        var (result, error) = crypt.Encrypt("Hello");

        Assert.Equal("Fu8uODSCDuk6tCZv5xE9Bw==", result);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void Decrypt_LegacyCompatibilityVector_ReturnsOriginalPlaintext()
    {
        using var crypt = new RijndaelCrypt();

        var (result, error) = crypt.Decrypt("Fu8uODSCDuk6tCZv5xE9Bw==");

        Assert.Equal("Hello", result);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void EncryptThenDecrypt_RoundTripsPlainText()
    {
        using var crypt = new RijndaelCrypt();

        var (encrypted, encryptError) = crypt.Encrypt("The quick brown fox");
        var (decrypted, decryptError) = crypt.Decrypt(encrypted);

        Assert.Equal(string.Empty, encryptError);
        Assert.Equal("The quick brown fox", decrypted);
        Assert.Equal(string.Empty, decryptError);
    }

    [Fact]
    public void Encrypt_SameInput_IsDeterministic()
    {
        using var crypt = new RijndaelCrypt();

        var (first, _) = crypt.Encrypt("deterministic");
        var (second, _) = crypt.Encrypt("deterministic");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Decrypt_InvalidBase64_ThrowsFormatException()
    {
        using var crypt = new RijndaelCrypt();

        Assert.Throws<FormatException>(() => crypt.Decrypt("##not-valid-base64##"));
    }
}