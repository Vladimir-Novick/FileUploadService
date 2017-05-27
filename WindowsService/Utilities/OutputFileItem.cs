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

namespace SGCombo.Extensions.Utils
{
    public class OutputFileItem
    {
        public string deviceFileName { get; set; }
        public string outputFilename { get; set; }

        public OutputFileItem(string _deviceFileName, string _outputFilename)
        {
            deviceFileName = _deviceFileName;
            outputFilename = _outputFilename;
        }
    }
}
