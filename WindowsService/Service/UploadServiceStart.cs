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
using System.Reflection;
using System.IO;
using System.Threading;
using System.Configuration;



namespace SGCombo.Services
{
    public class SGCombo_UploadServiceStart
    {

        private string watchDirectory { get; set; }

        public string FTPServer { get; set; }
        public string FTP_userName { get; set; }
        public string FTP_password { get; set; }



        public static string logDirectory { get; set; } 
        public string NET_FTPServer { get; set; }
        public string NET_userName { get; set; }
        public string NET_password { get; set; }
        public string DataBaseConnectionString { get; set; }

        private Boolean deleteFolder { get; set; }

     



        public SGCombo_UploadServiceStart()
        {

            DataBaseConnectionString = ConfigurationManager.AppSettings["DataBaseConnectionString"];

            watchDirectory = ConfigurationManager.AppSettings["CacheFolder"];
            logDirectory = watchDirectory + @"/Log";
            Directory.CreateDirectory(logDirectory);
            FTPServer = ConfigurationManager.AppSettings["FTPServer"];
            FTP_userName = ConfigurationManager.AppSettings["FTP_userName"];
            FTP_password = ConfigurationManager.AppSettings["FTP_password"];

            NET_FTPServer = ConfigurationManager.AppSettings["NET_FTPServer"];
            NET_userName = ConfigurationManager.AppSettings["NET_userName"];
            NET_password = ConfigurationManager.AppSettings["NET_password"];

            string str_deleteFolder = ConfigurationManager.AppSettings["deleteFolder"];
            if (str_deleteFolder != null ) {
                str_deleteFolder = str_deleteFolder.ToLower();
            }
            
            if (str_deleteFolder == "true")
            {
                deleteFolder = true;
            }
            else
            {
                deleteFolder = false;
            }


        }



        public void OnStop()
        {
            oSignalEvent.Reset();
        }


        ManualResetEvent oSignalEvent = new ManualResetEvent(false);

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }



        public FileSystemWatcher watcher = null;
        private JobStack jobStack = null;


        public void OnStart()
        {
            try
            {
                jobStack = new JobStack();
                watcher = new FileSystemWatcher();

                watcher.NotifyFilter = NotifyFilters.DirectoryName;

                watcher.Path = watchDirectory;


                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnCreated);
                watcher.Deleted += new FileSystemEventHandler(OnDeleted);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                watcher.EnableRaisingEvents = true;

                oSignalEvent.Set();



            }
            catch (Exception ex)
            {

                //   ServiceExceptionLogger cr = new ServiceExceptionLogger();
                //    cr.WriteToLog("Service Start: " + ex.Message, ex);
                //    throw ex;
            }

        }



        #region FileSystemWatcher

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {

            //           Console.WriteLine("File Changed: " + e.Name + " " + e.ChangeType);
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {

            //           Console.WriteLine("File Created: " + e.Name + " " + e.ChangeType);
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {

            //          Console.WriteLine("File Deleted: " + e.Name + " " + e.ChangeType);
        }


        private static Object lockOnRenamed = new Object();

        private void OnRenamed(object source, RenamedEventArgs e)
        {




            if (e.FullPath.Contains("_PART"))
            {
                Console.WriteLine("PROCESSED : {0}", e.Name);

                IdentifyQueryBackground param = new IdentifyQueryBackground(e.FullPath);
                param.watchDirectory = watchDirectory;

                param.FTPServer = this.FTPServer;
                param.FTP_userName = this.FTP_userName;
                param.FTP_password = this.FTP_password;

                param.NET_FTPServer = this.NET_FTPServer;
                param.NET_userName = this.NET_userName;
                param.NET_password = this.NET_password;



                jobStack.StartFTPWorkProcess(param);
            }
        }



        #endregion



    }
}
