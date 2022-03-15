using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safebox
{
    internal class AesHandler
    {
        //Generate temporary key for store string securly in memory
        private static string memK = System.Guid.NewGuid().ToString();
        //Generate temporary salt for store string securly
        private static string memS = System.Guid.NewGuid().ToString();

        /// <summary>
        /// Encode some string to prevent stole
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Encoded string</returns>
        public static string memEnc(string text)
        {        
            return Encrypt(memK, text, memS);
        }

        /// <summary>
        /// Decode the encoded text which is temporarily stored in memory.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Decoded string</returns>
        public static string memDec(string text)
        {
            return Decrypt(memK, text, memS);
        }

        public static string Encrypt(string key, string encryptString, string salt)
        {
            string EncryptionKey = key;
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            byte[] saltBytes = Encoding.Unicode.GetBytes(salt);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, saltBytes);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }

        public static string Decrypt(string key, string cipherText, string salt)
        {
            string EncryptionKey = key;
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] saltBytes = Encoding.Unicode.GetBytes(salt);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, saltBytes);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }




    }
}

