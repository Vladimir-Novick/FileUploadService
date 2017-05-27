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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCombo.Services
{
    public class IdentifyQueryBackground
    {
        public string directoryPatch { get; set; }
        public IdentifyQueryBackground(string _dirName)
        {
            directoryPatch = _dirName;
            error_message = "";
            status_error = false;
            deleteFolder = true;
        }


        public IdentifyQueryBackground(string _dirName, string _errorText, Boolean error)
        {
            directoryPatch = _dirName;
            error_message = _errorText;
            status_error = error;
            deleteFolder = true;
        }

        public Boolean status_error { get; set; }
        public string error_message { get; set; }
        public string watchDirectory { get; set; }

        public string FTPServer { get; set; }
        public string FTP_userName { get; set; }
        public string FTP_password { get; set; }

        public string NET_FTPServer { get; set; }
        public string NET_userName { get; set; }
        public string NET_password { get; set; }

      
        public Boolean deleteFolder { get; set; }


    }
}
