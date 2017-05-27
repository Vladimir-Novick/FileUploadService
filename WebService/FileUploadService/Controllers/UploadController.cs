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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Configuration;
using System;
using SGCombo.FileUploadService.Utils;
using System.Net.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading;


    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public CustomMultipartFormDataStreamProvider(string path)
            : base(path)
        {
        }

        public override string GetLocalFileName(System.Net.Http.Headers.HttpContentHeaders headers)
        {
            var name = !string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName) ? headers.ContentDisposition.FileName : "NoName";
            return name.Replace("\"", string.Empty);
        }
    }


public class UploadController : ApiController
{


    public static void AppendToFile(string outputFile, string inputFile)
    {
        byte[] bytes = File.ReadAllBytes(inputFile);

        FileStream stream = new FileStream(outputFile, FileMode.Append);
        {
            stream.Write(bytes, 0, bytes.Length);
        }
        stream.Flush();
        stream.Close();
    }



    public async Task<HttpResponseMessage> PostFile()
    {
        // Check if the request contains multipart/form-data.
        if (!Request.Content.IsMimeMultipartContent())
        {
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }




        string root = HttpContext.Current.Server.MapPath("~/App_Data");


        CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(root);

     
        string UploadFolder = ConfigurationManager.AppSettings["UploadFolder"];


        try
        {
            StringBuilder sb = new StringBuilder(); // Holds the response body


            // Read the form data and return an async task.
            await Request.Content.ReadAsMultipartAsync(provider);

           
            string folderName = UploadFolder;

            string part = "";

            string FinishFileName = "";
            string sections = "0";
            int intSection = 0;
            string CHECK = "";


            // This illustrates how to get the form data.
            foreach (var key in provider.FormData.AllKeys)
            {
                foreach (var val in provider.FormData.GetValues(key))
                {
                    String strKey = key.Replace("\"", "");



                    switch (strKey)
                    {
                        case "TOKEN":
                            folderName = System.IO.Path.Combine(folderName, val) + ".PART";
                            break;
                        case "PART":
                            part = val;
                            break;
                        case "FINISHED":
                            FinishFileName = val;
                            break;
                        case "CHECK":
                            CHECK = val;
                            break;

                        case "SECTIONS":
                            sections = val;
                            break;

                    }

                   sb.Append(string.Format("{0}: {1}\n", key, val));


                }


                Int32.TryParse(sections, out intSection);



            
                sb.Append(string.Format("{0}: {1}\n", "New_ Directory", folderName));

            }


            String folderData = "";

            try
            {
                GlobalNamedLock gl = new GlobalNamedLock("SGCOMBO_COM_FILE_UPLOADER");

                try
                {
                    if (gl.enterCRITICAL_SECTION())
                    {
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                            folderData = System.IO.Path.Combine(folderName, "DATA");
                            Directory.CreateDirectory(folderData);
                        }
                        else
                        {
                            folderData = System.IO.Path.Combine(folderName, "DATA");
                        }


                    }
                }
                finally
                {
                    gl.leaveCRITICAL_SECTION();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ctitical section " + ex.Message);
            }


            if ((FinishFileName.Length == 0) && ( CHECK.Length == 0 ))
            {
                foreach (var file in provider.FileData)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file.LocalFileName);
                        string fileName = file.Headers.ContentDisposition.Name;
                        sb.Append(string.Format("Uploaded block: {0} , name: {2} , ({1} bytes)\n", fileInfo.Name, fileInfo.Length, fileName));

                        String fName = fileName.Replace("\"", "");

                        String destinationName = folderData + @"\" + fName;

                        destinationName += "-" + part;
                        Console.WriteLine(">" + fName + "-" + part.ToString());



                        int cout = 0;

                        do
                        {
                            try
                            {
                                fileInfo.Refresh();
                                fileInfo.CopyTo(destinationName);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(100);
                                cout++;
                            }

                        } while (cout < 100);


                        cout = 0;

                        do
                        {
                            try
                            {
                                fileInfo.Refresh();
                                fileInfo.Delete();
                                break;
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(100);
                                cout++;
                            }
                        } while (cout < 100);



                     
                        
                       
                    }
                    catch (Exception ex)
                    {
                        sb.Append("Error" + ex.Message);
                        Console.Write("*****" + ex.Message);

                    }
                }
            }
            else
            {
                try
                {

                    bool operationFinish = false;
                    if (FinishFileName.Length > 0) operationFinish = true;
                    String fName = FinishFileName.Replace("\"", "");
                  
                    Console.WriteLine("Start Verification : " + fName);
                 

                    String destinationName = folderData + @"\" + fName;
                       String f = destinationName  + "-0";
                        if (!File.Exists(f)){
                            throw new Exception(String.Format(" File {0} not exists ", Path.GetFileName(f)));
                        }
                    if (operationFinish)
                    {
                        File.Move(f, destinationName);
                    }
                   


                    for (int i = 1; i < intSection; i++)
                    {
                      
                        sb.Append(Environment.NewLine);
                       String inpFile = destinationName  +"-" + i.ToString();
                        if (operationFinish)
                        {  
                            if (!File.Exists(inpFile))
                            {
                                throw new Exception(String.Format(" File {0} not exists ", Path.GetFileName(inpFile)));
                            }
                           
                            AppendToFile(destinationName, inpFile);
                            File.Delete(inpFile);
                        }
                        else
                        {
                            
                            if (!File.Exists(inpFile))
                            {
                                throw new Exception(String.Format(" File {0} not exists ", Path.GetFileName(inpFile)));
                            }
                        }
                    }
                  
                  
                    sb.Clear();
                    sb.Append("OK" );
                }
                catch (Exception ex)
                {
                    sb.Clear();
                    sb.Append("Error: " + ex.Message);
                }
                        
            }

      //      String newFolderExtention = folderName.Replace(".PART", ".NEW");
      //      DirectoryInfo dirInfo = new DirectoryInfo(folderName);
      //      dirInfo.Refresh();
       //     dirInfo.MoveTo(newFolderExtention);

            return new HttpResponseMessage()
            {
                Content = new StringContent(sb.ToString())
            };
        }
        catch (System.Exception e)
        {
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        }
    }

}