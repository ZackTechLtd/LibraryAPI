

namespace Common.Util
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public interface IRijndaelCrypt
    {
        (string result, string error) Decrypt(string text);

        (string result, string error) Encrypt(string text);
    }

    /// <summary>
    /// AES Encryptor / Decryptor Helper (Rijndael, 128-bit block)
    ///
    /// <remarks>
    /// Uses AES with the same key material and CBC mode as the original
    /// RijndaelManaged implementation, so existing data remains compatible.
    /// </remarks>
    public class RijndaelCrypt : IRijndaelCrypt, IDisposable
    {
        private readonly ICryptoTransform _decryptor;
        private readonly ICryptoTransform _encryptor;

        /// <summary>
        /// 16-byte Initialization Vector
        /// </summary>
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("=HeK3aymt,gvN~23");

        /// <summary>
        /// Derived key material (MD5 of the password)
        /// </summary>
        private readonly byte[] _password;

        /// <summary>
        /// AES cipher instance
        /// </summary>
        private readonly Aes _cipher;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="password">Public key</param>
        public RijndaelCrypt(string password = "&;A&+!UtmI^fx|Cl?ctz")
        {
            using var md5 = MD5.Create();
            _password = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            _cipher = Aes.Create();
            _decryptor = _cipher.CreateDecryptor(_password, IV);
            _encryptor = _cipher.CreateEncryptor(_password, IV);
        }

        /// <summary>
        /// Decryptor
        /// </summary>
        /// <param name="text">Base64 string to be decrypted</param>
        /// <returns>
        public (string result, string error) Decrypt(string text)
        {
            try
            {
                byte[] input = Convert.FromBase64String(text);

                var newClearData = _decryptor.TransformFinalBlock(input, 0, input.Length);
                return (result: Encoding.ASCII.GetString(newClearData), error: string.Empty);
            }
            catch (ArgumentException ae)
            {
                return (result: string.Empty, error: "inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
            }
            catch (ObjectDisposedException oe)
            {
                return (result: string.Empty, error: "The object has already been disposed." + oe);
            }
        }

        /// <summary>
        /// Encryptor
        /// </summary>
        /// <param name="text">String to be encrypted</param>
        /// <returns>
        public (string result, string error) Encrypt(string text)
        {
            try
            {
                var buffer = Encoding.ASCII.GetBytes(text);
                return (result: Convert.ToBase64String(_encryptor.TransformFinalBlock(buffer, 0, buffer.Length)), error: string.Empty);
            }
            catch (ArgumentException ae)
            {
                return (result: string.Empty, error: "inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
            }
            catch (ObjectDisposedException oe)
            {
                return (result: string.Empty, error: "The object has already been disposed." + oe);
            }
        }

        public void Dispose()
        {
            _decryptor.Dispose();
            _encryptor.Dispose();
            _cipher.Dispose();
        }
    }
}