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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace SGCombo.Extensions.Utils
{
    public class Barcode
    {

        Font plainTextF = new Font("Arial", 13, FontStyle.Regular, GraphicsUnit.Pixel);

        public static System.Drawing.FontFamily LoadFontFamily(string fileName, out PrivateFontCollection fontCollection)
        {
            fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(fileName);
            return fontCollection.Families[0];
        }

        public Image stringToImage(string inputString,String barcodeFont)
        {

            PrivateFontCollection fonts;
            FontFamily family = LoadFontFamily(barcodeFont, out fonts);
            Font barCodeF = new Font(family, 30, FontStyle.Regular, GraphicsUnit.Pixel);
            Bitmap bmp = new Bitmap(1, 1);

            try
            {

            //remove the blank space after and before actual text
            string text = inputString.Trim();

            Graphics graphics = Graphics.FromImage(bmp);

            int barCodewidth = (int)graphics.MeasureString(text, barCodeF).Width;
            int barCodeHeight = (int)graphics.MeasureString(text, barCodeF).Height;

            int plainTextWidth = (int)graphics.MeasureString(text, plainTextF).Width;
            int plainTextHeight = (int)graphics.MeasureString(text, plainTextF).Height;

            //image width 
            if (barCodewidth > plainTextWidth)
            {
                bmp = new Bitmap(bmp,
                                 new Size(barCodewidth, barCodeHeight + plainTextHeight));
            }
            else
            {
                bmp = new Bitmap(bmp,
                                 new Size(plainTextWidth, barCodeHeight + plainTextHeight));
            }
            graphics = Graphics.FromImage(bmp);

            //Specify the background color of the image
            graphics.Clear(Color.White);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            //Specify the text, font, Text Color, X position and Y position of the image
            if (barCodewidth > plainTextWidth)
            {
                //bar code strip
                graphics.DrawString(text,
                                    barCodeF,
                                    new SolidBrush(Color.Black),
                                    0,
                                    0);

                // plain text
                graphics.DrawString(text,
                                    plainTextF,
                                    new SolidBrush(Color.Black),
                                    (barCodewidth - plainTextWidth) / 2,
                                    barCodeHeight);
            }
            else
            {
                //barcode stripe
                graphics.DrawString(text,
                                    barCodeF,
                                    new SolidBrush(Color.Black),
                                    (plainTextWidth - barCodewidth) / 2,
                                    0);

                //plain text
                graphics.DrawString(text,
                                    plainTextF,
                                    new SolidBrush(Color.Black),
                                    0,
                                    barCodeHeight);
            }

            graphics.Flush();
            graphics.Dispose();
            graphics = null;

            //if you want to save the image  uncomment the below line.
            //bmp.Save(@"d:\myimage.jpg", ImageFormat.Jpeg);

            }
            catch (Exception)
            {
            }
            finally
            {
                if (barCodeF != null)
                {

                    barCodeF.Dispose();
                    barCodeF = null;
                }
                if (family != null)
                {

                    family.Dispose();
                    family = null;
                }
            }

            return bmp;
        }
    }
}
