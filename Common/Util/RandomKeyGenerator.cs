

namespace Common.Util
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public interface IRandomKeyGenerator
    {
        string GetUniqueKey(int maxSize);

        string CreateEmbededCustomerKey(string embedItem, int stringLength = 50);

        string GetEmbededCode(string secretString, int stringLength = 9);
    }

    public class RandomKeyGenerator : IRandomKeyGenerator
    {
        public string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public string CreateEmbededCustomerKey(string embedItem, int stringLength = 50)
        {
            Random rnd = new Random();
            int position = rnd.Next(10, 40);
            StringBuilder customerKey = new StringBuilder();
            customerKey.Append($"{position.ToString("000")}{GetUniqueKey(stringLength)}");
            customerKey.Insert(position, embedItem);
            return customerKey.ToString();
        }

        public string GetEmbededCode(string secretString, int stringLength = 9)
        {
            if (int.TryParse(secretString.Substring(0, 3), out int pos))
            {
                return secretString.Substring(pos, stringLength);
            }
            else
            {
                return null;
            }
        }
    }
}
