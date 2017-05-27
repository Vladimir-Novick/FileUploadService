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
