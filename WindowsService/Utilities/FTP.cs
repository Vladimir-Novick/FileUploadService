﻿/*

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
using System.Net;
using System.IO;

namespace SGCombo.Extensions
{
    public class Ftp
    {

    private string host = null;
    private string user = null;
    private string pass = null;
    private FtpWebRequest ftpRequest = null;
    private FtpWebResponse ftpResponse = null;
    private Stream ftpStream = null;
    private int bufferSize = 2048;
    public const string OK = "OK"; 

    public Ftp(string hostIP, string userName, string password) { host = hostIP; user = userName; pass = password; }

    public bool checkConnection()
    {
        try
        {
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public string download(string remoteFile, string localFile)
    {
        try
        {

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);

            ftpRequest.Credentials = new NetworkCredential(user, pass);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpStream = ftpResponse.GetResponseStream();

            FileStream localFileStream = new FileStream(localFile, FileMode.Create);

            byte[] byteBuffer = new byte[bufferSize];
            int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);

            try
            {
                while (bytesRead > 0)
                {
                    localFileStream.Write(byteBuffer, 0, bytesRead);
                    bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            localFileStream.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
        }
        catch (Exception ex) { return ex.ToString(); }
        return Ftp.OK;
    }

    public string upload(string remoteFile, string localFile)
    {
        try
        {

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);

            ftpRequest.Credentials = new NetworkCredential(user, pass);
 
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
 
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            ftpStream = ftpRequest.GetRequestStream();
 
            FileStream localFileStream = new FileStream(localFile, FileMode.Open );

            byte[] byteBuffer = new byte[bufferSize];
            int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);

            try
            {
                while (bytesSent != 0)
                {
                    ftpStream.Write(byteBuffer, 0, bytesSent);
                    bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            localFileStream.Close();
            localFileStream = null;
            ftpStream.Close();
            ftpStream = null;
            ftpRequest = null;
        }
        catch (Exception ex) { 
           return ex.ToString();
        }
        return Ftp.OK;
    }

    public string delete(string deleteFile)
    {
        try
        {

            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteFile);

            ftpRequest.Credentials = new NetworkCredential(user, pass);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpResponse.Close();
            ftpRequest = null;
        }
        catch (Exception ex) { return ex.ToString(); }
        return Ftp.OK;
    }

    public string rename(string currentFileNameAndPath, string newFileName, bool KeepAlive)
    {
        try
        {

            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);

            ftpRequest.Credentials = new NetworkCredential(user, pass);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = KeepAlive;

            ftpRequest.Method = WebRequestMethods.Ftp.Rename;

            ftpRequest.RenameTo = newFileName;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpResponse.Close();
            ftpRequest = null;
        }
        catch (Exception ex) { return ex.ToString(); }
        return Ftp.OK;
    }

    public string createDirectory(string newDirectory)
    {
        try
        {

            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);

            ftpRequest.Credentials = new NetworkCredential(user, pass);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpResponse.Close();
            ftpRequest = null;
        }
        catch (Exception ex) { return ex.Message; }
        return Ftp.OK;
    }

    /* Get the Date/Time a File was Created */
    public string getFileCreatedDateTime(string fileName)
    {
        try
        {

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);

            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);

            string fileInfo = null;

            try { fileInfo = ftpReader.ReadToEnd(); }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;

            return fileInfo;
        }
        catch (Exception ex) { return "Error: " + ex.ToString(); }

        return "";
    }

    public string getFileSize(string fileName)
    {
        try
        {

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);

            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);

            string fileInfo = null;

            try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;

            return fileInfo;
        }
        catch (Exception ex) { return "Error: " + ex.ToString(); }

        return "";
    }

    public string[] directoryListSimple(string directory)
    {
        try
        {

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);

            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);

            string directoryRaw = null;

            try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
            try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }
        catch (Exception ex) { Console.WriteLine(ex.ToString()); }

        return new string[] { "" };
    }

    /* List Directory Contents in Detail (Name, Size, Created, etc.) */
    public string[] directoryListDetailed(string directory)
    {
        try
        {

            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);

            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);

            string directoryRaw = null;

            try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
            try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }
        catch (Exception ex) { Console.WriteLine(ex.ToString()); }

        return new string[] { "" };
    }
    }
}
