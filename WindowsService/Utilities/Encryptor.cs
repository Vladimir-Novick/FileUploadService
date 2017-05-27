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
using System.Security.Cryptography;

namespace SGCombo.SimpleUtils
{
    public class Encryptor
    {
        private static readonly string DEFAULT_KEY = "8831234A-F34A-4107-8B61-0AE5C9743C9A";
        private static HMACSHA1 s_objHmac = new HMACSHA1(ConvertStringToByteArray(DEFAULT_KEY));
        private static UnicodeEncoding s_objEncoding;

        private Encryptor()
        {
        }

        public static string CreatePassword(string sValue)
        {

            byte[] lo_arrHash = s_objHmac.ComputeHash(ConvertStringToByteArray(sValue));
            string l_sMAC = Convert.ToBase64String(lo_arrHash, 0, lo_arrHash.Length);
            return l_sMAC;
        }

        public static bool ValidateMAC(string sValue, string sMAC)
        {
            string l_sMAC = CreatePassword(sValue);
            return l_sMAC == sMAC;
        }

        private static Byte[] ConvertStringToByteArray(String s)
        {
            if (s_objEncoding == null)
                s_objEncoding = new UnicodeEncoding();
            Byte[] l_arr = s_objEncoding.GetBytes(s);
            return l_arr;
        }
    }
}
