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
using System.Text;
using System.Collections.Generic;

namespace SGCombo.Extensions.Utilites
{
    public class LogLine 
    {
        public bool Debug = false;
        public string Message = "";
        public DateTime Time = System.DateTime.Now;
    }

    public class Logger
    {
        private StreamWriter Writer = null;
        public List<LogLine> Lines = new List<LogLine>();

        public event EventHandler<LineArgs> NewLine;

        public Logger(string Path, string Name)
        {
            string LogDir = System.IO.Path.Combine(Path, "Logs");
            string LogFile = System.IO.Path.Combine(LogDir, Name + ".log");

            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);

            Writer = new StreamWriter(LogFile, true);
        }

        ~Logger()
        {
            Close();
        }

        public void LogLine(string Val)
        {
            var LL = new LogLine();

            if (Val.StartsWith("DBG:"))
            {
                LL.Debug = true;
                Val = Val.Substring(5);
            }

            LL.Message = Val;

            if (Lines.Count > 50000) Lines.Clear();
            Lines.Add(LL);

            string sText = "[" + LL.Time + "] " + LL.Message;

            if ((Writer != null) &&  (Writer.BaseStream != null))
            {
                Writer.WriteLine(sText);
                Writer.Flush();
            }

            if (NewLine != null) NewLine(this, new LineArgs(LL));
        }

        public void Close()
        {
            if (Writer != null)
            {
                Writer.Close();
                Writer = null;
            }
        }
    }

    public class ConsoleWriterEventArgs : EventArgs
    {
        public string Value { get; private set; }

        public ConsoleWriterEventArgs(string value)
        {
            Value = value;
        }
    }

    public class ConsoleWriter : TextWriter
    {
        public event EventHandler<ConsoleWriterEventArgs> WriteEvent;
        public event EventHandler<ConsoleWriterEventArgs> WriteLineEvent;

        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public override void Write(string value)
        {
            if (WriteEvent != null) WriteEvent(this, new ConsoleWriterEventArgs(value));
            base.Write(value);
        }

        public override void WriteLine(string value)
        {
            if (WriteLineEvent != null) WriteLineEvent(this, new ConsoleWriterEventArgs(value));
            base.WriteLine(value);
        }
    }
}
