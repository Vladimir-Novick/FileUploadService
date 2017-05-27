////////////////////////////////////////////////////////////////////////////
//	Copyright 2012-2016 : Vladimir Novick    https://www.linkedin.com/in/vladimirnovick/  
//
//    NO WARRANTIES ARE EXTENDED. USE AT YOUR OWN RISK.
//
//    Available under the BSD and MIT licenses
//
// To contact the author with suggestions or comments, use  :vlad.novick@gmail.com
//
////////////////////////////////////////////////////////////////////////////
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;

using System.Runtime.InteropServices;   
using System.Reflection;                

namespace SGCombo.FileUploadService.Utils
{
    public class GlobalNamedLock
    {
        private Mutex mtx;

        public GlobalNamedLock(string strLockName)
        {
               if (string.IsNullOrWhiteSpace(strLockName))
            {
                strLockName = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            }

            //Create security permissions for everyone
            //It is needed in case the mutex is used by a process with
            //different set of privileges than the one that created it
            //Setting it will avoid access_denied errors.
            MutexSecurity mSec = new MutexSecurity();
            mSec.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                MutexRights.FullControl, AccessControlType.Allow));

            bool bCreatedNew;
            mtx = new Mutex(false, @"Global\" + strLockName, out bCreatedNew, mSec);
        }

        public bool enterCRITICAL_SECTION()
        {

            return mtx.WaitOne();
        }

        public void leaveCRITICAL_SECTION()
        {
            mtx.ReleaseMutex();
        }
    }
}