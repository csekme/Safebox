using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safebox
{
    internal class AesHandler
    {
        private static string memK = "f!:klo342545z432456zu45645zrtgfv54z6htrvfdpÁQ3FhJn";
        private static string memS = "__--ErDwn";

        public static string memEnc(string text)
        {
            return Encrypt(memK, text, memS);
        }

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

