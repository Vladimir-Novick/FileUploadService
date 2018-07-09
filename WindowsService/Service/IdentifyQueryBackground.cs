/*

Copyright (C) 2014 by Vladimir Novick http://www.linkedin.com/in/vladimirnovick ,

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
