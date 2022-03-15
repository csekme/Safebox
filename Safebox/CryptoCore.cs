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
        /// ZeroMemory Windows API method
        /// </summary>
        /// <remarks>
        /// Fills a specified memory area with zeros
        /// can be used to make the password we type
        /// immediately after the password is used
        /// </remarks>
        /// <param name="Destination">A mutató egy memória címre</param>
        /// <param name="Length">Memory in bytes</param>
        /// <returns></returns>
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);


        public static void cleanString(ref string stringholder )
        {
            GCHandle gch = GCHandle.Alloc(stringholder, GCHandleType.Pinned);
            CryptoCore.ZeroMemory(gch.AddrOfPinnedObject(), stringholder.Length * 2);
            gch.Free();
            gch = GCHandle.Alloc(stringholder, GCHandleType.Pinned);
            CryptoCore.ZeroMemory(gch.AddrOfPinnedObject(), stringholder.Length * 2);
            gch.Free();
        }
 
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

  
     

    }
}
