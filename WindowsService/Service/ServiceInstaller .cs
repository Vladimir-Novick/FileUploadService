
////////////////////////////////////////////////////////////////////////////
//	Copyright 2017 : Vladimir Novick    https://www.linkedin.com/in/vladimirnovick/  
//
//    NO WARRANTIES ARE EXTENDED. USE AT YOUR OWN RISK. 
//
//      Available under the BSD and MIT licenses
//
// To contact the author with suggestions or comments, use  :vlad.novick@gmail.com
//
////////////////////////////////////////////////////////////////////////////
using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;

namespace SGCombo.Upload.FTP.Service
{

    [RunInstaller(true)]
    public class ServiceInstaller : Installer
    {
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;

        private ServiceProcessInstaller processInstaller;

        public ServiceInstaller()
        {
           
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();


           
            processInstaller.Account = ServiceAccount.LocalSystem;

            
            serviceInstaller1.StartType = ServiceStartMode.Automatic;

            
            serviceInstaller1.ServiceName = "SGCombo.FTP.Uploader";
            serviceInstaller1.Description = "Automatic Upload to FTP server ";
          


            Installers.Add(serviceInstaller1);

            Installers.Add(processInstaller);
          
        }


    }
}
