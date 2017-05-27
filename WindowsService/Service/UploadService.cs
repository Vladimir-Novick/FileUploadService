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
using SGCombo.Services;
using System;
using System.ServiceProcess;

namespace SGCombo.Upload.FTP.Service
{

    public class SGCombo_UploadService : ServiceBase
    {

        internal void TestStartupAndStop(string[] args)
        {
            Console.WriteLine("SGCombo.FTP.Uploader Started.. ");
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }



        private System.ComponentModel.Container components = null;



        public SGCombo_UploadService()
        {

            InitializeComponent();


        }




        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                SGCombo_UploadService service1 = new SGCombo_UploadService();
                service1.TestStartupAndStop(args);
            }
            else
            {
                Main1();
            }
        }




        static void Main1()
        {
            System.ServiceProcess.ServiceBase[] ServicesToRun;


            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SGCombo_UploadService() };

            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        }


        private void InitializeComponent()
        {
 
            this.ServiceName = "SGCombo.FTP.Uploader";


            String s = System.AppDomain.CurrentDomain.BaseDirectory;
            System.IO.Directory.SetCurrentDirectory(s);


        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        Object wmServiceStart = null;

        protected override void OnStart(string[] args)
        {
            
            wmServiceStart = new SGCombo_UploadServiceStart();
            ((SGCombo_UploadServiceStart)wmServiceStart).OnStart();
        }


        protected override void OnStop()
        {


            ((SGCombo_UploadServiceStart)wmServiceStart).OnStart(); ((SGCombo_UploadServiceStart)wmServiceStart).OnStop();

        }
    }
}
