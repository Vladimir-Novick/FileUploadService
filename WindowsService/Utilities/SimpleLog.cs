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
using System.IO;

namespace SGCombo.Extensions
{
    public static class SimpleLog
    {
        public static Object LockLogFile = new Object();

        public static void WriteLog(String logDirectory, String text)
        {
            WriteToLogFile(logDirectory, text,"log");
        }

        public static void WriteError(String logDirectory, String text)
        {
            WriteToLogFile(logDirectory, text, "err");
        }

        private static void WriteToLogFile(String logDirectory, String message,String extention)
        {
            lock (LockLogFile)
            {
                try
                {

                    DateTime dateNow = DateTime.Now;
                    String text = String.Format("{0:00}:{1:00}:{2:00} - {3}", dateNow.Hour, dateNow.Minute, dateNow.Second, message);
                    String logFile = logDirectory + "/" + dateNow.Year.ToString() + "-" + (dateNow.Month + 1).ToString().PadLeft(2, '0') + "-" + dateNow.Day + "." + extention;

                    if (!File.Exists(logFile))
                    {

                        using (StreamWriter sw = File.CreateText(logFile))
                        {
                            sw.WriteLine(text);
                        }
                    }
                    else
                    {

                        using (StreamWriter sw = File.AppendText(logFile))
                        {
                            sw.WriteLine(text);
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }
}
