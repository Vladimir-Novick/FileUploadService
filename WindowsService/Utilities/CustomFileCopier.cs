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

namespace SGCombo.Extensions.Utils
{
    public delegate void ProgressChangeDelegate(double Persentage, ref bool Cancel);
    public delegate void Completedelegate(String Message);

    public class CustomFileCopier
    {
        public CustomFileCopier(string Source, string Dest)
        {
            this.SourceFilePath = Source;
            this.DestFilePath = Dest;

            OnProgressChanged += delegate { };
            OnComplete += delegate { };
        }

        public Boolean Copy()
        {
            try
            {
                byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                bool cancelFlag = false;

                using (FileStream source = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    long fileLength = source.Length;
                    using (FileStream dest = new FileStream(DestFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        long totalBytes = 0;
                        int currentBlockSize = 0;

                        while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytes += currentBlockSize;
                            double persentage = (double)totalBytes * 100.0 / fileLength;

                            dest.Write(buffer, 0, currentBlockSize);

                            cancelFlag = false;
                            OnProgressChanged(persentage, ref cancelFlag);

                            if (cancelFlag == true)
                            {
                                OnComplete("Stop User Action");
                                return false;
                            }
                        }
                    }
                }

                OnComplete("");
                return true;
            }
            catch (Exception ex)
            {
                OnComplete(ex.Message);
                return false;
            }

        }

        public string SourceFilePath { get; set; }
        public string DestFilePath { get; set; }

        public event ProgressChangeDelegate OnProgressChanged;
        public event Completedelegate OnComplete;
    }
}
