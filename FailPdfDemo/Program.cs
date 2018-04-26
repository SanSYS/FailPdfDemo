using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using HiQPdf;

namespace FailPdfDemo
{
    public class Program
    {
        public static void Main()
        {
            #region генерация PDF
            var urls = new List<string>
            {
                "http://localhost/api/def/get",
                "http://localhost/api/con.aspx",
                "http://localhost/api/def/get/1",
                @"C:\inetpub\wwwroot\api\web.config"
            };

            foreach (var url in urls)
            {
                string htmlCode = $"<iframe src='{url}' width='1024' height='768'></iframe>";

                htmlCode = "<br/><br/><br/>" + htmlCode.Replace("<", "&lt;") + htmlCode;

                CreatePdfHq(htmlCode);
            }
            #endregion

            #region генерация PNG
            CreatePdfWb("<script>alert(1);</script>");
            #endregion
        }

        private static void CreatePdfWb(string htmlCode, int scale = 1)
        {
            if (scale != 1)
                htmlCode = htmlCode.Replace("<body>", $"<body style=\"zoom: {scale.ToString(System.Globalization.CultureInfo.InvariantCulture)}\">");

            using (MemoryStream stream = new MemoryStream())
            {
                AutoResetEvent bytesWaiter = new AutoResetEvent(false);

                Thread thread = new Thread(() =>
                {
                    using (WebBrowser br = new WebBrowser())
                    {
                        AutoResetEvent loadingWaiter = new AutoResetEvent(false);
                        br.AllowNavigation = true;
                        br.ScrollBarsEnabled = false;
                        br.ScriptErrorsSuppressed = true;
                        br.DocumentText = "0";
                        br.Width = 1024;
                        br.Document.OpenNew(true);
                        br.Document.Write(htmlCode);
                        br.Refresh();

                        br.DocumentCompleted += (a, b) => loadingWaiter.Set();

                        loadingWaiter.WaitOne(590);

                        br.Height = (int)(br.Document.Body.ScrollRectangle.Height * scale + 1);

                        using (Bitmap bmp = new Bitmap(br.Width, br.Height))
                        {
                            br.DrawToBitmap(bmp, new Rectangle(0, 0, br.Width, br.Height));
                            bmp.Save(stream, ImageFormat.Png);
                        }
                    }
                    bytesWaiter.Set();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();


                if (bytesWaiter.WaitOne(TimeSpan.FromSeconds(600)))
                {
                    string fileName = Environment.TickCount + ".png";

                    File.WriteAllBytes(fileName, stream.ToArray());

                    Process.Start(fileName);
                }
                else
                {
                    throw new TimeoutException("Слишком долго формировался принт - не хватило 10 минут!");
                }
            }
        }

        private static void CreatePdfHq(string htmlCode)
        {
            HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

            htmlToPdfConverter.BrowserWidth = 1024;
            htmlToPdfConverter.BrowserHeight = 768;

            // запрет запуска JS
            //htmlToPdfConverter.RunJavaScript = false;

            byte[] pdfBuffer = null;

            pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(htmlCode, "");

            string fileName = Environment.TickCount + ".pdf";

            File.WriteAllBytes(fileName, pdfBuffer);

            Process.Start(fileName);
        }
    }
}
