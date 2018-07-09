/*

Copyright (C) 2014-2018 by Vladimir Novick http://www.linkedin.com/in/vladimirnovick ,

    vlad.novick@gmail.com , http://www.sgcombo.com , https://github.com/Vladimir-Novick
	

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
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
