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
