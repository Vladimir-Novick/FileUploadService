# Windows Service for automatically upload files to FTP server 

You can:

- Easily share and send  files of any format to your team and clients;
- Save and backup all your files with unlimited cloud storage on Real-Time;
- Ability automatic upload a files from cache folder to FTP server.


### Installing UploadFTPService...

	c:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installUtil /i UploadFTPService.exe
	
### UnInstalling UploadFTPService...

	c:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installUtil /u UploadFTPService.exe	
	
### FTP configuration :

 file App.config:	

      <appSettings>
         <add key="deleteFolder" value="true"/>    
         <add key="CacheFolder" value="D:\ftp_cache"/>        
         <add key="FTPServer" value="ftp://192.168.11.51"/>  
         <add key="FTP_userName" value="order"/>
         <add key="FTP_password" value="password"/>
     </appSettings>


Copyright (C) 2016-2018 by Vladimir Novick http://www.linkedin.com/in/vladimirnovick , 

vlad.novick@gmail.com , http://www.sgcombo.com , https://github.com/Vladimir-Novick
		 
# License
		 
		 Copyright (C) 2016-2018 by Vladimir Novick http://www.linkedin.com/in/vladimirnovick

		Permission is hereby granted, free of charge, to any person obtaining a copy
		of this software and associated documentation files (the "Software"), to deal
		in the Software without restriction, including without limitation the rights
		to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		copies of the Software, and to permit persons to whom the Software is
		furnished to do so, subject to the following conditions:

		The above copyright notice and this permission notice shall be included in
		all copies or substantial portions of the Software.

		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
		IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
		AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
		OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
		THE SOFTWARE. 
