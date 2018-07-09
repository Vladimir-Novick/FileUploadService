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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Data;
using System.IO;
using SGCombo.Extensions;

namespace SGCombo.Services
{
    public class JobStack
    {
        List<BackgroundWorker> list_BackgroundWorker = new List<BackgroundWorker>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void StartFTPWorkProcess(IdentifyQueryBackground param)
        {
            try
            {
                BackgroundWorker m_BackgroundWorkerAsync = new BackgroundWorker();
                m_BackgroundWorkerAsync.WorkerSupportsCancellation = true;
                m_BackgroundWorkerAsync.DoWork += new DoWorkEventHandler(BackgroundWorkerAsyncRequest_DoWork);
                m_BackgroundWorkerAsync.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorkerAsyncRequest_RunWorkerCompleted);

                m_BackgroundWorkerAsync.RunWorkerAsync(param);
                list_BackgroundWorker.Add(m_BackgroundWorkerAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Directory {0} Message {1}", param.directoryPatch, ex.Message);
            }
        }

        private static List<String> DirSearch(string sDir)
        {
            List<String> files = new List<String>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (System.Exception excpt)
            {

            }

            return files;
        }

        private static DataTable ReadBaseOrderData(string fileLocationDirectory)
        {
            DataTable dataTableUserInfo = null;

            return dataTableUserInfo;

        }

        private static void saveStatus(String ORDER_ID, String DirectoryID, String Status, int Status_ID, String connectionString)
        {

        }

        private static void BackgroundWorkerAsyncRequest_DoWork(object sender, DoWorkEventArgs e)
        {

            IdentifyQueryBackground identifiedQuery = null;

            identifiedQuery = (IdentifyQueryBackground)e.Argument;
            IdentifyQueryBackground identifiedQueryRet = null;

            String sourceDirName = identifiedQuery.directoryPatch;
            char[] delimiterChars = { '\\' };

            String baseDirectory = null;
            String sourceDir = "";
            String completedDir = "";

            Ftp ftp = null;

            DataRow row = null;
            String mantage = "";

            try
            {
                List<String> filesList = DirSearch(sourceDirName);
                for (int i = 0; i < filesList.Count; i++)
                {
                    string inpFile = filesList[i];
                    string ftpFile = inpFile.Substring(identifiedQuery.watchDirectory.Length + 1);
                    string fileName = Path.GetFileName(inpFile);
                    String ftpPath = ftpFile.Substring(0, ftpFile.Length - fileName.Length);

                    if (baseDirectory == null)
                    {
                        String[] s = ftpPath.Split(delimiterChars);
                        baseDirectory = s[0];

                    }

                        try
                        {

                                ftp = new Ftp(identifiedQuery.FTPServer, identifiedQuery.FTP_userName, identifiedQuery.FTP_password);

                        }
                        catch (Exception ex)
                        {

                            SimpleLog.WriteError(SGCombo_UploadServiceStart.logDirectory, "Error:  > " + completedDir + " Message " + ex.Message);

                            return;
                        }

                    ftp.createDirectory(ftpPath);
                    ftp.upload(ftpFile, inpFile);
                }

                String new_baseDirectory = baseDirectory.Replace("_PART", "_NEW");
                ftp.rename(baseDirectory, new_baseDirectory, false);

                 identifiedQueryRet = new IdentifyQueryBackground(identifiedQuery.directoryPatch, "", false);
                e.Result = identifiedQueryRet;

                 sourceDir = identifiedQuery.watchDirectory + "\\" + baseDirectory;

                 if (identifiedQuery.deleteFolder)
                 {
                     Directory.Delete(sourceDir, true);

                 }
                 else
                 {
                     completedDir = sourceDir.Replace("_PART", "_COMPLETED");
                     Directory.Move(sourceDir, completedDir);
                 }

                 SimpleLog.WriteLog(SGCombo_UploadServiceStart.logDirectory, "Compleyed:  > "  + baseDirectory);

            }
            catch (Exception ex)
            {
                 identifiedQueryRet = new IdentifyQueryBackground(identifiedQuery.directoryPatch, ex.Message, true);
                e.Result = identifiedQueryRet;
                 sourceDir = identifiedQuery.watchDirectory + "\\" + baseDirectory;
                 completedDir = sourceDir.Replace("_PART", "_ERR");
                Directory.Move(sourceDir, completedDir);

                SimpleLog.WriteError(SGCombo_UploadServiceStart.logDirectory, "Error:  > " + completedDir + " Message " + ex.Message);
            }

        }

        void BackgroundWorkerAsyncRequest_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            IdentifyQueryBackground identifiedQuery = null;
            identifiedQuery = (IdentifyQueryBackground)e.Result;


            try
            {
                if (identifiedQuery.status_error)
                {
                    Console.WriteLine("Error: Directory {0} Message: {1}", identifiedQuery.directoryPatch, identifiedQuery.error_message);
                }
                else
                {
                    Console.WriteLine("Completed.");
                }
            }
            finally
            {
                try
                {
                    BackgroundWorker backgroundWorker = sender as BackgroundWorker;

                    list_BackgroundWorker.Remove(backgroundWorker);

                    identifiedQuery = null;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;

                }
                catch (Exception) { }

            }

        }

    }

}
