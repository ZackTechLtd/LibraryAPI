

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
    /// Rijndael Encryptor / Decryptor Helper
    /// 
    /// <remarks>
    /// Created by: Jafet Sanchez 
    /// Last Update: [date],[author],[description]
    /// https://gist.github.com/jafetsanchez/1080133
    /// 
    public class RijndaelCrypt : IRijndaelCrypt
    {


        /// <summary>
        /// Decryptor
        /// 
        private readonly ICryptoTransform _decryptor;

        /// <summary>
        /// Encryptor
        /// 
        private readonly ICryptoTransform _encryptor;

        /// <summary>
        /// 16-byte Private Key
        /// 
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("=HeK3aymt,gvN~23"); //"ThisIsUrPassword");

        /// <summary>
        /// Public Key
        /// 
        private readonly byte[] _password;

        /// <summary>
        /// Rijndael cipher algorithm
        /// 
        private readonly RijndaelManaged _cipher;


        private ICryptoTransform Decryptor { get { return _decryptor; } }
        private ICryptoTransform Encryptor { get { return _encryptor; } }


        /// <summary>
        /// Constructor
        /// 
        /// <param name="password">Public key
        public RijndaelCrypt()
        {
            string password = "&;A&+!UtmI^fx|Cl?ctz";
            //Encode digest
            var md5 = new MD5CryptoServiceProvider();
            _password = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            //Initialize objects
            _cipher = new RijndaelManaged();
            _decryptor = _cipher.CreateDecryptor(_password, IV);
            _encryptor = _cipher.CreateEncryptor(_password, IV);

        }


        /// <summary>
        /// Constructor
        /// 
        /// <param name="password">Public key
        public RijndaelCrypt(string password = "&;A&+!UtmI^fx|Cl?ctz")
        {
            //Encode digest
            var md5 = new MD5CryptoServiceProvider();
            _password = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            //Initialize objects
            _cipher = new RijndaelManaged();
            _decryptor = _cipher.CreateDecryptor(_password, IV);
            _encryptor = _cipher.CreateEncryptor(_password, IV);

        }

        /// <summary>
        /// Decryptor
        /// 
        /// <param name="text">Base64 string to be decrypted
        /// <returns>
        public (string result, string error) Decrypt(string text)
        {
            try
            {
                byte[] input = Convert.FromBase64String(text);

                var newClearData = Decryptor.TransformFinalBlock(input, 0, input.Length);
                return (result: Encoding.ASCII.GetString(newClearData), error: string.Empty);
            }
            catch (ArgumentException ae)
            {
                //Console.WriteLine("inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
                return (result: string.Empty, error: "inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
            }
            catch (ObjectDisposedException oe)
            {
                //Console.WriteLine("The object has already been disposed." + oe);
                return (result: string.Empty, error: "The object has already been disposed." + oe);
            }
        }

        /// <summary>
        /// Encryptor
        /// 
        /// <param name="text">String to be encrypted
        /// <returns>
        public (string result, string error) Encrypt(string text)
        {
            try
            {
                var buffer = Encoding.ASCII.GetBytes(text);
                return (result: Convert.ToBase64String(Encryptor.TransformFinalBlock(buffer, 0, buffer.Length)), error: string.Empty);
            }
            catch (ArgumentException ae)
            {
                //Console.WriteLine("inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
                return (result: string.Empty, error: "inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
            }
            catch (ObjectDisposedException oe)
            {
                //Console.WriteLine("The object has already been disposed." + oe);
                return (result: string.Empty, error: "The object has already been disposed." + oe);
            }

        }

    }
}
