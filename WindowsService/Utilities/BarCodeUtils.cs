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

namespace SGCombo.Extensions.Utils
{
    public class BarCodeUtils
    {

        public static string getBarcod(String orderNo, String deviceName)
        {

            String key = Crypto.Encrypt(orderNo);
            key = key.Substring(0, key.Length - 2);
           // String md5String = Crypto.MD5Hash(key);
            String retCode = key;
            return retCode;
        }
    }
}
