using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safebox
{
    public class CryptoCore
    {
        /// <summary>
        /// A titkosítás alkalmával használt "sózás" hossza
        /// </summary>
        private static readonly int SALT_LENGTH = 56;

        /// <summary>
        /// ZeroMemory Windows API függvényhívás
        /// </summary>
        /// <remarks>
        ///  egy meghatározott memória területet feltölt <b>nullákkal</b><br/> 
        ///  arra használható, hogy az általunk begépelt jelszót<br/>
        ///  felhasználását követően azonnal ki is töröljük a memóriából
        /// </remarks>
        /// <param name="Destination">A mutató egy memória címre</param>
        /// <param name="Length">Memória byte-ok száma</param>
        /// <returns></returns>
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        /// <summary>
        /// Random generátor sózáshoz
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[SALT_LENGTH];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // A buffert feltöltjük a generált adatokkal
                    rng.GetBytes(salt);
                }
            }
            return salt;
        }

        /// <summary>
        /// Memóriában lévő objektum szérializációja és tikosítva való kiírása
        /// </summary>
        /// <param name="fileName">Fájlnév amibe az objektumunkat titkosítva ki szeretnénk írni</param>
        /// <param name="password">Jelszó a titkosításhoz</param>
        /// <param name="obj">Memória objektum</param>
        public void encrypt(string fileName, string password, Object obj)
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files
            byte[] salt = GenerateRandomSalt();

            FileStream fsCrypt = new FileStream(fileName, FileMode.Create);

            //convert password string to byte arrray
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CFB;

            // write salt to the begining of the output file, so in this case can be random every time
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            //Instantiate a memory stream object.
            Stream serializedStream = new MemoryStream();

            //Instantiate binary formatter object.
            IFormatter formatterEn = new BinaryFormatter();

            //First serialize our data object to memory stream.        
            formatterEn.Serialize(serializedStream, obj);

            //reset back out stream to Position 0. this is due to the serialization process, the stream data position has reach

            //the last position. this is important else we might face the  Exception as 'Binary stream '0' does not contain a valid BinaryHeader.

            serializedStream.Seek(0, SeekOrigin.Begin);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = serializedStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cs.Write(buffer, 0, read);
                }

                // Close up
                serializedStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }

        }

        /// <summary>
        /// Fájlba rögzített objektum helyreállítása a memóriába
        /// </summary>
        /// <param name="filename">A fájl elérési útvonala</param>
        /// <param name="password">Jelszó a helyreállításhoz</param>
        /// <returns></returns>
        public Object decrypt(string filename, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[SALT_LENGTH];

            FileStream fsCrypt = new FileStream(filename, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            //create a new memory stream.
            MemoryStream memoryStream = new MemoryStream();

            int read;
            byte[] buffer = new byte[1048576];

            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Application.DoEvents();
                memoryStream.Write(buffer, 0, read);
            }

            //create formatter.

            IFormatter formatter = new BinaryFormatter();

            //reposition our memory stream to position 0.

            memoryStream.Seek(0, SeekOrigin.Begin);

            Object o = formatter.Deserialize(memoryStream);

            cs.Flush();
            cs.Close();
            fsCrypt.Close();
            memoryStream.Flush();
            memoryStream.Close();
            return o;
        }

        /// <summary>
        /// Fájl titkosítása kulcs segítségével, amennyiben a oneself értéke igaz, egy új fájl jön létre az eredetiből
        /// és az új fájl lesz titkosítva valamint kap egy .sec fájlvégződést
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        /// <param name="oneself"></param>
        public void FileEncrypt(string inputFile, string password, bool oneself)
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files

            //generate random salt
            byte[] salt = GenerateRandomSalt();

            //convert password string to byte arrray
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CFB;

            //Önmagát titkosítjuk
            if (oneself)
            {
                MemoryStream fsInM = null;
                CryptoStream cs = null;
                FileStream fsCrypt = null;
                try
                {
                    fsInM = new MemoryStream();
                    // fsInM.Write(salt, 0, salt.Length);
                    FileStream fsIn = new FileStream(inputFile, FileMode.Open);
                    byte[] buffer = new byte[1048576];
                    int read;
                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                        fsInM.Write(buffer, 0, read);
                    }

                    // Felolvastuk a fájlt nincs már rá szükség bezárjuk
                    fsIn.Close();

                    fsCrypt = new FileStream(inputFile, FileMode.Create);
                    // write salt to the begining of the output file, so in this case can be random every time
                    fsCrypt.Write(salt, 0, salt.Length);
                    cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
                    buffer = new byte[1048576];
                    read = 0;
                    fsInM.Flush();
                    fsInM.Seek(0, SeekOrigin.Begin);
                    while ((read = fsInM.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                        cs.Write(buffer, 0, read);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                }
                finally
                {
                    if (cs != null)
                    {
                        cs.Close();
                    }

                }
            }
            else
            {
                FileStream fsCrypt = new FileStream(inputFile + ".sec", FileMode.Create);
                // write salt to the begining of the output file, so in this case can be random every time
                fsCrypt.Write(salt, 0, salt.Length);
                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
                FileStream fsIn = new FileStream(inputFile, FileMode.Open);
                //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
                byte[] buffer = new byte[1048576];
                int read;
                try
                {
                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                        //Ha önmagát titkosítjuk akkor elsőnek kiolvassuk az egész fájlt memóriába
                        cs.Write(buffer, 0, read);
                    }
                    // Felolvastuk a fájlt nincs már rá szükség
                    fsIn.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    cs.Close();
                    fsCrypt.Close();
                }
            }
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        public void FileDecrypt(string inputFile, string outputFile, string password)
        {

            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[SALT_LENGTH];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;

            //Ha nincs megadva kimeneti fájl akkor önmagára fejtjük vissza
            if (outputFile == null)
            {
                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
                MemoryStream inFsM = new MemoryStream();

                int read;
                byte[] buffer = new byte[1048576];

                try
                {
                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Application.DoEvents();
                        inFsM.Write(buffer, 0, read);
                    }
                }
                catch (CryptographicException ex_CryptographicException)
                {
                    Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    GC.SuppressFinalize(this);
                }

                inFsM.Flush();
                fsCrypt.Close();
                cs.Close();


                inFsM.Seek(0, SeekOrigin.Begin);
                buffer = new byte[1048576];
                FileStream fsOut = new FileStream(inputFile, FileMode.Create);
                try
                {
                    while ((read = inFsM.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Application.DoEvents();
                        fsOut.Write(buffer, 0, read);
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    fsOut.Flush();
                    fsOut.Close();
                    inFsM.Close();
                }



            }
            else
            {


                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
                FileStream fsOut = new FileStream(outputFile, FileMode.Create);

                int read;
                byte[] buffer = new byte[1048576];

                try
                {
                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Application.DoEvents();
                        fsOut.Write(buffer, 0, read);
                    }
                }
                catch (CryptographicException ex_CryptographicException)
                {
                    Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                try
                {
                    cs.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error by closing CryptoStream: " + ex.Message);
                }
                finally
                {
                    fsOut.Close();
                    fsCrypt.Close();
                }
            }
        }

    }
}
