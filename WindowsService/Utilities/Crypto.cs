////////////////////////////////////////////////////////////////////////////
//	Copyright 2014 : Vladimir Novick    https://www.linkedin.com/in/vladimirnovick/  
//
//    NO WARRANTIES ARE EXTENDED. USE AT YOUR OWN RISK. 
//
//      Available under the BSD and MIT licenses
//
// To contact the author with suggestions or comments, use  :vlad.novick@gmail.com
//
////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace SGCombo.Extensions.Utils
{

    public class Crypto
    {
        static string passPhrase = "{59446469-CB03-47c3-986B-9BE1C21A16AB} - Vladimir Novick ";
        static string saltValue = "05EC243C@";
        static string hashAlgorithm = "SHA1";
        static int passwordIterations = 2;
        static string initVector = "e5Fc3D46g7H8@1B2";
        static int keySize = 256;

    public static string MD5Hash(string text)
    {
      MD5 md5 = new MD5CryptoServiceProvider();

      md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
 
      byte[] result = md5.Hash;
 
      StringBuilder strBuilder = new StringBuilder();
      for (int i = 0; i < result.Length; i++)
      {
        strBuilder.Append(result[i].ToString("x2"));
      }
 
      return strBuilder.ToString();
    }

        public static String Encrypt(string plainText)
        {
            string cipherText = Crypto.Encrypt(plainText,
                                            passPhrase,
                                            saltValue,
                                            hashAlgorithm,
                                            passwordIterations,
                                            initVector,
                                            keySize);
            return cipherText;
        }

        public static string Decrypt(string cipherText)
        {
            string plainText = Crypto.Decrypt(cipherText,
                                            passPhrase,
                                            saltValue,
                                            hashAlgorithm,
                                            passwordIterations,
                                            initVector,
                                            keySize);
            return plainText;
        }

// ------------------------------------------------------------

        public static string Encrypt(string plainText,
                                     string passPhrase,
                                     string saltValue,
                                     string hashAlgorithm,
                                     int passwordIterations,
                                     string initVector,
                                     int keySize)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(
                                                            passPhrase,
                                                            saltValueBytes,
                                                            hashAlgorithm,
                                                            passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);
            string cipherText = "";
            using (RijndaelManaged symmetricKey = new RijndaelManaged())
            {
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    byte[] cipherTextBytes = memoryStream.ToArray();
                    memoryStream.Close();
                    cryptoStream.Close();
                    cipherText = Convert.ToBase64String(cipherTextBytes);
                }
            }
            return cipherText;
        }
        public static string Decrypt(string cipherText,
                                     string passPhrase,
                                     string saltValue,
                                     string hashAlgorithm,
                                     int passwordIterations,
                                     string initVector,
                                     int keySize)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(
                                                            passPhrase,
                                                            saltValueBytes,
                                                            hashAlgorithm,
                                                            passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);
            string plainText = "";
            using (RijndaelManaged symmetricKey = new RijndaelManaged())
            {
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

                using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                    byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                    memoryStream.Close();
                    cryptoStream.Close();
                    plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                }
            }
            return plainText;
        }
    }
}
