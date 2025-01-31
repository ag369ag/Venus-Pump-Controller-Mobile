using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Graphics.Pdf;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using static AndroidX.Navigation.ActivityNavigator;
using static System.Net.Mime.MediaTypeNames;
using Java.Util;
using XamarinFormsEnablerMobile.Droid;
using static XamarinFormsEnablerMobile.MainPage;
using Java.Nio.FileNio;
using Android.Print;
using AndroidX.Emoji2.Text.FlatBuffer;
using AndroidX.ConstraintLayout.Core.Motion.Utils;

[assembly: Xamarin.Forms.Dependency(typeof(BluetooothPrinting))]
namespace XamarinFormsEnablerMobile.Droid
{
    public class BluetooothPrinting : PrintBluetooth
    {
        public async void PrintBT(byte[] pdf)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
#pragma warning restore CS0618 // Type or member is obsolete
            var pairedDevices = bluetoothAdapter.BondedDevices;
            BluetoothDevice currentDevice = null;
            foreach (BluetoothDevice device in pairedDevices)
            {
                if (device.Name.ToLower().Contains("inner"))
                {
                    currentDevice = device;
                    
                    UUID uuid1 = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                    /*foreach(var item in currentDevice.GetUuids().ToList())
                    {
                        Console.WriteLine(item.Uuid.ToString() ?? "None");
                    }*/
                    //ParcelUuid uuid = currentDevice.GetUuids().ElementAt(1);
                    var btSocket = currentDevice.CreateInsecureRfcommSocketToServiceRecord(uuid1);
                    btSocket.Connect();
                    if (btSocket.IsConnected)
                    {
                        List<byte> data = new List<byte>();
                        /*List<string> printText = new List<string>
                        {
                            "Test",
                            "printing"
                        };
                        foreach (string text in printText)
                        {
                            data.AddRange(System.Text.Encoding.UTF8.GetBytes(text));
                            data.Add(0x0A);
                        }*/
                        /*byte[] font = { 0x1b, 0x21, 0x01 };
                        await btSocket.OutputStream.WriteAsync(font);*/
                        await btSocket.OutputStream.WriteAsync(pdf, 0, pdf.Length);
                        data.Add(0x0A);
                        data.Add(0x0A);

                        /*var dataStream = btSocket.OutputStream;
                        byte[] byteArray = Encoding.UTF8.GetBytes("Test print");*/
                        await btSocket.OutputStream.WriteAsync(data.ToArray(), 0, data.Count);
                        byte[] cut = { 0x1D, 0x56, 66, 0x00 };
                        await btSocket.OutputStream.WriteAsync(cut, 0, cut.Length);
                        btSocket.Close();
                    }
                    else
                    {
                       
                    }
                    break;
                }
            }
        }

        public void PrintReceiptBT(string receiptData, int paperWidth)
        {
            char[] delimiter = { '\n' };
            string[] testPrint = receiptData.Split(delimiter);
            PdfDocument pdfDocument = new PdfDocument();
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                /*PdfDocument.PageInfo pageInfo = new PdfDocument.PageInfo.Builder(303, ((testPrint.Count() + 2) * 12), 1).Create();*/
                
                PdfDocument.PageInfo pageInfo = new PdfDocument.PageInfo.Builder(paperWidth * 60, ((testPrint.Count() + 2) * 100), 1).Create();
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
                //Bitmap bitmap = null;
                //Stream stream1 = new MemoryStream();
                using (MemoryStream memory = new MemoryStream())
                {
                    pdfDocument.WriteTo(memory);
                    data = memory.ToArray();
                    //stream1 = memory;
                   // bitmap.Compress(Bitmap.CompressFormat.Png, 100, memory);
                }
                pdfDocument.Close();
                Java.IO.File lastReceipt = new Java.IO.File("/storage/emulated/0/LastReceipt.pdf");
                FileOutputStream output = new FileOutputStream(lastReceipt);

               /* System.Console.WriteLine("Start");
                System.Console.WriteLine(lastReceipt.Path);
                System.Console.WriteLine("End");*/

                output.Write(data);
                output.Close();
                
                /*Bitmap testBitmap = await BitmapFactory.DecodeByteArrayAsync(data, 0, data.Length);
                ByteArrayOutputStream stream = new ByteArrayOutputStream();
                testBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream1);*/
                //Bitmap testBitmap = await BitmapFactory.DecodeFileAsync(lastReceipt.Path);

                byte[] SELECT_BIT_IMAGE_MODE = { 0x1B, 0x2A, 33, 255, 3 };
                
                PrintBT(data);


            }
            catch(Exception e) { }
#pragma warning restore CS0168 // Variable is declared but never used
        }
    }
}