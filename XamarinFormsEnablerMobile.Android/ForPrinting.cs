using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.App;
#pragma warning disable CS0105 // Using directive appeared previously in this namespace
using Android.Content;
#pragma warning restore CS0105 // Using directive appeared previously in this namespace
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Print;
using Android.Util;
using XamarinFormsEnablerMobile.Droid;
using Xamarin.Forms;
using static XamarinFormsEnablerMobile.MainPage;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(ForPrinting))]
namespace XamarinFormsEnablerMobile.Droid
{
    public class ForPrinting : IPrintService
    {
        public class CustomAdapter : PrintDocumentAdapter
        {
            string test = "";
            int paperWidth = 0;

            public CustomAdapter(string content, int width)
            {
                test = content;
                paperWidth = width;
            }

            public override void OnWrite(PageRange[] pages, ParcelFileDescriptor destination, CancellationSignal cancellationSignal, WriteResultCallback callback)
            {
                //    throw new System.Exception(test.ToString());
                char[] delimiter = { '\n' };
                string[] testPrint = test.Split(delimiter);
                PdfDocument pdfDocument = new PdfDocument();
                try
                {
                    /*PdfDocument.PageInfo pageInfo = new PdfDocument.PageInfo.Builder(303, ((testPrint.Count() + 2) * 12), 1).Create();*/
                    PdfDocument.PageInfo pageInfo = new PdfDocument.PageInfo.Builder(paperWidth *60, ((testPrint.Count() + 2) * 100), 1).Create();
                    PdfDocument.Page page = pdfDocument.StartPage(pageInfo);
                    Canvas canvas = page.Canvas;
                    Paint paint = new Paint();
                    Typeface typeface = Typeface.Create(Typeface.Monospace, TypefaceStyle.Normal);
                    paint.SetTypeface(typeface);
                    //paint.TextSize = 12;
                    paint.TextSize = 100;

                    //paint.TextSize = 40;
                    int x = 1; int y = 100;
                    for (int i = 0; i < testPrint.Length; i++)
                    {
                        canvas.DrawText(testPrint[i], x, y * (i + 1), paint);
                    }
                    pdfDocument.FinishPage(page);
                    byte[] data;
                    using (MemoryStream memory = new MemoryStream())
                    {
                        pdfDocument.WriteTo(memory);
                        data = memory.ToArray();
                    }
                    pdfDocument.Close();
                    Java.IO.FileOutputStream output = new Java.IO.FileOutputStream(destination.FileDescriptor);
                    output.Write(data);
                    output.Close();
                    callback.OnWriteFinished(new PageRange[] { PageRange.AllPages });
                }
                catch (System.Exception e)
                {
                    Log.Debug("Error", e.Message);
                }
                finally
                {
                    pdfDocument.Close();
                    pdfDocument = null;
                }
            }

            public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
            {
                if (cancellationSignal.IsCanceled) return;
                PrintDocumentInfo info = new PrintDocumentInfo.Builder("PRINT_OUTPUT").SetContentType(PrintContentType.Document).Build();
                callback.OnLayoutFinished(info, true);
            }
        }

        public async void Print(string content, int width)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            PrintManager printManager = (PrintManager)Forms.Context.GetSystemService(Context.PrintService);
#pragma warning restore CS0618 // Type or member is obsolete
            string jobName = "Document";
            CustomAdapter printerAdapter = new CustomAdapter(content, width);
            await Task.Delay(1000);
            printManager.Print(jobName, printerAdapter, null);
        }


        public void PrintReceiptOnNative(string receipt, int fontSize)
        {

            /*string receiptValue = "";
            foreach(string value in receipt)
            {
                receiptValue += 
            }*/

            Intent intent = new Intent("com.sunmisilentprinterlibrary.PrintReceipt");
            intent.SetAction("com.sunmisilentprinterlibrary.PrintReceipt");
            intent.AddFlags(ActivityFlags.IncludeStoppedPackages);
            intent.PutExtra("testReceipt", receipt);
            intent.PutExtra("fontSize", fontSize);
            Android.App.Application.Context.SendBroadcast(intent);

        }




    }
}