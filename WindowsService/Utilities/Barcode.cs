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
